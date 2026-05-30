using System;
using System.Collections;
using System.Collections.Generic;
using BongoCat.Multiplayer;
using DG.Tweening;
using Steamworks;
using TMPro;
using UnityEngine;
using Vfx;

namespace BongoCat
{
	public class Shop : MonoBehaviour
	{
		[SerializeField]
		private bool _isEmoteShop;

		private int _stockRefreshTime = 1800;

		public int StockRefreshTimeLeft;

		[SerializeField]
		private string _shopTimeKey = "TIME_LEFT";

		[SerializeField]
		private float _delayDefocusOnStartup;

		private bool _canDefocus;

		[SerializeField]
		private ShopItem _shopItem;

		[SerializeField]
		private GameObject _shopVisuals;

		[SerializeField]
		private GameObject _loadingObj;

		[SerializeField]
		private GameObject _successObj;

		[SerializeField]
		private GameObject _failObj;

		[SerializeField]
		private GameObject _outOfStockObj;

		private TMP_Text _stockRefreshText;

		[SerializeField]
		private PricePaid _pricePaid;

		[SerializeField]
		private PlayerPrefsToggle _animateChestPurchase;

		[SerializeField]
		private ErrorMessage _genericSteamError;

		[SerializeField]
		private FlashAnimation _counterFlash;

		private ShakeAnimation _shakeAnimation;

		[SerializeField]
		private PlayerPrefsToggle _showChestPopup;

		public static HashSet<int> OnceEquippedItems = new HashSet<int>();

		public bool ChestIsReady;

		public static Shop NormalShop;

		public static Shop EmoteShop;

		public bool CanGetChest
		{
			get
			{
				if (ChestIsReady)
				{
					return _shopItem.CanBuy();
				}
				return false;
			}
		}

		private void Awake()
		{
			_shakeAnimation = _shopVisuals.GetComponent<ShakeAnimation>();
			if (_isEmoteShop)
			{
				EmoteShop = this;
			}
			else
			{
				NormalShop = this;
			}
		}

		private IEnumerator Start()
		{
			StockRefreshTimeLeft = PlayerPrefs.GetInt(_shopTimeKey, _stockRefreshTime);
			StockRefreshTimeLeft = Mathf.Min(StockRefreshTimeLeft, _stockRefreshTime);
			_shopItem.gameObject.SetActive(value: false);
			_outOfStockObj.SetActive(value: true);
			_stockRefreshText = _outOfStockObj.GetComponent<TMP_Text>();
			_stockRefreshText.text = $"{TimeSpan.FromSeconds(StockRefreshTimeLeft):mm':'ss}";
			yield return new WaitUntil(() => CatInventory.Instance.WasLoadedFromSteam);
			_shopVisuals.SetActive(_showChestPopup.Value || SettingsManager.Instance.AlwaysShowChest.Value);
			StartCoroutine(DelayDefocusApplication());
			StartCoroutine(TimerUpdate());
		}

		public void SetLoadingVisuals()
		{
			_shopItem.gameObject.SetActive(value: false);
			_outOfStockObj.SetActive(value: false);
			_loadingObj.SetActive(value: true);
		}

		public void SetSuccessVisuals(bool success, bool showError = true)
		{
			if (success && _animateChestPurchase.Value)
			{
				_shopVisuals.transform.DOPunchScale(Vector3.one * 0.15f, 0.5f);
				_pricePaid.Animate();
			}
			_successObj.SetActive(success);
			_failObj.SetActive(!success);
			_shopItem.gameObject.SetActive(value: false);
			_outOfStockObj.SetActive(value: false);
			_loadingObj.SetActive(value: false);
			StartCoroutine(SetOutOfStockDelayed());
			if (success)
			{
				return;
			}
			StockRefreshTimeLeft = 60;
			_stockRefreshText.text = $"{TimeSpan.FromSeconds(StockRefreshTimeLeft):mm':'ss}";
			ChestIsReady = false;
			if (showError)
			{
				ErrorMessageHandler.Instance.SetErrorMessage(_genericSteamError, delegate
				{
					ErrorMessageHandler.Instance.CloseErrorPopup();
				});
			}
		}

		private IEnumerator SetOutOfStockDelayed()
		{
			yield return new WaitForSeconds(0.8f);
			_successObj.SetActive(value: false);
			_failObj.SetActive(value: false);
			_outOfStockObj.SetActive(value: true);
		}

		private IEnumerator DelayDefocusApplication()
		{
			yield return new WaitForSecondsRealtime(_delayDefocusOnStartup);
			_canDefocus = true;
			OnApplicationFocus(Application.isFocused);
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (!hasFocus && (StockRefreshTimeLeft > 0 || !CanGetChest) && _canDefocus)
			{
				HideTimer(applicationDefocused: true);
			}
		}

		private IEnumerator TimerUpdate()
		{
			while (true)
			{
				if (_outOfStockObj.activeSelf)
				{
					StockRefreshTimeLeft--;
					PlayerPrefs.SetInt(_shopTimeKey, StockRefreshTimeLeft);
					_stockRefreshText.text = $"{TimeSpan.FromSeconds(StockRefreshTimeLeft):mm':'ss}";
					SteamItemDetails_t steamItemDetails_t = (_isEmoteShop ? CatInventory.Instance.EmoteChestToken : CatInventory.Instance.ChestToken);
					if (StockRefreshTimeLeft <= 0)
					{
						if (steamItemDetails_t.m_unQuantity == 0)
						{
							StockRefreshTimeLeft = 60;
						}
						else
						{
							StockRefreshTimeLeft = 0;
							_shopItem.gameObject.SetActive(value: true);
							_outOfStockObj.SetActive(value: false);
							ChestIsReady = true;
							if (_showChestPopup.Value && _shopItem.CanBuy())
							{
								if (!_isEmoteShop)
								{
									SteamMultiplayer.Instance.SendChestReady(ChestIsReady);
								}
								_shopVisuals.SetActive(value: true);
							}
						}
					}
				}
				else if (StockRefreshTimeLeft <= 0 && !_shopVisuals.activeInHierarchy && _showChestPopup.Value && _shopItem.CanBuy())
				{
					if (!_isEmoteShop)
					{
						SteamMultiplayer.Instance.SendChestReady(ChestIsReady);
					}
					_shopVisuals.SetActive(value: true);
				}
				yield return new WaitForSecondsRealtime(1f);
			}
		}

		private void OnDestroy()
		{
			PlayerPrefs.Save();
		}

		public void ItemGotBought()
		{
			StockRefreshTimeLeft = _stockRefreshTime;
			PlayerPrefs.SetInt(_shopTimeKey, StockRefreshTimeLeft);
			ChestIsReady = false;
		}

		public void HideTimer(bool applicationDefocused = false)
		{
			if (!SettingsManager.Instance.AlwaysShowChest.Value && (!ChestIsReady || applicationDefocused))
			{
				_shopVisuals.SetActive(value: false);
			}
		}

		public void OnClick()
		{
			if (!_shopItem.gameObject.activeSelf)
			{
				_shakeAnimation.Shake();
			}
			else if (!_shopItem.CanBuy())
			{
				_shakeAnimation.Shake();
				_counterFlash.Flash();
			}
			else
			{
				_shopItem.Buy();
			}
		}
	}
}

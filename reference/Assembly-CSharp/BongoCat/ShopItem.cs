using BongoCat.Multiplayer;
using IroxGames.StoreFronts.Steam;
using Steam;
using Steamworks;
using TMPro;
using UnityEngine;

namespace BongoCat
{
	public class ShopItem : MonoBehaviour
	{
		[SerializeField]
		private int _price;

		[SerializeField]
		private TMP_Text _priceText;

		[SerializeField]
		private bool _waitingForServer;

		[SerializeField]
		private NewItemPopup _newItemPopup;

		[SerializeField]
		private SteamMultiplayer _steamMultiplayer;

		[SerializeField]
		private bool _isEmoteChest;

		[SerializeField]
		private Shop _shop;

		[SerializeField]
		private ChestExchanger _chestExchanger;

		private void Awake()
		{
			_priceText.text = $"{_price}";
		}

		public void Buy()
		{
			if (Pets.Instance.CanSpendPets(_price) && !_waitingForServer && SteamManager.s_EverInitialized)
			{
				SteamUtils.GetSecondsSinceAppActive();
				_waitingForServer = true;
				_shop.SetLoadingVisuals();
				if (_isEmoteChest)
				{
					_chestExchanger.OpenEmoteChest(Callback);
				}
				else
				{
					_chestExchanger.OpenChest(Callback);
				}
			}
		}

		private void Callback((SteamItem, bool) res)
		{
			_waitingForServer = false;
			if (res.Item1 != null)
			{
				_shop.SetSuccessVisuals(success: true);
				Pets.Instance.TrySpendPets(_price);
				_shop.ItemGotBought();
				_newItemPopup.ShowPopup(res.Item1, !res.Item2);
				_steamMultiplayer.SendReceivedItem(res.Item1.SteamItemDefId);
				if (!_isEmoteChest && SettingsManager.Instance.AutoEquipDrops)
				{
					CatCosmetics.Instance.AutoEquipDrop(res.Item1);
				}
			}
			else
			{
				_shop.SetSuccessVisuals(success: false);
			}
			if (!_isEmoteChest)
			{
				_steamMultiplayer.SendChestReady(ready: false);
			}
		}

		public bool CanBuy()
		{
			return Pets.Instance.CanSpendPets(_price);
		}
	}
}

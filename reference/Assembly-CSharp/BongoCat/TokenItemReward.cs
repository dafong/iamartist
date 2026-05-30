using System.Collections;
using BongoCat.SteamJsonParser;
using BongoCat.TokenRewards;
using Steam;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat
{
	public class TokenItemReward : MonoBehaviour
	{
		[SerializeField]
		private SteamItemUnity _token;

		private SteamBundleExchange _selectedBundle;

		[SerializeField]
		private GameObject _selectionUi;

		[SerializeField]
		private Button _confirmButton;

		private Coroutine _conditionCoroutine;

		private Callback<SteamInventoryResultReady_t> _inventoryResultReadyCallback;

		private bool _hasAnyRewardItem;

		[SerializeField]
		private TokenRewardCondition _condition;

		[SerializeField]
		private bool _checkContinuously;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => CatInventory.Instance.WasLoadedFromSteam);
			if (_checkContinuously)
			{
				_inventoryResultReadyCallback = Callback<SteamInventoryResultReady_t>.Create(OnInventoryResultReady);
				_conditionCoroutine = StartCoroutine(CheckRewardConditionsRoutine());
			}
		}

		private void OnInventoryResultReady(SteamInventoryResultReady_t result)
		{
			StartCoroutine(TryOpenUiDelayed());
		}

		private IEnumerator TryOpenUiDelayed()
		{
			yield return new WaitForEndOfFrame();
			TryOpenUi();
		}

		private IEnumerator CheckRewardConditionsRoutine()
		{
			WaitForSeconds waitThirtySeconds = new WaitForSeconds(30f);
			while (!CheckRewardConditions())
			{
				yield return waitThirtySeconds;
			}
			_selectionUi.SetActive(value: true);
			_conditionCoroutine = null;
		}

		public void TryOpenUi()
		{
			if (CheckRewardConditions())
			{
				_selectionUi.SetActive(value: true);
				if (_conditionCoroutine != null)
				{
					StopCoroutine(_conditionCoroutine);
				}
			}
			else
			{
				_selectionUi.SetActive(value: false);
			}
		}

		private bool CheckRewardConditions()
		{
			if (!CatInventory.Instance.OtherTokens.ContainsKey(_token.Id))
			{
				return false;
			}
			if ((bool)_condition)
			{
				return _condition.CheckRewardConditions();
			}
			return true;
		}

		public void SetSelectedItem(SteamBundleExchange selectedBundle)
		{
			_selectedBundle = selectedBundle;
			_confirmButton.interactable = true;
		}

		public void ExchangeItems()
		{
			SteamAnalytics.Instance.SetStatToOne(_selectedBundle.name);
			TokenExchanger.Instance.Exchange(_selectedBundle);
			_selectionUi.SetActive(value: false);
		}
	}
}

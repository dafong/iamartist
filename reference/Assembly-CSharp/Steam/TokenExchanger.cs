using System.Collections;
using BongoCat;
using BongoCat.SteamJsonParser;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace Steam
{
	public class TokenExchanger : MonoBehaviour
	{
		[SerializeField]
		private NewItemPopup _itemPopup;

		private Callback<SteamInventoryResultReady_t> _inventoryResultCallback;

		private SteamInventoryResult_t _resultHandle;

		private SteamBundleExchange _exchangedBundle;

		public static TokenExchanger Instance;

		public bool IsExchanging => _resultHandle != SteamInventoryResult_t.Invalid;

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			_resultHandle = SteamInventoryResult_t.Invalid;
			yield return new WaitUntil(() => CatInventory.Instance.WasLoadedFromSteam);
			_inventoryResultCallback = Callback<SteamInventoryResultReady_t>.Create(OnInventoryResultReady);
		}

		private void OnInventoryResultReady(SteamInventoryResultReady_t result)
		{
			if (_resultHandle == SteamInventoryResult_t.Invalid || _resultHandle != result.m_handle)
			{
				return;
			}
			if (SteamInventory.GetResultStatus(result.m_handle) != EResult.k_EResultOK)
			{
				CleanupHandle();
				return;
			}
			uint punOutItemsArraySize = 0u;
			if (!SteamInventory.GetResultItems(_resultHandle, null, ref punOutItemsArraySize) || punOutItemsArraySize == 0)
			{
				Debug.Log("TokenExchanger | Array size <= 0: " + punOutItemsArraySize);
				CleanupHandle();
				return;
			}
			SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
			SteamInventory.GetResultItems(_resultHandle, array, ref punOutItemsArraySize);
			if (punOutItemsArraySize <= 1)
			{
				Debug.Log("TokenExchanger | itemArraySize <= 1: should not happen");
				CleanupHandle();
			}
			for (int i = 0; i < _exchangedBundle.Input.Count; i++)
			{
				if (CatInventory.Instance.OtherTokens.ContainsKey(array[i].m_iDefinition.m_SteamItemDef))
				{
					if (array[i].m_unQuantity == 0)
					{
						CatInventory.Instance.OtherTokens.Remove(array[i].m_iDefinition.m_SteamItemDef);
					}
					else
					{
						CatInventory.Instance.OtherTokens[array[i].m_iDefinition.m_SteamItemDef] = array[i];
					}
				}
			}
			for (int j = _exchangedBundle.Input.Count; j < punOutItemsArraySize; j++)
			{
				bool isNew;
				SteamItem steamItem = CatInventory.Instance.HandleItem(array[j], CatInventory.GetLastModifiedDateFromItem(_resultHandle, 0u, array[j]), out isNew);
				if (steamItem != null)
				{
					_itemPopup.ShowPopup(steamItem, !isNew);
				}
			}
			CatInventory.Instance.UpdateVisuals();
			_exchangedBundle = null;
			CleanupHandle();
			CatInventory.Instance.OnOtherTokenReceived?.Invoke();
		}

		private void CleanupHandle()
		{
			if (SteamManager.s_EverInitialized && _resultHandle != SteamInventoryResult_t.Invalid)
			{
				SteamInventory.DestroyResult(_resultHandle);
				_resultHandle = SteamInventoryResult_t.Invalid;
			}
		}

		private void OnDisable()
		{
			MonoBehaviour.print("OnDisable TokenExchanger");
			CleanupHandle();
		}

		public bool CanExchange(SteamBundleExchange exchange)
		{
			foreach (SteamItemUnity item in exchange.Input)
			{
				if (!CatInventory.Instance.OtherTokens.ContainsKey(item.Id))
				{
					return false;
				}
			}
			return true;
		}

		public void Exchange(SteamBundleExchange bundleExchange)
		{
			if (SteamManager.ShuttingDown)
			{
				return;
			}
			SteamItemDef_t steamItemDef_t = new SteamItemDef_t(bundleExchange.Id);
			SteamItemInstanceID_t[] array = new SteamItemInstanceID_t[bundleExchange.Input.Count];
			uint[] array2 = new uint[bundleExchange.Input.Count];
			for (int i = 0; i < array.Length; i++)
			{
				if (!CatInventory.Instance.OtherTokens.TryGetValue(bundleExchange.Input[i].Id, out var value))
				{
					return;
				}
				array[i] = value.m_itemId;
				array2[i] = 1u;
			}
			_exchangedBundle = bundleExchange;
			SteamInventory.ExchangeItems(out _resultHandle, new SteamItemDef_t[1] { steamItemDef_t }, new uint[1] { 1u }, 1u, array, array2, (uint)array.Length);
		}
	}
}

using System.Collections;
using System.Threading;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace BongoCat
{
	public class PlaytimeItemDrop : MonoBehaviour
	{
		[SerializeField]
		private ErrorMessage _serverLostError;

		private Callback<SteamInventoryResultReady_t> _t;

		private SteamInventoryResult_t _resultHandle;

		private Timer _timer;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			_resultHandle = SteamInventoryResult_t.Invalid;
			_t = Callback<SteamInventoryResultReady_t>.Create(InventoryResultReady);
			int chestItemId = SteamItemIdReference.Instance.ChestGenerator;
			_timer = new Timer(delegate
			{
				if (CatInventory.Instance.ChestToken.m_unQuantity < 10 || CatInventory.Instance.EmoteChestToken.m_unQuantity < 10)
				{
					TriggerItemDrop(chestItemId);
				}
			}, null, 60000, 60000);
		}

		private void OnDisable()
		{
			CleanupHandle();
			_timer?.Dispose();
		}

		private void CleanupHandle()
		{
			if (SteamManager.s_EverInitialized && _resultHandle != SteamInventoryResult_t.Invalid)
			{
				SteamInventory.DestroyResult(_resultHandle);
				_resultHandle = SteamInventoryResult_t.Invalid;
			}
		}

		private void InventoryResultReady(SteamInventoryResultReady_t result)
		{
			if (_resultHandle == SteamInventoryResult_t.Invalid || _resultHandle != result.m_handle)
			{
				return;
			}
			EResult resultStatus = SteamInventory.GetResultStatus(result.m_handle);
			if (resultStatus != EResult.k_EResultOK)
			{
				Debug.Log("PlaytimeItemDrop | " + _resultHandle.m_SteamInventoryResult + " " + resultStatus.ToString("G"));
				ErrorMessageHandler.Instance.SetErrorMessage(_serverLostError, leftButtonAction: Application.Quit, rightButtonAction: ErrorMessageHandler.Instance.CloseErrorPopup);
				return;
			}
			uint punOutItemsArraySize = 0u;
			if (!SteamInventory.GetResultItems(_resultHandle, null, ref punOutItemsArraySize) || punOutItemsArraySize == 0)
			{
				CleanupHandle();
				return;
			}
			SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
			SteamInventory.GetResultItems(_resultHandle, array, ref punOutItemsArraySize);
			SteamItemDetails_t[] array2 = array;
			foreach (SteamItemDetails_t steamItemDetails_t in array2)
			{
				CatInventory.Instance.HandleItem(steamItemDetails_t, CatInventory.GetLastModifiedDateFromItem(_resultHandle, 0u, steamItemDetails_t), out var _);
			}
			CatInventory.Instance.UpdateVisuals();
			CleanupHandle();
		}

		private void TriggerItemDrop(int itemId)
		{
			if (!SteamManager.ShuttingDown)
			{
				CleanupHandle();
				SteamInventory.TriggerItemDrop(out _resultHandle, new SteamItemDef_t(itemId));
			}
		}
	}
}

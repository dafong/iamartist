using System.Collections.Generic;
using System.Linq;
using BongoCat;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

public class ItemConsumer : MonoBehaviour
{
	public static ItemConsumer Instance;

	private Callback<SteamInventoryResultReady_t> _inventoryResultCallback;

	private List<SteamInventoryResult_t> _resultHandles;

	private void Awake()
	{
		Instance = this;
		_resultHandles = new List<SteamInventoryResult_t>();
		_inventoryResultCallback = Callback<SteamInventoryResultReady_t>.Create(OnInventoryResultReady);
	}

	public void ConsumeItem(SteamItem item)
	{
		SteamInventory.ConsumeItem(out var pResultHandle, item.InstanceIds.First().m_itemId, 1u);
		_resultHandles.Add(pResultHandle);
	}

	private void OnInventoryResultReady(SteamInventoryResultReady_t result)
	{
		SteamInventoryResult_t handle = result.m_handle;
		if (_resultHandles.Contains(handle))
		{
			if (SteamInventory.GetResultStatus(result.m_handle) != EResult.k_EResultOK)
			{
				CleanupHandle(handle);
			}
			uint punOutItemsArraySize = 0u;
			if (!SteamInventory.GetResultItems(handle, null, ref punOutItemsArraySize) || punOutItemsArraySize != 1)
			{
				CleanupHandle(handle);
				return;
			}
			SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
			SteamInventory.GetResultItems(handle, array, ref punOutItemsArraySize);
			CatInventory.Instance.ConsumeItem(array[0]);
			CleanupHandle(handle);
		}
	}

	private void CleanupHandle(SteamInventoryResult_t resultHandle)
	{
		if (SteamManager.s_EverInitialized)
		{
			SteamInventory.DestroyResult(resultHandle);
			_resultHandles.Remove(resultHandle);
		}
	}
}

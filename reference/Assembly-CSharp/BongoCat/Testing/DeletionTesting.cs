using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;

namespace BongoCat.Testing
{
	public class DeletionTesting : MonoBehaviour
	{
		private struct InventorySaveData
		{
			public int ItemId;

			public int Quantity;
		}

		private SteamInventoryResult_t _resultHandle;

		private List<InventorySaveData> _savedInventory = new List<InventorySaveData>();

		private int _steamItemToGenerateId;

		private IEnumerator Start()
		{
			_resultHandle = SteamInventoryResult_t.Invalid;
			yield return new WaitUntil(() => CatInventory.Instance.WasLoadedFromSteam);
			SaveInventory();
		}

		private void OnInventoryResultReady(SteamInventoryResultReady_t result)
		{
			if (!(result.m_handle != _resultHandle) && result.m_result == EResult.k_EResultOK)
			{
				CatInventory.Instance.RestartRoutine();
				CleanupHandle();
			}
		}

		private void CleanupHandle()
		{
			if (_resultHandle != SteamInventoryResult_t.Invalid)
			{
				SteamInventory.DestroyResult(_resultHandle);
				_resultHandle = SteamInventoryResult_t.Invalid;
			}
		}

		public void OnEndEdit(string itemIdString)
		{
			int.TryParse(itemIdString, out _steamItemToGenerateId);
		}

		public void GenerateItem()
		{
			SteamItemDef_t[] pArrayItemDefs = new SteamItemDef_t[1]
			{
				new SteamItemDef_t(_steamItemToGenerateId)
			};
			uint[] punArrayQuantity = new uint[1] { 1u };
			SteamInventory.GenerateItems(out _resultHandle, pArrayItemDefs, punArrayQuantity, 1u);
		}

		public void Generate50Items()
		{
			SteamItemDef_t[] pArrayItemDefs = new SteamItemDef_t[1]
			{
				new SteamItemDef_t(_steamItemToGenerateId)
			};
			uint[] punArrayQuantity = new uint[1] { 50u };
			SteamInventory.GenerateItems(out _resultHandle, pArrayItemDefs, punArrayQuantity, 1u);
		}

		public void SaveInventory()
		{
			_savedInventory.Clear();
			foreach (SteamItem item2 in CatInventory.Instance.Items.Where((SteamItem steamItem) => steamItem.ItemAmount > 0))
			{
				InventorySaveData item = new InventorySaveData
				{
					ItemId = item2.SteamItemDefId,
					Quantity = item2.ItemAmount
				};
				_savedInventory.Add(item);
			}
		}

		public void RestoreInventory()
		{
			SteamItemDef_t[] array = new SteamItemDef_t[_savedInventory.Count];
			uint[] array2 = new uint[_savedInventory.Count];
			for (int i = 0; i < _savedInventory.Count; i++)
			{
				array[i] = new SteamItemDef_t(_savedInventory[i].ItemId);
				array2[i] = (uint)_savedInventory[i].Quantity;
			}
			SteamInventory.GenerateItems(out _resultHandle, array, array2, (uint)array.Length);
		}
	}
}

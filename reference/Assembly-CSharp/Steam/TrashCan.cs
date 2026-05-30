using System.Collections.Generic;
using System.Linq;
using BongoCat;
using BongoCat.Localizer;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Steam
{
	public class TrashCan : MonoBehaviour
	{
		public static TrashCan Instance;

		[SerializeField]
		private Transform _trashSlotRoot;

		[SerializeField]
		private Button _deleteButton;

		[SerializeField]
		private Button _confirmSelectedItemDeletionButton;

		[SerializeField]
		private Button _confirmFullInventoryDeletionButton;

		[SerializeField]
		private MenuTabs _menuTabs;

		private Dictionary<SteamItem, TrashSlot> _slots = new Dictionary<SteamItem, TrashSlot>();

		private Callback<SteamInventoryResultReady_t> _inventoryResultCallback;

		private HashSet<SteamInventoryResult_t> _results = new HashSet<SteamInventoryResult_t>();

		[SerializeField]
		private bool _inTrashMode;

		[SerializeField]
		private TrashSlot _trashSlotPrefab;

		[SerializeField]
		private TMP_Text _deleteSelectedWarningText;

		private bool _confirmedItemDeletion;

		private const string DELETE_ITEMS_WARNING_LOCA_KEY = "DeleteSelectedItemsWarning";

		public bool InTrashMode => _inTrashMode;

		private void Awake()
		{
			Instance = this;
			_inventoryResultCallback = Callback<SteamInventoryResultReady_t>.Create(OnInventoryResult);
		}

		private void OnInventoryResult(SteamInventoryResultReady_t result)
		{
			if (!_results.Contains(result.m_handle))
			{
				return;
			}
			SteamInventoryResult_t handle = result.m_handle;
			uint punOutItemsArraySize = 0u;
			if (!SteamInventory.GetResultItems(handle, null, ref punOutItemsArraySize) || punOutItemsArraySize == 0)
			{
				MonoBehaviour.print("TrashCan: Result is empty");
				CleanupHandle(handle);
				return;
			}
			SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
			SteamInventory.GetResultItems(handle, array, ref punOutItemsArraySize);
			for (uint num = 0u; num < punOutItemsArraySize; num++)
			{
				CatInventory.Instance.RemoveItem(array[num]);
			}
			CatInventory.Instance.RestartRoutine();
			CleanupHandle(handle);
		}

		public void AddToTrash(SteamItem item)
		{
			if (_slots.ContainsKey(item))
			{
				RemoveFromTrash(item);
			}
			TrashSlot trashSlot = Object.Instantiate(_trashSlotPrefab, _trashSlotRoot);
			trashSlot.SetItem(item);
			_slots.Add(item, trashSlot);
			_deleteButton.interactable = _slots.Keys.Count > 0;
			_deleteSelectedWarningText.text = string.Format(Loca.Instance.Get("DeleteSelectedItemsWarning"), _slots.Keys.Count);
			item.OnItemUpdated?.Invoke();
		}

		public void RemoveFromTrash(SteamItem item)
		{
			if (_slots.TryGetValue(item, out var value))
			{
				Object.Destroy(value.gameObject);
				_slots.Remove(item);
				_deleteButton.interactable = _slots.Keys.Count > 0;
				item.OnItemUpdated?.Invoke();
			}
		}

		public void RestoreAllItems()
		{
			List<SteamItem> list = _slots.Keys.ToList();
			foreach (TrashSlot value in _slots.Values)
			{
				Object.Destroy(value.gameObject);
			}
			_slots.Clear();
			foreach (SteamItem item in list)
			{
				item.OnItemUpdated?.Invoke();
			}
			_inTrashMode = false;
			_deleteButton.interactable = _slots.Keys.Count > 0;
		}

		public void DeleteSlottedItems()
		{
			if (!_confirmedItemDeletion)
			{
				return;
			}
			foreach (SteamItem key in _slots.Keys)
			{
				foreach (SteamItemDetails_t instanceId in key.InstanceIds)
				{
					DeleteItem(instanceId);
				}
			}
			_inTrashMode = false;
			_deleteButton.interactable = _slots.Keys.Count > 0;
			_confirmedItemDeletion = false;
			ConfirmSelectedItemsDeletion(confirmed: false);
		}

		public void DeleteFullInventory()
		{
			if (!_confirmedItemDeletion)
			{
				return;
			}
			foreach (SteamItem item in CatInventory.Instance.Items.Where((SteamItem item) => item.ItemAmount > 0))
			{
				foreach (SteamItemDetails_t instanceId in item.InstanceIds)
				{
					DeleteItem(instanceId);
				}
			}
			ConfirmFullInventoryDeletion(confirmed: false);
		}

		private void DeleteItem(SteamItemDetails_t instanceId)
		{
			SteamInventory.ConsumeItem(out var pResultHandle, instanceId.m_itemId, instanceId.m_unQuantity);
			_results.Add(pResultHandle);
		}

		private void CleanupHandle(SteamInventoryResult_t handle)
		{
			if (SteamManager.s_EverInitialized)
			{
				_results.Remove(handle);
				SteamInventory.DestroyResult(handle);
			}
		}

		public void EnterTrashMode()
		{
			_inTrashMode = true;
			_menuTabs.OpenMenuTab(MenuTabs.MenuTab.TrashBin);
		}

		public void ConfirmSelectedItemsDeletion(bool confirmed)
		{
			_confirmedItemDeletion = confirmed;
			_confirmSelectedItemDeletionButton.interactable = confirmed;
		}

		public void ConfirmFullInventoryDeletion(bool confirmed)
		{
			_confirmedItemDeletion = confirmed;
			_confirmFullInventoryDeletionButton.interactable = confirmed;
		}

		public bool IsItemInTrash(SteamItem item)
		{
			return _slots.ContainsKey(item);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BongoCat
{
	public class DisableIfItemsInInventory : MonoBehaviour
	{
		[SerializeField]
		private List<int> _itemIds;

		[SerializeField]
		private bool _disableIfAnyItemFound;

		private List<SteamItem> _items = new List<SteamItem>();

		public bool _initialized;

		public Action OnDisabled;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => CatInventory.Instance.IsInitialized);
			foreach (SteamItem item in CatInventory.Instance.Items.Where((SteamItem item) => _itemIds.Contains(item.SteamItemDefId)))
			{
				_items.Add(item);
				item.OnItemUpdated = (Action)Delegate.Combine(item.OnItemUpdated, new Action(OnItemUpdated));
			}
			OnItemUpdated();
			_initialized = true;
		}

		private void OnItemUpdated()
		{
			if (_disableIfAnyItemFound)
			{
				base.gameObject.SetActive(!_items.Any((SteamItem item) => item.ItemAmount > 0));
			}
			else
			{
				base.gameObject.SetActive(!_items.All((SteamItem item) => item.ItemAmount > 0));
			}
		}

		private void OnEnable()
		{
			if (!_initialized)
			{
				return;
			}
			foreach (SteamItem item in _items)
			{
				item.OnItemUpdated = (Action)Delegate.Combine(item.OnItemUpdated, new Action(OnItemUpdated));
			}
			OnItemUpdated();
		}

		private void OnDisable()
		{
			if (!_initialized)
			{
				return;
			}
			foreach (SteamItem item in _items)
			{
				item.OnItemUpdated = (Action)Delegate.Remove(item.OnItemUpdated, new Action(OnItemUpdated));
			}
			OnDisabled?.Invoke();
		}
	}
}

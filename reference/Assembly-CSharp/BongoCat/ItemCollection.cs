using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BongoCat
{
	public class ItemCollection : MonoBehaviour
	{
		[SerializeField]
		private InventoryItem _collectionItemPrefab;

		[SerializeField]
		private InventoryItem _emoteItemPrefab;

		[SerializeField]
		private Transform _collectionRoot;

		[SerializeField]
		private TMP_Text _collectionNameText;

		private string _collectionName;

		private bool _isPartOfBongoDex;

		private bool _isEventCollection;

		private List<InventoryItem> _collectionItems = new List<InventoryItem>();

		public int Count => _collectionItems.Count((InventoryItem item) => item.SteamItem.ItemAmount > 0 || !item.SteamItem.HideUntilReceived);

		private int OwnedItems => _collectionItems.Count((InventoryItem item) => item.SteamItem.ItemAmount > 0);

		private bool OwnsItems => _collectionItems.Any((InventoryItem item) => item.SteamItem.ItemAmount > 0);

		public int InventoryDisplayCount => _collectionItems.Count((InventoryItem item) => item.SteamItem.DisplayedItemAmount > 0);

		public List<InventoryItem> CollectionItems => _collectionItems;

		public void CreateCollection(string collectionName, List<SteamItem> collectionSteamItems, bool isPartOfBongoDex, bool isEventCollection = false)
		{
			_isPartOfBongoDex = isPartOfBongoDex;
			_isEventCollection = isEventCollection;
			if ((bool)_collectionNameText)
			{
				base.gameObject.name = collectionName + "Collection";
				_collectionNameText.text = collectionName;
				_collectionNameText.gameObject.SetActive(collectionName.Length > 0);
				_collectionName = collectionName;
			}
			foreach (SteamItem collectionSteamItem in collectionSteamItems)
			{
				Add(collectionSteamItem, isPartOfBongoDex);
			}
			OnItemUpdated();
		}

		private void OnEnable()
		{
			SortCollection();
		}

		public void SortCollection()
		{
			if (base.gameObject.activeInHierarchy)
			{
				_collectionItems.RemoveAll((InventoryItem item) => !item);
				SortCollectionInitial();
				StartCoroutine(SortCollectionDelayed());
			}
		}

		private void SortCollectionInitial()
		{
			if (_isPartOfBongoDex)
			{
				_collectionItems = _collectionItems.OrderByDescending((InventoryItem i) => (int)i.SteamItem.QualityCategory).ThenByDescending(delegate(InventoryItem i)
				{
					string itemSlot = i.SteamItem.ItemSlot;
					if (itemSlot == "hat")
					{
						return 2;
					}
					return (itemSlot == "skin") ? 1 : 0;
				}).ThenBy((InventoryItem i) => i.SteamItem.SteamItemDefId)
					.ToList();
			}
			else
			{
				_collectionItems = (from i in _collectionItems
					orderby i.SteamItem.IsFavorite descending, (int)i.SteamItem.QualityCategory descending, i.SteamItem.OldestItemTimestamp descending
					select i).ToList();
			}
			foreach (InventoryItem collectionItem in _collectionItems)
			{
				collectionItem.transform.SetAsLastSibling();
			}
		}

		private IEnumerator SortCollectionDelayed()
		{
			yield return null;
			SortCollectionInitial();
		}

		public void Add(SteamItem item, bool isPartOfBongoDex)
		{
			InventoryItem inventoryItem = ((!item.IsEmote && !item.IsConsumable) ? global::UnityEngine.Object.Instantiate(_collectionItemPrefab, _collectionRoot) : global::UnityEngine.Object.Instantiate(_emoteItemPrefab, _collectionRoot));
			inventoryItem.SetItem(item, isPartOfBongoDex);
			_collectionItems.Add(inventoryItem);
			if (isPartOfBongoDex)
			{
				SteamItem steamItem = inventoryItem.SteamItem;
				steamItem.OnItemUpdated = (Action)Delegate.Combine(steamItem.OnItemUpdated, new Action(OnItemUpdated));
			}
		}

		private void OnItemUpdated()
		{
			if ((bool)_collectionNameText)
			{
				_collectionNameText.text = $"{_collectionName} ({OwnedItems}/{Count})";
				if (_isEventCollection)
				{
					base.gameObject.SetActive(OwnsItems);
				}
			}
		}
	}
}

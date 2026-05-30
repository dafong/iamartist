using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BongoCat.SteamJsonParser;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat
{
	public class BongoDex : MonoBehaviour
	{
		[SerializeField]
		private ItemCollection _collectionPrefab;

		[SerializeField]
		private Transform _uiRoot;

		[SerializeField]
		private CatInventory _catInventory;

		[SerializeField]
		private GameObject _inventoryContentUi;

		[SerializeField]
		private GameObject _bongodexContentUi;

		[SerializeField]
		private Image _chestIcon;

		[SerializeField]
		private CollectionNames _collectionNames;

		private Dictionary<string, List<SteamItem>> _eventItemsDict = new Dictionary<string, List<SteamItem>>();

		private Dictionary<QualityCategory, List<SteamItem>> _baseItemsDict = new Dictionary<QualityCategory, List<SteamItem>>();

		private Dictionary<string, ItemCollection> _collections = new Dictionary<string, ItemCollection>();

		private const string INDIE_DEMO_KEY = "Indie Demo";

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => _catInventory.IsInitialized);
			yield return new WaitUntil(() => _catInventory.Items.Count > 0);
			foreach (SteamItem item in _catInventory.Items)
			{
				if (item.IsConsumable)
				{
					AddToEventCollection(item, "consum");
					continue;
				}
				if (!string.IsNullOrEmpty(item.CollabTag))
				{
					AddToEventCollection(item, item.CollabTag);
					continue;
				}
				if (!string.IsNullOrEmpty(item.EventTag))
				{
					AddToEventCollection(item, item.EventTag);
					continue;
				}
				if (item.IsEmote)
				{
					AddToEventCollection(item, item.ItemSlot);
					continue;
				}
				if (!_baseItemsDict.ContainsKey(item.QualityCategory))
				{
					_baseItemsDict.Add(item.QualityCategory, new List<SteamItem>());
				}
				_baseItemsDict[item.QualityCategory].Add(item);
			}
			yield return new WaitUntil(() => _catInventory.WasLoadedFromSteam);
			List<KeyValuePair<string, List<SteamItem>>> list = (from pair in _eventItemsDict.ToList()
				orderby _collectionNames.GetIndex(pair.Key)
				select pair).ToList();
			for (int num = 4; num >= 0; num--)
			{
				ItemCollection itemCollection = global::UnityEngine.Object.Instantiate(_collectionPrefab, _uiRoot);
				CollectionNames collectionNames = _collectionNames;
				QualityCategory qualityCategory = (QualityCategory)num;
				string displayName = collectionNames.GetDisplayName(qualityCategory.ToString());
				itemCollection.CreateCollection(displayName, _baseItemsDict[(QualityCategory)num], isPartOfBongoDex: true);
				_collections.Add(displayName, itemCollection);
			}
			foreach (KeyValuePair<string, List<SteamItem>> item2 in list)
			{
				if (item2.Key == "Indie Demo")
				{
					CreateIndieDemoCollection();
					continue;
				}
				ItemCollection itemCollection2 = global::UnityEngine.Object.Instantiate(_collectionPrefab, _uiRoot);
				itemCollection2.CreateCollection(item2.Key, _eventItemsDict[item2.Key], isPartOfBongoDex: true, isEventCollection: true);
				_collections.Add(item2.Key, itemCollection2);
			}
		}

		private void AddToEventCollection(SteamItem item, string key)
		{
			string displayName = _collectionNames.GetDisplayName(key);
			if (!_eventItemsDict.ContainsKey(displayName))
			{
				_eventItemsDict.Add(displayName, new List<SteamItem>());
			}
			_eventItemsDict[displayName].Add(item);
		}

		private void CreateIndieDemoCollection()
		{
			_chestIcon.enabled = false;
			if (!_eventItemsDict.ContainsKey("Indie Demo"))
			{
				return;
			}
			ItemCollection itemCollection = global::UnityEngine.Object.Instantiate(_collectionPrefab, _uiRoot);
			itemCollection.CreateCollection("Indie Demo", _eventItemsDict["Indie Demo"], isPartOfBongoDex: true, isEventCollection: true);
			_collections.Add("Indie Demo", itemCollection);
			if (itemCollection.CollectionItems.Any((InventoryItem item) => item.SteamItem.ItemAmount == 0 && item.SteamItem.IsActivePromo))
			{
				_chestIcon.enabled = true;
			}
			foreach (InventoryItem collectionItem in itemCollection.CollectionItems)
			{
				SteamItem steamItem = collectionItem.SteamItem;
				steamItem.OnItemUpdated = (Action)Delegate.Combine(steamItem.OnItemUpdated, new Action(OnIndieDemoCollectionUpdated));
			}
		}

		private void OnIndieDemoCollectionUpdated()
		{
			if (_collections.TryGetValue("Indie Demo", out var value) && value.CollectionItems.All((InventoryItem item) => item.SteamItem.ItemAmount > 0))
			{
				_chestIcon.enabled = false;
			}
		}

		public void Toggle()
		{
			bool flag = !_bongodexContentUi.activeInHierarchy;
			_inventoryContentUi.SetActive(!flag);
			_bongodexContentUi.SetActive(flag);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BongoCat.Achievements;
using IroxGames.StoreFronts.Steam;
using Steam;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat
{
	public class CatInventory : MonoBehaviour
	{
		public static CatInventory Instance;

		[SerializeField]
		private GameObject _seperator;

		[SerializeField]
		private GameObject _presetsSeparator;

		[SerializeField]
		private GameObject _consumableSeparator;

		[SerializeField]
		private GameObject _emoteSeparator;

		[SerializeField]
		private List<Image> _duplicateExchangeBadges;

		[SerializeField]
		private QualityColors _qualityColors;

		[SerializeField]
		private EmoteDonut _emoteDonut;

		public bool IsInitialized;

		public bool WasLoadedFromSteam;

		private Callback<SteamInventoryResultReady_t> _t;

		private SteamInventoryResult_t _resultHandle;

		public List<SteamItem> Items = new List<SteamItem>();

		public List<ulong> _itemIdsOwned = new List<ulong>();

		private Coroutine _routine;

		public SteamItemDetails_t EmoteChestToken;

		public SteamItemDetails_t ChestToken;

		private SteamItemDetails_t _achievementToken;

		private SteamItemDetails_t _manualToken;

		private SteamItemDetails_t _godPackToken;

		private Dictionary<string, SteamItemDetails_t> _collabTokens = new Dictionary<string, SteamItemDetails_t>();

		private Dictionary<int, SteamItemDetails_t> _otherTokens = new Dictionary<int, SteamItemDetails_t>();

		[SerializeField]
		private ItemCollection _hatsCollection;

		[SerializeField]
		private ItemCollection _skinsCollection;

		[SerializeField]
		private ItemCollection _consumableCollection;

		[SerializeField]
		private ItemCollection _emoteCollection;

		private bool _useCollectionView;

		[SerializeField]
		private SteamItemIdReference _itemIdReference;

		public Action OnOtherTokenReceived;

		private bool _flaggedPremium;

		private const string CONSUMABLE_CATEGORY = "consumable";

		private const string HAT_CATEGORY = "hat";

		private const string SKIN_CATEGORY = "skin";

		private const string EMOTE_CATEGORY = "emote";

		public Dictionary<int, SteamItemDetails_t> OtherTokens => _otherTokens;

		public SteamItemDetails_t GodPackToken => _godPackToken;

		public SteamItemDetails_t AchievementToken => _achievementToken;

		public SteamItemDetails_t ManualToken => _manualToken;

		private void Awake()
		{
			Instance = this;
			SteamItemIdReference.Instance = _itemIdReference;
		}

		private IEnumerator Start()
		{
			Shop.OnceEquippedItems = new HashSet<int>();
			yield return new WaitUntil(() => SteamManager.Initialized);
			_t = Callback<SteamInventoryResultReady_t>.Create(InventoryResultReady);
			yield return InstantiateItemsRoutine();
			IsInitialized = true;
			Debug.Log("CatInventory | IsInitialized: true");
			SteamInventory.GetAllItems(out _resultHandle);
			_routine = StartCoroutine(FetchAllItems());
		}

		private IEnumerator InstantiateItemsRoutine()
		{
			int itemsInstantiated = 0;
			foreach (int id in _itemIdReference.Ids)
			{
				SteamItem steamItem = new SteamItem(new SteamItemDef_t(id));
				if (!steamItem.Hidden && !steamItem.GameOnly)
				{
					Items.Add(steamItem);
					itemsInstantiated++;
					if (itemsInstantiated % 20 == 0)
					{
						yield return null;
					}
				}
			}
			_hatsCollection.CreateCollection("", Items.Where((SteamItem item) => item.ItemSlot == "hat").ToList(), isPartOfBongoDex: false);
			_skinsCollection.CreateCollection("", Items.Where((SteamItem item) => item.ItemSlot == "skin").ToList(), isPartOfBongoDex: false);
			_consumableCollection.CreateCollection("", Items.Where((SteamItem item) => item.ItemSlot == "consumable").ToList(), isPartOfBongoDex: false);
			_emoteCollection.CreateCollection("", Items.Where((SteamItem item) => item.ItemSlot == "emote").ToList(), isPartOfBongoDex: false);
			foreach (SteamItem item in Items)
			{
				item.OnItemUpdated?.Invoke();
			}
		}

		private IEnumerator FetchAllItems()
		{
			while (!SteamManager.ShuttingDown)
			{
				yield return new WaitForSecondsRealtime(30f);
				CleanupHandle();
				SteamInventory.GetAllItems(out _resultHandle);
			}
		}

		public void UpdateItemsUI()
		{
			foreach (SteamItem item in Items)
			{
				item.OnItemUpdated?.Invoke();
			}
		}

		public void RestartRoutine()
		{
			if (_routine != null)
			{
				StopCoroutine(_routine);
				_routine = StartCoroutine(FetchAllItems());
			}
		}

		private void OnDisable()
		{
			if (SteamManager.s_EverInitialized)
			{
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

		private void InventoryResultReady(SteamInventoryResultReady_t result)
		{
			if (_resultHandle == SteamInventoryResult_t.Invalid)
			{
				return;
			}
			if (_resultHandle != result.m_handle)
			{
				Debug.Log($"Inventory: skipped result handle {result.m_handle.m_SteamInventoryResult} did not match my {_resultHandle.m_SteamInventoryResult}");
				return;
			}
			if (SteamInventory.GetResultStatus(result.m_handle) != EResult.k_EResultOK)
			{
				Debug.Log($"Inventory: Skipped result handle {result.m_handle.m_SteamInventoryResult}. Result status not OK");
			}
			uint punOutItemsArraySize = 0u;
			if (!SteamInventory.GetResultItems(result.m_handle, null, ref punOutItemsArraySize) || punOutItemsArraySize == 0)
			{
				Debug.Log("Inventory: Array size <= 0, skipped result");
				CleanupHandle();
				return;
			}
			bool flag = !WasLoadedFromSteam;
			if (!WasLoadedFromSteam)
			{
				WasLoadedFromSteam = true;
				Debug.Log("CatInventory | WasLoadedFromSteam: true");
			}
			SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
			SteamInventory.GetResultItems(result.m_handle, array, ref punOutItemsArraySize);
			if (punOutItemsArraySize != 0)
			{
				foreach (SteamItem item in Items)
				{
					item.InstanceIds.Clear();
				}
				_itemIdsOwned.Clear();
				_manualToken = default(SteamItemDetails_t);
				_achievementToken = default(SteamItemDetails_t);
				ChestToken = default(SteamItemDetails_t);
				EmoteChestToken = default(SteamItemDetails_t);
				_godPackToken = default(SteamItemDetails_t);
				_otherTokens.Clear();
				for (uint num = 0u; num < array.Length; num++)
				{
					HandleItem(array[num], GetLastModifiedDateFromItem(result.m_handle, num, array[num]), out var _);
				}
				AchievementStats.SetTotalItems(Items.Where((SteamItem item) => !item.Hidden).Sum((SteamItem item) => item.ItemAmount) + 1);
				if (flag)
				{
					foreach (SteamItem item2 in Items.Where((SteamItem steamItem) => steamItem.ItemAmount > 0).ToList())
					{
						Shop.OnceEquippedItems.Add(item2.SteamItemDefId);
						item2.OnItemUpdated?.Invoke();
					}
				}
				UpdateVisuals();
				if (HasPremiumItem())
				{
					if (!_flaggedPremium)
					{
						_flaggedPremium = true;
						SteamAnalytics.Instance.SetStatToOne(string.Format("{0}_{1}_{2}", "PREM", DateTime.UtcNow.Year, DateTime.UtcNow.Month));
					}
					SteamUserStats.SetStat(string.Format("{0}_{1}_{2}", "PREM_AMT", DateTime.UtcNow.Year, DateTime.UtcNow.Month), CountPremiumItems());
					SteamUserStats.StoreStats();
				}
			}
			CleanupHandle();
		}

		public static DateTime GetLastModifiedDateFromItem(SteamInventoryResult_t resultHandle, uint i, SteamItemDetails_t steamItemDetails)
		{
			if (SteamManager.ShuttingDown)
			{
				return DateTime.MinValue;
			}
			string pchValueBuffer = "";
			uint punValueBufferSizeOut = 0u;
			SteamInventory.GetResultItemProperty(resultHandle, i, null, out pchValueBuffer, ref punValueBufferSizeOut);
			SteamInventory.GetResultItemProperty(resultHandle, i, null, out pchValueBuffer, ref punValueBufferSizeOut);
			if (string.IsNullOrEmpty(pchValueBuffer))
			{
				Debug.LogWarning("ERROR: No timestamp found for item: " + steamItemDetails.m_iDefinition.m_SteamItemDef);
				return DateTime.UtcNow;
			}
			if (pchValueBuffer.Contains("error"))
			{
				SteamInventory.GetResultItemProperty(resultHandle, i, "error", out pchValueBuffer, ref punValueBufferSizeOut);
				SteamInventory.GetResultItemProperty(resultHandle, i, "error", out pchValueBuffer, ref punValueBufferSizeOut);
				Debug.Log("error: " + pchValueBuffer);
				SteamInventory.GetResultItemProperty(resultHandle, i, "item_json", out pchValueBuffer, ref punValueBufferSizeOut);
				SteamInventory.GetResultItemProperty(resultHandle, i, "item_json", out pchValueBuffer, ref punValueBufferSizeOut);
				Debug.Log("item_json: " + pchValueBuffer);
			}
			SteamInventory.GetResultItemProperty(resultHandle, i, "state_changed_timestamp", out pchValueBuffer, ref punValueBufferSizeOut);
			SteamInventory.GetResultItemProperty(resultHandle, i, "state_changed_timestamp", out pchValueBuffer, ref punValueBufferSizeOut);
			if (string.IsNullOrEmpty(pchValueBuffer))
			{
				Debug.LogWarning("ERROR: No timestamp found for item: " + steamItemDetails.m_iDefinition.m_SteamItemDef);
				return DateTime.UtcNow;
			}
			return DateTime.ParseExact(pchValueBuffer, "yyyyMMddTHHmmssZ", new CultureInfo("en-US"), DateTimeStyles.AssumeUniversal);
		}

		public void UpdateVisuals()
		{
			foreach (SteamItem item in Items.Where((SteamItem item) => item.ItemAmount == 0).ToList())
			{
				item.OnItemUpdated?.Invoke();
			}
			CatCosmetics.Instance.Validate();
			_emoteDonut.Validate();
			UpdateExchangeBadges();
			SortItems();
		}

		private void UpdateExchangeBadges()
		{
			if (ItemExchange.Instance.CanFillExchangeWithDuplicates(out var duplicateQuality))
			{
				Color color = _qualityColors.GetColor(duplicateQuality.ToQuality());
				{
					foreach (Image duplicateExchangeBadge in _duplicateExchangeBadges)
					{
						duplicateExchangeBadge.gameObject.SetActive(value: true);
						duplicateExchangeBadge.color = color;
					}
					return;
				}
			}
			foreach (Image duplicateExchangeBadge2 in _duplicateExchangeBadges)
			{
				duplicateExchangeBadge2.gameObject.SetActive(value: false);
			}
		}

		public void SortItems()
		{
			_hatsCollection.SortCollection();
			_skinsCollection.SortCollection();
			_emoteCollection.SortCollection();
			_consumableCollection.SortCollection();
			SetSeparatorVisibility();
			StartCoroutine(SortDelayed());
		}

		private IEnumerator SortDelayed()
		{
			yield return null;
			_consumableCollection.gameObject.transform.SetAsLastSibling();
			_consumableSeparator.gameObject.transform.SetAsLastSibling();
			_skinsCollection.gameObject.transform.SetAsLastSibling();
			_seperator.transform.SetAsLastSibling();
			_hatsCollection.gameObject.transform.SetAsLastSibling();
			_emoteSeparator.transform.SetAsLastSibling();
			_emoteCollection.gameObject.transform.SetAsLastSibling();
			_hatsCollection.SortCollection();
			_skinsCollection.SortCollection();
			_emoteCollection.SortCollection();
			_consumableCollection.SortCollection();
			SetSeparatorVisibility();
		}

		private void SetSeparatorVisibility()
		{
			_presetsSeparator.SetActive(_skinsCollection.InventoryDisplayCount > 0 || _hatsCollection.InventoryDisplayCount > 0);
			_consumableSeparator.SetActive((_skinsCollection.InventoryDisplayCount > 0 || _hatsCollection.InventoryDisplayCount > 0) && _consumableCollection.InventoryDisplayCount > 0);
			_emoteSeparator.SetActive((_skinsCollection.InventoryDisplayCount > 0 || _hatsCollection.InventoryDisplayCount > 0 || _consumableCollection.InventoryDisplayCount > 0) && _emoteCollection.InventoryDisplayCount > 0);
			_seperator.SetActive(_skinsCollection.InventoryDisplayCount > 0 && _hatsCollection.InventoryDisplayCount > 0);
		}

		public SteamItem HandleItem(SteamItemDetails_t itemDetails, DateTime timestamp, out bool isNew)
		{
			isNew = false;
			if (timestamp == default(DateTime))
			{
				timestamp = DateTime.MaxValue;
			}
			int steamItemDefId = itemDetails.m_iDefinition.m_SteamItemDef;
			if (_itemIdsOwned.Contains(itemDetails.m_itemId.m_SteamItemInstanceID) && Items.Any((SteamItem item) => item.SteamItemDefId == steamItemDefId))
			{
				SteamItem steamItem = Items.FirstOrDefault((SteamItem it) => it.SteamItemDefId == steamItemDefId);
				int num = steamItem.InstanceIds.FindIndex((SteamItemDetails_t i) => i.m_itemId.m_SteamItemInstanceID == itemDetails.m_itemId.m_SteamItemInstanceID);
				if (num != -1)
				{
					steamItem.InstanceIds[num] = itemDetails;
					steamItem.OnItemUpdated?.Invoke();
					if (steamItem.OldestItemTimestamp == default(DateTime) || steamItem.OldestItemTimestamp > timestamp)
					{
						steamItem.OldestItemTimestamp = timestamp;
					}
					isNew = false;
					return steamItem;
				}
			}
			switch (steamItemDefId)
			{
			case 97:
				_achievementToken = itemDetails;
				return null;
			case 96:
				_manualToken = itemDetails;
				return null;
			case 98:
				ChestToken = itemDetails;
				return null;
			case 90:
				EmoteChestToken = itemDetails;
				return null;
			default:
			{
				string collabTokenTagFromId = _itemIdReference.GetCollabTokenTagFromId(steamItemDefId);
				if (!string.IsNullOrEmpty(collabTokenTagFromId))
				{
					_collabTokens.TryAdd(collabTokenTagFromId, itemDetails);
					return null;
				}
				if (_itemIdReference.IsOtherToken(steamItemDefId))
				{
					if (_otherTokens.TryAdd(steamItemDefId, itemDetails))
					{
						OnOtherTokenReceived?.Invoke();
					}
					return null;
				}
				if (!_itemIdReference.Ids.Contains(steamItemDefId))
				{
					return null;
				}
				_itemIdsOwned.Add(itemDetails.m_itemId.m_SteamItemInstanceID);
				if (Items.Any((SteamItem item) => item.SteamItemDefId == steamItemDefId))
				{
					SteamItem steamItem2 = Items.FirstOrDefault((SteamItem it) => it.SteamItemDefId == steamItemDefId);
					steamItem2.InstanceIds.Add(itemDetails);
					steamItem2.OnItemUpdated?.Invoke();
					if (steamItem2.OldestItemTimestamp == default(DateTime) || steamItem2.OldestItemTimestamp > timestamp)
					{
						steamItem2.OldestItemTimestamp = timestamp;
					}
					isNew = steamItem2.ItemAmount == 1;
					return steamItem2;
				}
				SteamItem steamItem3 = new SteamItem(itemDetails.m_iDefinition);
				steamItem3.InstanceIds.Add(itemDetails);
				steamItem3.OldestItemTimestamp = timestamp;
				if (steamItem3.Hidden || steamItem3.GameOnly)
				{
					return null;
				}
				if (steamItem3.ItemSlot == "hat")
				{
					_hatsCollection.Add(steamItem3, isPartOfBongoDex: false);
				}
				if (steamItem3.ItemSlot == "skin")
				{
					_skinsCollection.Add(steamItem3, isPartOfBongoDex: false);
				}
				if (steamItem3.ItemSlot == "emote")
				{
					_emoteCollection.Add(steamItem3, isPartOfBongoDex: false);
				}
				if (steamItem3.ItemSlot == "consumable")
				{
					_consumableCollection.Add(steamItem3, isPartOfBongoDex: false);
				}
				Items.Add(steamItem3);
				isNew = true;
				return steamItem3;
			}
			}
		}

		public void ConsumeItem(SteamItemDetails_t itemDetails)
		{
			int steamItemDefId = itemDetails.m_iDefinition.m_SteamItemDef;
			SteamItem steamItem = Items.FirstOrDefault((SteamItem i) => i.SteamItemDefId == steamItemDefId);
			if (steamItem == null)
			{
				Debug.LogWarning("ERROR Item not found " + steamItemDefId);
				return;
			}
			steamItem.Consumed = Mathf.Max(0, steamItem.Consumed - 1);
			int num = -1;
			for (int num2 = 0; num2 < steamItem.InstanceIds.Count; num2++)
			{
				SteamItemDetails_t value = steamItem.InstanceIds[num2];
				if (value.m_itemId.m_SteamItemInstanceID == itemDetails.m_itemId.m_SteamItemInstanceID && value.m_unQuantity > 0)
				{
					value.m_unQuantity = itemDetails.m_unQuantity;
					steamItem.InstanceIds[num2] = value;
					if (value.m_unQuantity == 0)
					{
						num = num2;
					}
					break;
				}
			}
			if (num >= 0)
			{
				steamItem.InstanceIds.RemoveAt(num);
			}
			if (steamItem.ItemAmount == 0)
			{
				_itemIdsOwned.Remove(itemDetails.m_itemId.m_SteamItemInstanceID);
			}
			steamItem.OnItemUpdated?.Invoke();
		}

		public void RemoveItem(SteamItemDetails_t itemDetails)
		{
			int steamItemDefId = itemDetails.m_iDefinition.m_SteamItemDef;
			SteamItem steamItem = Items.FirstOrDefault((SteamItem i) => i.SteamItemDefId == steamItemDefId);
			if (steamItem == null)
			{
				Debug.LogWarning("ERROR Item not found " + steamItemDefId);
				return;
			}
			_itemIdsOwned.Remove(itemDetails.m_itemId.m_SteamItemInstanceID);
			int num = -1;
			for (int num2 = 0; num2 < steamItem.InstanceIds.Count; num2++)
			{
				if (steamItem.InstanceIds[num2].m_itemId.m_SteamItemInstanceID == itemDetails.m_itemId.m_SteamItemInstanceID && steamItem.InstanceIds[num2].m_unQuantity > 0)
				{
					SteamItemDetails_t value = steamItem.InstanceIds[num2];
					value.m_unQuantity = itemDetails.m_unQuantity;
					steamItem.InstanceIds[num2] = value;
					if (value.m_unQuantity == 0)
					{
						num = num2;
					}
					break;
				}
			}
			if (num >= 0)
			{
				steamItem.InstanceIds.RemoveAt(num);
			}
			steamItem.OnItemUpdated?.Invoke();
		}

		public bool HasItem(int itemId)
		{
			return Items.Any((SteamItem item) => item.SteamItemDefId == itemId && item.ItemAmount > 0);
		}

		public SteamItemDetails_t GetCollabToken(string key)
		{
			_collabTokens.TryGetValue(key, out var value);
			return value;
		}

		private bool HasPremiumItem()
		{
			return Items.Any((SteamItem item) => item.IsPremium && item.ItemAmount > 0);
		}

		private int CountPremiumItems()
		{
			return Items.Where((SteamItem item) => item.IsPremium).Sum((SteamItem item) => item.ItemAmount);
		}
	}
}

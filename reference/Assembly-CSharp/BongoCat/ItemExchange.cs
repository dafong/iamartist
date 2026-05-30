using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BongoCat.Achievements;
using BongoCat.Localizer;
using BongoCat.Multiplayer;
using BongoCat.SteamJsonParser;
using IroxGames.StoreFronts.Steam;
using Steam.Exchanges;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat
{
	public class ItemExchange : MonoBehaviour
	{
		private Callback<SteamInventoryResultReady_t> _t;

		private SteamInventoryResult_t _resultHandle;

		[SerializeField]
		private List<ExchangeSlot> _exchangeSlots;

		[SerializeField]
		private Button _exchangeButton;

		[SerializeField]
		private Image _fillDuplicatesFill;

		[SerializeField]
		private QualityColors _qualityColors;

		[SerializeField]
		private NewItemPopup _newItemPopup;

		[SerializeField]
		private PlayerPrefsToggle _allowAutoSlotFavorites;

		[SerializeField]
		private TMP_Text _exchangeButtonText;

		public static ItemExchange Instance;

		private bool _quitting;

		private int _possibleExchanges;

		private bool _exchangeCompleted;

		public bool IsVisible { get; private set; }

		private void Awake()
		{
			Instance = this;
		}

		public void OnOpenExchange()
		{
			if (SteamManager.s_EverInitialized)
			{
				IsVisible = true;
				List<SteamItem> duplicates = GetDuplicates();
				QualityCategoryWithInfo slotDuplicateQuality = GetSlotDuplicateQuality(duplicates);
				_fillDuplicatesFill.color = _qualityColors.GetColor(slotDuplicateQuality.ToQuality());
				CatInventory.Instance.UpdateItemsUI();
			}
		}

		private void OnCloseExchange()
		{
			IsVisible = false;
		}

		private IEnumerator Start()
		{
			MenuTabs.OnCloseExchange = (Action)Delegate.Combine(MenuTabs.OnCloseExchange, new Action(OnCloseExchange));
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			_resultHandle = SteamInventoryResult_t.Invalid;
			_t = Callback<SteamInventoryResultReady_t>.Create(InventoryResultReady);
		}

		private void OnEnable()
		{
			Loca instance = Loca.Instance;
			instance.OnLanguageChanged = (Action)Delegate.Combine(instance.OnLanguageChanged, new Action(OnLanguageChanged));
			OnLanguageChanged();
		}

		private void OnDisable()
		{
			CleanupHandle();
			if ((bool)Loca.Instance)
			{
				_exchangeButtonText.text = Loca.Instance.Get("Exchange");
				Loca instance = Loca.Instance;
				instance.OnLanguageChanged = (Action)Delegate.Remove(instance.OnLanguageChanged, new Action(OnLanguageChanged));
			}
		}

		private void OnLanguageChanged()
		{
			_exchangeButtonText.text = Loca.Instance.Get("Exchange");
			if (_possibleExchanges > 1)
			{
				_exchangeButtonText.text += $" ({_possibleExchanges}x)";
			}
		}

		private void InventoryResultReady(SteamInventoryResultReady_t result)
		{
			if (_resultHandle == SteamInventoryResult_t.Invalid || _resultHandle != result.m_handle)
			{
				return;
			}
			foreach (ExchangeSlot exchangeSlot in _exchangeSlots)
			{
				exchangeSlot.SetItem(null);
			}
			Debug.Log("Exchange " + _resultHandle.m_SteamInventoryResult);
			uint punOutItemsArraySize = 0u;
			bool resultItems = SteamInventory.GetResultItems(_resultHandle, null, ref punOutItemsArraySize);
			if (SteamInventory.GetResultStatus(result.m_handle) != EResult.k_EResultOK)
			{
				Debug.Log($"[Item Exchange]: {result.m_handle.m_SteamInventoryResult} Result status not OK");
			}
			if (!resultItems || punOutItemsArraySize == 0)
			{
				CleanupHandle();
				return;
			}
			SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
			SteamInventory.GetResultItems(_resultHandle, array, ref punOutItemsArraySize);
			Debug.Log("Exchange amt " + array.Length);
			for (uint num = 0u; num < array.Length; num++)
			{
				if (num == array.Length - 1)
				{
					bool isNew;
					SteamItem steamItem = CatInventory.Instance.HandleItem(array[num], CatInventory.GetLastModifiedDateFromItem(_resultHandle, num, array[num]), out isNew);
					_newItemPopup.ShowPopup(steamItem, !isNew);
					SteamMultiplayer.Instance.SendReceivedItem(steamItem.SteamItemDefId);
					if (!steamItem.IsEmote && SettingsManager.Instance.AutoEquipDrops)
					{
						CatCosmetics.Instance.AutoEquipDrop(steamItem);
					}
					Debug.Log($"[Item Exchange] Received new item {steamItem.SteamItemDefId} - {steamItem.ItemName}");
				}
				else
				{
					CatInventory.Instance.RemoveItem(array[num]);
				}
			}
			CatInventory.Instance.UpdateVisuals();
			CleanupHandle();
			UpdateInteractable();
			CatInventory.Instance.RestartRoutine();
			_exchangeCompleted = true;
			AchievementStats.IncrementExchanges();
		}

		private void CleanupHandle()
		{
			if (SteamManager.s_EverInitialized && _resultHandle != SteamInventoryResult_t.Invalid)
			{
				SteamInventory.DestroyResult(_resultHandle);
				_resultHandle = SteamInventoryResult_t.Invalid;
			}
		}

		public void TryExchange()
		{
			Debug.Log("[Item Exchange] Trying item exchange.");
			List<SteamItem> list = (from slot in _exchangeSlots
				where slot.ItemSlotted != null
				select slot.ItemSlotted).ToList();
			if (_possibleExchanges > 1)
			{
				StartCoroutine(ExchangeRoutine(list, _possibleExchanges));
			}
			else if (_possibleExchanges == 0)
			{
				Debug.Log("[Item Exchange] No exchanges possible. Aborting item exchange.");
			}
			else if (list.Select((SteamItem item) => item.Quality).Distinct().Count() != 1)
			{
				Debug.Log("[Item Exchange] Not all items had same quality. Aborting item exchange.");
			}
			else
			{
				StartCoroutine(ExchangeRoutine(list, 1));
			}
		}

		private IEnumerator ExchangeRoutine(List<SteamItem> itemsSlotted, int iterations)
		{
			SteamItem[] items = itemsSlotted.ToArray();
			Dictionary<SteamItem, uint> steamItemAmount = new Dictionary<SteamItem, uint>();
			SteamItem[] array = items;
			foreach (SteamItem steamItem in array)
			{
				if (!steamItemAmount.ContainsKey(steamItem))
				{
					steamItemAmount[steamItem] = (uint)steamItem.CurrentlyInExchangeSlot;
				}
			}
			for (int iteration = 0; iteration < iterations; iteration++)
			{
				_exchangeCompleted = false;
				List<SteamItemDetails_t> list = new List<SteamItemDetails_t>();
				List<uint> list2 = new List<uint>();
				List<SteamItem> list3 = new List<SteamItem>();
				long num = 0L;
				foreach (KeyValuePair<SteamItem, uint> item2 in new Dictionary<SteamItem, uint>(steamItemAmount))
				{
					SteamItem key = item2.Key;
					uint num2 = item2.Value;
					int num3 = 0;
					while (num2 != 0 && num < 10)
					{
						SteamItemDetails_t item = key.InstanceIds[num3++];
						list.Add(item);
						list3.Add(key);
						uint num4 = (uint)Mathf.Min(num2, (int)item.m_unQuantity);
						if (num + num4 > 10)
						{
							num4 = (uint)(10 - num);
						}
						num += num4;
						list2.Add(num4);
						num2 -= num4;
						steamItemAmount[key] -= num4;
						if (steamItemAmount[key] == 0)
						{
							steamItemAmount.Remove(key);
						}
					}
					if (num == 10)
					{
						break;
					}
				}
				int itemExchangeId = GetItemExchangeId(items);
				if (itemExchangeId == -1)
				{
					Debug.Log("[Item Exchange] Could not find exchange id for item exchange. Aborting item exchange.");
					yield break;
				}
				_exchangeButton.interactable = false;
				_possibleExchanges = 0;
				OnLanguageChanged();
				string text = "";
				foreach (SteamItem item3 in list3)
				{
					text += $"{item3.SteamItemDefId} - {item3.ItemName}, ";
				}
				if (text.Length > 2)
				{
					text = text.Remove(text.Length - 2, 2);
				}
				Debug.Log("[Item Exchange] Exchanging " + text);
				SteamInventory.ExchangeItems(out _resultHandle, new SteamItemDef_t[1]
				{
					new SteamItemDef_t(itemExchangeId)
				}, new uint[1] { 1u }, 1u, list.Select((SteamItemDetails_t steamItemDetails_t) => steamItemDetails_t.m_itemId).ToArray(), list2.ToArray(), (uint)list2.Count);
				yield return new WaitUntil(() => _exchangeCompleted);
			}
			CatInventory.Instance.UpdateItemsUI();
		}

		private int GetItemExchangeId(SteamItem[] items)
		{
			if (items.Length == 0)
			{
				return -1;
			}
			return items.First().QualityCategoryWithInfo switch
			{
				QualityCategoryWithInfo.CommonEmote => 30, 
				QualityCategoryWithInfo.UncommonEmote => 31, 
				QualityCategoryWithInfo.RareEmote => 32, 
				QualityCategoryWithInfo.EpicEmote => 33, 
				QualityCategoryWithInfo.CommonCosmetic => 16, 
				QualityCategoryWithInfo.UncommonCosmetic => 17, 
				QualityCategoryWithInfo.RareCosmetic => 18, 
				QualityCategoryWithInfo.EpicCosmetic => 19, 
				_ => -1, 
			};
		}

		public void UpdateInteractable()
		{
			int num = (from slot in _exchangeSlots
				where slot.ItemSlotted != null
				select slot.SlottedAmount).Sum();
			_possibleExchanges = Mathf.FloorToInt((float)num / 10f);
			_exchangeButton.interactable = _possibleExchanges > 0;
			OnLanguageChanged();
			ExchangeSlot exchangeSlot = _exchangeSlots.FirstOrDefault((ExchangeSlot slot) => slot.ItemSlotted != null);
			if ((bool)exchangeSlot)
			{
				_fillDuplicatesFill.color = _qualityColors.GetColor(exchangeSlot.ItemSlotted.QualityCategory);
				return;
			}
			List<SteamItem> duplicates = GetDuplicates();
			QualityCategoryWithInfo slotDuplicateQuality = GetSlotDuplicateQuality(duplicates);
			_fillDuplicatesFill.color = _qualityColors.GetColor(slotDuplicateQuality.ToQuality());
		}

		public void SlotItem(SteamItem steamItem, int amount = 1)
		{
			if (steamItem.QualityCategory == QualityCategory.Legendary)
			{
				return;
			}
			int currentlyInExchangeSlot = steamItem.CurrentlyInExchangeSlot;
			if ((currentlyInExchangeSlot > 0 && currentlyInExchangeSlot >= steamItem.ItemAmount) || _exchangeSlots.Any((ExchangeSlot slot) => slot.ItemSlotted != null && slot.ItemSlotted.QualityCategoryWithInfo != steamItem.QualityCategoryWithInfo))
			{
				return;
			}
			_exchangeButton.interactable = false;
			foreach (ExchangeSlot exchangeSlot in _exchangeSlots)
			{
				if (exchangeSlot.ItemSlotted == null)
				{
					exchangeSlot.SetItem(steamItem, amount);
					break;
				}
			}
			int num = (from slot in _exchangeSlots
				where slot.ItemSlotted != null
				select slot.SlottedAmount).Sum();
			Debug.Log($"Total slotted: {num}");
			_possibleExchanges = Mathf.FloorToInt((float)num / 10f);
			if (num >= 10)
			{
				_exchangeButton.interactable = true;
			}
			_fillDuplicatesFill.color = _qualityColors.GetColor(steamItem.QualityCategory);
			OnLanguageChanged();
			steamItem.OnItemUpdated?.Invoke();
			CatInventory.Instance.UpdateItemsUI();
		}

		private bool CanSlotDuplicate(SteamItem item)
		{
			if ((item.IsFavorite && !_allowAutoSlotFavorites.Value) || item.ItemAmount <= 1)
			{
				return false;
			}
			if (_exchangeSlots.Any((ExchangeSlot slot) => slot.ItemSlotted != null && slot.ItemSlotted.QualityCategoryWithInfo != item.QualityCategoryWithInfo))
			{
				return false;
			}
			return true;
		}

		private bool HasDuplicates()
		{
			return CatInventory.Instance.Items.Any((SteamItem item) => item.ItemAmount > 1 && CanSlotDuplicate(item));
		}

		private List<SteamItem> GetDuplicates()
		{
			return CatInventory.Instance.Items.Where((SteamItem item) => item.ItemAmount > 1 && CanSlotDuplicate(item)).ToList();
		}

		public bool CanFillExchangeWithDuplicates(out QualityCategoryWithInfo duplicateQuality)
		{
			List<SteamItem> duplicates = GetDuplicates();
			foreach (QualityCategoryWithInfo quality in Enum.GetValues(typeof(QualityCategoryWithInfo)))
			{
				if (quality != QualityCategoryWithInfo.LegendaryCosmetic && quality != QualityCategoryWithInfo.LegendaryEmote && duplicates.Where((SteamItem item) => item.QualityCategoryWithInfo == quality).Sum((SteamItem item) => item.ItemAmount - 1) >= _exchangeSlots.Count)
				{
					duplicateQuality = quality;
					return true;
				}
			}
			duplicateQuality = QualityCategoryWithInfo.CommonCosmetic;
			return false;
		}

		public void SlotDuplicates()
		{
			if (_exchangeSlots.All((ExchangeSlot slot) => slot.ItemSlotted != null) || !HasDuplicates())
			{
				return;
			}
			List<SteamItem> duplicates = GetDuplicates();
			QualityCategoryWithInfo targetQuality = GetSlotDuplicateQuality(duplicates);
			ExchangeSlot exchangeSlot = _exchangeSlots.FirstOrDefault((ExchangeSlot slot) => slot.ItemSlotted != null);
			if ((bool)exchangeSlot)
			{
				targetQuality = exchangeSlot.ItemSlotted.QualityCategoryWithInfo;
			}
			else
			{
				int num = 0;
				foreach (QualityCategoryWithInfo quality in Enum.GetValues(typeof(QualityCategoryWithInfo)))
				{
					if (quality != QualityCategoryWithInfo.LegendaryEmote && quality != QualityCategoryWithInfo.LegendaryCosmetic)
					{
						int num2 = duplicates.Where((SteamItem item) => item.QualityCategoryWithInfo == quality && CanSlotDuplicate(item)).Sum((SteamItem item) => item.ItemAmount - 1);
						if (num2 > num)
						{
							num = num2;
							targetQuality = quality;
						}
					}
				}
			}
			duplicates = CatInventory.Instance.Items.Where((SteamItem item) => item.QualityCategoryWithInfo == targetQuality && CanSlotDuplicate(item) && item.ItemAmount > 1).ToList();
			foreach (SteamItem item in duplicates)
			{
				for (int num3 = 0; num3 < item.ItemAmount - 1; num3++)
				{
					SlotDuplicate(item);
					if (_exchangeSlots.All((ExchangeSlot slot) => slot.ItemSlotted != null))
					{
						return;
					}
				}
			}
		}

		private QualityCategoryWithInfo GetSlotDuplicateQuality(List<SteamItem> duplicateItems)
		{
			QualityCategoryWithInfo result = QualityCategoryWithInfo.CommonCosmetic;
			ExchangeSlot exchangeSlot = _exchangeSlots.FirstOrDefault((ExchangeSlot slot) => slot.ItemSlotted != null);
			if ((bool)exchangeSlot)
			{
				result = exchangeSlot.ItemSlotted.QualityCategoryWithInfo;
			}
			else
			{
				int num = 0;
				foreach (QualityCategoryWithInfo quality in Enum.GetValues(typeof(QualityCategoryWithInfo)))
				{
					if (quality != QualityCategoryWithInfo.LegendaryCosmetic && quality != QualityCategoryWithInfo.LegendaryEmote)
					{
						int num2 = duplicateItems.Where((SteamItem item) => item.QualityCategoryWithInfo == quality && CanSlotDuplicate(item)).Sum((SteamItem item) => item.ItemAmount - 1);
						if (num2 > num)
						{
							num = num2;
							result = quality;
						}
					}
				}
			}
			return result;
		}

		private void SlotDuplicate(SteamItem duplicate)
		{
			int num = _exchangeSlots.Where((ExchangeSlot slot) => slot.ItemSlotted != null && slot.ItemSlotted.SteamItemDefId == duplicate.SteamItemDefId).Sum((ExchangeSlot slot) => slot.SlottedAmount);
			if (num <= 0 || num < duplicate.ItemAmount - 1)
			{
				SlotItem(duplicate);
			}
		}

		public bool IsItemSlottable(SteamItem steamItem)
		{
			if (!IsVisible)
			{
				return true;
			}
			if (steamItem.IsConsumable)
			{
				return false;
			}
			if (steamItem.QualityCategory == QualityCategory.Legendary)
			{
				return false;
			}
			if (_exchangeSlots.Any((ExchangeSlot slot) => slot.ItemSlotted != null && slot.ItemSlotted.QualityCategoryWithInfo != steamItem.QualityCategoryWithInfo))
			{
				return false;
			}
			int num = _exchangeSlots.Where((ExchangeSlot slot) => slot.ItemSlotted != null && slot.ItemSlotted.SteamItemDefId == steamItem.SteamItemDefId).Sum((ExchangeSlot slot) => slot.SlottedAmount);
			if (num > 0 && num >= steamItem.ItemAmount)
			{
				return false;
			}
			return true;
		}
	}
}

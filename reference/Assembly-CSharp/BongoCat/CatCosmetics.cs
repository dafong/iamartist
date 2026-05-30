using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BongoCat.Multiplayer;
using BongoCat.TapTapLootIntegration;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat
{
	public class CatCosmetics : MonoBehaviour
	{
		[SerializeField]
		private Image _hatImage;

		[SerializeField]
		private FlipVisuals _flipVisuals;

		[NonSerialized]
		public List<SteamItem> _equippedItems;

		private List<SteamItem> _locallyEquippedItems;

		private Cat _cat;

		public static CatCosmetics Instance;

		public bool initialized;

		public bool validated;

		private const string EQUIPPED_ITEMS_KEY = "EQUIPPED_ITEMS_2";

		[SerializeField]
		private MenuTabs _menu;

		public List<SteamItem> EquippedItems => _equippedItems;

		private void Awake()
		{
			Instance = this;
			_cat = global::UnityEngine.Object.FindAnyObjectByType<Cat>();
			_equippedItems = new List<SteamItem>();
			_locallyEquippedItems = new List<SteamItem>();
		}

		private void OnEnable()
		{
			MenuTabs menu = _menu;
			menu.OnCloseBongoDecks = (Action)Delegate.Combine(menu.OnCloseBongoDecks, new Action(RestoreActuallyEquippedItems));
		}

		private void OnDisable()
		{
			MenuTabs menu = _menu;
			menu.OnCloseBongoDecks = (Action)Delegate.Remove(menu.OnCloseBongoDecks, new Action(RestoreActuallyEquippedItems));
		}

		public Sprite GetHatSprite()
		{
			return _hatImage.sprite;
		}

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => CatInventory.Instance.IsInitialized);
			if (PlayerPrefs.HasKey("EQUIPPED_ITEMS_2") && !string.IsNullOrEmpty(PlayerPrefs.GetString("EQUIPPED_ITEMS_2")))
			{
				List<int> ids = PlayerPrefs.GetString("EQUIPPED_ITEMS_2").Split(',').Select(int.Parse)
					.ToList();
				List<SteamItem> itemsToEquip = CatInventory.Instance.Items.Where((SteamItem i) => ids.Contains(i.SteamItemDefId)).ToList();
				yield return new WaitUntil(() => itemsToEquip.All((SteamItem i) => i.IsReady));
				foreach (SteamItem item in itemsToEquip)
				{
					Equip(item, playAnimation: true, isLocal: false);
				}
			}
			initialized = true;
			Debug.Log("CatCosmetics initialized.");
		}

		public void Validate()
		{
			if (_locallyEquippedItems.Count > 0 || !PlayerPrefs.HasKey("EQUIPPED_ITEMS_2"))
			{
				return;
			}
			string text = PlayerPrefs.GetString("EQUIPPED_ITEMS_2");
			if (string.IsNullOrEmpty(text))
			{
				return;
			}
			List<int> ids = text.Split(',').Select(int.Parse).ToList();
			List<SteamItem> list = CatInventory.Instance.Items.Where((SteamItem i) => ids.Contains(i.SteamItemDefId) && i.ItemAmount > 0).ToList();
			if (list.Count((SteamItem i) => i.ItemSlot == "hat") == 0)
			{
				_hatImage.sprite = null;
				_hatImage.enabled = false;
			}
			if (list.Count((SteamItem i) => i.ItemSlot == "skin") == 0)
			{
				_cat.SetSkin(null);
			}
			foreach (SteamItem item in list)
			{
				EquipItem(item, playAnimation: false);
			}
			validated = true;
		}

		public void UpdateFlip()
		{
			bool flag = _flipVisuals.IsFlipped && MemoryImageCache.Instance.LetterSprites.Contains(_hatImage.sprite);
			_hatImage.transform.localScale = (flag ? new Vector3(-1f, 1f, 1f) : Vector3.one);
		}

		public void EquipItem(SteamItem steamItem, bool playAnimation = true, bool unequipIfSameItemIsEquipped = false)
		{
			bool flag = steamItem.ItemAmount == 0;
			if (unequipIfSameItemIsEquipped && ((!flag && _equippedItems.Any((SteamItem i) => i.SteamItemDefId == steamItem.SteamItemDefId)) || (flag && _locallyEquippedItems.Any((SteamItem i) => i.SteamItemDefId == steamItem.SteamItemDefId))))
			{
				Unequip(steamItem, flag);
			}
			else
			{
				Equip(steamItem, playAnimation, flag);
			}
		}

		private void Unequip(SteamItem steamItem, bool isLocal)
		{
			if (isLocal)
			{
				_locallyEquippedItems.RemoveAll((SteamItem i) => i.SteamItemDefId == steamItem.SteamItemDefId);
			}
			else
			{
				_equippedItems.RemoveAll((SteamItem i) => i.SteamItemDefId == steamItem.SteamItemDefId);
				PlayerPrefs.SetString("EQUIPPED_ITEMS_2", string.Join(",", _equippedItems.Select((SteamItem i) => i.SteamItemDefId)));
				PlayerPrefs.Save();
			}
			string itemSlot = steamItem.ItemSlot;
			if (!(itemSlot == "hat"))
			{
				if (itemSlot == "skin")
				{
					_cat.SetSkin(null);
					if (isLocal)
					{
						foreach (SteamItem equippedItem in _equippedItems)
						{
							if (equippedItem.ItemSlot == "skin")
							{
								Equip(equippedItem, playAnimation: true, isLocal: false);
								break;
							}
						}
					}
					else
					{
						steamItem.IsEquipped = false;
						steamItem.OnItemUpdated?.Invoke();
						SteamMultiplayer.Instance.ClientEquippedCosmetic("skin", -1);
					}
				}
			}
			else
			{
				_hatImage.sprite = null;
				_hatImage.enabled = _hatImage.sprite;
				if (isLocal)
				{
					foreach (SteamItem equippedItem2 in _equippedItems)
					{
						if (equippedItem2.ItemSlot == "hat")
						{
							Equip(equippedItem2, playAnimation: true, isLocal: false);
							break;
						}
					}
				}
				else
				{
					steamItem.IsEquipped = false;
					steamItem.OnItemUpdated?.Invoke();
					SteamMultiplayer.Instance.ClientEquippedCosmetic("hat", -1);
				}
			}
			Ipc.Instance.UpdateBuffs();
		}

		private void Equip(SteamItem steamItem, bool playAnimation, bool isLocal)
		{
			string itemSlot = steamItem.ItemSlot;
			if (!(itemSlot == "hat"))
			{
				if (itemSlot == "skin")
				{
					_cat.SetSkin(steamItem.ItemName);
					if (!isLocal)
					{
						SteamMultiplayer.Instance.ClientEquippedCosmetic("skin", steamItem.SteamItemDefId);
						steamItem.IsEquipped = true;
						steamItem.OnItemUpdated?.Invoke();
					}
				}
			}
			else
			{
				_hatImage.sprite = steamItem.FullImage;
				_hatImage.enabled = _hatImage.sprite;
				bool flag = _flipVisuals.IsFlipped && MemoryImageCache.Instance.LetterSprites.Contains(_hatImage.sprite);
				if (playAnimation)
				{
					_hatImage.GetComponent<OpenScaleAnimation>()?.PlayAnimation(flag ? new Vector3(-1f, 1f, 1f) : Vector3.one);
				}
				else
				{
					_hatImage.transform.localScale = (flag ? new Vector3(-1f, 1f, 1f) : Vector3.one);
				}
				if (!isLocal)
				{
					steamItem.IsEquipped = true;
					steamItem.OnItemUpdated?.Invoke();
					SteamMultiplayer.Instance.ClientEquippedCosmetic("hat", steamItem.SteamItemDefId);
				}
			}
			if (isLocal)
			{
				_locallyEquippedItems = RemoveItemsWithSameCategory(_locallyEquippedItems, steamItem);
				return;
			}
			_equippedItems = RemoveItemsWithSameCategory(_equippedItems, steamItem);
			PlayerPrefs.SetString("EQUIPPED_ITEMS_2", string.Join(",", _equippedItems.Select((SteamItem i) => i.SteamItemDefId)));
			PlayerPrefs.Save();
			Ipc.Instance.UpdateBuffs();
		}

		public void AutoEquipDrop(SteamItem item)
		{
			StartCoroutine(DoAutoEquip(item));
		}

		private IEnumerator DoAutoEquip(SteamItem item)
		{
			yield return new WaitUntil(() => validated);
			Shop.OnceEquippedItems.Add(item.SteamItemDefId);
			item.OnItemUpdated?.Invoke();
			Equip(item, playAnimation: true, isLocal: false);
			validated = false;
		}

		private List<SteamItem> RemoveItemsWithSameCategory(List<SteamItem> items, SteamItem steamItem)
		{
			items.Add(steamItem);
			foreach (SteamItem item in items)
			{
				if (item.ItemSlot == steamItem.ItemSlot && item != steamItem)
				{
					item.IsEquipped = false;
					item.OnItemUpdated?.Invoke();
				}
			}
			items.RemoveAll((SteamItem i) => i.ItemSlot == steamItem.ItemSlot && i != steamItem);
			return items;
		}

		private void RestoreActuallyEquippedItems()
		{
			if (_locallyEquippedItems.Count == 0)
			{
				return;
			}
			while (_locallyEquippedItems.Count > 0)
			{
				Unequip(_locallyEquippedItems[0], isLocal: true);
			}
			foreach (SteamItem equippedItem in _equippedItems)
			{
				Equip(equippedItem, playAnimation: true, isLocal: true);
			}
			_locallyEquippedItems.Clear();
		}
	}
}

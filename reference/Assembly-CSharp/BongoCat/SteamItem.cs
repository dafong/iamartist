using System;
using System.Collections.Generic;
using System.Linq;
using BongoCat.IPC;
using BongoCat.SteamJsonParser;
using Steamworks;
using UnityEngine;

namespace BongoCat
{
	public class SteamItem
	{
		public int SteamItemDefId;

		public List<SteamItemDetails_t> InstanceIds = new List<SteamItemDetails_t>();

		public Sprite Icon;

		public Sprite FullImage;

		public string ItemName;

		public string ItemSlot;

		private string Tags;

		public string EventTag;

		public string CollabTag;

		public string Quality;

		public bool IsActivePromo;

		public QualityCategory QualityCategory;

		public QualityCategoryWithInfo QualityCategoryWithInfo;

		public List<TapTapLootBuff> TapTapLootBuffs;

		public int CurrentlyInExchangeSlot;

		public int Consumed;

		public bool IsEquipped;

		private string _iconUrl;

		private string _fullImageUrl;

		public bool Hidden;

		public bool HideUntilReceived;

		public bool GameOnly;

		public bool Marketable;

		public string Promo;

		public bool IsPremium;

		public DateTime OldestItemTimestamp = DateTime.UtcNow;

		public Action OnItemUpdated;

		private SteamItemDef_t _steamItemDef;

		public int ItemAmount => InstanceIds.Sum((SteamItemDetails_t i) => i.m_unQuantity);

		public int DisplayedItemAmount
		{
			get
			{
				int itemAmount = ItemAmount;
				if (IsConsumable)
				{
					return itemAmount - Consumed;
				}
				if (CurrentlyInExchangeSlot == 0 || itemAmount == 0)
				{
					return itemAmount;
				}
				if (itemAmount - CurrentlyInExchangeSlot <= 1)
				{
					return 1;
				}
				return itemAmount - CurrentlyInExchangeSlot;
			}
		}

		public bool IsReady => Icon != null;

		public bool IsEmote => ItemSlot == "emote";

		public bool IsConsumable => ItemSlot == "consumable";

		private string Key => "Favorite_" + SteamItemDefId;

		public bool IsFavorite => PlayerPrefs.HasKey(Key);

		public void SetFavorite(bool value)
		{
			if (value)
			{
				PlayerPrefs.SetInt(Key, 0);
			}
			else
			{
				PlayerPrefs.DeleteKey(Key);
			}
			PlayerPrefs.Save();
		}

		public SteamItem(SteamItemDef_t steamItemDef)
		{
			_steamItemDef = steamItemDef;
			SteamItemDefId = steamItemDef.m_SteamItemDef;
			InitWithCache();
		}

		private void InitWithCache()
		{
			SteamItemUnity steamItemUnity = SteamItemIdReference.Instance.AllItems.FirstOrDefault((SteamItemUnity x) => x.Id == SteamItemDefId);
			if (!steamItemUnity)
			{
				Hidden = true;
				return;
			}
			ItemName = steamItemUnity.Name;
			_iconUrl = "https://pub-d6170922fa31462fa15463d240b18424.r2.dev/" + steamItemUnity.Name.Replace(" ", "") + "Icon.png";
			if (!string.IsNullOrEmpty(_iconUrl))
			{
				_fullImageUrl = _iconUrl.Replace("Icon", "");
			}
			ItemSlot = steamItemUnity.ItemSlot;
			Marketable = steamItemUnity.Marketable;
			GameOnly = steamItemUnity.GameOnly;
			Promo = steamItemUnity.Promo;
			HideUntilReceived = steamItemUnity.HideUntilReceived;
			QualityCategory = steamItemUnity.QualityCategory;
			EventTag = steamItemUnity.EventTag;
			CollabTag = steamItemUnity.CollabTag;
			Quality = steamItemUnity.QualityCategory.ToString("G").ToLower();
			Hidden = steamItemUnity.Hidden;
			IsPremium = !string.IsNullOrEmpty(steamItemUnity.PriceCategory);
			QualityCategoryWithInfo = QualityCategory.ToQualityWithInfo(ItemSlot == "emote");
			IsActivePromo = steamItemUnity.IsActivePromo;
			TapTapLootBuffs = steamItemUnity.TapTapLootBuffs;
			if (string.IsNullOrEmpty(_iconUrl))
			{
				Debug.Log("Icon url is empty for item: " + ItemName + " " + SteamItemDefId);
				Hidden = true;
				return;
			}
			string text = _iconUrl.Substring(_iconUrl.LastIndexOf('/') + 1).Replace(".png", "");
			string spriteName = _fullImageUrl.Substring(_fullImageUrl.LastIndexOf('/') + 1).Replace(".png", "");
			Icon = MemoryImageCache.Instance.GetSprite(text);
			FullImage = MemoryImageCache.Instance.GetSprite(spriteName);
			if (Icon == null && QualityCategory != QualityCategory.Special)
			{
				Debug.Log("Icon not found in cache: " + text);
				UpdateHelper.Instance.TryUpdate();
			}
		}
	}
}

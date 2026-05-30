using System.Collections.Generic;
using BongoCat.IPC;
using UnityEngine;

namespace BongoCat.SteamJsonParser
{
	public class SteamItemUnity : ScriptableObject
	{
		public int Id;

		public QualityCategory QualityCategory;

		public string Name;

		public int BundleFrequency = 1;

		public string Description;

		public string ItemSlot;

		public bool Tradable = true;

		public bool Marketable = true;

		public bool Hidden;

		public bool Dropable = true;

		public bool GameOnly;

		public string Promo;

		public string EventTag;

		public string CollabTag;

		public string DropStartTime;

		public string PriceCategory;

		public bool IsOtherToken;

		public bool FeatureInItemStore;

		public int PurchaseLimit;

		public bool HideUntilReceived;

		public bool IsActivePromo;

		public string StoreImages;

		public List<TapTapLootBuff> TapTapLootBuffs;

		public static Color GetColorForQuality(QualityCategory quality)
		{
			Color color = Color.white;
			switch (quality)
			{
			case QualityCategory.Common:
				ColorUtility.TryParseHtmlString("#B0B0B0", out color);
				return color;
			case QualityCategory.Uncommon:
				ColorUtility.TryParseHtmlString("#5ACC3D", out color);
				return color;
			case QualityCategory.Rare:
				ColorUtility.TryParseHtmlString("#258ED4", out color);
				return color;
			case QualityCategory.Epic:
				ColorUtility.TryParseHtmlString("#B939DB", out color);
				return color;
			case QualityCategory.Legendary:
				ColorUtility.TryParseHtmlString("#FFB000", out color);
				return color;
			default:
				return Color.white;
			}
		}

		public SteamItemBackend ToSteamItemBackend()
		{
			string text = "quality:" + QualityCategory.ToString("G").ToLower();
			if (QualityCategory != QualityCategory.Special)
			{
				text = text + ";itemslot:" + ItemSlot.ToLower();
				if (!string.IsNullOrEmpty(CollabTag))
				{
					text = text + ";collab:" + CollabTag;
				}
				if (!string.IsNullOrEmpty(EventTag))
				{
					text = text + ";event:" + EventTag;
				}
				text = ((!(ItemSlot.ToLower() == "emote")) ? (text + ";cosmetics_quality:" + QualityCategory.ToString("G").ToLower()) : (text + ";emote_quality:" + QualityCategory.ToString("G").ToLower()));
			}
			bool granted_manually = false;
			if (!string.IsNullOrEmpty(Promo) && string.IsNullOrEmpty(DropStartTime))
			{
				granted_manually = Promo.Contains("manual");
			}
			string text2 = "";
			if (!string.IsNullOrEmpty(PriceCategory))
			{
				text2 = ItemSlot;
				if (FeatureInItemStore)
				{
					text2 += ";featured";
				}
			}
			return new SteamItemBackend
			{
				itemdefid = Id,
				type = "item",
				name = Name,
				bundle = "",
				tags = text,
				tradable = Tradable,
				marketable = Marketable,
				hidden = Hidden,
				description = Description,
				name_color = ColorUtility.ToHtmlStringRGB(GetColorForQuality(QualityCategory)),
				store_tags = text2,
				item_slot = ItemSlot,
				icon_url = "https://pub-d6170922fa31462fa15463d240b18424.r2.dev/" + Name.Replace(" ", "") + "Icon.png",
				icon_url_large = "https://pub-d6170922fa31462fa15463d240b18424.r2.dev/" + Name.Replace(" ", "") + "Icon.png",
				auto_stack = true,
				promo = Promo,
				price_category = PriceCategory,
				game_only = GameOnly,
				drop_interval = ((!string.IsNullOrEmpty(DropStartTime)) ? 99999999 : 0),
				granted_manually = granted_manually,
				purchase_limit = PurchaseLimit,
				drop_start_time = DropStartTime,
				hide_until_received = HideUntilReceived,
				store_images = StoreImages
			};
		}
	}
}

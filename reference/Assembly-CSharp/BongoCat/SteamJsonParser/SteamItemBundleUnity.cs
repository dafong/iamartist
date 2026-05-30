using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BongoCat.SteamJsonParser
{
	public class SteamItemBundleUnity : ScriptableObject
	{
		public int Id;

		public string Name;

		[SerializeField]
		private List<SteamItemUnity> _itemsInBundle;

		public Sprite Image;

		public int PurchaseBundleDiscount;

		public bool UsePriceCategory;

		public string PriceCategory;

		public bool FeatureInItemStore;

		[SerializeField]
		private string _startDescription = "Thank you very much for supporting Bongo Cat!";

		public SteamItemBackend ToSteamItemBackend()
		{
			string text = "pack";
			if (FeatureInItemStore && !string.IsNullOrEmpty(PriceCategory))
			{
				text += ";featured";
			}
			string text2 = "";
			string text3 = "";
			foreach (SteamItemUnity item in _itemsInBundle)
			{
				text3 = text3 + item.Id + ";";
				text2 = text2 + "https://pub-d6170922fa31462fa15463d240b18424.r2.dev/" + item.Name.Replace(" ", "") + "Icon.png;";
			}
			text3 = text3.TrimEnd(';');
			text2 = text2.TrimEnd(';');
			string text4 = _startDescription + "\n\n\nThis bundle includes the skins ";
			text4 += string.Join(", ", (from i in _itemsInBundle
				where i.ItemSlot == "skin"
				select $"[url=https://store.steampowered.com/itemstore/{3419430}/detail/{i.Id}/]{i.Name}[/url]").ToArray());
			if (_itemsInBundle.Count((SteamItemUnity i) => i.ItemSlot == "hat") > 0)
			{
				text4 += ", as well as the hats ";
				text4 += string.Join(", ", (from i in _itemsInBundle
					where i.ItemSlot == "hat"
					select $"[url=https://store.steampowered.com/itemstore/{3419430}/detail/{i.Id}/]{i.Name}[/url]").ToArray());
			}
			text4 += ".\n\nCredits: [url=https://steamcommunity.com/id/neonflores/]Neon Flores[/url]";
			return new SteamItemBackendBundle
			{
				itemdefid = Id,
				type = "bundle",
				name = Name,
				bundle = text3,
				tradable = false,
				marketable = false,
				description = text4,
				store_tags = text,
				icon_url = "https://pub-d6170922fa31462fa15463d240b18424.r2.dev/" + Name.Replace(" ", "") + ".png",
				icon_url_large = "https://pub-d6170922fa31462fa15463d240b18424.r2.dev/" + Name.Replace(" ", "") + ".png",
				price_category = PriceCategory,
				use_bundle_price = UsePriceCategory,
				store_images = text2,
				purchase_bundle_discount = PurchaseBundleDiscount
			};
		}
	}
}

using System.Collections;
using BongoCat.SteamJsonParser;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat.Supporter
{
	public class SupporterItem : MonoBehaviour
	{
		[SerializeField]
		private SteamItemUnity _item;

		[SerializeField]
		private TMP_Text _priceText;

		[SerializeField]
		private Image _image;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => !string.IsNullOrEmpty(SupporterShop.Instance.PriceSymbol));
			SteamInventory.GetItemPrice(new SteamItemDef_t(_item.Id), out var pCurrentPrice, out var _);
			_priceText.SetText($"{SupporterShop.Instance.PriceSymbol}{(double)pCurrentPrice / 100.0:0.00}");
		}

		public void OnBuyButtonClicked()
		{
			Application.OpenURL($"steam://openurl/https://store.steampowered.com/itemstore/{3419430}/detail/{_item.Id}/");
		}
	}
}

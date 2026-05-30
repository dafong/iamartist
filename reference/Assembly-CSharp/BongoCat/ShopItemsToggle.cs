using UnityEngine;

namespace BongoCat
{
	public class ShopItemsToggle : MonoBehaviour
	{
		[SerializeField]
		private Shop _shop;

		[SerializeField]
		private GameObject _shopVisuals;

		[SerializeField]
		private Shop _emoteShop;

		[SerializeField]
		private GameObject _emoteShopVisuals;

		[SerializeField]
		private PlayerPrefsToggle _showEmoteChest;

		public void TryToggle()
		{
			if (SettingsManager.Instance.AlwaysShowChest.Value || _emoteShop.ChestIsReady || _shop.ChestIsReady)
			{
				_shopVisuals.SetActive(value: true);
				_emoteShopVisuals.SetActive(_showEmoteChest.Value);
			}
			else if (_shopVisuals.activeInHierarchy || _emoteShopVisuals.activeInHierarchy)
			{
				_shopVisuals.SetActive(value: false);
				_emoteShopVisuals.SetActive(value: false);
			}
			else
			{
				_shopVisuals.SetActive(value: true);
				_emoteShopVisuals.SetActive(_showEmoteChest.Value);
			}
		}
	}
}

using System.Collections.Generic;
using BongoCat.SteamJsonParser;
using Steam;
using UnityEngine;

namespace BongoCat
{
	public class EventBanner : MonoBehaviour
	{
		[SerializeField]
		private GameObject _popupMessage;

		[SerializeField]
		private string _steamUrl = "steam://openurl/https://store.steampowered.com/app/3590650?utm_source=game&utm_medium=followButton";

		[SerializeField]
		private string _youTubeUrl = "https://www.youtube.com/watch?v=KavTrZIB12Y";

		[SerializeField]
		private int _youTubeItemId;

		[SerializeField]
		private List<SteamItemUnity> _itemsToGrant;

		public void OpenPopUp()
		{
			_popupMessage.SetActive(value: true);
		}

		public void OpenYouTube()
		{
			PromoItemChecker.Instance.GrantPromoItemDelayed(_youTubeItemId, 5f);
			Application.OpenURL(_youTubeUrl);
		}

		public void OpenSteamPage()
		{
			foreach (SteamItemUnity item in _itemsToGrant)
			{
				PromoItemChecker.Instance.GrantPromoItemDelayed(item.Id, 8f);
			}
			Application.OpenURL(_steamUrl);
			_popupMessage.SetActive(value: false);
		}
	}
}

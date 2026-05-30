using Steam;
using UnityEngine;

namespace BongoCat
{
	public class OpenSteamInventory : MonoBehaviour
	{
		public void OpenInventory()
		{
			Application.OpenURL($"steam://openurl/https://steamcommunity.com/my/inventory/{3419430}");
		}

		public void OpenMarket()
		{
			Application.OpenURL($"steam://openurl/https://steamcommunity.com/market/search?appid={3419430}");
		}

		public void OpenDiscord()
		{
			Object.FindAnyObjectByType<PromoItemChecker>(FindObjectsInactive.Include).GrantDiscordItem();
			Application.OpenURL("https://discord.gg/hDRv2qa2ep");
		}

		public void OpenSteamPage()
		{
			Application.OpenURL("steam://openurl/https://store.steampowered.com/app/3419430?utm_source=game&utm_medium=followButton");
			SteamGroupHelper.Instance.ClickedFollowButton(Global.GroupId.m_SteamID);
		}
	}
}

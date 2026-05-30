using Steamworks;

namespace BongoCat
{
	public class SteamFriendsHelper
	{
		public static string GetFriendName(CSteamID steamID)
		{
			string text = SteamFriends.GetPlayerNickname(steamID);
			if (string.IsNullOrEmpty(text))
			{
				text = SteamFriends.GetFriendPersonaName(steamID);
			}
			return text;
		}
	}
}

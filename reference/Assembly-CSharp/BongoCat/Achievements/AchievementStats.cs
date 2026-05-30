using Steamworks;
using UnityEngine;

namespace BongoCat.Achievements
{
	public class AchievementStats : MonoBehaviour
	{
		private const string EMOTES_USED = "EmotesUsed";

		private const string MAX_MULTIPLAYER_LOBBY_SIZE = "MaxMultiplayerLobbySize";

		private const string TOTAL_ITEMS = "TotalItems";

		private const string EXCHANGES = "Exchanges";

		public static void IncrementEmotesUsed()
		{
			if ((bool)Pets.Instance && Pets.Instance.StatsInitialized)
			{
				SteamUserStats.GetStat("EmotesUsed", out var pData);
				pData++;
				SteamUserStats.SetStat("EmotesUsed", pData);
			}
		}

		public static void SetMaxMultiplayerLobbySize(int size)
		{
			if ((bool)Pets.Instance && Pets.Instance.StatsInitialized)
			{
				SteamUserStats.SetStat("MaxMultiplayerLobbySize", size);
			}
		}

		public static void SetTotalItems(int value)
		{
			if ((bool)Pets.Instance && Pets.Instance.StatsInitialized)
			{
				SteamUserStats.SetStat("TotalItems", value);
			}
		}

		public static void IncrementItems()
		{
			if ((bool)Pets.Instance && Pets.Instance.StatsInitialized)
			{
				SteamUserStats.GetStat("TotalItems", out var pData);
				pData++;
				SteamUserStats.SetStat("TotalItems", pData);
			}
		}

		public static void IncrementExchanges()
		{
			if ((bool)Pets.Instance && Pets.Instance.StatsInitialized)
			{
				SteamUserStats.GetStat("Exchanges", out var pData);
				pData++;
				SteamUserStats.SetStat("Exchanges", pData);
			}
		}
	}
}

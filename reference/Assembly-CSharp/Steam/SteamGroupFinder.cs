using System.Collections;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace Steam
{
	public class SteamGroupFinder : MonoBehaviour
	{
		private IEnumerator Start()
		{
			yield return new WaitForSeconds(1f);
			yield return new WaitUntil(() => SteamManager.Initialized);
			int clanCount = SteamFriends.GetClanCount();
			for (int num = 0; num < clanCount; num++)
			{
				CSteamID clanByIndex = SteamFriends.GetClanByIndex(num);
				Debug.Log($"Clan Name: {SteamFriends.GetClanName(clanByIndex)} Clan Id: {clanByIndex}");
			}
		}
	}
}

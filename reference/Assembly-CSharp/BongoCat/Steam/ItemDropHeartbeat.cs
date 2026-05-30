using System.Collections;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace BongoCat.Steam
{
	public class ItemDropHeartbeat : MonoBehaviour
	{
		private IEnumerator Start()
		{
			while (true)
			{
				yield return new WaitForSecondsRealtime(60f);
				if (SteamManager.s_EverInitialized)
				{
					SteamInventory.SendItemDropHeartbeat();
				}
			}
		}
	}
}

using BongoCat.Multiplayer;
using UnityEngine;

namespace BongoCat.Testing.Multiplayer
{
	public class MultiplayerItemSyncTester : MonoBehaviour
	{
		[SerializeField]
		private SteamMultiplayer _steamMultiplayer;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				_steamMultiplayer.SendChestReady(ready: true);
			}
			if (Input.GetKeyDown(KeyCode.F2))
			{
				_steamMultiplayer.SendChestReady(ready: false);
			}
			if (Input.GetKeyDown(KeyCode.F3))
			{
				_steamMultiplayer.SendReceivedItem(221);
			}
		}
	}
}

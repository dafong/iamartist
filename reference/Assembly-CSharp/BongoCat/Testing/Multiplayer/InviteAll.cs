using System.Linq;
using BongoCat.Multiplayer;
using UnityEngine;

namespace BongoCat.Testing.Multiplayer
{
	public class InviteAll : MonoBehaviour
	{
		[SerializeField]
		private MultiplayerLobby _lobby;

		private void Update()
		{
			if (!Input.GetKeyDown(KeyCode.F8))
			{
				return;
			}
			foreach (FriendListEntry item in Object.FindObjectsByType<FriendListEntry>(FindObjectsInactive.Include, FindObjectsSortMode.None).ToList())
			{
				if (item.PlayingBongoCat)
				{
					item.Invite();
				}
			}
		}
	}
}

using UnityEngine;

namespace BongoCat.Multiplayer
{
	public class FriendList : MonoBehaviour
	{
		private void OnEnable()
		{
			if ((bool)PlayerInviteHandler.Instance)
			{
				PlayerInviteHandler.Instance.UpdateFriendListStatus();
			}
		}
	}
}

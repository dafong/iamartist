using BongoCat;
using BongoCat.Multiplayer;
using BongoCat.TokenRewards;
using UnityEngine;

namespace TokenRewards
{
	public class MultiplayerRewardCondition : TokenRewardCondition
	{
		[SerializeField]
		private TokenItemReward _reward;

		private void Start()
		{
			MultiplayerLobby.Instance.EnteredLobby += OnLobbyEnter;
			MultiplayerLobby.Instance.PlayerJoinedLobby += OnPlayerJoinedLobby;
		}

		private void OnDisable()
		{
			MultiplayerLobby.Instance.EnteredLobby -= OnLobbyEnter;
			MultiplayerLobby.Instance.PlayerJoinedLobby -= OnPlayerJoinedLobby;
		}

		private void OnLobbyEnter()
		{
			_reward.TryOpenUi();
		}

		private void OnPlayerJoinedLobby()
		{
			_reward.TryOpenUi();
		}

		public override bool CheckRewardConditions()
		{
			if (MultiplayerLobby.Instance.IsInLobby && MultiplayerLobby.Instance.GetNumMembersInLobby() > 1)
			{
				return true;
			}
			return false;
		}
	}
}

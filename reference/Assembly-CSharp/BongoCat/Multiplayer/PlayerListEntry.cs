using IroxGames.StoreFronts.Steam;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat.Multiplayer
{
	public class PlayerListEntry : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _name;

		private MultiplayerLobby _lobby;

		private SteamNetworkingIdentity _playerIdentity;

		[SerializeField]
		private Button _inviteButton;

		[SerializeField]
		private Button _joinButton;

		[SerializeField]
		private Button _kickButton;

		[SerializeField]
		private int _maxInvitations;

		private int _invitationsSent;

		private void Awake()
		{
			_lobby = Object.FindAnyObjectByType<MultiplayerLobby>();
		}

		public void Join()
		{
			_lobby.Join(_playerIdentity);
			UpdateStatus();
		}

		public void Invite()
		{
			if (_invitationsSent < _maxInvitations)
			{
				_invitationsSent++;
				_lobby.Invite(_playerIdentity);
				UpdateStatus();
			}
		}

		public void Kick()
		{
			_lobby.Kick(_playerIdentity.GetSteamID());
		}

		public void SetData(SteamNetworkingIdentity identity)
		{
			_playerIdentity = identity;
			UpdateStatus();
		}

		public void UpdateStatus()
		{
			if (SteamManager.ShuttingDown)
			{
				return;
			}
			_name.text = SteamFriends.GetFriendPersonaName(_playerIdentity.GetSteamID());
			_inviteButton.gameObject.SetActive(value: true);
			_inviteButton.interactable = _invitationsSent < _maxInvitations;
			_joinButton.gameObject.SetActive(value: false);
			_kickButton.gameObject.SetActive(value: false);
			if (!SteamFriends.GetFriendGamePlayed(_playerIdentity.GetSteamID(), out var pFriendGameInfo) || pFriendGameInfo.m_gameID.AppID() != Global.SteamID || !pFriendGameInfo.m_steamIDLobby.IsLobby())
			{
				return;
			}
			SteamFriends.GetFriendGamePlayed(SteamUser.GetSteamID(), out var pFriendGameInfo2);
			if (pFriendGameInfo2.m_steamIDLobby == pFriendGameInfo.m_steamIDLobby)
			{
				_inviteButton.gameObject.SetActive(value: false);
				_invitationsSent = 0;
				if (SteamMatchmaking.GetLobbyOwner(_lobby.LobbyId) == SteamUser.GetSteamID())
				{
					_kickButton.gameObject.SetActive(value: true);
				}
			}
			else
			{
				_joinButton.gameObject.SetActive(value: true);
			}
		}
	}
}

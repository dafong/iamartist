using System.Collections;
using BongoCat.Localizer;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat.Multiplayer
{
	public class FriendListEntry : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private Image _contentFill;

		[SerializeField]
		private TMP_Text _statusText;

		[SerializeField]
		private Image _chestIcon;

		private MultiplayerLobby _lobby;

		[SerializeField]
		private Button _inviteButton;

		[SerializeField]
		private Button _joinButton;

		private bool _isInviteOnCooldown;

		private Coroutine _invitationCooldownRoutine;

		private SteamNetworkingIdentity _playerIdentity;

		private Callback<LobbyDataUpdate_t> _lobbyDataUpdateCallback;

		private EPersonaState _onlineState;

		private bool _playingBongoCat;

		private string _playerName;

		[SerializeField]
		private Color _colorOnline;

		[SerializeField]
		private Color _colorBongo;

		[SerializeField]
		private Color _colorBusy;

		[SerializeField]
		private Color _colorAway;

		public MultiplayerLobby Lobby
		{
			get
			{
				if (!_lobby)
				{
					_lobby = Object.FindAnyObjectByType<MultiplayerLobby>();
				}
				return _lobby;
			}
		}

		public EPersonaState OnlineState => _onlineState;

		public bool PlayingBongoCat => _playingBongoCat;

		public string PLayerName => _playerName;

		public CSteamID PlayerSteamID => _playerIdentity.GetSteamID();

		private void Awake()
		{
			_lobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
		}

		private void OnDestroy()
		{
			if (_lobbyDataUpdateCallback != null)
			{
				_lobbyDataUpdateCallback.Dispose();
				_lobbyDataUpdateCallback = null;
			}
		}

		public void Join()
		{
			Lobby.Join(_playerIdentity);
			UpdateStatus();
		}

		public void Invite()
		{
			if (!_isInviteOnCooldown)
			{
				Lobby.Invite(_playerIdentity);
				if (base.gameObject.activeInHierarchy)
				{
					_invitationCooldownRoutine = StartCoroutine(InvitationCooldown());
				}
				UpdateStatus();
			}
		}

		private IEnumerator InvitationCooldown()
		{
			_isInviteOnCooldown = true;
			yield return new WaitForSeconds(2f);
			_isInviteOnCooldown = false;
		}

		public void SetData(SteamNetworkingIdentity identity)
		{
			_playerIdentity = identity;
			UpdateStatus();
		}

		public void UpdateStatus()
		{
			_playerName = SteamFriendsHelper.GetFriendName(_playerIdentity.GetSteamID());
			_name.text = _playerName;
			_onlineState = SteamFriends.GetFriendPersonaState(_playerIdentity.GetSteamID());
			_playingBongoCat = false;
			if (_onlineState == EPersonaState.k_EPersonaStateOffline)
			{
				Object.Destroy(base.gameObject);
				return;
			}
			switch (_onlineState)
			{
			case EPersonaState.k_EPersonaStateOnline:
				_statusText.text = Loca.Instance.Get("Online");
				_contentFill.color = _colorOnline;
				break;
			case EPersonaState.k_EPersonaStateAway:
				_statusText.text = Loca.Instance.Get("Away");
				_contentFill.color = _colorAway;
				break;
			case EPersonaState.k_EPersonaStateSnooze:
				_statusText.text = Loca.Instance.Get("Away");
				_contentFill.color = _colorAway;
				break;
			case EPersonaState.k_EPersonaStateBusy:
				_statusText.text = Loca.Instance.Get("Busy");
				_contentFill.color = _colorBusy;
				break;
			}
			base.gameObject.SetActive(value: true);
			bool flag = !_isInviteOnCooldown && Lobby.IsInLobby && string.IsNullOrEmpty(SteamMatchmaking.GetLobbyData(Lobby.LobbyId, "LobbyClosed")) && (!Lobby.PlayerBlacklist.Contains(_playerIdentity.GetSteamID()) || SteamUser.GetSteamID() == SteamMatchmaking.GetLobbyOwner(Lobby.LobbyId));
			_inviteButton.gameObject.SetActive(flag);
			_chestIcon.enabled = flag;
			_joinButton.gameObject.SetActive(value: false);
			_statusText.gameObject.SetActive(!flag);
			if (!SteamFriends.GetFriendGamePlayed(_playerIdentity.GetSteamID(), out var pFriendGameInfo))
			{
				return;
			}
			if (pFriendGameInfo.m_gameID.AppID() == Global.SteamID)
			{
				_playingBongoCat = true;
				_statusText.text = "Bongo Cat";
				_contentFill.color = _colorBongo;
				base.transform.SetAsFirstSibling();
			}
			if (Lobby.IsInLobby)
			{
				if ((bool)Lobby.GetMember(PlayerSteamID))
				{
					base.gameObject.SetActive(value: false);
					if (_invitationCooldownRoutine != null)
					{
						StopCoroutine(_invitationCooldownRoutine);
					}
					_isInviteOnCooldown = false;
				}
			}
			else if (!(pFriendGameInfo.m_gameID.AppID() != Global.SteamID) && pFriendGameInfo.m_steamIDLobby.IsLobby())
			{
				SteamMatchmaking.RequestLobbyData(pFriendGameInfo.m_steamIDLobby);
			}
		}

		private void OnLobbyDataUpdate(LobbyDataUpdate_t lobbyDataUpdate)
		{
			if (!base.gameObject || !this || Lobby.IsInLobby || lobbyDataUpdate.m_bSuccess == 0 || !SteamFriends.GetFriendGamePlayed(_playerIdentity.GetSteamID(), out var pFriendGameInfo) || pFriendGameInfo.m_steamIDLobby.m_SteamID != lobbyDataUpdate.m_ulSteamIDLobby)
			{
				return;
			}
			_joinButton.gameObject.SetActive(value: true);
			_chestIcon.enabled = true;
			_statusText.gameObject.SetActive(value: false);
			string[] array = SteamMatchmaking.GetLobbyData(pFriendGameInfo.m_steamIDLobby, "BLACKLIST").Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				if (ulong.TryParse(array[i], out var result) && result == SteamUser.GetSteamID().m_SteamID)
				{
					_joinButton.gameObject.SetActive(value: false);
					_chestIcon.enabled = false;
					_statusText.gameObject.SetActive(value: true);
					return;
				}
			}
			if (!string.IsNullOrEmpty(SteamMatchmaking.GetLobbyData(pFriendGameInfo.m_steamIDLobby, "LobbyClosed")))
			{
				_joinButton.gameObject.SetActive(value: false);
				_chestIcon.enabled = false;
				_statusText.gameObject.SetActive(value: true);
			}
		}

		private void OnApplicationQuit()
		{
			if (_lobbyDataUpdateCallback != null)
			{
				_lobbyDataUpdateCallback.Dispose();
				_lobbyDataUpdateCallback = null;
			}
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BongoCat.Achievements;
using BongoCat.DiscordHelper;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace BongoCat.Multiplayer
{
	public class MultiplayerLobby : MonoBehaviour
	{
		private const string LOBBY_BLACKLIST_KEY = "BLACKLIST";

		private const string PRIVATE_LOBBY_KEY = "PRIVATE_LOBBY";

		private const string COMMAND_LINE_CONNECT = "+connect_lobby";

		private const string RICH_PRESENCE_CONNECT = "connect";

		private const string RICH_PRESENCE_GROUP = "steam_player_group";

		private const string RICH_PRESENCE_GROUP_SIZE = "steam_player_group_size";

		public const int MAX_PLAYERS = 250;

		[SerializeField]
		private LobbyMember _lobbyMemberPrefab;

		[SerializeField]
		private Transform _lobbyEntriesRoot;

		private CSteamID _lobbyId;

		private Dictionary<CSteamID, LobbyMember> _lobbyMembers = new Dictionary<CSteamID, LobbyMember>();

		private HashSet<CSteamID> _playerBlacklist = new HashSet<CSteamID>();

		private Callback<LobbyCreated_t> _lobbyCreatedCallback;

		private Callback<GameLobbyJoinRequested_t> _joinRequestCallback;

		private Callback<LobbyEnter_t> _lobbyEnteredCallback;

		private Callback<LobbyChatUpdate_t> _lobbyChatUpdateCallback;

		private Callback<SteamNetworkingMessagesSessionRequest_t> _networkingSessionRequestcallback;

		private Callback<LobbyDataUpdate_t> _lobbyDataCallback;

		private Callback<SteamServersDisconnected_t> _steamDisconnectedCallback;

		private Callback<SteamServersConnected_t> _steamConnectedCallback;

		public static MultiplayerLobby Instance;

		public bool IsClosedLobby => !string.IsNullOrEmpty(SteamMatchmaking.GetLobbyData(_lobbyId, "LobbyClosed"));

		public bool IsLobbyOwner => SteamMatchmaking.GetLobbyOwner(_lobbyId) == SteamUser.GetSteamID();

		public List<LobbyMember> LobbyMembers => _lobbyMembers.Values.ToList();

		public CSteamID LobbyId => _lobbyId;

		public bool IsInLobby => _lobbyId != CSteamID.Nil;

		public HashSet<CSteamID> PlayerBlacklist => _playerBlacklist;

		public int LobbyVisibility
		{
			get
			{
				int.TryParse(SteamMatchmaking.GetLobbyData(_lobbyId, "PRIVATE_LOBBY"), out var result);
				return Mathf.Clamp(result, 0, 2);
			}
		}

		public event Action CreatedLobby;

		public event Action EnteredLobby;

		public event Action PlayerJoinedLobby;

		public event Action PlayerLeftLobby;

		public event Action LeftLobby;

		public event Action LobbyMemberChange;

		public event Action SteamReconnected;

		public event Action SteamDisconnected;

		public event Action<bool> LobbySetPrivate;

		public event Action LobbyDataUpdate;

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			_lobbyId = CSteamID.Nil;
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			RegisterCallbacks();
			ParseCommandLineForLobbyInvite();
		}

		private void RegisterCallbacks()
		{
			_lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
			_joinRequestCallback = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
			_lobbyEnteredCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
			_lobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
			_networkingSessionRequestcallback = Callback<SteamNetworkingMessagesSessionRequest_t>.Create(OnNetworkingSessionRequest);
			_lobbyDataCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
			_steamConnectedCallback = Callback<SteamServersConnected_t>.Create(OnSteamConnected);
			_steamDisconnectedCallback = Callback<SteamServersDisconnected_t>.Create(OnSteamDisconnected);
		}

		private static void ParseCommandLineForLobbyInvite()
		{
			string[] commandLineArgs = Environment.GetCommandLineArgs();
			for (int i = 1; i < commandLineArgs.Length - 1; i++)
			{
				if (commandLineArgs[i] == "+connect_lobby" && ulong.TryParse(commandLineArgs[i + 1], out var result))
				{
					CSteamID steamIDLobby = new CSteamID(result);
					if (steamIDLobby.IsValid() && steamIDLobby.IsLobby())
					{
						Debug.Log("[Multiplayer] Connecting to lobby via command line arg");
						SteamMatchmaking.JoinLobby(steamIDLobby);
					}
					break;
				}
			}
		}

		public void SetLobbyVisibility(int lobbyVisibility)
		{
			if (IsInLobby && IsLobbyOwner)
			{
				ELobbyType eLobbyType = lobbyVisibility switch
				{
					1 => ELobbyType.k_ELobbyTypeFriendsOnly, 
					2 => ELobbyType.k_ELobbyTypePublic, 
					_ => ELobbyType.k_ELobbyTypePrivate, 
				};
				SteamMatchmaking.SetLobbyType(_lobbyId, eLobbyType);
				SteamMatchmaking.SetLobbyData(_lobbyId, "PRIVATE_LOBBY", lobbyVisibility.ToString());
			}
		}

		public void Create()
		{
			SteamMatchmaking.CreateLobby(SettingsManager.Instance.PrivateLobby.Selected switch
			{
				1 => ELobbyType.k_ELobbyTypeFriendsOnly, 
				2 => ELobbyType.k_ELobbyTypePublic, 
				_ => ELobbyType.k_ELobbyTypePrivate, 
			}, 250);
		}

		public void Invite(SteamNetworkingIdentity invitee)
		{
			if (!_lobbyId.IsLobby())
			{
				CreatedLobby += InviteAfterLobbyCreation;
				Create();
				return;
			}
			if (_playerBlacklist.Contains(invitee.GetSteamID()))
			{
				if (!IsLobbyOwner)
				{
					return;
				}
				_playerBlacklist.Remove(invitee.GetSteamID());
				SteamMatchmaking.SetLobbyData(_lobbyId, "BLACKLIST", string.Join(' ', _playerBlacklist));
			}
			SteamMatchmaking.InviteUserToLobby(_lobbyId, invitee.GetSteamID());
			void InviteAfterLobbyCreation()
			{
				SteamMatchmaking.InviteUserToLobby(_lobbyId, invitee.GetSteamID());
				CreatedLobby -= InviteAfterLobbyCreation;
			}
		}

		public void Join(SteamNetworkingIdentity friend)
		{
			if (_lobbyId.IsValid() && _lobbyId.IsLobby())
			{
				Leave();
			}
			SteamFriends.GetFriendGamePlayed(friend.GetSteamID(), out var pFriendGameInfo);
			if (pFriendGameInfo.m_steamIDLobby.IsLobby())
			{
				SteamMatchmaking.JoinLobby(pFriendGameInfo.m_steamIDLobby);
			}
		}

		public void Leave()
		{
			Debug.Log("[Multiplayer] Leaving lobby");
			StopAllCoroutines();
			SteamMatchmaking.LeaveLobby(_lobbyId);
			foreach (LobbyMember value in _lobbyMembers.Values)
			{
				RemoveLobbyMember(value);
			}
			SteamFriends.ClearRichPresence();
			_lobbyMembers.Clear();
			_lobbyId = CSteamID.Nil;
			this.LeftLobby?.Invoke();
			DiscordManager.Instance.SetDefaultActivity();
		}

		public void Kick(CSteamID kickedPlayer)
		{
			if (!_lobbyId.IsValid())
			{
				Debug.Log("[Multiplayer] Invalid Lobby ID when trying to kick user.");
				return;
			}
			if (!IsLobbyOwner)
			{
				MonoBehaviour.print("[Multiplayer] Kick attempt by non-lobby owner");
				return;
			}
			if (IsClosedLobby)
			{
				MonoBehaviour.print("[Multiplayer] Invalid kick attempt during Battle Royale");
				return;
			}
			_playerBlacklist.Add(kickedPlayer);
			SteamMatchmaking.SetLobbyData(_lobbyId, "BLACKLIST", string.Join(' ', _playerBlacklist));
		}

		private void OnLobbyCreated(LobbyCreated_t lobbyCreated)
		{
			if (lobbyCreated.m_eResult != EResult.k_EResultOK)
			{
				Debug.Log($"[Multiplayer] Lobby creation failed with {lobbyCreated.m_eResult}");
				return;
			}
			Debug.Log("[Multiplayer] Lobby created");
			_lobbyId = new CSteamID(lobbyCreated.m_ulSteamIDLobby);
			this.CreatedLobby?.Invoke();
			SteamMatchmaking.SetLobbyData(_lobbyId, "PRIVATE_LOBBY", SettingsManager.Instance.PrivateLobby.Selected.ToString());
		}

		private void OnJoinRequest(GameLobbyJoinRequested_t joinRequest)
		{
			if (!SteamManager.ShuttingDown && joinRequest.m_steamIDLobby.IsValid() && joinRequest.m_steamIDLobby.IsLobby())
			{
				if (IsInLobby)
				{
					Leave();
				}
				SteamMatchmaking.JoinLobby(joinRequest.m_steamIDLobby);
			}
		}

		private void OnLobbyEntered(LobbyEnter_t lobbyEntered)
		{
			Debug.Log("[Multiplayer] Entering lobby");
			if (!SteamManager.ShuttingDown)
			{
				UpdateBlackList();
				if (_playerBlacklist.Contains(SteamUser.GetSteamID()))
				{
					Leave();
					return;
				}
				if (IsClosedLobby)
				{
					Leave();
					return;
				}
				_lobbyId = new CSteamID(lobbyEntered.m_ulSteamIDLobby);
				SteamFriends.SetRichPresence("connect", _lobbyId.ToString());
				this.EnteredLobby?.Invoke();
				StartCoroutine(CreateLobbyMembers());
				DiscordManager.Instance.SetMultiplayerActivity(LobbyId.ToString(), SteamMatchmaking.GetNumLobbyMembers(_lobbyId));
			}
		}

		private void OnLobbyChatUpdate(LobbyChatUpdate_t lobbyChatUpdate)
		{
			if (SteamManager.ShuttingDown)
			{
				return;
			}
			if ((ulong)lobbyChatUpdate.m_rgfChatMemberStateChange == 1)
			{
				if (_lobbyMembers.ContainsKey(new CSteamID(lobbyChatUpdate.m_ulSteamIDUserChanged)) || _playerBlacklist.Contains(new CSteamID(lobbyChatUpdate.m_ulSteamIDUserChanged)) || IsClosedLobby)
				{
					return;
				}
				Debug.Log("[Multiplayer] Player joined lobby");
				CreateLobbyMember(new CSteamID(lobbyChatUpdate.m_ulSteamIDUserChanged));
			}
			if ((ulong)lobbyChatUpdate.m_rgfChatMemberStateChange == 2 || (ulong)lobbyChatUpdate.m_rgfChatMemberStateChange == 4)
			{
				Debug.Log("[Multiplayer] Player left lobby");
				if (_lobbyMembers.TryGetValue(new CSteamID(lobbyChatUpdate.m_ulSteamIDUserChanged), out var value))
				{
					RemoveLobbyMember(value);
					_lobbyMembers.Remove(new CSteamID(lobbyChatUpdate.m_ulSteamIDUserChanged));
				}
				UpdateLobbyOwner();
			}
			this.LobbyMemberChange?.Invoke();
			SteamFriends.SetRichPresence("steam_player_group_size", _lobbyMembers.Keys.Count.ToString());
			AchievementStats.SetMaxMultiplayerLobbySize(_lobbyMembers.Count);
			DiscordManager.Instance.SetMultiplayerActivity(LobbyId.ToString(), SteamMatchmaking.GetNumLobbyMembers(_lobbyId));
		}

		private IEnumerator CreateLobbyMembers()
		{
			yield return SteamMultiplayer.Instance.SetCosmeticsData();
			int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(_lobbyId);
			for (int i = 0; i < numLobbyMembers; i++)
			{
				CSteamID lobbyMemberByIndex = SteamMatchmaking.GetLobbyMemberByIndex(_lobbyId, i);
				if (numLobbyMembers == 1 && !lobbyMemberByIndex.IsValid())
				{
					Leave();
					yield break;
				}
				CreateLobbyMember(lobbyMemberByIndex);
			}
			AchievementStats.SetMaxMultiplayerLobbySize(numLobbyMembers);
			UpdateLobbyOwner();
			this.LobbyMemberChange?.Invoke();
			SteamFriends.SetRichPresence("steam_player_group", _lobbyId.ToString());
			SteamFriends.SetRichPresence("steam_player_group_size", numLobbyMembers.ToString());
			Leaderboard.Instance.Initialized = true;
		}

		private void CreateLobbyMember(CSteamID playerId)
		{
			if (!playerId.IsValid())
			{
				return;
			}
			LobbyMember lobbyMember = global::UnityEngine.Object.Instantiate(_lobbyMemberPrefab, base.transform);
			lobbyMember.Init(playerId, _lobbyEntriesRoot);
			if (!_lobbyMembers.TryAdd(lobbyMember.SteamId, lobbyMember))
			{
				Debug.LogWarning("[Multiplayer] Failed to add lobby member to dictionary");
				return;
			}
			Leaderboard.Instance.CreateMember(lobbyMember.SteamId);
			if (!(playerId == SteamUser.GetSteamID()))
			{
				lobbyMember.ReparentAndPositionCat(SteamMultiplayer.Instance.Root);
				lobbyMember.FetchCosmetics(LobbyId);
				SteamMultiplayer.Instance.SendChestReady(Shop.NormalShop.ChestIsReady);
				SteamFriends.SetRichPresence("steam_player_group_size", GetNumMembersInLobby().ToString());
				this.PlayerJoinedLobby?.Invoke();
			}
		}

		private void RemoveLobbyMember(LobbyMember member)
		{
			if ((bool)member && !SteamManager.ShuttingDown)
			{
				CSteamID steamId = member.SteamId;
				SteamNetworkingIdentity identityRemote = member.NetworkingIdentity;
				Leaderboard.Instance.RemoveMember(member.SteamId);
				global::UnityEngine.Object.Destroy(member.gameObject);
				OnDemandRenderHelper.Instance.ResumeRenderingForDuration(0.1f);
				SteamNetworkingMessages.CloseSessionWithUser(ref identityRemote);
				if (steamId != SteamUser.GetSteamID())
				{
					this.PlayerLeftLobby?.Invoke();
				}
			}
		}

		public void SetClientLobbyMemberData(string key, string data)
		{
			if (!SteamManager.ShuttingDown && IsInLobby && _lobbyId.IsLobby())
			{
				SteamMatchmaking.SetLobbyMemberData(_lobbyId, key, data);
			}
		}

		private void UpdateLobbyOwner()
		{
			if (SteamManager.ShuttingDown || !_lobbyMembers.TryGetValue(SteamMatchmaking.GetLobbyOwner(_lobbyId), out var value))
			{
				return;
			}
			LobbyListEntry lobbyListEntry = value.LobbyListEntry;
			lobbyListEntry.SetOwner(isOwner: true);
			lobbyListEntry.transform.SetAsFirstSibling();
			if (!IsLobbyOwner)
			{
				return;
			}
			foreach (LobbyMember value2 in _lobbyMembers.Values)
			{
				value2.LobbyListEntry.UpdateKickButton();
			}
		}

		private void OnNetworkingSessionRequest(SteamNetworkingMessagesSessionRequest_t sessionRequest)
		{
			if (!SteamManager.ShuttingDown && _lobbyMembers.TryGetValue(sessionRequest.m_identityRemote.GetSteamID(), out var _) && !_playerBlacklist.Contains(sessionRequest.m_identityRemote.GetSteamID()))
			{
				SteamNetworkingMessages.AcceptSessionWithUser(ref sessionRequest.m_identityRemote);
				StartCoroutine(SteamMultiplayer.Instance.SetCosmeticsData());
				SteamMultiplayer.Instance.SendChestReady(Shop.NormalShop.ChestIsReady);
			}
		}

		private void OnSteamConnected(SteamServersConnected_t serversConnected)
		{
			Debug.Log("[Multiplayer] Steam connected");
			this.SteamReconnected?.Invoke();
		}

		private void OnSteamDisconnected(SteamServersDisconnected_t serversDisconnected)
		{
			if (!SteamManager.ShuttingDown)
			{
				Debug.Log("[Multiplayer] Steam disconnected");
				Leave();
				this.SteamDisconnected?.Invoke();
			}
		}

		private void OnLobbyDataUpdate(LobbyDataUpdate_t lobbyDataUpdate)
		{
			if (!SteamManager.ShuttingDown && lobbyDataUpdate.m_bSuccess != 0)
			{
				UpdateBlackList();
				UpdateLobbyOwner();
				this.LobbySetPrivate?.Invoke(LobbyVisibility == 0);
				SettingsManager.Instance.PrivateLobby.SetSelection(LobbyVisibility);
				this.LobbyDataUpdate?.Invoke();
			}
		}

		private void UpdateBlackList()
		{
			string lobbyData = SteamMatchmaking.GetLobbyData(_lobbyId, "BLACKLIST");
			_playerBlacklist.Clear();
			string[] array = lobbyData.Split(' ');
			for (int i = 0; i < array.Length; i++)
			{
				if (ulong.TryParse(array[i], out var result))
				{
					CSteamID cSteamID = new CSteamID(result);
					if (cSteamID == SteamUser.GetSteamID())
					{
						Debug.Log("[Multiplayer] You were kicked from the lobby");
						LastLobbyJoin.Instance.OnLeave();
						Leave();
						break;
					}
					_playerBlacklist.Add(cSteamID);
					if (_lobbyMembers.TryGetValue(cSteamID, out var value))
					{
						Debug.Log("[Multiplayer] Player was kicked. Removing them from your game.");
						RemoveLobbyMember(value);
					}
				}
			}
		}

		public void HideAllLobbyMembers(bool hide)
		{
			foreach (LobbyMember member in GetMembers())
			{
				member.LobbyListEntry.SetVisibility(hide);
			}
		}

		private void OnApplicationQuit()
		{
			StopAllCoroutines();
			DeregisterCallbacks();
		}

		private void DeregisterCallbacks()
		{
			_lobbyCreatedCallback?.Dispose();
			_lobbyCreatedCallback = null;
			_joinRequestCallback?.Dispose();
			_joinRequestCallback = null;
			_lobbyEnteredCallback?.Dispose();
			_lobbyEnteredCallback = null;
			_lobbyChatUpdateCallback?.Dispose();
			_lobbyChatUpdateCallback = null;
			_networkingSessionRequestcallback?.Dispose();
			_networkingSessionRequestcallback = null;
			_lobbyDataCallback?.Dispose();
			_lobbyDataCallback = null;
			_steamDisconnectedCallback?.Dispose();
			_steamDisconnectedCallback = null;
			_steamConnectedCallback?.Dispose();
			_steamConnectedCallback = null;
		}

		public List<LobbyMember> GetMembers()
		{
			return _lobbyMembers.Values.ToList();
		}

		public LobbyMember GetMember(CSteamID id)
		{
			_lobbyMembers.TryGetValue(id, out var value);
			return value;
		}

		public int GetNumMembersInLobby()
		{
			if (!IsInLobby)
			{
				return 0;
			}
			return _lobbyMembers.Count;
		}
	}
}

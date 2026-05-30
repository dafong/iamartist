using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace BongoCat.Multiplayer
{
	public class PlayerInviteHandler : MonoBehaviour
	{
		[SerializeField]
		private FriendListEntry _playerEntryPrefab;

		[SerializeField]
		private Transform _friendsRoot;

		private Dictionary<ulong, FriendListEntry> _onlineFriends = new Dictionary<ulong, FriendListEntry>();

		private Callback<PersonaStateChange_t> _personaStateChangeCallback;

		private Callback<SteamServersConnected_t> _steamConnectedCallback;

		private Callback<SteamServersDisconnected_t> _steamDisconnectedCallback;

		public static PlayerInviteHandler Instance;

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			_personaStateChangeCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
			_steamConnectedCallback = Callback<SteamServersConnected_t>.Create(OnSteamConnected);
			_steamDisconnectedCallback = Callback<SteamServersDisconnected_t>.Create(OnSteamDisconnected);
			CreateFriendsList();
			StartCoroutine(UpdateOnlineFriendsRoutine());
			MultiplayerLobby.Instance.EnteredLobby += UpdateFriendListStatus;
			MultiplayerLobby.Instance.LeftLobby += UpdateFriendListStatus;
		}

		private void CreateFriendsList()
		{
			StartCoroutine(CreateFriendListRoutine());
		}

		private IEnumerator CreateFriendListRoutine()
		{
			yield return new WaitUntil(SteamUser.BLoggedOn);
			yield return new WaitForSeconds(0.2f);
			int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
			for (int i = 0; i < friendCount; i++)
			{
				CSteamID friendByIndex = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
				EPersonaState friendPersonaState = SteamFriends.GetFriendPersonaState(friendByIndex);
				if (friendPersonaState != EPersonaState.k_EPersonaStateOffline && friendPersonaState != EPersonaState.k_EPersonaStateInvisible && !_onlineFriends.ContainsKey(friendByIndex.m_SteamID))
				{
					CreateFriendListEntry(friendByIndex);
					SortFriendList();
				}
			}
		}

		private IEnumerator UpdateOnlineFriendsRoutine()
		{
			while (true)
			{
				yield return new WaitForSeconds(10f);
				if (SteamManager.ShuttingDown)
				{
					break;
				}
				UpdateFriendListStatus();
			}
		}

		private void OnPersonaStateChange(PersonaStateChange_t personaStateChange)
		{
			if (personaStateChange.m_ulSteamID == SteamUser.GetSteamID().m_SteamID)
			{
				return;
			}
			CSteamID cSteamID = new CSteamID(personaStateChange.m_ulSteamID);
			if (_onlineFriends.ContainsKey(personaStateChange.m_ulSteamID) && (bool)_onlineFriends[personaStateChange.m_ulSteamID])
			{
				if (SteamFriends.GetFriendPersonaState(cSteamID) == EPersonaState.k_EPersonaStateOffline)
				{
					Object.Destroy(_onlineFriends[cSteamID.m_SteamID]);
					_onlineFriends.Remove(cSteamID.m_SteamID);
				}
				else
				{
					_onlineFriends[cSteamID.m_SteamID].UpdateStatus();
				}
			}
			else if (personaStateChange.m_nChangeFlags != EPersonaChange.k_EPersonaChangeGoneOffline)
			{
				CreateFriendListEntry(cSteamID);
				SortFriendList();
			}
		}

		private void CreateFriendListEntry(CSteamID friendId)
		{
			if (!_onlineFriends.ContainsKey(friendId.m_SteamID) && SteamFriends.HasFriend(friendId, EFriendFlags.k_EFriendFlagImmediate))
			{
				SteamNetworkingIdentity data = default(SteamNetworkingIdentity);
				data.SetSteamID(friendId);
				FriendListEntry friendListEntry = Object.Instantiate(_playerEntryPrefab, _friendsRoot);
				friendListEntry.SetData(data);
				_onlineFriends.Add(friendId.m_SteamID, friendListEntry);
				friendListEntry.UpdateStatus();
			}
		}

		public void UpdateFriendListStatus()
		{
			SyncFriendList();
			List<ulong> list = new List<ulong>();
			foreach (ulong key in _onlineFriends.Keys)
			{
				if (!_onlineFriends[key])
				{
					list.Add(key);
				}
				else
				{
					_onlineFriends[key].UpdateStatus();
				}
			}
			foreach (ulong item in list)
			{
				_onlineFriends.Remove(item);
			}
			SortFriendList();
		}

		private void SyncFriendList()
		{
			if (_friendsRoot.childCount == _onlineFriends.Values.Count)
			{
				return;
			}
			List<GameObject> list = new List<GameObject>();
			foreach (Transform item in _friendsRoot)
			{
				if ((bool)item.gameObject)
				{
					if (!item.gameObject.TryGetComponent<FriendListEntry>(out var component))
					{
						list.Add(item.gameObject);
					}
					else if (!_onlineFriends.ContainsKey(component.PlayerSteamID.m_SteamID))
					{
						list.Add(item.gameObject);
					}
				}
			}
			for (int i = 0; i < list.Count; i++)
			{
				Object.Destroy(list[i]);
			}
		}

		public void UpdatePlayerEntry(ulong friendId)
		{
			if (_onlineFriends.ContainsKey(friendId))
			{
				if (!_onlineFriends[friendId])
				{
					_onlineFriends.Remove(friendId);
					return;
				}
				_onlineFriends[friendId].UpdateStatus();
				SortFriendList();
			}
		}

		private void SortFriendList()
		{
			foreach (FriendListEntry item in (from friend in _onlineFriends.Values.Where((FriendListEntry friend) => friend != null && (bool)friend.transform).ToList()
				orderby friend.PlayingBongoCat descending, friend.OnlineState, friend.PLayerName
				select friend).ToList())
			{
				if ((bool)item.transform)
				{
					item.transform.SetAsLastSibling();
				}
			}
		}

		private void OnSteamConnected(SteamServersConnected_t steamConnected)
		{
			CreateFriendsList();
		}

		private void OnSteamDisconnected(SteamServersDisconnected_t steamDisconnected)
		{
			foreach (FriendListEntry value in _onlineFriends.Values)
			{
				if ((bool)value && (bool)value.gameObject)
				{
					Object.Destroy(value.gameObject);
				}
			}
			_onlineFriends.Clear();
		}

		private void OnApplicationQuit()
		{
			_steamConnectedCallback?.Dispose();
			_steamConnectedCallback = null;
			_steamDisconnectedCallback?.Dispose();
			_steamDisconnectedCallback = null;
			_personaStateChangeCallback?.Dispose();
			_personaStateChangeCallback = null;
		}
	}
}

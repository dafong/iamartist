using System;
using System.Collections.Generic;
using System.Linq;
using BongoCat;
using BongoCat.Multiplayer;
using Steamworks;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
	public static Leaderboard Instance;

	private const string TAPS_SINCE_JOINED_KEY = "TapsSinceJoined";

	[SerializeField]
	private LeaderboardEntry _entryPrefab;

	[SerializeField]
	private Transform _entryParent;

	[SerializeField]
	private GameObject _leaderboardViewport;

	[SerializeField]
	private GameObject _resetButton;

	private bool _isVisible;

	private Dictionary<CSteamID, LeaderboardEntry> _leaderboardEntries;

	private Dictionary<CSteamID, int> _tapsSinceJoined;

	private bool _initialized;

	public Action<bool> OnShow;

	public bool Initialized
	{
		set
		{
			if (!_initialized && value)
			{
				_initialized = true;
			}
		}
	}

	public bool IsVisible => _isVisible;

	private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		MultiplayerLobby.Instance.EnteredLobby += CreateLeaderboardData;
		MultiplayerLobby.Instance.LeftLobby += DiscardLeaderboard;
		MultiplayerLobby.Instance.LobbyMemberChange += EnableResetButton;
	}

	private void OnDestroy()
	{
		MultiplayerLobby.Instance.EnteredLobby -= CreateLeaderboardData;
		MultiplayerLobby.Instance.LeftLobby -= DiscardLeaderboard;
		MultiplayerLobby.Instance.LobbyMemberChange -= EnableResetButton;
	}

	private void CreateLeaderboardData()
	{
		MultiplayerLobby.Instance.SetClientLobbyMemberData("TapsSinceJoined", 0.ToString());
		_leaderboardEntries = new Dictionary<CSteamID, LeaderboardEntry>();
		_tapsSinceJoined = new Dictionary<CSteamID, int> { 
		{
			SteamUser.GetSteamID(),
			0
		} };
		EnableResetButton();
	}

	private void DiscardLeaderboard()
	{
		_tapsSinceJoined = null;
		foreach (CSteamID key in _leaderboardEntries.Keys)
		{
			LeaderboardEntry leaderboardEntry = _leaderboardEntries[key];
			_leaderboardEntries.Remove(key);
			if ((bool)leaderboardEntry)
			{
				UnityEngine.Object.Destroy(leaderboardEntry.gameObject);
			}
		}
		_leaderboardEntries = null;
		_initialized = false;
		SetLeaderboardUIVisibility(visible: false);
	}

	private void TryUpdateLeaderboardUI()
	{
		if (!_isVisible)
		{
			return;
		}
		int num = 1;
		int num2 = 0;
		foreach (KeyValuePair<CSteamID, int> item in _tapsSinceJoined.OrderByDescending((KeyValuePair<CSteamID, int> kvp) => kvp.Value))
		{
			if (_leaderboardEntries != null && _leaderboardEntries.ContainsKey(item.Key))
			{
				_leaderboardEntries[item.Key].UpdateValues(num, item.Value);
				_leaderboardEntries[item.Key].transform.SetAsLastSibling();
				if (item.Value != num2)
				{
					num2 = item.Value;
					num++;
				}
			}
		}
	}

	public void CreateMember(CSteamID lobbyMemberId)
	{
		_leaderboardEntries[lobbyMemberId] = UnityEngine.Object.Instantiate(_entryPrefab, _entryParent);
		_leaderboardEntries[lobbyMemberId].Init(SteamFriendsHelper.GetFriendName(lobbyMemberId));
		UpdateMemberTaps(lobbyMemberId);
	}

	public void RemoveMember(CSteamID lobbyMemberId)
	{
		_tapsSinceJoined.Remove(lobbyMemberId);
		if (_leaderboardEntries != null && _leaderboardEntries.Remove(lobbyMemberId, out var value))
		{
			UnityEngine.Object.Destroy(value.gameObject);
			TryUpdateLeaderboardUI();
		}
	}

	public void UpdateOwnTaps(int amount)
	{
		_tapsSinceJoined[SteamUser.GetSteamID()] += amount;
		MultiplayerLobby.Instance.SetClientLobbyMemberData("TapsSinceJoined", _tapsSinceJoined[SteamUser.GetSteamID()].ToString());
		TryUpdateLeaderboardUI();
	}

	public void UpdateMemberTapsDirty(CSteamID lobbyMemberId, int amount)
	{
		if (_tapsSinceJoined != null)
		{
			if (!_tapsSinceJoined.ContainsKey(lobbyMemberId))
			{
				Debug.LogWarning($"Lobby member {SteamFriendsHelper.GetFriendName(lobbyMemberId)} with ID {lobbyMemberId} not recognized");
				return;
			}
			_tapsSinceJoined[lobbyMemberId] += amount;
			TryUpdateLeaderboardUI();
		}
	}

	public void UpdateMemberTaps(CSteamID lobbyMemberId)
	{
		if (_tapsSinceJoined != null && !(lobbyMemberId == SteamUser.GetSteamID()))
		{
			int result;
			int value = (int.TryParse(SteamMatchmaking.GetLobbyMemberData(MultiplayerLobby.Instance.LobbyId, lobbyMemberId, "TapsSinceJoined"), out result) ? result : 0);
			_tapsSinceJoined[lobbyMemberId] = value;
			TryUpdateLeaderboardUI();
		}
	}

	public void ResetLeaderboard()
	{
		if (!MultiplayerLobby.Instance.IsLobbyOwner)
		{
			Debug.Log("Couldn't reset leaderboard because user isn't lobby owner");
			return;
		}
		SteamMultiplayer.Instance.SendResetLeaderboard();
		ResetOwnTaps();
	}

	public void ResetOwnTaps()
	{
		_tapsSinceJoined[SteamUser.GetSteamID()] = 0;
		MultiplayerLobby.Instance.SetClientLobbyMemberData("TapsSinceJoined", 0.ToString());
		TryUpdateLeaderboardUI();
	}

	public void ToggleLeaderboardUI()
	{
		SetLeaderboardUIVisibility(!_isVisible);
	}

	public void SetLeaderboardUIVisibility(bool visible)
	{
		_isVisible = visible;
		_leaderboardViewport.SetActive(visible);
		OnShow?.Invoke(visible);
		if (visible)
		{
			TryUpdateLeaderboardUI();
		}
	}

	private void EnableResetButton()
	{
		_resetButton.SetActive(MultiplayerLobby.Instance.IsLobbyOwner);
	}

	public List<CSteamID> GetLowerHalf()
	{
		int cutoff = _tapsSinceJoined.OrderByDescending((KeyValuePair<CSteamID, int> p) => p.Value).ElementAt(Mathf.CeilToInt((float)_tapsSinceJoined.Count / 2f)).Value;
		return (from p in _tapsSinceJoined
			where p.Value <= cutoff
			select p.Key).ToList();
	}
}

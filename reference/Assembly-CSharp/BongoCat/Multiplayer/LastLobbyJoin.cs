using System.Collections;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace BongoCat.Multiplayer
{
	public class LastLobbyJoin : MonoBehaviour
	{
		private const string LAST_LOBBY_KEY = "LAST_LOBBY";

		[SerializeField]
		private MultiplayerLobby _lobby;

		private CSteamID _lobbyId;

		private Callback<LobbyDataUpdate_t> _lobbyDataCallback;

		private bool _tryJoinLobby;

		public static LastLobbyJoin Instance;

		private void Awake()
		{
			Instance = this;
			_lobbyDataCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
		}

		private void OnApplicationQuit()
		{
			_lobbyDataCallback.Dispose();
			_lobbyDataCallback = null;
		}

		private IEnumerator Start()
		{
			_lobby.EnteredLobby += OnLobbyJoined;
			string text = PlayerPrefs.GetString("LAST_LOBBY");
			if (!string.IsNullOrEmpty(text))
			{
				_lobbyId = new CSteamID(BaseConverter.DecodeFromBase59(text));
				yield return new WaitUntil(() => SteamManager.Initialized);
				if (_lobbyId.IsValid() && _lobbyId.IsLobby())
				{
					SteamMatchmaking.RequestLobbyData(_lobbyId);
				}
			}
		}

		private void OnDisable()
		{
			_lobby.EnteredLobby -= OnLobbyJoined;
		}

		private void OnLobbyJoined()
		{
			if (_lobby.LobbyId.IsLobby())
			{
				PlayerPrefs.SetString("LAST_LOBBY", BaseConverter.EncodeToBase59(_lobby.LobbyId.m_SteamID));
			}
			_lobbyId = _lobby.LobbyId;
		}

		public void OnLeave()
		{
			PlayerPrefs.DeleteKey("LAST_LOBBY");
			_lobbyId = CSteamID.Nil;
		}

		private void OnLobbyDataUpdate(LobbyDataUpdate_t lobbyDataUpdate)
		{
			if (!_lobby.IsInLobby && lobbyDataUpdate.m_bSuccess != 0)
			{
				CSteamID steamIDLobby = new CSteamID(lobbyDataUpdate.m_ulSteamIDLobby);
				if (SteamMatchmaking.GetNumLobbyMembers(steamIDLobby) != 0 && _lobbyId.m_SteamID == lobbyDataUpdate.m_ulSteamIDLobby && string.IsNullOrEmpty(SteamMatchmaking.GetLobbyData(steamIDLobby, "LobbyClosed")))
				{
					SteamMatchmaking.JoinLobby(_lobbyId);
				}
			}
		}
	}
}

using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat.Multiplayer
{
	public class LobbyListEntry : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _name;

		[SerializeField]
		private GameObject _ownerVisual;

		[SerializeField]
		private Button _kickButton;

		[SerializeField]
		private Button _hideButton;

		[SerializeField]
		private Button _showButton;

		private bool _isOwner;

		private MultiplayerLobby _lobby;

		private CSteamID _playerIdentity;

		private Callback<PersonaStateChange_t> _personaStateChangeCallback;

		public Action<bool> OnToggleVisibility;

		public MultiplayerLobby Lobby
		{
			get
			{
				if (!_lobby)
				{
					_lobby = global::UnityEngine.Object.FindAnyObjectByType<MultiplayerLobby>();
				}
				return _lobby;
			}
		}

		public void SetData(CSteamID identity, bool isOwner)
		{
			_playerIdentity = identity;
			_name.text = SteamFriendsHelper.GetFriendName(_playerIdentity);
			SetOwner(isOwner);
			UpdateKickButton();
			if (_playerIdentity == SteamUser.GetSteamID())
			{
				_showButton.gameObject.SetActive(value: false);
				_hideButton.gameObject.SetActive(value: false);
			}
			_personaStateChangeCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
			SettingsManager.Instance.AutoHideCats.OnToggleUpdated.AddListener(Hide);
		}

		public void UpdateKickButton()
		{
			_kickButton.gameObject.SetActive(Lobby.IsLobbyOwner && !Lobby.IsClosedLobby && _playerIdentity != SteamUser.GetSteamID());
		}

		private void OnPersonaStateChange(PersonaStateChange_t personaStateChange)
		{
			if (personaStateChange.m_ulSteamID == _playerIdentity.m_SteamID)
			{
				_name.text = SteamFriendsHelper.GetFriendName(_playerIdentity);
			}
		}

		public void SetOwner(bool isOwner)
		{
			_isOwner = isOwner;
			_ownerVisual.SetActive(_isOwner);
		}

		public void Kick()
		{
			Lobby.Kick(_playerIdentity);
		}

		private void Hide(bool hide)
		{
			SetVisibility(!hide);
		}

		public void SetVisibility(bool isVisible)
		{
			if (!(_playerIdentity == SteamUser.GetSteamID()))
			{
				_showButton.gameObject.SetActive(!isVisible);
				_hideButton.gameObject.SetActive(isVisible);
				OnToggleVisibility?.Invoke(isVisible);
			}
		}

		private void OnApplicationQuit()
		{
			_personaStateChangeCallback.Dispose();
			_personaStateChangeCallback = null;
		}
	}
}

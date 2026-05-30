using System.Collections;
using BongoCat.Localizer;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat.Multiplayer
{
	public class MultiplayerLobbyUi : MonoBehaviour
	{
		[SerializeField]
		private GameObject _lobbyHeader;

		[SerializeField]
		private TMP_Text _headerText;

		[SerializeField]
		private GameObject _createLobbyButton;

		[SerializeField]
		private GameObject _leaveLobbyButton;

		[SerializeField]
		private GameObject _offlineWarning;

		[SerializeField]
		private LobbyIdJoin _lobbyIdJoin;

		[SerializeField]
		private GameObject _settingsbar;

		[SerializeField]
		private Image _autoHideCatsIcon;

		[SerializeField]
		private TMP_Text _autoHideCatsTooltip;

		[SerializeField]
		private GameObject _lobbyInfo;

		[SerializeField]
		private LobbyIdDisplay _lobbyIdDisplay;

		[SerializeField]
		private GameObject _privateLobbyDisplay;

		[SerializeField]
		private GameObject _privateLobbyToggle;

		[SerializeField]
		private Sprite _showIcon;

		[SerializeField]
		private Sprite _hideIcon;

		private const string LOBBY_HEADER_LOCA_KEY = "LobbyHeader";

		private IEnumerator Start()
		{
			MultiplayerLobby.Instance.EnteredLobby += OnLobbyEntered;
			MultiplayerLobby.Instance.LeftLobby += OnLobbyLeft;
			MultiplayerLobby.Instance.SteamDisconnected += OnLostConnection;
			MultiplayerLobby.Instance.SteamReconnected += OnReconnected;
			MultiplayerLobby.Instance.LobbyMemberChange += OnLobbyMemberChange;
			MultiplayerLobby.Instance.LobbyDataUpdate += OnLobbyDataUpdate;
			MultiplayerLobby.Instance.LobbySetPrivate += HideLobbyId;
			SettingsManager.Instance.AutoHideCats.OnToggleUpdated.AddListener(ToggleAutoHideCats);
			yield return new WaitUntil(() => SteamManager.Initialized);
			bool flag = SteamUser.BLoggedOn();
			_createLobbyButton.SetActive(flag);
			_offlineWarning.SetActive(!flag);
			_leaveLobbyButton.SetActive(value: false);
		}

		private void OnLobbyEntered()
		{
			_lobbyIdDisplay.SetLobbyId(MultiplayerLobby.Instance.LobbyId);
			_createLobbyButton.SetActive(value: false);
			_lobbyIdJoin.gameObject.SetActive(value: false);
			_lobbyInfo.gameObject.SetActive(value: true);
			_leaveLobbyButton.SetActive(value: true);
			_settingsbar.SetActive(value: true);
			HideLobbyId(MultiplayerLobby.Instance.LobbyVisibility == 0);
			SettingsManager.Instance.PrivateLobby.SetSelection(MultiplayerLobby.Instance.LobbyVisibility);
			ToggleAutoHideCats(SettingsManager.Instance.AutoHideCats.Value);
			ShowLobbyHeader();
		}

		private void OnLobbyLeft()
		{
			_createLobbyButton.SetActive(value: true);
			_lobbyIdJoin.gameObject.SetActive(value: true);
			_lobbyInfo.SetActive(value: false);
			_leaveLobbyButton.SetActive(value: false);
			_lobbyHeader.SetActive(value: false);
			_settingsbar.SetActive(value: false);
			PlayerInviteHandler.Instance.UpdateFriendListStatus();
		}

		private void OnLostConnection()
		{
			_lobbyHeader.SetActive(value: false);
			_createLobbyButton.SetActive(value: false);
			_offlineWarning.SetActive(value: true);
		}

		private void OnReconnected()
		{
			_createLobbyButton.SetActive(value: true);
			_offlineWarning.SetActive(value: false);
		}

		private void OnLobbyMemberChange()
		{
			PlayerInviteHandler.Instance.UpdateFriendListStatus();
			OnLobbyDataUpdate();
		}

		private void ShowLobbyHeader()
		{
			if (MultiplayerLobby.Instance.IsInLobby)
			{
				_headerText.text = string.Format("{0} ({1}/{2})", Loca.Instance.Get("LobbyHeader"), MultiplayerLobby.Instance.GetNumMembersInLobby(), 250);
				_lobbyHeader.transform.SetAsFirstSibling();
				_lobbyHeader.SetActive(value: true);
			}
		}

		private void OnLobbyDataUpdate()
		{
			_leaveLobbyButton.transform.SetAsFirstSibling();
			_lobbyInfo.transform.SetAsFirstSibling();
			ShowLobbyHeader();
			_settingsbar.transform.SetAsFirstSibling();
		}

		private void HideLobbyId(bool hide)
		{
			_lobbyIdDisplay.gameObject.SetActive(!hide);
			_privateLobbyDisplay.SetActive(hide);
		}

		private void ToggleAutoHideCats(bool hide)
		{
			_autoHideCatsIcon.sprite = (hide ? _showIcon : _hideIcon);
			_autoHideCatsTooltip.text = (hide ? "Disable Auto-Hide" : "Enable Auto-Hide");
		}
	}
}

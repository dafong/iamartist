using Steamworks;
using TMPro;
using UnityEngine;

namespace BongoCat.Multiplayer
{
	public class PlayerNameTag : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _playerName;

		[SerializeField]
		private ToggleGameObject _handle;

		[SerializeField]
		private GameObject _visuals;

		private CSteamID _steamID;

		private Callback<PersonaStateChange_t> _personaStateChangeCallback;

		private ToggleObjectsWithKey _toggleObjectsWithKey;

		public void Init(CSteamID steamID)
		{
			_steamID = steamID;
			_playerName.text = SteamFriendsHelper.GetFriendName(_steamID);
			_toggleObjectsWithKey = Object.FindAnyObjectByType<ToggleObjectsWithKey>();
			OnShowNameTag(SettingsManager.Instance.ShowPlayerNames.Value);
			SettingsManager.Instance.ShowPlayerNames.OnToggleUpdated.AddListener(OnShowNameTag);
			_handle.OnToggle += OnHandleToggleChanged;
			_personaStateChangeCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
		}

		private void OnPersonaStateChange(PersonaStateChange_t personaStateChange)
		{
			if (personaStateChange.m_ulSteamID == _steamID.m_SteamID)
			{
				_playerName.text = SteamFriendsHelper.GetFriendName(_steamID);
			}
		}

		private void OnHandleToggleChanged()
		{
			OnShowNameTag(SettingsManager.Instance.ShowPlayerNames.Value && _toggleObjectsWithKey.ShowObjects);
		}

		private void OnShowNameTag(bool toggle)
		{
			_visuals.SetActive(toggle && !_handle.gameObject.activeInHierarchy);
			if (toggle)
			{
				_toggleObjectsWithKey.OnToggleWithKey += OnToggleWithKey;
			}
			else
			{
				_toggleObjectsWithKey.OnToggleWithKey -= OnToggleWithKey;
			}
		}

		private void OnToggleWithKey(bool toggle)
		{
			_visuals.SetActive(toggle && SettingsManager.Instance.ShowPlayerNames.Value && !_handle.gameObject.activeInHierarchy);
		}

		private void OnApplicationQuit()
		{
			_personaStateChangeCallback.Dispose();
			_personaStateChangeCallback = null;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (!hasFocus)
			{
				_handle.gameObject.SetActive(value: false);
				_visuals.SetActive(SettingsManager.Instance.ShowPlayerNames.Value && _toggleObjectsWithKey.ShowObjects);
			}
		}

		private void OnDestroy()
		{
			if ((bool)_toggleObjectsWithKey)
			{
				_toggleObjectsWithKey.OnToggleWithKey -= OnToggleWithKey;
			}
			_handle.OnToggle -= OnHandleToggleChanged;
		}
	}
}

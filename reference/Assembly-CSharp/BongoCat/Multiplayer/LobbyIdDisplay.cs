using Steamworks;
using TMPro;
using UnityEngine;

namespace BongoCat.Multiplayer
{
	public class LobbyIdDisplay : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private PlayerPrefsToggle _hideLobbyIdToggle;

		[SerializeField]
		private GameObject _hideIdObject;

		private string _lobbyId;

		private bool _hidden;

		private void Start()
		{
			_hideLobbyIdToggle.OnToggleUpdated.AddListener(SetHidden);
			SetHidden(_hideLobbyIdToggle.Value);
		}

		public void SetLobbyId(CSteamID lobbyId)
		{
			_lobbyId = BaseConverter.EncodeToBase59(lobbyId.m_SteamID);
			_text.text = _lobbyId;
			SetHidden(_hideLobbyIdToggle.Value);
		}

		public void CopyToClipboard()
		{
			GUIUtility.systemCopyBuffer = _lobbyId;
		}

		private void SetHidden(bool hide)
		{
			_hidden = hide;
			_hideIdObject.SetActive(_hidden);
		}
	}
}

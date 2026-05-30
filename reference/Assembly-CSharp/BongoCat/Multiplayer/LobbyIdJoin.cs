using System.Collections;
using Steam;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat.Multiplayer
{
	public class LobbyIdJoin : MonoBehaviour
	{
		[SerializeField]
		private Button _joinButton;

		[SerializeField]
		private GameObject _randomJoinButton;

		[SerializeField]
		private Button _createLobbyButton;

		[SerializeField]
		private TMP_InputField _inputField;

		private string _currentLobbyCodeInput;

		private CSteamID _lobbyId;

		private Callback<LobbyDataUpdate_t> _lobbyDataCallback;

		private CallResult<LobbyMatchList_t> _callResultLobbyMatchList;

		private int _lobbyListRequest;

		private Coroutine _validationRoutine;

		[SerializeField]
		private SpecialLobbyCodeReferencer _specialLobbyCodeReferencer;

		private void Awake()
		{
			_lobbyDataCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
			_callResultLobbyMatchList = CallResult<LobbyMatchList_t>.Create(OnLobbyMatchList);
		}

		private void OnLobbyDataUpdate(LobbyDataUpdate_t lobbyDataUpdate)
		{
			_joinButton.interactable = false;
			if (base.gameObject.activeInHierarchy && lobbyDataUpdate.m_bSuccess != 0 && SteamMatchmaking.GetNumLobbyMembers(new CSteamID(lobbyDataUpdate.m_ulSteamIDLobby)) != 0)
			{
				bool flag = _lobbyId.m_SteamID == lobbyDataUpdate.m_ulSteamIDLobby;
				_joinButton.interactable = flag || _specialLobbyCodeReferencer.IsSpecialLobbyCode(_currentLobbyCodeInput);
				if (!flag && _validationRoutine != null)
				{
					StopCoroutine(_validationRoutine);
					_validationRoutine = null;
				}
			}
		}

		private IEnumerator ValidateLobbyIdRoutine()
		{
			WaitForSeconds waitForOneSecond = new WaitForSeconds(1f);
			while (_lobbyId.IsValid() && _lobbyId.IsLobby())
			{
				SteamMatchmaking.RequestLobbyData(_lobbyId);
				yield return waitForOneSecond;
			}
			_validationRoutine = null;
		}

		public void OnValueChanged(string val)
		{
			if (string.IsNullOrWhiteSpace(val))
			{
				_randomJoinButton.SetActive(value: true);
				_joinButton.gameObject.SetActive(value: false);
				return;
			}
			_randomJoinButton.SetActive(value: false);
			_joinButton.gameObject.SetActive(value: true);
			_joinButton.interactable = false;
			_currentLobbyCodeInput = val.Trim().ToLower();
			if (_specialLobbyCodeReferencer.IsSpecialLobbyCode(_currentLobbyCodeInput))
			{
				_joinButton.interactable = true;
			}
			ulong ulSteamID = BaseConverter.DecodeFromBase59(val.Trim());
			_lobbyId = new CSteamID(ulSteamID);
			if (_lobbyId.IsValid() && _lobbyId.IsLobby())
			{
				_validationRoutine = StartCoroutine(ValidateLobbyIdRoutine());
			}
		}

		public void JoinLobby()
		{
			if (_specialLobbyCodeReferencer.IsSpecialLobbyCode(_currentLobbyCodeInput))
			{
				foreach (int itemId in _specialLobbyCodeReferencer.GetItemIds(_currentLobbyCodeInput))
				{
					PromoItemChecker.Instance.GrantPromoItem(itemId);
				}
				_joinButton.interactable = false;
			}
			else if (_lobbyId.IsValid() && _lobbyId.IsLobby())
			{
				SteamMatchmaking.JoinLobby(_lobbyId);
				if (_validationRoutine != null)
				{
					StopCoroutine(_validationRoutine);
				}
				_validationRoutine = null;
				_inputField.text = string.Empty;
				_lobbyId = CSteamID.Nil;
				_joinButton.interactable = false;
			}
		}

		public void JoinRandomLobby()
		{
			_createLobbyButton.interactable = false;
			_lobbyListRequest = 0;
			SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterDefault);
			RequestLobbyList();
		}

		private void RequestLobbyList()
		{
			SteamAPICall_t hAPICall = SteamMatchmaking.RequestLobbyList();
			_callResultLobbyMatchList.Set(hAPICall);
		}

		private void OnLobbyMatchList(LobbyMatchList_t pLobbyMatchList, bool bIOFailure)
		{
			if (bIOFailure)
			{
				_createLobbyButton.interactable = true;
			}
			else if (pLobbyMatchList.m_nLobbiesMatching == 0)
			{
				switch (_lobbyListRequest)
				{
				case 0:
					SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterFar);
					break;
				case 1:
					SteamMatchmaking.AddRequestLobbyListDistanceFilter(ELobbyDistanceFilter.k_ELobbyDistanceFilterWorldwide);
					break;
				default:
					Debug.Log("No open lobbies were found");
					_createLobbyButton.interactable = true;
					return;
				}
				_lobbyListRequest++;
				Invoke("RequestLobbyList", 1f);
			}
			else
			{
				SteamMatchmaking.JoinLobby(SteamMatchmaking.GetLobbyByIndex(Random.Range(0, (int)pLobbyMatchList.m_nLobbiesMatching)));
				_createLobbyButton.interactable = true;
			}
		}

		private void OnDisable()
		{
			if (_validationRoutine != null)
			{
				StopCoroutine(_validationRoutine);
				_validationRoutine = null;
			}
			_inputField.text = string.Empty;
			_lobbyId = CSteamID.Nil;
			_joinButton.interactable = false;
			_randomJoinButton.SetActive(value: true);
			_joinButton.gameObject.SetActive(value: false);
			_createLobbyButton.interactable = true;
		}
	}
}

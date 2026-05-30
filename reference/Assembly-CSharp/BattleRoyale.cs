using System;
using System.Collections;
using System.Linq;
using BongoCat;
using BongoCat.Multiplayer;
using BongoCat.SteamJsonParser;
using DG.Tweening;
using Steam;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleRoyale : MonoBehaviour
{
	[Serializable]
	public struct RoundDuration
	{
		public int playerCountAnchor;

		public int roundDuration;
	}

	public static BattleRoyale Instance;

	[SerializeField]
	private int _matchmakingDuration;

	[SerializeField]
	private RoundDuration[] _roundDuration;

	[SerializeField]
	private int _minPlayerAmountToStart;

	[SerializeField]
	private TMP_Text _countdownTxt;

	[SerializeField]
	private TMP_Text _infoTxt;

	[SerializeField]
	private RectTransform _announcementPanel;

	[SerializeField]
	private TMP_Text _announcementTitle;

	[SerializeField]
	private TMP_Text _announcementSubtitle;

	[SerializeField]
	private FrameAnimation _bloodExplosion;

	[SerializeField]
	private Transform _canvas;

	[SerializeField]
	private Image _eliminatedPanel;

	[SerializeField]
	private Color _eliminatedColor;

	private bool _matchmakingRound;

	private int _currentRound;

	private long _endOfRoundTime;

	private Coroutine _countdownRoutine;

	private bool _eliminatingPlayers;

	private string _eliminatedPlayers;

	private int _eliminationRequest;

	private MultiplayerLobby _lobby;

	private CSteamID _lobbyId;

	[SerializeField]
	private SteamItemUnity _winnerItem;

	private const string COUNTDOWN_KEY = "BattleCountdownEnd";

	private const string CLOSED_KEY = "LobbyClosed";

	private const string ELIM_PLAYERS_KEY = "BattleEliminatedPlayers";

	private const string START_BATTLE = "Battle will start soon";

	private const string START_BATTLE_SUB = "Type as much you can to be the last cat standing";

	private const string NEXT_ROUND = "Round {0}";

	private const string NEXT_ROUND_SUB = "{0} players remaining";

	private const string ELIMINATION = "You have been eliminated";

	private const string ELIMINATION_SUB = "Better luck next time";

	private const string WIN = "Congratulations";

	private const string WIN_SUB = "You have won";

	public bool InBattle => _endOfRoundTime != -1;

	public bool HasWon { get; private set; }

	private void OnEnable()
	{
		Instance = this;
	}

	private void Start()
	{
		_lobby = MultiplayerLobby.Instance;
		Cleanup();
		_lobby.EnteredLobby += SubscribeMethods;
		_lobby.LeftLobby += Cleanup;
	}

	private void SubscribeMethods()
	{
		_lobbyId = _lobby.LobbyId;
		_lobby.LobbyMemberChange += CheckForBattleStart;
		_lobby.LobbyDataUpdate += StartBattleRound;
		_lobby.LobbyDataUpdate += CheckIfEliminated;
		_lobby.LobbyMemberChange += StartNextRound;
		_infoTxt.text = "Waiting for players...";
		if (SteamMatchmaking.GetNumLobbyMembers(_lobbyId) < _minPlayerAmountToStart)
		{
			_countdownTxt.text = "";
		}
	}

	private void Cleanup()
	{
		_lobby.LobbyMemberChange -= CheckForBattleStart;
		_lobby.LobbyDataUpdate -= StartBattleRound;
		_lobby.LobbyDataUpdate -= CheckIfEliminated;
		_lobby.LobbyMemberChange -= StartNextRound;
		_eliminationRequest = 0;
		if (_countdownRoutine != null)
		{
			StopCoroutine(_countdownRoutine);
		}
		_eliminatedPlayers = "";
		_eliminatingPlayers = false;
		_endOfRoundTime = -1L;
		_currentRound = 0;
		_matchmakingRound = false;
		_lobbyId = CSteamID.Nil;
	}

	private void CheckForBattleStart()
	{
		long.TryParse(SteamMatchmaking.GetLobbyData(_lobbyId, "BattleCountdownEnd"), out var result);
		if (!InBattle && result > 0)
		{
			StartBattleRound();
		}
		else if (_lobby.IsLobbyOwner && SteamMatchmaking.GetNumLobbyMembers(_lobbyId) >= _minPlayerAmountToStart)
		{
			SendCountdown(_matchmakingDuration);
		}
	}

	private void SendCountdown(int minutes)
	{
		long num = ((minutes > 0) ? (SteamUtils.GetServerRealTime() + minutes * 60) : (-1));
		SteamMatchmaking.SetLobbyData(_lobbyId, "BattleCountdownEnd", num.ToString());
	}

	private void StartBattleRound()
	{
		string lobbyData = SteamMatchmaking.GetLobbyData(_lobbyId, "BattleCountdownEnd");
		if (!long.TryParse(lobbyData, out var result))
		{
			return;
		}
		if (result < 0)
		{
			if (InBattle)
			{
				Cleanup();
				SubscribeMethods();
			}
		}
		else if (!string.IsNullOrEmpty(lobbyData) && _endOfRoundTime != result)
		{
			_matchmakingRound = !InBattle;
			if (_matchmakingRound)
			{
				DisplayAnnouncement("Battle will start soon", "Type as much you can to be the last cat standing");
			}
			else
			{
				_infoTxt.text = "The players with the least taps will be eliminated!";
				DisplayAnnouncement($"Round {_currentRound}", $"{SteamMatchmaking.GetNumLobbyMembers(_lobbyId)} players remaining");
			}
			_endOfRoundTime = result;
			Leaderboard.Instance.ResetOwnTaps();
			_lobby.LobbyMemberChange -= CheckForBattleStart;
			_eliminatingPlayers = false;
			_eliminationRequest = 0;
			if (_countdownRoutine != null)
			{
				StopCoroutine(_countdownRoutine);
			}
			_countdownRoutine = StartCoroutine(CountdownRound());
			_currentRound++;
		}
	}

	private IEnumerator CountdownRound()
	{
		long remaining = 1L;
		while (remaining > 0)
		{
			remaining = _endOfRoundTime - SteamUtils.GetServerRealTime();
			DisplayCountdown(remaining);
			yield return new WaitForSeconds(1f);
		}
		if (_endOfRoundTime >= 0)
		{
			DisplayCountdown(0L);
			EndRound();
		}
	}

	private void EndRound()
	{
		if (_lobby.IsLobbyOwner)
		{
			if (_matchmakingRound)
			{
				SteamMatchmaking.SetLobbyJoinable(_lobbyId, bLobbyJoinable: false);
				SteamMatchmaking.SetLobbyData(_lobbyId, "LobbyClosed", "True");
				SendCountdown(GetRoundDuration());
			}
			else
			{
				string pchValue = string.Join(' ', Leaderboard.Instance.GetLowerHalf());
				SteamMatchmaking.SetLobbyData(_lobbyId, "BattleEliminatedPlayers", pchValue);
			}
		}
	}

	private void CheckIfEliminated()
	{
		if (!InBattle)
		{
			return;
		}
		string lobbyData = SteamMatchmaking.GetLobbyData(_lobbyId, "BattleEliminatedPlayers");
		if (lobbyData == _eliminatedPlayers || string.IsNullOrEmpty(lobbyData))
		{
			return;
		}
		_eliminatedPlayers = lobbyData;
		if (IsPlayerEliminated(SteamUser.GetSteamID()))
		{
			EliminateSelf();
			return;
		}
		if (_eliminationRequest > 0)
		{
			if (_lobby.IsLobbyOwner)
			{
				CheckNextRoundStart(SteamMatchmaking.GetNumLobbyMembers(_lobby.LobbyId));
			}
			_eliminationRequest = 0;
		}
		_eliminatingPlayers = true;
	}

	private void EliminateSelf()
	{
		DisplayAnnouncement("You have been eliminated", "Better luck next time");
		_eliminatedPanel.color = _eliminatedColor;
		_eliminatedPanel.DOFade(0f, 0.3f);
		Cleanup();
		LastLobbyJoin.Instance.OnLeave();
		_lobby.Leave();
		PromoItemChecker.Instance.GrantPromoItem(_winnerItem.Id);
	}

	private void StartNextRound()
	{
		if (!InBattle || !_lobby.IsLobbyOwner)
		{
			return;
		}
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(_lobbyId);
		if (!_matchmakingRound)
		{
			if (numLobbyMembers == 1)
			{
				BattleWon();
				return;
			}
		}
		else if (numLobbyMembers < _minPlayerAmountToStart)
		{
			SendCountdown(-1);
			return;
		}
		if (!_eliminatingPlayers)
		{
			_eliminationRequest++;
		}
		else
		{
			CheckNextRoundStart(numLobbyMembers);
		}
	}

	private void CheckNextRoundStart(int playerCount)
	{
		for (int i = 0; i < playerCount; i++)
		{
			if (IsPlayerEliminated(SteamMatchmaking.GetLobbyMemberByIndex(_lobbyId, i)))
			{
				return;
			}
		}
		SendCountdown(GetRoundDuration());
	}

	private void BattleWon()
	{
		DisplayAnnouncement("Congratulations", "You have won");
		HasWon = true;
		LastLobbyJoin.Instance.OnLeave();
		_lobby.Leave();
		PromoItemChecker.Instance.GrantPromoItem(_winnerItem.Id);
	}

	private bool IsPlayerEliminated(CSteamID playerId)
	{
		string[] array = _eliminatedPlayers.Split(' ');
		for (int i = 0; i < array.Length; i++)
		{
			if (ulong.TryParse(array[i], out var result) && new CSteamID(result) == playerId)
			{
				return true;
			}
		}
		return false;
	}

	private void DisplayCountdown(long remainingTime)
	{
		if (remainingTime <= 10 && remainingTime > 0)
		{
			DisplayAnnouncement(remainingTime.ToString(), "", 1f);
		}
		int num = Mathf.FloorToInt((float)remainingTime / 60f);
		int num2 = Mathf.FloorToInt((float)remainingTime % 60f);
		_countdownTxt.text = $"{num:00}:{num2:00}";
	}

	private void DisplayAnnouncement(string title, string subtitle, float length = 2f)
	{
		_announcementTitle.text = title;
		_announcementSubtitle.text = subtitle;
		DOTween.Kill(_announcementPanel);
		DOTween.Kill(_announcementTitle);
		DOTween.Kill(_announcementSubtitle);
		if (_announcementPanel.sizeDelta.y > 0f)
		{
			_announcementPanel.sizeDelta = new Vector2(_announcementPanel.sizeDelta.x, 200f);
			OnDemandRenderHelper.Instance.ResumeRenderingForDuration(length);
			FadeTexts(length);
		}
		else
		{
			OnDemandRenderHelper.Instance.ResumeRenderingForDuration(length + 1f);
			_announcementPanel.DOSizeDelta(new Vector2(_announcementPanel.sizeDelta.x, 200f), 0.5f).OnComplete(delegate
			{
				FadeTexts(length);
			});
		}
	}

	private void FadeTexts(float length)
	{
		_announcementTitle.DOFade(1f, length / 2f).OnComplete(delegate
		{
			_announcementTitle.DOFade(0f, length / 2f).SetEase(Ease.InCubic);
		});
		_announcementSubtitle.DOFade(1f, length / 2f).OnComplete(delegate
		{
			_announcementSubtitle.DOFade(0f, length / 2f).SetEase(Ease.InCubic).OnComplete(delegate
			{
				_announcementPanel.DOSizeDelta(new Vector2(_announcementPanel.sizeDelta.x, 0f), 0.5f);
			});
		});
	}

	private int GetRoundDuration()
	{
		int playerCount = SteamMatchmaking.GetNumLobbyMembers(_lobbyId);
		return _roundDuration.OrderBy((RoundDuration c) => Mathf.Abs(c.playerCountAnchor - playerCount)).First().roundDuration;
	}

	public void KillCat(Transform cat)
	{
		UnityEngine.Object.Instantiate(_bloodExplosion, cat.position, Quaternion.identity, _canvas).PlayAnimation(delegate
		{
			UnityEngine.Object.Destroy(cat.gameObject);
		});
		HasWon = false;
	}
}

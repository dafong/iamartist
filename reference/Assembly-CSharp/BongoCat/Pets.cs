using System;
using System.Collections;
using System.Globalization;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace BongoCat
{
	public class Pets : MonoBehaviour
	{
		[FormerlySerializedAs("_cashText")]
		[SerializeField]
		private TMP_Text _petsText;

		[SerializeField]
		private TMP_Text _lifetimePetsText;

		public static Pets Instance;

		private int _currentGained;

		private int _currentAchievement;

		private int _totalSpent;

		private bool _init;

		private const string STAT_NAME = "BongoTap";

		private const string STAT_NAME_ACHIEVEMENTS = "BongoBeat";

		private const string STAT_NAME_MINUS = "BongoMinus";

		private CallResult<UserStatsReceived_t> _t;

		[NonSerialized]
		public bool StatsInitialized;

		public int Current => Mathf.Max(_currentGained - _totalSpent, 0);

		public void AddPet(int value)
		{
			if (_init)
			{
				_currentGained += value;
				_currentAchievement += value;
				UpdateStats();
			}
		}

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			_t = CallResult<UserStatsReceived_t>.Create(OnUserStatsReceived);
			SteamAPICall_t hAPICall = SteamUserStats.RequestUserStats(SteamUser.GetSteamID());
			_t.Set(hAPICall);
			yield return new WaitUntil(() => StatsInitialized);
			SteamUserStats.GetStat("BongoTap", out _currentGained);
			SteamUserStats.GetStat("BongoBeat", out _currentAchievement);
			SteamUserStats.GetStat("BongoMinus", out _totalSpent);
			if (_currentGained < _totalSpent)
			{
				_currentGained = _totalSpent;
			}
			UpdateText();
			_init = true;
			Debug.Log("Pets initialized.");
			while (true)
			{
				yield return new WaitForSeconds(60f);
				if (SteamManager.ShuttingDown)
				{
					break;
				}
				SteamUserStats.StoreStats();
			}
		}

		private void OnUserStatsReceived(UserStatsReceived_t userStatsReceivedT, bool bIOFailure)
		{
			if (bIOFailure || userStatsReceivedT.m_eResult != EResult.k_EResultOK)
			{
				Debug.LogWarning("Retrying fetching user stats..");
			}
			else
			{
				StatsInitialized = true;
			}
		}

		private void UpdateStats()
		{
			if (!SteamManager.ShuttingDown)
			{
				SteamUserStats.SetStat("BongoTap", _currentGained);
				SteamUserStats.SetStat("BongoBeat", _currentAchievement);
				SteamUserStats.SetStat("BongoMinus", _totalSpent);
				UpdateText();
			}
		}

		private void UpdateText()
		{
			NumberFormatInfo numberFormatInfo = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
			numberFormatInfo.NumberGroupSeparator = " ";
			_petsText.text = Current.ToString("#,0", numberFormatInfo);
			_lifetimePetsText.text = _currentAchievement.ToString("#,0", numberFormatInfo);
		}

		public bool CanSpendPets(int value)
		{
			return Current >= value;
		}

		public bool TrySpendPets(int value)
		{
			if (!_init)
			{
				return false;
			}
			if (CanSpendPets(value))
			{
				_totalSpent += value;
				UpdateStats();
				return true;
			}
			return false;
		}

		public void ResetPets()
		{
			if (_init)
			{
				_totalSpent = _currentGained;
				UpdateStats();
			}
		}
	}
}

using System;
using System.Collections;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace Steam
{
	public class SteamAnalytics : MonoBehaviour
	{
		private CallResult<GlobalStatsReceived_t> _t;

		private CallResult<UserStatsReceived_t> _t2;

		public bool Initialized;

		public static SteamAnalytics Instance;

		private const string MAU_KEY = "MAU";

		public const string PREM_KEY = "PREM";

		public const string PREM_AMT_KEY = "PREM_AMT";

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			_t = CallResult<GlobalStatsReceived_t>.Create(OnGlobalStatsReceived);
			_t2 = CallResult<UserStatsReceived_t>.Create(OnUserStatsReceived);
			SteamAPICall_t hAPICall = SteamUserStats.RequestUserStats(SteamUser.GetSteamID());
			_t2.Set(hAPICall);
			yield return new WaitUntil(() => Initialized);
			SteamUserStats.SetStat(string.Format("{0}_{1}_{2}", "MAU", DateTime.UtcNow.Year, DateTime.UtcNow.Month), 1);
			SteamUserStats.StoreStats();
		}

		private void OnUserStatsReceived(UserStatsReceived_t userStatsReceivedT, bool bIOFailure)
		{
			if (bIOFailure || userStatsReceivedT.m_eResult != EResult.k_EResultOK)
			{
				Debug.LogWarning("Retrying fetching user stats..");
				SteamAPICall_t hAPICall = SteamUserStats.RequestUserStats(SteamUser.GetSteamID());
				_t2.Set(hAPICall);
			}
			else if (!Initialized)
			{
				SteamAPICall_t hAPICall2 = SteamUserStats.RequestGlobalStats(1);
				_t.Set(hAPICall2);
			}
		}

		private void OnGlobalStatsReceived(GlobalStatsReceived_t param, bool biofailure)
		{
			if (!(param.m_eResult != EResult.k_EResultOK || biofailure))
			{
				Initialized = true;
			}
		}

		public void SetStatToOne(string stat)
		{
			SteamUserStats.SetStat(stat, 1);
			SteamUserStats.StoreStats();
		}
	}
}

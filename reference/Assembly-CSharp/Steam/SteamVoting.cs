using System;
using System.Collections;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Steam
{
	public class SteamVoting : MonoBehaviour
	{
		[SerializeField]
		private GameObject _votingParentObject;

		[SerializeField]
		private Slider _slider;

		[SerializeField]
		private TMP_Text _percentageTextYes;

		[SerializeField]
		private TMP_Text _percentageTextNo;

		[SerializeField]
		private TMP_Text _totalVotes;

		[SerializeField]
		private GameObject _votingResultsObject;

		[SerializeField]
		private GameObject _votingObject;

		private int _myVote;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SteamAnalytics.Instance.Initialized);
			SteamUserStats.GetStat("VOTING_TEST_YES", out var pData);
			SteamUserStats.GetStat("VOTING_TEST_NO", out var pData2);
			if (pData != 0 || pData2 != 0)
			{
				UpdateBar();
			}
			else
			{
				_votingParentObject.SetActive(value: true);
			}
		}

		public void VoteYes()
		{
			_myVote = 1;
			SteamUserStats.SetStat("VOTING_TEST_YES", 1);
			SteamUserStats.StoreStats();
			UpdateBar();
		}

		public void VoteNo()
		{
			_myVote = -1;
			SteamUserStats.SetStat("VOTING_TEST_NO", 1);
			SteamUserStats.StoreStats();
			UpdateBar();
		}

		private void UpdateBar()
		{
			_votingObject.SetActive(value: false);
			_votingResultsObject.SetActive(value: true);
			SteamUserStats.GetGlobalStat("VOTING_TEST_YES", out var pData);
			SteamUserStats.GetGlobalStat("VOTING_TEST_NO", out var pData2);
			switch (_myVote)
			{
			case 1:
				pData++;
				break;
			case -1:
				pData2++;
				break;
			}
			long num = pData + pData2;
			if (num == 0L)
			{
				_slider.value = 0.5f;
				_percentageTextYes.text = "0%";
				_percentageTextNo.text = "0%";
				_totalVotes.text = "0";
			}
			else
			{
				float num2 = (float)pData / (float)num;
				_slider.value = num2;
				_percentageTextYes.text = $"{Math.Round(num2 * 100f)}%";
				_percentageTextNo.text = $"{Math.Round((1f - num2) * 100f)}%";
				_totalVotes.text = num.ToString();
			}
		}
	}
}

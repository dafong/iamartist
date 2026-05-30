using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BongoCat;
using BongoCat.SteamJsonParser;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace Steam
{
	public class SteamGroupHelper : MonoBehaviour
	{
		[Serializable]
		public struct GameFollowReward
		{
			public ulong GameGroupId;

			public List<SteamItemUnity> RewardItems;
		}

		[SerializeField]
		private List<GameFollowReward> _gameFollowRewards;

		private Dictionary<ulong, List<SteamItemUnity>> _gameFollowRewardsDict = new Dictionary<ulong, List<SteamItemUnity>>();

		private Dictionary<ulong, Coroutine> _runningRoutines = new Dictionary<ulong, Coroutine>();

		public static SteamGroupHelper Instance;

		private void Awake()
		{
			Instance = this;
			foreach (GameFollowReward gameFollowReward in _gameFollowRewards)
			{
				_gameFollowRewardsDict.Add(gameFollowReward.GameGroupId, gameFollowReward.RewardItems);
			}
		}

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			yield return new WaitUntil(() => PromoItemChecker.Instance.IsInitialized);
			float num = 0f;
			foreach (GameFollowReward gameFollowReward in _gameFollowRewards)
			{
				if (!IsFollowingGame(gameFollowReward.GameGroupId))
				{
					continue;
				}
				foreach (SteamItemUnity rewardItem in gameFollowReward.RewardItems)
				{
					PromoItemChecker.Instance.GrantPromoItemDelayed(rewardItem.Id, num++);
				}
			}
		}

		public void ClickedFollowButton(ulong gameId)
		{
			if (!_runningRoutines.ContainsKey(gameId) && _gameFollowRewardsDict.ContainsKey(gameId))
			{
				_runningRoutines[gameId] = StartCoroutine(FollowChecker(gameId));
			}
		}

		private IEnumerator FollowChecker(ulong gameId)
		{
			List<SteamItemUnity> items = _gameFollowRewardsDict[gameId];
			while (true)
			{
				if (SteamManager.ShuttingDown)
				{
					yield break;
				}
				if (items.All((SteamItemUnity item) => CatInventory.Instance.Items.Any((SteamItem i) => i.SteamItemDefId == item.Id && i.ItemAmount > 0)))
				{
					break;
				}
				if (IsFollowingGame(gameId))
				{
					float num = 0f;
					foreach (SteamItemUnity item in items)
					{
						PromoItemChecker.Instance.GrantPromoItemDelayed(item.Id, num++);
					}
				}
				yield return new WaitForSeconds(60f);
			}
			_runningRoutines.Remove(gameId);
		}

		public bool IsFollowingGame(ulong gameGroupId)
		{
			return SteamFriends.IsUserInSource(SteamUser.GetSteamID(), new CSteamID(gameGroupId));
		}
	}
}

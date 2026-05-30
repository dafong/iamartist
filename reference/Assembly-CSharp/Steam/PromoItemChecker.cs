using System;
using System.Collections;
using System.Collections.Generic;
using BongoCat;
using BongoCat.Multiplayer;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace Steam
{
	public class PromoItemChecker : MonoBehaviour
	{
		public static PromoItemChecker Instance;

		private bool _grantedDiscordItem;

		private Callback<SteamInventoryResultReady_t> _t;

		private SteamInventoryResult_t _resultHandle;

		public Action<SteamItem> OnItemUpdated;

		[SerializeField]
		private NewItemPopup _newItemPopup;

		[SerializeField]
		private GameObject _discordChest;

		[SerializeField]
		private GameObject _followChest;

		public const int FOLLOW_ITEM_ID = 335;

		public const int DISCORD_ITEM_ID = 336;

		public bool IsInitialized;

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			_resultHandle = SteamInventoryResult_t.Invalid;
			_t = Callback<SteamInventoryResultReady_t>.Create(InventoryResultReady);
			yield return new WaitUntil(() => CatInventory.Instance.WasLoadedFromSteam);
			IsInitialized = true;
			Debug.Log("PromoItemChecker | IsInitialized: true");
			yield return new WaitUntil(() => CatInventory.Instance.Items.Count > 0);
			if (CatInventory.Instance.Items.Exists((SteamItem item) => item.ItemAmount > 0 && item.SteamItemDefId == 336))
			{
				_grantedDiscordItem = true;
				_discordChest.SetActive(value: false);
			}
			else
			{
				_grantedDiscordItem = false;
				_discordChest.SetActive(value: true);
			}
			if (CatInventory.Instance.Items.Exists((SteamItem item) => item.ItemAmount > 0 && item.SteamItemDefId == 335))
			{
				_followChest.SetActive(value: false);
			}
			else
			{
				_followChest.SetActive(value: true);
			}
			StartCoroutine(GrantPromoItemsRoutine());
			StartCoroutine(CheckForCollabTokensRoutine());
		}

		private IEnumerator GrantPromoItemsRoutine()
		{
			WaitForSeconds waitForTwoMinutes = new WaitForSeconds(900f);
			while (true)
			{
				if (_resultHandle != SteamInventoryResult_t.Invalid)
				{
					CleanupHandle();
				}
				SteamInventory.GrantPromoItems(out _resultHandle);
				yield return waitForTwoMinutes;
			}
		}

		private void OnDisable()
		{
			CleanupHandle();
		}

		private void CleanupHandle()
		{
			if (SteamManager.s_EverInitialized && _resultHandle != SteamInventoryResult_t.Invalid)
			{
				SteamInventory.DestroyResult(_resultHandle);
				_resultHandle = SteamInventoryResult_t.Invalid;
			}
		}

		public void GrantDiscordItem()
		{
			if (!_grantedDiscordItem)
			{
				_grantedDiscordItem = true;
				StartCoroutine(AddPromoItemRoutine(336, 3f));
			}
		}

		public void GrantPromoItem(int itemId)
		{
			StartCoroutine(AddPromoItemRoutine(itemId));
		}

		public void GrantPromoItemDelayed(int itemId, float delay)
		{
			StartCoroutine(AddPromoItemRoutine(itemId, delay));
		}

		private IEnumerator AddPromoItemRoutine(int itemId, float delay = 0f)
		{
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			yield return new WaitForSeconds(delay);
			TriggerItemDrop(itemId, delegate(SteamItem item)
			{
				if (item == null)
				{
					Debug.Log("PromoItemChecker | Promo Item null this should not happen");
				}
				else
				{
					SteamMultiplayer.Instance.SendReceivedItem(item.SteamItemDefId);
				}
			});
		}

		private void InventoryResultReady(SteamInventoryResultReady_t result)
		{
			if (_resultHandle == SteamInventoryResult_t.Invalid)
			{
				return;
			}
			if (_resultHandle != result.m_handle)
			{
				Debug.Log($"PromoItemChecker | skipped result handle {result.m_handle.m_SteamInventoryResult} did not match my {_resultHandle.m_SteamInventoryResult}");
				return;
			}
			EResult resultStatus = SteamInventory.GetResultStatus(result.m_handle);
			Debug.Log("PromoItemChecker | " + _resultHandle.m_SteamInventoryResult + " " + resultStatus.ToString("G"));
			if (resultStatus != EResult.k_EResultOK)
			{
				CleanupHandle();
				return;
			}
			uint punOutItemsArraySize = 0u;
			if (!SteamInventory.GetResultItems(_resultHandle, null, ref punOutItemsArraySize) || punOutItemsArraySize == 0)
			{
				Debug.Log("PromoItemChecker | Array size <= 0: " + punOutItemsArraySize);
				OnItemUpdated?.Invoke(null);
				CleanupHandle();
			}
			else
			{
				SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
				SteamInventory.GetResultItems(_resultHandle, array, ref punOutItemsArraySize);
				StartCoroutine(HandleItems(array));
			}
		}

		private IEnumerator HandleItems(SteamItemDetails_t[] steamItemDetails)
		{
			WaitForSeconds waitForSeconds = new WaitForSeconds(3f);
			for (int i = 0; i < steamItemDetails.Length; i++)
			{
				if (SteamManager.ShuttingDown)
				{
					yield break;
				}
				if (!(steamItemDetails[i].m_itemId == SteamItemInstanceID_t.Invalid) && steamItemDetails[i].m_iDefinition.m_SteamItemDef != 0)
				{
					bool isNew;
					SteamItem steamItem = CatInventory.Instance.HandleItem(steamItemDetails[i], CatInventory.GetLastModifiedDateFromItem(_resultHandle, 0u, steamItemDetails[i]), out isNew);
					OnItemUpdated?.Invoke(steamItem);
					if (steamItem != null)
					{
						_newItemPopup?.ShowPopup(steamItem, steamItem.InstanceIds != null && steamItem.InstanceIds.Count > 1);
						yield return waitForSeconds;
					}
				}
			}
			CatInventory.Instance.UpdateVisuals();
			CleanupHandle();
		}

		private void TriggerItemDrop(int itemId, Action<SteamItem> callback)
		{
			OnItemUpdated = (Action<SteamItem>)Delegate.Combine(OnItemUpdated, new Action<SteamItem>(Call));
			if (!SteamManager.ShuttingDown)
			{
				if (_resultHandle != SteamInventoryResult_t.Invalid)
				{
					CleanupHandle();
				}
				SteamInventory.AddPromoItem(out _resultHandle, new SteamItemDef_t(itemId));
			}
			void Call(SteamItem item)
			{
				if (item != null)
				{
					Debug.Log("PromoItemChecker | Item dropped " + item.SteamItemDefId + " " + item.ItemName);
					CatInventory.Instance.RestartRoutine();
				}
				else
				{
					Debug.Log("PromoItemChecker | Item null");
				}
				callback?.Invoke(item);
				OnItemUpdated = (Action<SteamItem>)Delegate.Remove(OnItemUpdated, new Action<SteamItem>(Call));
			}
		}

		private IEnumerator CheckForCollabTokensRoutine()
		{
			List<SteamItemDef_t> promoItems = new List<SteamItemDef_t>();
			if (CatInventory.Instance.AchievementToken.m_iDefinition.m_SteamItemDef != 97)
			{
				promoItems.Add(new SteamItemDef_t(97));
			}
			if (CatInventory.Instance.ManualToken.m_iDefinition.m_SteamItemDef != 96)
			{
				promoItems.Add(new SteamItemDef_t(96));
			}
			foreach (string collabName in SteamItemIdReference.Instance.GetCollabNames())
			{
				int collabTokenIdFromTag = SteamItemIdReference.Instance.GetCollabTokenIdFromTag(collabName);
				if (CatInventory.Instance.GetCollabToken(collabName).m_iDefinition.m_SteamItemDef != collabTokenIdFromTag)
				{
					promoItems.Add(new SteamItemDef_t(collabTokenIdFromTag));
				}
			}
			if (promoItems.Count == 0)
			{
				yield break;
			}
			promoItems.AddRange(SteamItemIdReference.Instance.GetPromoTokens());
			WaitForSeconds waitForTwoMinutes = new WaitForSeconds(900f);
			while (!SteamManager.ShuttingDown)
			{
				if (_resultHandle != SteamInventoryResult_t.Invalid)
				{
					CleanupHandle();
				}
				SteamInventory.AddPromoItems(out _resultHandle, promoItems.ToArray(), (uint)promoItems.Count);
				yield return waitForTwoMinutes;
			}
		}
	}
}

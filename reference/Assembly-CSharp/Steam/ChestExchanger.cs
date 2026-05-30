using System;
using System.Collections;
using BongoCat;
using BongoCat.Achievements;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace Steam
{
	public class ChestExchanger : MonoBehaviour
	{
		private Callback<SteamInventoryResultReady_t> _t;

		private SteamInventoryResult_t _resultHandle;

		private Action<(SteamItem, bool)> _onItemUpdated;

		public static bool IsExchanging;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			_resultHandle = SteamInventoryResult_t.Invalid;
			_t = Callback<SteamInventoryResultReady_t>.Create(InventoryResultReady);
		}

		private void InventoryResultReady(SteamInventoryResultReady_t result)
		{
			if (_resultHandle == SteamInventoryResult_t.Invalid)
			{
				return;
			}
			if (_resultHandle != result.m_handle)
			{
				Debug.Log($"ChestExchanger | Skipped result handle {result.m_handle.m_SteamInventoryResult} did not match my {_resultHandle.m_SteamInventoryResult}");
				return;
			}
			IsExchanging = false;
			EResult resultStatus = SteamInventory.GetResultStatus(result.m_handle);
			Debug.Log("ChestExchanger | " + _resultHandle.m_SteamInventoryResult + " " + resultStatus.ToString("G"));
			if (resultStatus != EResult.k_EResultOK)
			{
				Debug.Log($"ChestExchanger | Result status is {resultStatus}");
				_onItemUpdated?.Invoke((null, false));
				CleanupHandle();
				return;
			}
			uint punOutItemsArraySize = 0u;
			if (!SteamInventory.GetResultItems(_resultHandle, null, ref punOutItemsArraySize) || punOutItemsArraySize == 0)
			{
				Debug.Log("ChestExchanger | Array size <= 0: " + punOutItemsArraySize);
				_onItemUpdated?.Invoke((null, false));
				CatInventory.Instance.UpdateVisuals();
				CleanupHandle();
				return;
			}
			SteamItemDetails_t[] array = new SteamItemDetails_t[punOutItemsArraySize];
			SteamInventory.GetResultItems(_resultHandle, array, ref punOutItemsArraySize);
			for (uint num = 3u; num < array.Length; num++)
			{
				bool isNew;
				SteamItem steamItem = CatInventory.Instance.HandleItem(array[num], CatInventory.GetLastModifiedDateFromItem(_resultHandle, num, array[num]), out isNew);
				if ((steamItem != null && steamItem.ItemSlot == "consumable") || array[num].m_itemId == CatInventory.Instance.AchievementToken.m_itemId || array[num].m_itemId == CatInventory.Instance.ManualToken.m_itemId || array[num].m_itemId == CatInventory.Instance.ChestToken.m_itemId || array[num].m_itemId == CatInventory.Instance.EmoteChestToken.m_itemId)
				{
					continue;
				}
				AchievementStats.IncrementItems();
				string collabChestTagFromId = SteamItemIdReference.Instance.GetCollabChestTagFromId(array[num].m_iDefinition.m_SteamItemDef);
				if (!string.IsNullOrEmpty(collabChestTagFromId))
				{
					if (CatInventory.Instance.GetCollabToken(collabChestTagFromId).m_iDefinition.m_SteamItemDef == SteamItemIdReference.Instance.GetCollabTokenIdFromTag(collabChestTagFromId))
					{
						CleanupHandle();
						OpenCollabChest(collabChestTagFromId, array[num]);
					}
					else
					{
						CleanupHandle();
						OpenFallbackChest(collabChestTagFromId, array[num]);
					}
					return;
				}
				_onItemUpdated?.Invoke((steamItem, isNew));
			}
			CatInventory.Instance.UpdateVisuals();
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

		private void OnDisable()
		{
			CleanupHandle();
		}

		public void OpenChest(Action<(SteamItem, bool)> callback)
		{
			if (!SteamManager.ShuttingDown)
			{
				_onItemUpdated = (Action<(SteamItem, bool)>)Delegate.Combine(_onItemUpdated, new Action<(SteamItem, bool)>(Call));
				CleanupHandle();
				StartCoroutine(ExchangeWhenReady(delegate
				{
					SteamInventory.ExchangeItems(out _resultHandle, new SteamItemDef_t[1]
					{
						new SteamItemDef_t(SteamItemIdReference.Instance.ChestExchange)
					}, new uint[1] { 1u }, 1u, new SteamItemInstanceID_t[3]
					{
						CatInventory.Instance.ChestToken.m_itemId,
						CatInventory.Instance.AchievementToken.m_itemId,
						CatInventory.Instance.ManualToken.m_itemId
					}, new uint[3] { 1u, 1u, 1u }, 3u);
				}));
			}
			void Call((SteamItem, bool) tuple)
			{
				if (tuple.Item1 != null)
				{
					Debug.Log("ChestExchanger | Item dropped: " + tuple.Item1.SteamItemDefId + " " + tuple.Item1.ItemName);
					CatInventory.Instance.RestartRoutine();
				}
				else
				{
					Debug.Log("ChestExchanger | Item null");
				}
				CatCosmetics.Instance.validated = false;
				callback?.Invoke(tuple);
				_onItemUpdated = (Action<(SteamItem, bool)>)Delegate.Remove(_onItemUpdated, new Action<(SteamItem, bool)>(Call));
			}
		}

		private IEnumerator ExchangeWhenReady(Action exchangeAction)
		{
			yield return new WaitUntil(() => !IsExchanging && CatInventory.Instance.AchievementToken.m_unQuantity > 0 && CatInventory.Instance.ManualToken.m_unQuantity > 0);
			IsExchanging = true;
			exchangeAction?.Invoke();
		}

		public void OpenEmoteChest(Action<(SteamItem, bool)> callback)
		{
			if (!SteamManager.ShuttingDown)
			{
				_onItemUpdated = (Action<(SteamItem, bool)>)Delegate.Combine(_onItemUpdated, new Action<(SteamItem, bool)>(Call));
				CleanupHandle();
				StartCoroutine(ExchangeWhenReady(delegate
				{
					SteamInventory.ExchangeItems(out _resultHandle, new SteamItemDef_t[1]
					{
						new SteamItemDef_t(SteamItemIdReference.Instance.EmoteChestExchange)
					}, new uint[1] { 1u }, 1u, new SteamItemInstanceID_t[3]
					{
						CatInventory.Instance.EmoteChestToken.m_itemId,
						CatInventory.Instance.AchievementToken.m_itemId,
						CatInventory.Instance.ManualToken.m_itemId
					}, new uint[3] { 1u, 1u, 1u }, 3u);
				}));
			}
			void Call((SteamItem, bool) tuple)
			{
				if (tuple.Item1 != null)
				{
					Debug.Log("EmoteChestExchanger | Item dropped: " + tuple.Item1.SteamItemDefId + " " + tuple.Item1.ItemName);
					CatInventory.Instance.RestartRoutine();
				}
				else
				{
					Debug.Log("EmoteChestExchanger | Item null");
				}
				callback?.Invoke(tuple);
				_onItemUpdated = (Action<(SteamItem, bool)>)Delegate.Remove(_onItemUpdated, new Action<(SteamItem, bool)>(Call));
			}
		}

		private void OpenCollabChest(string collabKey, SteamItemDetails_t chest)
		{
			if (!SteamManager.ShuttingDown)
			{
				int exchangeItemId = SteamItemIdReference.Instance.GetCollabExchangeFromTag(collabKey);
				SteamItemInstanceID_t collabToken = CatInventory.Instance.GetCollabToken(collabKey).m_itemId;
				CleanupHandle();
				StartCoroutine(ExchangeWhenReady(delegate
				{
					SteamInventory.ExchangeItems(out _resultHandle, new SteamItemDef_t[1]
					{
						new SteamItemDef_t(exchangeItemId)
					}, new uint[1] { 1u }, 1u, new SteamItemInstanceID_t[2] { collabToken, chest.m_itemId }, new uint[2] { 1u, 1u }, 2u);
				}));
			}
		}

		private void OpenFallbackChest(string collabKey, SteamItemDetails_t chest)
		{
			if (!SteamManager.ShuttingDown)
			{
				int exchangeItemId = SteamItemIdReference.Instance.GetCollabFallbackExchangeFromTag(collabKey);
				CleanupHandle();
				StartCoroutine(ExchangeWhenReady(delegate
				{
					SteamInventory.ExchangeItems(out _resultHandle, new SteamItemDef_t[1]
					{
						new SteamItemDef_t(exchangeItemId)
					}, new uint[1] { 1u }, 1u, new SteamItemInstanceID_t[1] { chest.m_itemId }, new uint[1] { 1u }, 1u);
				}));
			}
		}
	}
}

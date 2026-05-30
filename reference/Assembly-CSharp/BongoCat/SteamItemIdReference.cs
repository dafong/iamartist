using System;
using System.Collections.Generic;
using System.Linq;
using BongoCat.SteamJsonParser;
using Steamworks;
using UnityEngine;

namespace BongoCat
{
	public class SteamItemIdReference : ScriptableObject
	{
		[Serializable]
		public struct CollabReference
		{
			public string Collab;

			public int Id;
		}

		public const int ACHIEVEMENT_TOKEN_ID = 97;

		public const int CHEST_TOKEN_ID = 98;

		public const int EMOTE_CHEST_TOKEN_ID = 90;

		public const int MANUAL_TOKEN_ID = 96;

		public List<int> Ids;

		public List<SteamItemUnity> AllItems;

		public static SteamItemIdReference Instance;

		public int ChestExchange = 20;

		public int ChestGenerator = 10;

		public int EmoteChestExchange;

		public List<CollabReference> CollabTokenReferences;

		public List<CollabReference> CollabChestReferences;

		public List<CollabReference> CollabExchangeReferences;

		public List<CollabReference> CollabFallbackExchangeReferences;

		private Dictionary<string, int> _collabTokenTagToIdDict;

		private Dictionary<int, string> _collabTokenIdToTagDict;

		private Dictionary<int, string> _collabChestIdToTagDict;

		private Dictionary<string, int> _collabExchangeDict;

		private Dictionary<string, int> _collabFallbackExchangeDict;

		public List<int> OtherTokens;

		public int GetCollabTokenIdFromTag(string tag)
		{
			if (_collabTokenTagToIdDict == null)
			{
				_collabTokenTagToIdDict = new Dictionary<string, int>();
				foreach (CollabReference collabTokenReference in CollabTokenReferences)
				{
					_collabTokenTagToIdDict.Add(collabTokenReference.Collab, collabTokenReference.Id);
				}
			}
			_collabTokenTagToIdDict.TryGetValue(tag, out var value);
			return value;
		}

		public string GetCollabTokenTagFromId(int id)
		{
			if (_collabTokenIdToTagDict == null)
			{
				_collabTokenIdToTagDict = new Dictionary<int, string>();
				foreach (CollabReference collabTokenReference in CollabTokenReferences)
				{
					_collabTokenIdToTagDict.Add(collabTokenReference.Id, collabTokenReference.Collab);
				}
			}
			_collabTokenIdToTagDict.TryGetValue(id, out var value);
			return value;
		}

		public string GetCollabChestTagFromId(int id)
		{
			if (_collabChestIdToTagDict == null)
			{
				_collabChestIdToTagDict = new Dictionary<int, string>();
				foreach (CollabReference collabChestReference in CollabChestReferences)
				{
					_collabChestIdToTagDict.Add(collabChestReference.Id, collabChestReference.Collab);
				}
			}
			_collabChestIdToTagDict.TryGetValue(id, out var value);
			return value;
		}

		public int GetCollabExchangeFromTag(string tag)
		{
			if (_collabExchangeDict == null)
			{
				_collabExchangeDict = new Dictionary<string, int>();
				foreach (CollabReference collabExchangeReference in CollabExchangeReferences)
				{
					_collabExchangeDict.Add(collabExchangeReference.Collab, collabExchangeReference.Id);
				}
			}
			_collabExchangeDict.TryGetValue(tag, out var value);
			return value;
		}

		public int GetCollabFallbackExchangeFromTag(string tag)
		{
			if (_collabFallbackExchangeDict == null)
			{
				_collabFallbackExchangeDict = new Dictionary<string, int>();
				foreach (CollabReference collabFallbackExchangeReference in CollabFallbackExchangeReferences)
				{
					_collabFallbackExchangeDict.Add(collabFallbackExchangeReference.Collab, collabFallbackExchangeReference.Id);
				}
			}
			_collabFallbackExchangeDict.TryGetValue(tag, out var value);
			return value;
		}

		public List<SteamItemDef_t> GetPromoTokens()
		{
			List<SteamItemDef_t> list = new List<SteamItemDef_t>();
			foreach (CollabReference collabTokenReference in CollabTokenReferences)
			{
				list.Add(new SteamItemDef_t(collabTokenReference.Id));
			}
			return list;
		}

		public List<string> GetCollabNames()
		{
			return CollabTokenReferences.Select((CollabReference token) => token.Collab).ToList();
		}

		public bool IsOtherToken(int tokenId)
		{
			return OtherTokens.Contains(tokenId);
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BongoCat
{
	public class InventoryItemMarketLinks : ScriptableObject
	{
		[Serializable]
		private struct MarketLinkData
		{
			public string Name;

			public List<int> Ids;

			public string Link;
		}

		[SerializeField]
		private List<MarketLinkData> _marketLinkData;

		private Dictionary<int, string> _linkDict;

		public bool HasCustomLink(SteamItem steamItem)
		{
			InitDict();
			return _linkDict.ContainsKey(steamItem.SteamItemDefId);
		}

		public string GetMarketLink(SteamItem steamItem)
		{
			InitDict();
			if (_linkDict.TryGetValue(steamItem.SteamItemDefId, out var value))
			{
				return "steam://openurl/" + value;
			}
			return $"steam://openurl/https://steamcommunity.com/market/listings/{3419430}/{Uri.EscapeUriString(steamItem.ItemName)}";
		}

		private void InitDict()
		{
			if (_linkDict != null)
			{
				return;
			}
			_linkDict = new Dictionary<int, string>();
			foreach (MarketLinkData marketLinkDatum in _marketLinkData)
			{
				foreach (int id in marketLinkDatum.Ids)
				{
					_linkDict.Add(id, marketLinkDatum.Link);
				}
			}
		}
	}
}

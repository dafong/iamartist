using System.Collections.Generic;
using BongoCat.SteamJsonParser;
using UnityEngine;
using UnityEngine.Serialization;

namespace Steam.Exchanges
{
	public class ExchangeUnity : ScriptableObject
	{
		[FormerlySerializedAs("_id")]
		[SerializeField]
		public int Id;

		[SerializeField]
		private string _name;

		[FormerlySerializedAs("_inputAmount")]
		[SerializeField]
		public int InputAmount;

		[FormerlySerializedAs("_inputQuality")]
		[SerializeField]
		public QualityCategory InputQuality;

		public QualityCategory OutputQuality;

		public bool IsEmoteExchange;

		public SteamItemBackend ToSteamBackendItem()
		{
			Dictionary<QualityCategory, int> dictionary = new Dictionary<QualityCategory, int>
			{
				{
					QualityCategory.Common,
					11
				},
				{
					QualityCategory.Uncommon,
					12
				},
				{
					QualityCategory.Rare,
					13
				},
				{
					QualityCategory.Epic,
					14
				},
				{
					QualityCategory.Legendary,
					15
				}
			};
			Dictionary<QualityCategory, int> dictionary2 = new Dictionary<QualityCategory, int>
			{
				{
					QualityCategory.Common,
					25
				},
				{
					QualityCategory.Uncommon,
					26
				},
				{
					QualityCategory.Rare,
					27
				},
				{
					QualityCategory.Epic,
					28
				},
				{
					QualityCategory.Legendary,
					29
				}
			};
			int num = (IsEmoteExchange ? dictionary2[OutputQuality] : dictionary[OutputQuality]);
			string exchange = (IsEmoteExchange ? string.Format("emote_quality:{0}*{1}", InputQuality.ToString("G").ToLower(), InputAmount) : string.Format("cosmetics_quality:{0}*{1}", InputQuality.ToString("G").ToLower(), InputAmount));
			return new SteamItemBackend
			{
				itemdefid = Id,
				type = "generator",
				name = _name,
				bundle = $"{num}",
				tradable = false,
				marketable = false,
				description = "",
				store_tags = "",
				exchange = exchange,
				auto_stack = false
			};
		}
	}
}

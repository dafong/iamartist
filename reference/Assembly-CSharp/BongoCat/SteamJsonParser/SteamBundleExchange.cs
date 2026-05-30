using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BongoCat.SteamJsonParser
{
	public class SteamBundleExchange : ScriptableObject
	{
		public int Id;

		[SerializeField]
		private string _name;

		[FormerlySerializedAs("_exchangeItems")]
		[SerializeField]
		private List<SteamItemUnity> _input;

		[FormerlySerializedAs("_bundleItems")]
		[SerializeField]
		private List<SteamItemUnity> _output;

		public List<SteamItemUnity> Output => _output;

		public List<SteamItemUnity> Input => _input;

		public SteamItemBackend ToSteamItemBackend()
		{
			string text = "";
			if (_output.Count > 0)
			{
				text += _output[0].Id;
				for (int i = 1; i < _output.Count; i++)
				{
					text += $";{_output[i].Id}";
				}
			}
			string text2 = "";
			if (_input.Count > 0)
			{
				text2 += _input[0].Id;
				for (int j = 1; j < _input.Count; j++)
				{
					text2 += $",{_input[j].Id}";
				}
			}
			return new SteamItemBackend
			{
				itemdefid = Id,
				type = "bundle",
				bundle = text,
				name = _name,
				tradable = false,
				marketable = false,
				description = "",
				store_tags = "",
				exchange = text2,
				auto_stack = false
			};
		}
	}
}

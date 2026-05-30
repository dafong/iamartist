using System.Collections.Generic;
using System.Linq;
using BongoCat.SteamJsonParser;
using UnityEngine;

namespace BongoCat.Multiplayer
{
	public class SpecialLobbyCodeReferencer : ScriptableObject
	{
		public List<SpecialLobbyCode> SpecialLobbyCodes;

		public bool IsSpecialLobbyCode(string lobbyCode)
		{
			if (SpecialLobbyCodes.Count == 0)
			{
				return false;
			}
			string codeHash = Hash128.Compute(lobbyCode).ToString();
			return SpecialLobbyCodes.Any((SpecialLobbyCode code) => code != null && code.CodeHash == codeHash);
		}

		public List<int> GetItemIds(string lobbyCode)
		{
			if (SpecialLobbyCodes.Count == 0)
			{
				return new List<int>();
			}
			string codeHash = Hash128.Compute(lobbyCode).ToString();
			return (from item in SpecialLobbyCodes.Where((SpecialLobbyCode code) => code != null && code.CodeHash == codeHash).SelectMany((SpecialLobbyCode code) => code.Items)
				select item.Id).ToList();
		}
	}
}

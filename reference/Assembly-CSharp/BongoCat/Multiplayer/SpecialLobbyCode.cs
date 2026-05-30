using System.Collections.Generic;
using BongoCat.SteamJsonParser;
using UnityEngine;

namespace BongoCat.Multiplayer
{
	public class SpecialLobbyCode : ScriptableObject
	{
		public string CodeHash;

		public List<SteamItemUnity> Items;
	}
}

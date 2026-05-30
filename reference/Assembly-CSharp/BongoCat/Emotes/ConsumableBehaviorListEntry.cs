using System;
using BongoCat.SteamJsonParser;

namespace BongoCat.Emotes
{
	[Serializable]
	public struct ConsumableBehaviorListEntry
	{
		public SteamItemUnity Consumable;

		public EmoteBehaviorOverride Behavior;
	}
}

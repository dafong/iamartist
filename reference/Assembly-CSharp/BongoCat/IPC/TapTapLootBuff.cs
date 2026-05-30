using System;

namespace BongoCat.IPC
{
	[Serializable]
	public struct TapTapLootBuff
	{
		public string Name;

		public float Value;

		public TapTapLootBuff(string name, float val)
		{
			Name = name;
			Value = val;
		}
	}
}

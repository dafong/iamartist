using System;
using System.Collections;
using UnityEngine;

namespace BongoCat.OSSpecific
{
	public class Taskbar : MonoBehaviour
	{
		[NonSerialized]
		public int Height;

		[NonSerialized]
		public int Width;

		public static Taskbar Instance;

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			while (true)
			{
				Height = Screen.mainWindowDisplayInfo.height - Screen.mainWindowDisplayInfo.workArea.height;
				Width = Screen.mainWindowDisplayInfo.width;
				yield return null;
			}
		}
	}
}

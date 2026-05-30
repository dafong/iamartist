using System.Collections;
using CursorExtensions;
using UnityEngine;

namespace BongoCat.OSSpecific
{
	public class CursorVisible : MonoBehaviour
	{
		public static CursorVisible Instance;

		public bool IsVisible;

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			while (true)
			{
				IsVisible = WindowsCursorExtensions.IsVisible();
				yield return new WaitForSeconds(0.25f);
			}
		}
	}
}

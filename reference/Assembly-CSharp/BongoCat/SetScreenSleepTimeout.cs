using UnityEngine;

namespace BongoCat
{
	public class SetScreenSleepTimeout : MonoBehaviour
	{
		private void Start()
		{
			Screen.sleepTimeout = -2;
		}
	}
}

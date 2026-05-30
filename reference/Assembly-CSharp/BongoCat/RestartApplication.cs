using UnityEngine;

namespace BongoCat
{
	public class RestartApplication : MonoBehaviour
	{
		public void Restart()
		{
			Application.Quit();
			Application.OpenURL($"steam://run/{3419430}");
		}

		public static void RestartGameViaSteam()
		{
			Application.Quit();
			Application.OpenURL($"steam://run/{3419430}");
		}
	}
}

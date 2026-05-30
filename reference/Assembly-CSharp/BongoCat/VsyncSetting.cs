using UnityEngine;

namespace BongoCat
{
	public class VsyncSetting : MonoBehaviour
	{
		[SerializeField]
		private PlayerPrefsToggle _toggle;

		public void ToggleVSync(bool toggle)
		{
			Debug.Log($"VSync enabled: {_toggle.Value}");
			QualitySettings.vSyncCount = (toggle ? 1 : 0);
		}
	}
}

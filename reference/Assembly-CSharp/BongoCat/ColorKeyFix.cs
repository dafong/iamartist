using System.Collections;
using BongoCat.OSSpecific;
using UnityEngine;

namespace BongoCat
{
	public class ColorKeyFix : MonoBehaviour
	{
		[SerializeField]
		private PlayerPrefsToggle _toggle;

		[SerializeField]
		private PlayerPrefsToggle _controllerSupportToggle;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => TransparentWindow.Instance.Initialized);
			yield return null;
			TransparentWindow.Instance.SetTransparencyFix(_toggle.Value);
			Debug.Log($"ColorKeyFix enabled: {_toggle.Value}");
			_toggle.OnToggleUpdated.AddListener(OnToggleUpdated);
		}

		private void OnToggleUpdated(bool value)
		{
			TransparentWindow.Instance.SetTransparencyFix(value);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F3))
			{
				_controllerSupportToggle.Value = false;
				_toggle.Toggle();
			}
		}
	}
}

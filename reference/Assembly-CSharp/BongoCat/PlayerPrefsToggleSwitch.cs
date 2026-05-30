using UnityEngine;

namespace BongoCat
{
	public class PlayerPrefsToggleSwitch : MonoBehaviour
	{
		[SerializeField]
		private PlayerPrefsToggle _toggleA;

		[SerializeField]
		private PlayerPrefsToggle _toggleB;

		private void Start()
		{
			_toggleA.OnToggleUpdated.AddListener(OnToggleA);
			_toggleB.OnToggleUpdated.AddListener(OnToggleB);
			OnToggleA(_toggleA.Value);
			OnToggleB(_toggleB.Value);
		}

		private void OnToggleA(bool toggle)
		{
			if (toggle && _toggleB.Value)
			{
				_toggleB.Toggle();
			}
		}

		private void OnToggleB(bool toggle)
		{
			if (toggle && _toggleA.Value)
			{
				_toggleA.Toggle();
			}
		}
	}
}

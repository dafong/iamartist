using UnityEngine;

namespace BongoCat
{
	public class GamingMode : MonoBehaviour
	{
		public bool IsEnabled;

		[SerializeField]
		private ToggleGameObject _toggleGameObject;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F4))
			{
				IsEnabled = false;
				_toggleGameObject.SetActive(active: false);
			}
		}

		public void Toggle()
		{
			IsEnabled = !IsEnabled;
		}
	}
}

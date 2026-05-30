using UnityEngine;
using UnityEngine.UI;

namespace BongoCat
{
	public class OptionAvailability : MonoBehaviour
	{
		[SerializeField]
		private Button _optionButton;

		public void EnableOption()
		{
			_optionButton.interactable = true;
			base.gameObject.SetActive(value: false);
		}

		public void DisableOption()
		{
			_optionButton.interactable = false;
			base.gameObject.SetActive(value: true);
		}
	}
}

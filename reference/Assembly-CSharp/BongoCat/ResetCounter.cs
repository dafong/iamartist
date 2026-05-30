using UnityEngine;

namespace BongoCat
{
	public class ResetCounter : MonoBehaviour
	{
		[SerializeField]
		private ErrorMessage _warning;

		[SerializeField]
		private Pets _pets;

		public void ShowResetPrompt()
		{
			ErrorMessageHandler.Instance.SetErrorMessage(_warning, leftButtonAction: _pets.ResetPets, rightButtonAction: ErrorMessageHandler.Instance.CloseErrorPopup);
		}
	}
}

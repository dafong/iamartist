using BongoCat.Localizer;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BongoCat
{
	public class ErrorMessageHandler : MonoBehaviour
	{
		[SerializeField]
		private GameObject _errorPopup;

		[SerializeField]
		private Button _rightButton;

		[SerializeField]
		private Button _leftButton;

		[SerializeField]
		private GameObject _menu;

		[SerializeField]
		private LocalizedText _errorText;

		public static ErrorMessageHandler Instance;

		private void Awake()
		{
			Instance = this;
		}

		public void SetErrorMessage(ErrorMessage errorMessage, UnityAction rightButtonAction = null, UnityAction leftButtonAction = null)
		{
			_menu.SetActive(value: false);
			_errorPopup.SetActive(value: true);
			_errorText.UpdateKey(errorMessage.ErrorKey);
			_rightButton.gameObject.SetActive(errorMessage.UseRightButton);
			_leftButton.gameObject.SetActive(errorMessage.UseLeftButton);
			if (errorMessage.UseRightButton)
			{
				_rightButton.onClick.RemoveAllListeners();
				_rightButton.GetComponentInChildren<LocalizedText>().UpdateKey(errorMessage.RightButtonLocaKey);
				_rightButton.onClick.AddListener(rightButtonAction);
				_rightButton.onClick.AddListener(CloseErrorPopup);
			}
			if (errorMessage.UseLeftButton)
			{
				_leftButton.onClick.RemoveAllListeners();
				_leftButton.GetComponentInChildren<LocalizedText>().UpdateKey(errorMessage.LeftButtonLocaKey);
				_leftButton.onClick.AddListener(leftButtonAction);
				_leftButton.onClick.AddListener(CloseErrorPopup);
			}
		}

		public void CloseErrorPopup()
		{
			_errorPopup.SetActive(value: false);
		}
	}
}

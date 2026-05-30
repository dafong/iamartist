using UnityEngine;

namespace BongoCat
{
	public class ErrorMessage : ScriptableObject
	{
		[SerializeField]
		private bool _useLeftButton;

		[SerializeField]
		private string _leftButtonLocaKey;

		[SerializeField]
		private bool _useRightButton;

		[SerializeField]
		private string _rightButtonLocaKey;

		[SerializeField]
		private string _errorKey;

		public string ErrorKey => _errorKey;

		public bool UseLeftButton => _useLeftButton;

		public string LeftButtonLocaKey => _leftButtonLocaKey;

		public bool UseRightButton => _useRightButton;

		public string RightButtonLocaKey => _rightButtonLocaKey;
	}
}

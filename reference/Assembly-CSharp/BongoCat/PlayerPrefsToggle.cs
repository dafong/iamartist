using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace BongoCat
{
	public class PlayerPrefsToggle : MonoBehaviour
	{
		[SerializeField]
		private string _playerPrefsKey;

		[SerializeField]
		private ToggleGameObject _toggle;

		[SerializeField]
		private Image _image;

		[SerializeField]
		private bool _invert;

		[SerializeField]
		private bool _defaultValue;

		private bool _value;

		public UnityEvent<bool> OnToggleUpdated;

		public bool Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
				PlayerPrefs.SetInt(_playerPrefsKey, _value ? 1 : 0);
				PlayerPrefs.Save();
				SetUI();
				OnToggleUpdated?.Invoke(_value);
			}
		}

		private void Awake()
		{
			_value = PlayerPrefs.GetInt(_playerPrefsKey, _defaultValue ? 1 : 0) == 1;
			SetUI();
		}

		private void Start()
		{
			OnToggleUpdated?.Invoke(_value);
		}

		private void SetUI()
		{
			bool flag = (_invert ? (!_value) : _value);
			if ((bool)_toggle)
			{
				_toggle.SetActive(flag);
			}
			if ((bool)_image)
			{
				Color color = _image.color;
				color.a = (flag ? 1f : 0.3f);
				_image.color = color;
			}
		}

		public void Toggle()
		{
			Value = !_value;
		}

		public void ToggleOn()
		{
			Value = true;
		}

		public void ToggleOff()
		{
			Value = false;
		}
	}
}

using AutoStart;
using BongoCat.Localizer;
using UnityEngine;

namespace BongoCat
{
	public class SetAutoStart : MonoBehaviour
	{
		[SerializeField]
		private ToggleGameObject _toggle;

		[SerializeField]
		private OptionAvailability _optionAvailability;

		[SerializeField]
		private LocalizedText _tooltipText;

		private IAutoStart _autoStart;

		private void Awake()
		{
			_autoStart = new WinAutoStart();
		}

		private void Start()
		{
			((WinAutoStart)_autoStart).Init();
			bool flag = _autoStart.IsEnabled();
			_toggle.SetActive(flag);
			Debug.Log($"AutoStart enabled: {flag}");
		}

		public void ToggleAutoStart()
		{
			EnableAutoStart(!_autoStart.IsEnabled());
		}

		public void FullyDisable()
		{
			_tooltipText.UpdateKey("AutostartTooltipDisabled");
			_optionAvailability.DisableOption();
			_toggle.SetActive(active: false);
			EnableAutoStart(enable: false);
		}

		public void EnableOption()
		{
			_tooltipText.UpdateKey("AutostartTooltip");
			_optionAvailability.EnableOption();
		}

		private void EnableAutoStart(bool enable)
		{
			if (enable)
			{
				if (!Application.isEditor)
				{
					_autoStart.EnableAutoStart();
				}
			}
			else
			{
				_autoStart.DisableAutoStart();
			}
		}
	}
}

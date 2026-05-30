using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Principal;
using BongoCat.Localizer;
using Microsoft.Win32;
using UnityEngine;

namespace BongoCat
{
	public class SetRunAsAdmin : MonoBehaviour
	{
		[SerializeField]
		private ToggleGameObject _toggle;

		[SerializeField]
		private SetAutoStart _autoStart;

		[SerializeField]
		private OptionAvailability _option;

		[SerializeField]
		private LocalizedText _tooltipText;

		private RegistryKey _rk;

		private bool _isAdminUser;

		private List<string> _adminIdentifiers = new List<string>
		{
			"BUILTIN\\Administrators", "BUILTIN\\Administrateurs", "BUILTIN\\Administratoren", "VORDEFINIERT\\Administratoren", "BUILTIN\\Администраторы", "BUILTIN\\Administradores", "BUILTIN\\Administratorzy", "BUILTIN\\Administratorer", "BUILTIN\\Administratörer", "INGEBOUWD\\Administrators",
			"BUILTIN\\Rendszergazdák", "BUILTIN\\Järjestelmänvalvojat"
		};

		private string _key => AppDomain.CurrentDomain.BaseDirectory + "\\" + Global.Exe;

		private void Awake()
		{
			_isAdminUser = IsAdmin();
			MonoBehaviour.print($"[Admin Mode] User is admin group: {_isAdminUser}");
		}

		private void OnEnable()
		{
			if (!_isAdminUser)
			{
				_option.DisableOption();
				_tooltipText.UpdateKey("AdminModeTooltipDisabled");
			}
			_rk = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\AppCompatFlags\\Layers", writable: true);
			if (!_isAdminUser)
			{
				_rk.SetValue(_key, "~ DISABLEDXMAXIMIZEDWINDOWEDMODE");
			}
			string text = (string)_rk.GetValue(_key);
			bool flag = text?.Contains("RUNASADMIN") ?? false;
			if (text == null)
			{
				_rk.SetValue(_key, "~ DISABLEDXMAXIMIZEDWINDOWEDMODE");
				Application.OpenURL($"steam://run/{3419430}");
				Application.Quit();
				return;
			}
			if (!text.Contains("DISABLEDXMAXIMIZEDWINDOWEDMODE"))
			{
				_rk.SetValue(_key, flag ? "~ DISABLEDXMAXIMIZEDWINDOWEDMODE RUNASADMIN" : "~ DISABLEDXMAXIMIZEDWINDOWEDMODE");
				Application.OpenURL($"steam://run/{3419430}");
				Application.Quit();
				return;
			}
			_toggle.SetActive(flag);
			if (flag)
			{
				_autoStart.FullyDisable();
			}
			Debug.Log($"[Admin Mode] Admin mode enabled: {flag}");
		}

		public void ToggleAdminMode()
		{
			string text = (string)_rk.GetValue(_key);
			if (text != null && text.Contains("RUNASADMIN"))
			{
				_autoStart.EnableOption();
				_rk.SetValue(_key, "~ DISABLEDXMAXIMIZEDWINDOWEDMODE");
				Application.OpenURL($"steam://run/{3419430}");
				Application.Quit();
			}
			else if (!Application.isEditor && _isAdminUser)
			{
				_autoStart.FullyDisable();
				_rk.SetValue(_key, "~ DISABLEDXMAXIMIZEDWINDOWEDMODE RUNASADMIN");
				Application.OpenURL($"steam://run/{3419430}");
				Application.Quit();
			}
		}

		private bool IsAdmin()
		{
			WindowsPrincipal windowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
			foreach (string adminIdentifier in _adminIdentifiers)
			{
				if (windowsPrincipal.IsInRole(adminIdentifier))
				{
					return true;
				}
			}
			foreach (WindowsBuiltInRole value in Enum.GetValues(typeof(WindowsBuiltInRole)))
			{
				if (value != WindowsBuiltInRole.Administrator && windowsPrincipal.IsInRole(value))
				{
					return false;
				}
			}
			string[] obj = (string[])windowsPrincipal.GetType().GetField("m_roles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(windowsPrincipal);
			string text = "";
			string[] array = obj;
			foreach (string text2 in array)
			{
				text = text + "\n" + text2 + " ";
			}
			Debug.Log("[Admin Mode] User has roles: " + text);
			return false;
		}
	}
}

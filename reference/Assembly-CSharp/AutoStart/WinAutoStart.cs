using System;
using BongoCat;
using Microsoft.Win32;
using UnityEngine;

namespace AutoStart
{
	public class WinAutoStart : IAutoStart
	{
		private RegistryKey _rk;

		private string _oldKey => AppDomain.CurrentDomain.BaseDirectory + "\\" + Global.Exe;

		private string _key => "BongoCat";

		private string _dataEntry => "\"" + AppDomain.CurrentDomain.BaseDirectory + "\\" + Global.Exe + "\"";

		public RegistryKey Rk
		{
			get
			{
				if (_rk == null)
				{
					_rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", writable: true);
				}
				return _rk;
			}
		}

		public void Init()
		{
			if (Rk.GetValue(Application.identifier) != null)
			{
				Rk.DeleteValue(Application.identifier, throwOnMissingValue: false);
			}
			if (Rk.GetValue(_oldKey) != null)
			{
				Rk.DeleteValue(_oldKey, throwOnMissingValue: false);
				Rk.SetValue(_key, _dataEntry);
			}
		}

		public void EnableAutoStart()
		{
			Rk.SetValue(_key, _dataEntry);
		}

		public void DisableAutoStart()
		{
			Rk.DeleteValue(_key, throwOnMissingValue: false);
		}

		public bool IsEnabled()
		{
			return Rk.GetValue(_key) != null;
		}
	}
}

using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BongoCat
{
	public class HideTaskbarIcon : MonoBehaviour
	{
		private const int GWL_EXSTYLE = -20;

		private const int WS_EX_TOOLWINDOW = 128;

		private IntPtr _pMainWindow;

		[SerializeField]
		private PlayerPrefsToggle _toggle;

		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		[DllImport("User32.dll")]
		private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("User32.dll")]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => Application.isFocused);
			_pMainWindow = GetActiveWindow();
			yield return null;
			if (!_toggle.Value)
			{
				Hide();
			}
			_toggle.OnToggleUpdated.AddListener(OnToggleUpdated);
		}

		private void OnToggleUpdated(bool val)
		{
			if (val)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}

		private void Show()
		{
			SetWindowLong(_pMainWindow, -20, GetWindowLong(_pMainWindow, -20) & -129);
		}

		private void Hide()
		{
			SetWindowLong(_pMainWindow, -20, GetWindowLong(_pMainWindow, -20) | 0x80);
		}
	}
}

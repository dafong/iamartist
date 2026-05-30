using System;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using BongoCat.OSSpecific;
using UnityEngine;

namespace BongoCat
{
	public class AlwaysOnTop : MonoBehaviour
	{
		[SerializeField]
		private PlayerPrefsToggle _toggle;

		private Coroutine _routine;

		private const int GWL_EXSTYLE = -20;

		private const int WS_EX_TOPMOST = 8;

		private IntPtr _pMainWindow;

		private const string ClassName = "Shell_TrayWnd";

		private IntPtr _taskbarHandle;

		private IntPtr _startMenuHandle;

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		private void Awake()
		{
			_taskbarHandle = FindWindow("Shell_TrayWnd", null);
		}

		private IEnumerator Start()
		{
			if (!Application.isEditor)
			{
				TransparentWindow.Instance.SetTopMost(_toggle.Value);
			}
			_toggle.OnToggleUpdated.AddListener(OnToggleUpdated);
			if (_toggle.Value)
			{
				_routine = StartCoroutine(TopMostUpdater());
			}
			yield return new WaitUntil(() => Application.isFocused);
			_pMainWindow = GetActiveWindow();
			if (!Application.isEditor)
			{
				TransparentWindow.Instance.SetTopMost(_toggle.Value);
			}
		}

		private void OnToggleUpdated(bool val)
		{
			if (val)
			{
				if (_routine == null)
				{
					_routine = StartCoroutine(TopMostUpdater());
				}
			}
			else if (_routine != null)
			{
				StopCoroutine(_routine);
				_routine = null;
			}
			TransparentWindow.Instance.SetTopMost(val);
		}

		private static bool IsWindowTopMost(IntPtr hWnd)
		{
			return (GetWindowLong(hWnd, -20) & 8) == 8;
		}

		private IEnumerator TopMostUpdater()
		{
			while (true)
			{
				if (_pMainWindow == IntPtr.Zero)
				{
					yield return null;
					continue;
				}
				IntPtr activeForegroundWindow = GetForegroundWindow();
				if (ActiveWindowTracker.Instance.LastThreeActiveWindows.Any((IntPtr window) => window != activeForegroundWindow))
				{
					TransparentWindow.Instance.SetTopMost(topMost: true);
				}
				if (!IsWindowTopMost(_pMainWindow) || _taskbarHandle == activeForegroundWindow)
				{
					TransparentWindow.Instance.SetTopMost(topMost: true);
				}
				yield return null;
			}
		}
	}
}

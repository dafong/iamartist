using System;
using System.Collections;
using System.Collections.Generic;
using BongoCat;
using BongoCat.OSSpecific.Windows;
using UnityEngine;

namespace Transparency
{
	public class WinTransparentWindow : ITransparentWindow
	{
		private Camera _mainCamera;

		private IntPtr _windowHandle;

		private IntPtr _topmostHwnd = WinApi.HWND_TOPMOST;

		private bool _useColorKey;

		private Color _transparentColor;

		private PlayerPrefsToggle _showTaskbarIcon;

		public IEnumerator Init(bool useColorKeyFix, bool alwaysOnTop, Color transparentColor, PlayerPrefsToggle showTaskbarIcon)
		{
			_mainCamera = Camera.main;
			_windowHandle = WinApi.GetActiveWindow();
			_transparentColor = transparentColor;
			_showTaskbarIcon = showTaskbarIcon;
			yield return null;
			_useColorKey = useColorKeyFix;
			_topmostHwnd = (alwaysOnTop ? WinApi.HWND_TOPMOST : WinApi.HWND_NOTOPMOST);
			bool flag = TrySetupDwmTransparency();
			if (!flag || _useColorKey)
			{
				Debug.Log("TransparentWindow | DWM transparency failed or disabled, using color key method: " + flag);
				SetupColorKeyTransparency();
			}
		}

		public void SetClickthrough(bool isClickthrough)
		{
			ulong windowLong = WinApi.GetWindowLong(_windowHandle, WinApi.GWL_EXSTYLE);
			if (isClickthrough)
			{
				windowLong |= WinApi.WS_EX_TRANSPARENT;
				windowLong |= WinApi.WS_EX_LAYERED;
			}
			else
			{
				windowLong &= ~WinApi.WS_EX_TRANSPARENT;
				if (!_useColorKey)
				{
					windowLong &= ~WinApi.WS_EX_LAYERED;
				}
			}
			WinApi.SetWindowLong(_windowHandle, WinApi.GWL_EXSTYLE, windowLong);
		}

		public void OnTaskbarIconToggleUpdated(bool iconEnabled)
		{
			ulong windowLong = WinApi.GetWindowLong(_windowHandle, WinApi.GWL_EXSTYLE);
			WinApi.SetWindowLong(value: (!iconEnabled) ? (windowLong | WinApi.WS_EX_TOOLWINDOW) : (windowLong & ~WinApi.WS_EX_TOOLWINDOW), hWnd: _windowHandle, nIndex: WinApi.GWL_EXSTYLE);
		}

		public void SetTopMost(bool topMost)
		{
			_topmostHwnd = (topMost ? WinApi.HWND_TOPMOST : WinApi.HWND_NOTOPMOST);
			WinApi.SetWindowPos(_windowHandle, _topmostHwnd, 0, 0, 0, 0, WinApi.SWP_NOMOVE | WinApi.SWP_NOSIZE | WinApi.SWP_FRAMECHANGED);
		}

		public void MoveWindowToMonitor(int monitorIndex)
		{
			if (_windowHandle == IntPtr.Zero)
			{
				Debug.LogError("TransparentWindow | Could not get window handle");
				return;
			}
			WinApi.WinRect monitorBounds = GetMonitorBounds(monitorIndex);
			if (monitorBounds.Width == 0 || monitorBounds.Height == 0)
			{
				Debug.LogError($"TransparentWindow | Invalid monitor bounds for monitor {monitorIndex}");
				return;
			}
			Debug.Log($"TransparentWindow | Moving window to monitor {monitorIndex} at position ({monitorBounds.Left}, {monitorBounds.Top}) with size {monitorBounds.Width}x{monitorBounds.Height}");
			WinApi.SetWindowPos(_windowHandle, _topmostHwnd, monitorBounds.Left, monitorBounds.Top - 1, monitorBounds.Width, monitorBounds.Height, WinApi.SWP_FRAMECHANGED | WinApi.SWP_SHOWWINDOW);
		}

		private WinApi.WinRect GetMonitorBounds(int monitorIndex)
		{
			List<WinApi.WinRect> monitors = new List<WinApi.WinRect>();
			WinApi.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate(IntPtr _, IntPtr _, ref WinApi.WinRect lprcMonitor, IntPtr _)
			{
				monitors.Add(lprcMonitor);
				return true;
			}, IntPtr.Zero);
			if (monitorIndex >= 0 && monitorIndex < monitors.Count)
			{
				Debug.Log($"TransparentWindow | Monitor {monitorIndex} bounds: {monitors[monitorIndex].Left}, {monitors[monitorIndex].Top}, {monitors[monitorIndex].Width}x{monitors[monitorIndex].Height}");
				return monitors[monitorIndex];
			}
			Debug.LogError($"TransparentWindow | Monitor index {monitorIndex} out of range. Found {monitors.Count} monitors.");
			return default(WinApi.WinRect);
		}

		public WinApi.WinRect GetCurrentWindowBounds()
		{
			if (_windowHandle == IntPtr.Zero)
			{
				return default(WinApi.WinRect);
			}
			WinApi.GetWindowRect(_windowHandle, out var rect);
			return rect;
		}

		private bool TrySetupDwmTransparency()
		{
			try
			{
				_mainCamera.backgroundColor = Color.clear;
				_mainCamera.clearFlags = CameraClearFlags.Color;
				WinApi.SetWindowLong(_windowHandle, WinApi.GWL_STYLE, WinApi.WS_VISIBLE | WinApi.WS_POPUP);
				WinApi.Margins pMarInset = new WinApi.Margins
				{
					cxLeftWidth = -1,
					cxRightWidth = -1,
					cyTopHeight = -1,
					cyBottomHeight = -1
				};
				int num = WinApi.DwmExtendFrameIntoClientArea(_windowHandle, ref pMarInset);
				if (num != 0)
				{
					Debug.LogWarning($"TransparentWindow | DwmExtendFrameIntoClientArea failed with error: {num}");
					return false;
				}
				OnTaskbarIconToggleUpdated(_showTaskbarIcon.Value);
				WinApi.WinRect currentWindowBounds = GetCurrentWindowBounds();
				WinApi.SetWindowPos(_windowHandle, _topmostHwnd, currentWindowBounds.Left, currentWindowBounds.Top, currentWindowBounds.Width, currentWindowBounds.Height, WinApi.SWP_FRAMECHANGED | WinApi.SWP_SHOWWINDOW);
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError("TransparentWindow | DWM setup failed: " + ex.Message);
				return false;
			}
		}

		private void SetupColorKeyTransparency()
		{
			_mainCamera.backgroundColor = _transparentColor;
			_mainCamera.clearFlags = CameraClearFlags.Color;
			WinApi.Margins pMarInset = new WinApi.Margins
			{
				cxLeftWidth = -1,
				cxRightWidth = -1,
				cyTopHeight = -1,
				cyBottomHeight = -1
			};
			WinApi.DwmExtendFrameIntoClientArea(_windowHandle, ref pMarInset);
			WinApi.SetWindowLong(_windowHandle, WinApi.GWL_EXSTYLE, WinApi.WS_EX_LAYERED);
			WinApi.SetLayeredWindowAttributes(_windowHandle, 9605778u, 255, WinApi.LWA_COLORKEY);
			OnTaskbarIconToggleUpdated(_showTaskbarIcon.Value);
		}

		public void SetUseColorKey(bool useColorKey)
		{
			if (_useColorKey && !useColorKey)
			{
				RestartApplication.RestartGameViaSteam();
				return;
			}
			_useColorKey = useColorKey;
			if (_useColorKey)
			{
				SetupColorKeyTransparency();
			}
			else if (!TrySetupDwmTransparency())
			{
				SetupColorKeyTransparency();
			}
		}
	}
}

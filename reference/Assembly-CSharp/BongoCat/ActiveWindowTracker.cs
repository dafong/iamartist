using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace BongoCat
{
	public class ActiveWindowTracker : MonoBehaviour
	{
		private const string ClassName = "Shell_TrayWnd";

		private IntPtr _taskbarHandle;

		private IntPtr _pMainWindow;

		private Queue<IntPtr> _lastThreeActiveWindows = new Queue<IntPtr>();

		public static ActiveWindowTracker Instance;

		public Queue<IntPtr> LastThreeActiveWindows => _lastThreeActiveWindows;

		public event Action ActiveWindowChanged;

		[DllImport("user32.dll")]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		private static extern IntPtr GetActiveWindow();

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => Application.isFocused);
			_pMainWindow = GetActiveWindow();
			_taskbarHandle = FindWindow("Shell_TrayWnd", null);
			while (true)
			{
				if (_pMainWindow == IntPtr.Zero)
				{
					yield return null;
					continue;
				}
				IntPtr activeForegroundWindow = GetForegroundWindow();
				_lastThreeActiveWindows.Enqueue(activeForegroundWindow);
				if (_lastThreeActiveWindows.Any((IntPtr window) => window != activeForegroundWindow))
				{
					this.ActiveWindowChanged?.Invoke();
				}
				while (_lastThreeActiveWindows.Count > 3)
				{
					_lastThreeActiveWindows.Dequeue();
				}
				yield return null;
			}
		}
	}
}

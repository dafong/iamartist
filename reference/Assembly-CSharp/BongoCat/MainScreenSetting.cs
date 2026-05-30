using System;
using System.Collections;
using System.Collections.Generic;
using BongoCat.Localizer;
using BongoCat.OSSpecific;
using BongoCat.OSSpecific.Windows;
using TMPro;
using UnityEngine;

namespace BongoCat
{
	public class MainScreenSetting : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _text;

		private List<DisplayInfo> _displays = new List<DisplayInfo>();

		private int _index;

		private const string PLAYER_PREFS_KEY = "MAIN_MONITOR";

		private const string MAIN_SCREEN_LOCA_KEY = "MainScreen";

		private Coroutine _switchScreenRoutine;

		public bool IsInitialized;

		public event Action ScreenSwitched;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => TransparentWindow.Instance.Initialized);
			Screen.GetDisplayLayout(_displays);
			Debug.Log($"MainScreenSetting | Found {_displays.Count} connected display(s)");
			_index = PlayerPrefs.GetInt("MAIN_MONITOR", 0);
			Debug.Log($"MainScreenSetting | Display index found in player prefs {_index}");
			_index = Mathf.Clamp(_index, 0, _displays.Count - 1);
			Debug.Log($"MainScreenSetting | Clamped index is {_index}");
			SwitchScreen();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F8))
			{
				ToggleForward();
			}
		}

		public void ToggleForward()
		{
			Screen.GetDisplayLayout(_displays);
			Debug.Log($"MainScreenSetting | Toggle forward found {_displays.Count} connected display(s)");
			if (_displays.Count < 1)
			{
				Debug.Log("MainScreenSetting | No displays found");
				return;
			}
			if (_displays.Count == 1 && _index == 0)
			{
				Debug.Log("MainScreenSetting | Only one display and index already 0");
				return;
			}
			_index = (_index + 1) % _displays.Count;
			SwitchScreen();
		}

		public void ToggleBackward()
		{
			Screen.GetDisplayLayout(_displays);
			Debug.Log($"MainScreenSetting | Toggle backward found {_displays.Count} connected display(s)");
			if (_displays.Count < 1)
			{
				Debug.Log("MainScreenSetting | No displays found");
				return;
			}
			if (_displays.Count == 1 && _index == 0)
			{
				Debug.Log("MainScreenSetting | Only one display and index already 0");
				return;
			}
			_index = (_index - 1 + _displays.Count) % _displays.Count;
			SwitchScreen();
		}

		private void SwitchScreen()
		{
			Debug.Log("MainScreenSetting | Switching screen");
			if (_switchScreenRoutine != null)
			{
				Debug.Log("MainScreenSetting | Already switching screen. Interrupting routine.");
				StopCoroutine(_switchScreenRoutine);
			}
			_switchScreenRoutine = StartCoroutine(UpdateMainMonitor());
		}

		private string GetWindowBounds(out Vector2 resolution)
		{
			WinApi.WinRect currentWindowBounds = TransparentWindow.Instance.GetCurrentWindowBounds();
			resolution = new Vector2(currentWindowBounds.Width, currentWindowBounds.Height);
			return $"{currentWindowBounds.Left}, {currentWindowBounds.Top}, {currentWindowBounds.Width}x{currentWindowBounds.Height}";
		}

		private IEnumerator UpdateMainMonitor()
		{
			OnDemandRenderHelper.Instance.ResumeRendering();
			Debug.Log("MainScreenSetting | current window bounds: " + GetWindowBounds(out var resolution));
			Debug.Log($"MainScreenSetting | Moving window to monitor {_index}");
			TransparentWindow.Instance.MoveWindowToMonitor(_index);
			yield return null;
			Debug.Log("MainScreenSetting | After move - Window bounds: " + GetWindowBounds(out resolution));
			Screen.SetResolution((int)resolution.x, (int)resolution.y, FullScreenMode.Windowed);
			yield return new WaitForSeconds(0.2f);
			_text.text = string.Format("{0}: {1} (F8)", Loca.Instance.Get("MainScreen"), _index + 1);
			PlayerPrefs.SetInt("MAIN_MONITOR", _index);
			PlayerPrefs.Save();
			Debug.Log($"MainScreenSetting | After screen update: screen res width: {Screen.currentResolution.width}, height: {Screen.currentResolution.height}) " + $"with window at {Screen.mainWindowPosition.x} {Screen.mainWindowPosition.y}");
			Debug.Log("MainScreenSetting | Final window bounds: " + GetWindowBounds(out resolution));
			MonoBehaviour.print(Screen.mainWindowDisplayInfo.name);
			_switchScreenRoutine = null;
			OnDemandRenderHelper.Instance.TryPauseRendering();
			this.ScreenSwitched?.Invoke();
			IsInitialized = true;
		}
	}
}

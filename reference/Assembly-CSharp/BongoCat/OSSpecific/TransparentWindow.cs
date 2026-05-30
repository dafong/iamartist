using System.Collections;
using System.Collections.Generic;
using BongoCat.OSSpecific.Windows;
using Transparency;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BongoCat.OSSpecific
{
	public class TransparentWindow : MonoBehaviour
	{
		public static TransparentWindow Instance;

		[SerializeField]
		private PlayerPrefsToggle _alwaysOnTop;

		[SerializeField]
		private PlayerPrefsToggle _showTaskbarIcon;

		[SerializeField]
		private GamingMode _gamingMode;

		[SerializeField]
		private PlayerPrefsToggle _emoteChestSettings;

		private ITransparentWindow _transparentWindow;

		[SerializeField]
		private PlayerPrefsToggle _useColorKeyFix;

		[SerializeField]
		private Color _transparentColor;

		private bool _initialized;

		public bool Initialized => _initialized;

		public bool GamingModeEnabled
		{
			get
			{
				if (_gamingMode.IsEnabled)
				{
					if (!Shop.NormalShop.CanGetChest)
					{
						if (Shop.EmoteShop.CanGetChest)
						{
							return !_emoteChestSettings.Value;
						}
						return true;
					}
					return false;
				}
				return false;
			}
		}

		private void Awake()
		{
			Instance = this;
			_transparentWindow = new WinTransparentWindow();
			_showTaskbarIcon.OnToggleUpdated.AddListener(_transparentWindow.OnTaskbarIconToggleUpdated);
		}

		private IEnumerator Start()
		{
			if (!Application.isEditor)
			{
				yield return new WaitUntil(() => Application.isFocused);
				yield return null;
				yield return ((WinTransparentWindow)_transparentWindow).Init(_useColorKeyFix.Value, _alwaysOnTop.Value, _transparentColor, _showTaskbarIcon);
				_initialized = true;
			}
		}

		private void Update()
		{
			if (GamingModeEnabled || !CursorVisible.Instance.IsVisible)
			{
				_transparentWindow.SetClickthrough(isClickthrough: true);
			}
			else
			{
				_transparentWindow.SetClickthrough(!MouseOnObject());
			}
		}

		private bool MouseOnObject()
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			Vector2 position = Input.mousePosition;
			List<RaycastResult> list = new List<RaycastResult>();
			pointerEventData.position = position;
			EventSystem.current.RaycastAll(pointerEventData, list);
			foreach (RaycastResult item in list)
			{
				int num = ~LayerMask.GetMask("Ignore Raycast");
				if (((1 << item.gameObject.layer) & num) > 0)
				{
					return true;
				}
			}
			return false;
		}

		public void SetTopMost(bool topMost)
		{
			if (CursorVisible.Instance.IsVisible)
			{
				_transparentWindow.SetTopMost(topMost);
			}
		}

		public void MoveWindowToMonitor(int monitorIndex)
		{
			Debug.Log($"TransparentWindow | Moving window to monitor {monitorIndex}");
			_transparentWindow.MoveWindowToMonitor(monitorIndex);
		}

		public WinApi.WinRect GetCurrentWindowBounds()
		{
			return _transparentWindow.GetCurrentWindowBounds();
		}

		public void SetTransparencyFix(bool useColorKey)
		{
			((WinTransparentWindow)_transparentWindow).SetUseColorKey(useColorKey);
		}
	}
}

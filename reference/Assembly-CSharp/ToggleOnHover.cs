using System.Collections.Generic;
using BongoCat;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToggleOnHover : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> toggleOnHover;

	[SerializeField]
	private List<GameObject> _hideWhenNotHoveringOnly;

	[SerializeField]
	private PlayerPrefsToggle _toggle;

	[SerializeField]
	private PlayerPrefsToggle _chestPopUp;

	private int _uiLayer;

	private bool _lastFrameIsOver;

	[SerializeField]
	private PlayerPrefsToggle _emoteChestSettings;

	private void Awake()
	{
		_uiLayer = LayerMask.NameToLayer("UI");
	}

	private void Update()
	{
		if (!_toggle.Value)
		{
			return;
		}
		bool flag = IsPointerOverUIElement(GetEventSystemRaycastResults());
		if (_lastFrameIsOver != flag)
		{
			if (flag)
			{
				OnDemandRenderHelper.Instance.ResumeRendering();
			}
			else
			{
				OnDemandRenderHelper.Instance.TryPauseRendering();
			}
		}
		_lastFrameIsOver = flag;
		if ((Shop.NormalShop.CanGetChest || (Shop.EmoteShop.CanGetChest && _emoteChestSettings.Value)) && _chestPopUp.Value)
		{
			foreach (GameObject item in toggleOnHover)
			{
				item.SetActive(value: true);
			}
			{
				foreach (GameObject item2 in _hideWhenNotHoveringOnly)
				{
					item2.SetActive(value: true);
				}
				return;
			}
		}
		if (flag != toggleOnHover.TrueForAll((GameObject go) => go.activeSelf))
		{
			foreach (GameObject item3 in toggleOnHover)
			{
				item3.SetActive(flag);
			}
		}
		if (flag || SettingsManager.Instance.AlwaysShowChest.Value)
		{
			return;
		}
		foreach (GameObject item4 in _hideWhenNotHoveringOnly)
		{
			item4.SetActive(value: false);
		}
	}

	private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
	{
		foreach (RaycastResult eventSystemRaysastResult in eventSystemRaysastResults)
		{
			if (eventSystemRaysastResult.gameObject.layer == _uiLayer)
			{
				return true;
			}
		}
		return false;
	}

	private static List<RaycastResult> GetEventSystemRaycastResults()
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = Input.mousePosition
		};
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, list);
		return list;
	}
}

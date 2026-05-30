using System.Collections;
using System.Collections.Generic;
using BongoCat;
using BongoCat.OSSpecific;
using Kirurobo;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableFrame : MonoBehaviour, IDragHandler, IEventSystemHandler, IEndDragHandler
{
	[SerializeField]
	private Vector2 _windowPos;

	[SerializeField]
	private UniWindowController _uniWindowController;

	[SerializeField]
	private PlayerPrefsToggle _lockPosition;

	[SerializeField]
	private PlayerPrefsToggle _snapping;

	public void OnDrag(PointerEventData eventData)
	{
		if (!_lockPosition.Value)
		{
			_windowPos += (Vector2)Input.mousePosition - new Vector2((float)Screen.width / 2f - 20f, 29f * RealWindowScale.Instance.RealScaling);
			if (!Application.isEditor && (!Mathf.Approximately(_uniWindowController.windowPosition.x, _windowPos.x) || !Mathf.Approximately(_uniWindowController.windowPosition.y, _windowPos.y)))
			{
				RealWindowScale.Instance.SetScale(RealWindowScale.Instance.Scaling);
			}
		}
	}

	private int GetCurrentDisplayNumber()
	{
		List<DisplayInfo> list = new List<DisplayInfo>();
		Screen.GetDisplayLayout(list);
		return list.IndexOf(Screen.mainWindowDisplayInfo);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (!_lockPosition.Value)
		{
			if (_snapping.Value && GetCurrentDisplayNumber() == 0 && _windowPos.y < (float)Taskbar.Instance.Height)
			{
				_windowPos.y = (float)Taskbar.Instance.Height - 29f * RealWindowScale.Instance.RealScaling;
			}
			if (!Application.isEditor && (!Mathf.Approximately(_uniWindowController.windowPosition.x, _windowPos.x) || !Mathf.Approximately(_uniWindowController.windowPosition.y, _windowPos.y)))
			{
				RealWindowScale.Instance.SetScale(RealWindowScale.Instance.Scaling);
			}
		}
	}

	private void BoundaryCheck()
	{
		DisplayInfo mainWindowDisplayInfo = Screen.mainWindowDisplayInfo;
		Vector2 vector = Screen.mainWindowPosition;
		int width = Screen.width;
		int height = Screen.height;
		if (vector.x < (float)(-width) / 2f)
		{
			vector.x = (float)(-width) / 2f;
		}
		else if (vector.x > (float)mainWindowDisplayInfo.width - (float)width / 2f)
		{
			vector.x = (float)mainWindowDisplayInfo.width - (float)width / 2f;
		}
		if (vector.y < (float)(-height) * 0.9f)
		{
			vector.y = (float)(-height) * 0.9f;
		}
		else if (vector.y > (float)(mainWindowDisplayInfo.height - height))
		{
			vector.y = mainWindowDisplayInfo.height - height;
		}
	}

	private IEnumerator Start()
	{
		yield return null;
		_windowPos = new Vector2((float)Taskbar.Instance.Width / 2f, 0f);
		yield return null;
		BoundaryCheck();
	}
}

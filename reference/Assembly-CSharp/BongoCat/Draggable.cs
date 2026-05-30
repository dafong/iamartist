using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace BongoCat
{
	public class Draggable : MonoBehaviour, IDragHandler, IEventSystemHandler, IBeginDragHandler, IEndDragHandler
	{
		private static readonly HashSet<Draggable> _draggables = new HashSet<Draggable>();

		private Vector2 _initialDragOffset;

		[SerializeField]
		private RectTransform _rectTransform;

		public UnityEvent OnDragStarted;

		public UnityEvent OnDragFinished;

		private float _relativeXPos;

		private float _relativeYPos;

		[SerializeField]
		private GameObject _requiredObject;

		[SerializeField]
		private CatRotator _catRotator;

		private bool _ignoring;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SettingsManager.Instance.MainScreenSetting.IsInitialized);
			_draggables.Add(this);
			FetchRelativePos();
			OutOfBoundsFix();
			HandleSnapping();
			SettingsManager.Instance.MainScreenSetting.ScreenSwitched += OnScreenSwitch;
			while (true)
			{
				yield return new WaitForSeconds(1f);
				OutOfBoundsFix();
			}
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if ((bool)_requiredObject && !eventData.hovered.Contains(_requiredObject))
			{
				_ignoring = true;
			}
			else if (!SettingsManager.Instance.LockPosition.Value)
			{
				OnDemandRenderHelper.Instance.ResumeRendering();
				_initialDragOffset = new Vector2(base.transform.position.x, base.transform.position.y) - eventData.position;
				OnDragStarted?.Invoke();
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (!_ignoring && !SettingsManager.Instance.LockPosition.Value)
			{
				base.transform.position = eventData.position + _initialDragOffset;
				OutOfBoundsFix();
			}
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (_ignoring)
			{
				_ignoring = false;
			}
			else if (!SettingsManager.Instance.LockPosition.Value)
			{
				HandleSnapping();
				FetchRelativePos();
				OnDragFinished?.Invoke();
				OnDemandRenderHelper.Instance.TryPauseRendering();
			}
		}

		public bool OutOfBoundsFix()
		{
			Vector3[] array = new Vector3[4];
			_rectTransform.GetWorldCorners(array);
			float num = array.Min((Vector3 c) => c.x);
			float num2 = array.Max((Vector3 c) => c.x);
			float num3 = array.Min((Vector3 c) => c.y);
			float num4 = array.Max((Vector3 c) => c.y);
			Vector3 zero = Vector3.zero;
			if (num < 0f)
			{
				zero.x = 0f - num;
			}
			else if (num2 > (float)ScreenSize.FullWidth)
			{
				zero.x = (float)ScreenSize.FullWidth - num2;
			}
			if (num4 > (float)ScreenSize.FullHeight)
			{
				zero.y = (float)ScreenSize.FullHeight - num4;
			}
			else if (num3 < 0f)
			{
				zero.y = 0f - num3;
			}
			if (zero == Vector3.zero)
			{
				return false;
			}
			base.transform.position += zero;
			return true;
		}

		private void HandleSnapping()
		{
			if (!_catRotator)
			{
				return;
			}
			if (SettingsManager.Instance.Snapping.Value)
			{
				if (base.transform.position.y < (float)ScreenSize.YMin)
				{
					if (_catRotator.RotatedNormally)
					{
						float y = ScreenSize.YMin - 2 - 1;
						base.transform.position = new Vector3(base.transform.position.x, y, base.transform.position.z);
					}
				}
				else if (base.transform.position.y + _rectTransform.rect.height > (float)ScreenSize.YMax && _catRotator.UpsideDown)
				{
					float y2 = (float)ScreenSize.YMax - _rectTransform.rect.height + 2f + 1f;
					base.transform.position = new Vector3(base.transform.position.x, y2, base.transform.position.z);
				}
			}
			if (!SettingsManager.Instance.SnapCats.Value)
			{
				return;
			}
			foreach (Draggable item in (from draggable in _draggables
				where draggable != this && Vector3.Distance(base.transform.position, draggable.transform.position) < 1.5f * SettingsManager.Instance.CatScaleSetting.GetRealScaleFactor() * _rectTransform.rect.width
				orderby Vector3.Distance(base.transform.position, draggable.transform.position)
				select draggable).ToList())
			{
				if (Mathf.Abs(item.transform.position.y - base.transform.position.y) < _rectTransform.rect.height * SettingsManager.Instance.CatScaleSetting.GetRealScaleFactor() / 2f)
				{
					base.transform.position = new Vector3(base.transform.position.x, item.transform.position.y, base.transform.position.z);
					break;
				}
			}
		}

		public void FetchRelativePos()
		{
			_relativeXPos = base.transform.position.x / (float)ScreenSize.FullWidth;
			_relativeYPos = base.transform.position.y / (float)ScreenSize.FullHeight;
		}

		private void OnScreenSwitch()
		{
			base.transform.position = new Vector3(_relativeXPos * (float)ScreenSize.FullWidth, _relativeYPos * (float)ScreenSize.FullHeight);
			OutOfBoundsFix();
			HandleSnapping();
		}

		private void OnDestroy()
		{
			SettingsManager.Instance.MainScreenSetting.ScreenSwitched -= OnScreenSwitch;
			_draggables.Remove(this);
		}
	}
}

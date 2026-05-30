using BongoCat;
using UnityEngine;
using UnityEngine.EventSystems;

public class Resizable : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler
{
	[SerializeField]
	private RectTransform _resizableRect;

	[SerializeField]
	private Vector2 _minSize;

	[SerializeField]
	private bool _resetSizeOnEnable;

	private Vector2 _baseSize;

	private Vector2 _lastMousePos;

	private Vector2 RealSize => _resizableRect.sizeDelta * _resizableRect.localScale;

	private float MinScale => SettingsManager.Instance.UIScaleSetting.GetRealScaleFactor();

	private void Awake()
	{
		_baseSize = _resizableRect.sizeDelta;
	}

	private void OnEnable()
	{
		if (_resetSizeOnEnable)
		{
			_resizableRect.sizeDelta = _baseSize;
			_resizableRect.localScale = MinScale * Vector3.one;
		}
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_lastMousePos = eventData.position;
	}

	public void OnDrag(PointerEventData eventData)
	{
		Vector2 delta = eventData.position - _lastMousePos;
		Resize(delta);
		CheckBoundaries();
		_lastMousePos = eventData.position;
	}

	private void Resize(Vector2 delta)
	{
		Vector2 vector = RealSize + new Vector2(delta.x, 0f - delta.y);
		vector.x = Mathf.Max(vector.x, _minSize.x);
		vector.y = Mathf.Max(vector.y, _minSize.y);
		bool flag = vector.x < vector.y;
		float num = (flag ? (vector.x / _baseSize.x) : (vector.y / _baseSize.y));
		if (num >= MinScale)
		{
			_resizableRect.localScale = Vector3.one * num;
			_resizableRect.sizeDelta = (flag ? new Vector2(_baseSize.x, vector.y / num) : new Vector2(vector.x / num, _baseSize.y));
		}
		else
		{
			_resizableRect.localScale = Vector3.one * MinScale;
			_resizableRect.sizeDelta = new Vector2(vector.x / MinScale, vector.y / MinScale);
		}
	}

	private void CheckBoundaries()
	{
		Vector3[] array = new Vector3[4];
		_resizableRect.GetWorldCorners(array);
		Vector2 zero = Vector2.zero;
		if (array[3].x > (float)Screen.mainWindowDisplayInfo.workArea.xMax)
		{
			zero.x = (float)Screen.mainWindowDisplayInfo.workArea.xMax - array[3].x;
		}
		if (array[0].y < (float)Screen.mainWindowDisplayInfo.workArea.yMin)
		{
			zero.y = (float)Screen.mainWindowDisplayInfo.workArea.yMin - array[0].y;
		}
		if (zero != Vector2.zero)
		{
			Resize(zero);
		}
	}
}

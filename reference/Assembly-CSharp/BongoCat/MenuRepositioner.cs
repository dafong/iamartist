using UnityEngine;

namespace BongoCat
{
	public class MenuRepositioner : MonoBehaviour
	{
		[SerializeField]
		private float _topBorderPivotY;

		[SerializeField]
		private float _bottomBorderPivotY;

		[SerializeField]
		private float _leftBorderPivotX;

		[SerializeField]
		private float _rightBorderPivotX;

		[SerializeField]
		private Vector3 _topBorderLocalPosOffset;

		[SerializeField]
		private Vector3 _bottomBorderLocalPosOffset;

		[SerializeField]
		private Vector3 _leftBorderLocalPosOffset;

		[SerializeField]
		private Vector3 _rightBorderLocalPosOffset;

		private RectTransform _rectTransform;

		private Vector2 _defaultPivot;

		private Vector3 _defaultLocalPos;

		[SerializeField]
		private Vector3 _catOffset;

		[SerializeField]
		private RectTransform _catTransform;

		private void Awake()
		{
			_rectTransform = base.transform as RectTransform;
			_defaultPivot = _rectTransform.pivot;
			_defaultLocalPos = _rectTransform.localPosition;
		}

		private void LateUpdate()
		{
			_rectTransform.pivot = _defaultPivot;
			_rectTransform.localPosition = _defaultLocalPos;
			if ((bool)_catTransform)
			{
				_rectTransform.localPosition = _defaultLocalPos + Vector3.up * (_catTransform.rect.height * _catTransform.localScale.y);
			}
			Vector3[] array = new Vector3[4];
			_rectTransform.GetWorldCorners(array);
			Vector2 defaultPivot = _defaultPivot;
			Vector3 defaultLocalPos = _defaultLocalPos;
			bool flag = false;
			if (array[0].x < (float)ScreenSize.XMin)
			{
				defaultPivot.x = _leftBorderPivotX;
				defaultLocalPos += _leftBorderLocalPosOffset;
				flag = true;
			}
			else if (array[3].x > (float)ScreenSize.XMax)
			{
				defaultPivot.x = _rightBorderPivotX;
				defaultLocalPos += _rightBorderLocalPosOffset;
				flag = true;
			}
			if (array[1].y > (float)ScreenSize.YMax)
			{
				defaultPivot.y = _topBorderPivotY;
				defaultLocalPos += _topBorderLocalPosOffset;
				flag = true;
			}
			else if (array[0].y < (float)ScreenSize.YMin)
			{
				defaultPivot.y = _bottomBorderPivotY;
				defaultLocalPos += _bottomBorderLocalPosOffset;
				flag = true;
			}
			if (flag)
			{
				_rectTransform.pivot = defaultPivot;
				_rectTransform.localPosition = defaultLocalPos;
			}
		}
	}
}

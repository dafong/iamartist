using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler
{
	private float _scaleBy;

	private float _downScaleBy;

	private Button _button;

	[SerializeField]
	private Transform _animatedContent;

	[SerializeField]
	private bool _dontScale;

	private bool _init;

	private void Awake()
	{
		_button = GetComponent<Button>();
	}

	private IEnumerator Start()
	{
		RectTransform rectTransform = base.transform as RectTransform;
		yield return new WaitUntil(() => rectTransform.rect.width > 0f && rectTransform.rect.height > 0f);
		float width = rectTransform.rect.width;
		float height = rectTransform.rect.height;
		_scaleBy = ((width + 3f) / width + (height + 3f) / height) * 0.5f;
		_downScaleBy = ((width - 2f) / width + (height - 2f) / height) * 0.5f;
		_init = true;
	}

	private void OnDisable()
	{
		if (_init && (!_button || _button.interactable))
		{
			if (!_dontScale)
			{
				base.transform.DOKill();
			}
			if (!_dontScale)
			{
				base.transform.localScale = Vector3.one;
			}
			if ((bool)_animatedContent)
			{
				_animatedContent.DOKill();
				_animatedContent.localRotation = Quaternion.identity;
				_animatedContent.localScale = Vector3.one;
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (_init && (!_button || _button.interactable))
		{
			if (!_dontScale)
			{
				base.transform.DOKill();
			}
			if (!_dontScale)
			{
				base.transform.DOScale(_scaleBy, 0.05f);
			}
			if ((bool)_animatedContent)
			{
				_animatedContent.DOKill();
				_animatedContent.DORotate(new Vector3(0f, 0f, -7f), 0.05f);
				_animatedContent.DOScale(Vector3.one * 1.15f, 0.05f);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (_init && (!_button || _button.interactable))
		{
			if (!_dontScale)
			{
				base.transform.DOKill();
			}
			if (!_dontScale)
			{
				base.transform.DOScale(1f, 0.1f);
			}
			if ((bool)_animatedContent)
			{
				_animatedContent.DOKill();
				_animatedContent.DORotate(new Vector3(0f, 0f, 0f), 0.05f);
				_animatedContent.DOScale(Vector3.one, 0.05f);
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (_init && (!_button || _button.interactable))
		{
			if (!_dontScale)
			{
				base.transform.DOKill();
			}
			if (!_dontScale)
			{
				base.transform.localScale = Vector3.one * _downScaleBy;
			}
			if (!_dontScale)
			{
				base.transform.DOScale(Vector3.one, 0.05f);
			}
			if ((bool)_animatedContent)
			{
				_animatedContent.DOKill();
				_animatedContent.DORotate(new Vector3(0f, 0f, 0f), 0.05f);
				_animatedContent.DOScale(Vector3.one, 0.05f);
			}
		}
	}
}

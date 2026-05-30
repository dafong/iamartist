using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DropdownToggle : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Set true if dropdown menu should be expanded by default")]
	private bool defaultValue = true;

	[SerializeField]
	private Transform dropdownArrow;

	[SerializeField]
	private ToggleGameObject foldoutWindow;

	private RectTransform _rectTransform;

	private RectTransform _windowTransform;

	private bool _expanded;

	private void Awake()
	{
		_rectTransform = GetComponent<RectTransform>();
		_windowTransform = foldoutWindow.GetComponent<RectTransform>();
		_expanded = defaultValue;
	}

	private void Start()
	{
		Display(animate: false);
	}

	public void Toggle()
	{
		_expanded = !_expanded;
		Display();
	}

	private void Display(bool animate = true)
	{
		foldoutWindow.SetActive(_expanded);
		DOTween.Kill(dropdownArrow);
		float z = (_expanded ? 0f : 90f);
		if (animate)
		{
			dropdownArrow.DOLocalRotate(new Vector3(0f, 0f, z), 0.2f);
		}
		else
		{
			dropdownArrow.localRotation = Quaternion.Euler(0f, 0f, z);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(_windowTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
	}
}

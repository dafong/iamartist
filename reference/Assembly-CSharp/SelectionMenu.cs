using System;
using BongoCat;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SelectionMenu : MonoBehaviour
{
	[Serializable]
	private class Option
	{
		public string tooltip;

		public Sprite icon;

		public UnityEvent onClick;

		public SelectionEntry entry;
	}

	[SerializeField]
	private string _playerPrefsKey;

	[SerializeField]
	private int _default;

	[SerializeField]
	private GameObject _selectionMenuPrefab;

	[SerializeField]
	private SelectionEntry _entryPrefab;

	[SerializeField]
	private Button _button;

	[SerializeField]
	private Image _buttonIcon;

	[SerializeField]
	private TooltipUiElement _tooltip;

	[SerializeField]
	private Option[] _options;

	private int _nOptions;

	private int _currentlySelected;

	private int _hoverSelected;

	private bool _selecting;

	private GameObject _parent;

	public int Selected => _currentlySelected;

	private void Awake()
	{
		_nOptions = _options.Length;
		_currentlySelected = PlayerPrefs.GetInt(_playerPrefsKey, _default);
	}

	private void Start()
	{
		_parent = UnityEngine.Object.Instantiate(_selectionMenuPrefab, _button.transform);
		_parent.GetComponent<OnHoverCallback>().onHoverExit.AddListener(OnMouseExit);
		_parent.GetComponent<MouseDownEvent>().onMouseDown.AddListener(ConfirmHoverSelection);
		_button.onClick.AddListener(Show);
		CreateSelection();
		SetSelection(_currentlySelected, performAction: true);
	}

	private void Update()
	{
		if (_selecting)
		{
			UpdateSelection();
		}
	}

	private void CreateSelection()
	{
		for (int i = 0; i < _nOptions; i++)
		{
			SelectionEntry selectionEntry = UnityEngine.Object.Instantiate(_entryPrefab, _parent.transform);
			selectionEntry.Init(i, _nOptions, _options[i].icon, _options[i].tooltip);
			_options[i].entry = selectionEntry;
		}
	}

	private void Show()
	{
		_parent.SetActive(value: true);
		_tooltip.Hide();
		_selecting = true;
	}

	private void OnMouseExit()
	{
		_parent.SetActive(value: false);
		_selecting = false;
		OnSelectionChanged(-1);
	}

	private void ConfirmHoverSelection()
	{
		_currentlySelected = _hoverSelected;
		SetSelection(_currentlySelected, performAction: true);
		OnMouseExit();
		if (!string.IsNullOrEmpty(_playerPrefsKey))
		{
			PlayerPrefs.SetInt(_playerPrefsKey, _currentlySelected);
			PlayerPrefs.Save();
		}
	}

	public void SetSelection(int selected, bool performAction = false)
	{
		_buttonIcon.sprite = _options[selected].icon;
		_tooltip.SetText(_options[selected].tooltip);
		if (performAction)
		{
			_options[selected].onClick?.Invoke();
		}
	}

	private void UpdateSelection()
	{
		Vector2 vector = (Vector2)Input.mousePosition - RectTransformUtility.WorldToScreenPoint(null, _parent.transform.position);
		float num = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		num = (90f - num + 360f) % 360f;
		int num2 = Mathf.FloorToInt(num / (360f / (float)_nOptions));
		if (num2 != _hoverSelected)
		{
			OnSelectionChanged(num2);
		}
	}

	private void OnSelectionChanged(int index)
	{
		if (_hoverSelected >= 0)
		{
			_options[_hoverSelected].entry.PopBack();
		}
		_hoverSelected = index;
		if (_hoverSelected >= 0)
		{
			_options[_hoverSelected].entry.PopOut();
		}
	}
}

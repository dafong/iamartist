using BongoCat;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SelectionEntry : MonoBehaviour
{
	[SerializeField]
	private Image _fill;

	[SerializeField]
	private Image _icon;

	[SerializeField]
	private TooltipUiElement _tooltip;

	[SerializeField]
	private Color _selectedColor;

	[SerializeField]
	private Color _unselectedColor;

	public void Init(int index, int totalOptions, Sprite icon, string text)
	{
		_fill.fillAmount = 1f / (float)totalOptions;
		float num = 360f / (float)totalOptions;
		float num2 = (0f - num) * (float)index;
		_fill.transform.rotation = Quaternion.Euler(0f, 0f, num2);
		_icon.sprite = icon;
		float num3 = 0f - num2 + num / 2f;
		Vector3 vector = Quaternion.Euler(0f, 0f, 0f - num3) * Vector3.up;
		_icon.transform.localPosition = vector * 25f;
		_tooltip.SetText(text);
		_tooltip.Hide();
	}

	public void PopOut()
	{
		DOTween.Kill(base.transform);
		base.transform.DOScale(Vector3.one * 1.05f, 0.5f);
		_fill.color = _selectedColor;
		_tooltip.Show();
	}

	public void PopBack()
	{
		DOTween.Kill(base.transform);
		base.transform.DOScale(Vector3.one, 0.5f);
		_fill.color = _unselectedColor;
		_tooltip.Hide();
	}
}

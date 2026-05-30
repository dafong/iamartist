using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FlashAnimation : MonoBehaviour
{
	[SerializeField]
	private MaskableGraphic graphic;

	[SerializeField]
	private Color flashingColor;

	[SerializeField]
	private float flashDuration = 0.5f;

	private Color _baseColor;

	private Tween _flashTween;

	private void Awake()
	{
		_baseColor = graphic.color;
	}

	public void Flash()
	{
		_flashTween?.Kill();
		graphic.color = flashingColor;
		_flashTween = graphic.DOColor(_baseColor, flashDuration);
	}
}

using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeAnimation : MonoBehaviour
{
	private CanvasGroup canvasGroup;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
	}

	private void OnEnable()
	{
		base.transform.DOComplete();
		canvasGroup.alpha = 0f;
		canvasGroup.DOFade(1f, 0.2f);
	}
}

using BongoCat;
using DG.Tweening;
using UnityEngine;

public class OpenScaleAnimation : MonoBehaviour
{
	private void OnEnable()
	{
		PlayAnimation(Vector3.one);
	}

	public void PlayAnimation(Vector3 scale)
	{
		OnDemandRenderHelper.Instance?.ResumeRenderingForDuration(0.4f);
		base.transform.DOKill();
		base.transform.localScale = 0.2f * scale;
		base.transform.DOScale(scale, 0.4f).SetEase(Ease.OutBounce);
	}
}

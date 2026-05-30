using DG.Tweening;
using UnityEngine;

public class ShakeAnimation : MonoBehaviour
{
	[SerializeField]
	private float duration;

	[SerializeField]
	private float distance;

	[SerializeField]
	private int vibrato = 10;

	private Vector3 _startPos;

	private Tween _shakeTween;

	private void Start()
	{
		_startPos = base.transform.localPosition;
	}

	public void Shake()
	{
		_shakeTween?.Kill();
		_shakeTween = base.transform.DOShakePosition(duration, distance * Vector3.right, vibrato, 0f, snapping: false, fadeOut: true, ShakeRandomnessMode.Harmonic).OnComplete(delegate
		{
			base.transform.localPosition = _startPos;
		});
	}
}

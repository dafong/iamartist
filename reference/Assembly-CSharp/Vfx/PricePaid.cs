using System;
using System.Collections;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Vfx
{
	public class PricePaid : MonoBehaviour
	{
		[SerializeField]
		private Vector3 _relativeEndPosition;

		[SerializeField]
		private Transform _uiAnchor;

		private Vector3 _originalPosition;

		private CanvasGroup _canvasGroup;

		private void Awake()
		{
			_canvasGroup = GetComponent<CanvasGroup>();
		}

		[ContextMenu("Animate")]
		public void Animate()
		{
			base.gameObject.SetActive(value: true);
			StartCoroutine(AnimationRoutine());
		}

		private IEnumerator AnimationRoutine()
		{
			_originalPosition = _uiAnchor.position;
			base.transform.DOMove(base.transform.position + _relativeEndPosition * PlayerPrefs.GetFloat("Scale", 1f), 1.5f);
			yield return new WaitForSeconds(1f);
			TweenerCore<float, float, FloatOptions> tweenerCore = _canvasGroup.DOFade(0f, 0.6f);
			tweenerCore.onComplete = (TweenCallback)Delegate.Combine(tweenerCore.onComplete, new TweenCallback(ResetAnimation));
		}

		private void ResetAnimation()
		{
			StopAllCoroutines();
			base.transform.DOComplete();
			_canvasGroup.DOComplete();
			base.gameObject.SetActive(value: false);
			_canvasGroup.alpha = 1f;
			base.transform.position = _originalPosition;
		}
	}
}

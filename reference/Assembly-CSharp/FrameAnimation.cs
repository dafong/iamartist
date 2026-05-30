using System;
using System.Collections;
using System.Collections.Generic;
using BongoCat;
using UnityEngine;
using UnityEngine.UI;

public class FrameAnimation : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private List<Sprite> animationFrames;

	[SerializeField]
	private int actionAtIndex = -1;

	[SerializeField]
	public int fps;

	[SerializeField]
	private bool destroyAfterPlay = true;

	[SerializeField]
	private bool PlayOnAwake;

	[SerializeField]
	private bool loop;

	private Coroutine _animation;

	private bool _isPlaying;

	private Action _forcedAction;

	private float FrameLength => 1f / (float)fps;

	private void Awake()
	{
		if (PlayOnAwake)
		{
			PlayAnimation(null);
		}
	}

	public void PlayAnimation(Action onAction, bool forceAction = false)
	{
		if (_animation != null)
		{
			StopCoroutine(_animation);
		}
		image.gameObject.SetActive(value: true);
		_animation = StartCoroutine(Play(onAction));
		_isPlaying = true;
		if (forceAction)
		{
			_forcedAction = onAction;
		}
	}

	private IEnumerator Play(Action onAction)
	{
		OnDemandRenderHelper.Instance.ResumeRenderingForDuration((float)animationFrames.Count * FrameLength);
		WaitForSeconds waitForSeconds = new WaitForSeconds(FrameLength);
		for (int i = 0; i < animationFrames.Count || loop; i++)
		{
			image.sprite = animationFrames[i % animationFrames.Count];
			if (i == actionAtIndex)
			{
				onAction?.Invoke();
			}
			yield return waitForSeconds;
		}
		_forcedAction = null;
		_isPlaying = false;
		EndAnim();
	}

	private void EndAnim()
	{
		if (destroyAfterPlay)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			image.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		if (_isPlaying)
		{
			EndAnim();
			_forcedAction?.Invoke();
		}
	}
}

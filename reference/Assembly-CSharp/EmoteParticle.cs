using System;
using System.Collections.Generic;
using BongoCat.Emotes;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class EmoteParticle : MonoBehaviour
{
	[SerializeField]
	private Image emoteImage;

	[SerializeField]
	private Image trailImage;

	[SerializeField]
	private FrameAnimation explosion;

	public bool isDoneSending;

	private Color _baseColor;

	private bool _forceStopSending;

	private EmoteSpawner _spawner;

	private Dictionary<EmoteBehaviorOverride, Action> _behaviours;

	public void Init(EmoteSpawner spawner)
	{
		_baseColor = emoteImage.color;
		_spawner = spawner;
		_behaviours = new Dictionary<EmoteBehaviorOverride, Action>
		{
			{
				EmoteBehaviorOverride.None,
				FloatUp
			},
			{
				EmoteBehaviorOverride.Rocket,
				Rocket
			},
			{
				EmoteBehaviorOverride.AfkTimer,
				Afk
			}
		};
	}

	public void StartEmote(Sprite icon, EmoteBehaviorOverride behaviour)
	{
		StopAllCoroutines();
		emoteImage.sprite = icon;
		emoteImage.color = _baseColor;
		_behaviours[behaviour]?.Invoke();
	}

	private void Afk()
	{
		emoteImage.enabled = true;
		trailImage.enabled = false;
		float num = UnityEngine.Random.Range(0.6f, 0.8f);
		base.transform.position = base.transform.position + new Vector3(UnityEngine.Random.Range(-40, 40), 0f, 0f);
		base.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.35f, 0.85f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(base.transform.DOMove(base.transform.position + new Vector3(0f, 70f * num, 0f), 1.25f * num));
		sequence.Insert(1f * num, emoteImage.DOFade(0f, 1f * num));
		sequence.OnComplete(delegate
		{
			_forceStopSending = false;
			_spawner.ReturnParticleToPool(this);
		});
	}

	private void FloatUp()
	{
		emoteImage.enabled = true;
		trailImage.enabled = false;
		base.transform.localScale = Vector3.one;
		float num = UnityEngine.Random.Range(0.8f, 1f);
		Sequence sequence = DOTween.Sequence();
		sequence.Append(base.transform.DOMove(base.transform.position + new Vector3(0f, 450f * num, 0f), 3f * num));
		sequence.Insert(1.5f * num, emoteImage.DOFade(0f, 1.5f * num));
		sequence.OnComplete(delegate
		{
			_forceStopSending = false;
			_spawner.ReturnParticleToPool(this);
		});
	}

	private void Rocket()
	{
		emoteImage.enabled = false;
		trailImage.enabled = true;
		base.transform.localScale = Vector3.one;
		float num = UnityEngine.Random.Range(0.0004f, 0.0006f);
		Vector3 position = base.transform.position;
		base.transform.localScale = Vector3.one * 0.5f;
		float x = UnityEngine.Random.Range(0, Screen.mainWindowDisplayInfo.width);
		float y = UnityEngine.Random.Range((float)Screen.mainWindowDisplayInfo.height * 0.5f, Screen.mainWindowDisplayInfo.height);
		Vector3 vector = new Vector3(x, y);
		num *= (vector - position).magnitude;
		Sequence sequence = DOTween.Sequence();
		sequence.Append(base.transform.DOScale(Vector3.one, num));
		int num2 = 250;
		Vector3[] path = new Vector3[3]
		{
			vector,
			Vector3.Lerp(position, vector, 0.5f) + new Vector3(UnityEngine.Random.Range(-num2, num2), UnityEngine.Random.Range(-num2, num2), 0f),
			Vector3.Lerp(position, vector, 0.2f) + new Vector3(UnityEngine.Random.Range(-num2, num2), UnityEngine.Random.Range(-num2, num2), 0f)
		};
		sequence.Join(base.transform.DOPath(path, num, PathType.CubicBezier).SetEase(Ease.Linear));
		sequence.OnComplete(delegate
		{
			FrameAnimation frameAnimation = UnityEngine.Object.Instantiate(explosion, base.transform.position, Quaternion.identity, base.transform.parent);
			frameAnimation.PlayAnimation(null);
			frameAnimation.transform.localScale = UnityEngine.Random.Range(0.9f, 1.3f) * Vector3.one;
			frameAnimation.fps = UnityEngine.Random.Range(45, 60);
			Image component = frameAnimation.GetComponent<Image>();
			component.material = new Material(component.material);
			component.material.SetFloat("_ColorShift", UnityEngine.Random.Range(0f, 1f));
			_forceStopSending = false;
			_spawner.ReturnParticleToPool(this);
		});
	}

	private void OnDisable()
	{
		if (!_forceStopSending)
		{
			_forceStopSending = true;
			return;
		}
		DOTween.Kill(base.transform);
		DOTween.Kill(emoteImage);
		_spawner.ReturnParticleToPool(this);
	}
}

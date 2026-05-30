using System.Collections;
using BongoCat.Emotes;
using UnityEngine;

namespace BongoCat.Multiplayer
{
	public class AfkDetector : MonoBehaviour
	{
		[SerializeField]
		private Cat _cat;

		private float _afkTimer;

		private bool _isAfk;

		[SerializeField]
		private EmoteSpawner _emoteSpawner;

		[SerializeField]
		private Sprite _sleepyEmoteIcon;

		private void Start()
		{
			_cat.OnTap.AddListener(OnCatTap);
		}

		private void OnCatTap(int taps)
		{
			_afkTimer = 0f;
			if (_isAfk)
			{
				_isAfk = false;
			}
		}

		private IEnumerator Sleepy()
		{
			while (_isAfk)
			{
				_emoteSpawner.SpawnEmoteParticle(_sleepyEmoteIcon, EmoteBehaviorOverride.AfkTimer);
				yield return new WaitForSeconds(Random.Range(1.5f, 2.5f));
			}
		}

		private void Update()
		{
			if (!SettingsManager.Instance.AfkIndicator.Value)
			{
				_isAfk = false;
			}
			else if (!_isAfk)
			{
				_afkTimer += Time.deltaTime;
				if (_afkTimer >= 120f)
				{
					_isAfk = true;
					StartCoroutine(Sleepy());
				}
			}
		}
	}
}

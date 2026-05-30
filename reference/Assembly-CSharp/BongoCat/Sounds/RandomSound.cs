using System.Collections.Generic;
using UnityEngine;

namespace BongoCat.Sounds
{
	public class RandomSound : MonoBehaviour
	{
		[SerializeField]
		private List<AudioClip> _audioClips;

		[SerializeField]
		private AudioSource _audioSource;

		[SerializeField]
		private bool _logging;

		private int _index;

		private void Start()
		{
			BongoAudioSettings.Instance.RegisterAudioSource(_audioSource);
		}

		public void PlayRandomClip()
		{
			AudioClip audioClip = _audioClips[Random.Range(0, _audioClips.Count)];
			_audioSource.PlayOneShot(audioClip);
			if (_logging)
			{
				Debug.Log("Playing: " + audioClip.name);
			}
		}

		private void OnDestroy()
		{
			BongoAudioSettings.Instance.UnregisterAudioSource(_audioSource);
		}
	}
}

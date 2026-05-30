using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat
{
	public class BongoAudioSettings : MonoBehaviour
	{
		public static BongoAudioSettings Instance;

		private List<AudioSource> _audioSources = new List<AudioSource>();

		[SerializeField]
		private Slider _volumeSlider;

		[SerializeField]
		private Image _mutedVisual;

		[SerializeField]
		private Image _unmutedVisual;

		private float _volume;

		private bool _muted;

		private const string MUTE_KEY = "MUTE";

		private const string VOLUME_KEY = "VOLUME";

		private void Awake()
		{
			Instance = this;
			if (PlayerPrefs.HasKey("MUTE"))
			{
				_muted = PlayerPrefs.GetInt("MUTE") == 1;
			}
			else
			{
				_muted = false;
			}
			MonoBehaviour.print($"Muted: {_muted}");
			_volume = PlayerPrefs.GetFloat("VOLUME", 0f);
			MonoBehaviour.print($"Volume: {_volume}");
			_volumeSlider.value = _volume;
			foreach (AudioSource audioSource in _audioSources)
			{
				audioSource.volume = _volume;
			}
			if (_muted)
			{
				Mute();
			}
			else
			{
				Unmute();
			}
		}

		public void OnValueChanged(float value)
		{
			_volume = value;
			if (_muted && _volume > 0f)
			{
				Unmute();
			}
			foreach (AudioSource audioSource in _audioSources)
			{
				audioSource.volume = _volume;
			}
			PlayerPrefs.SetFloat("VOLUME", _volume);
			PlayerPrefs.Save();
			if ((double)_volume < 0.01)
			{
				Mute();
			}
		}

		public void ToggleMute()
		{
			if (_muted)
			{
				Unmute();
			}
			else
			{
				Mute();
			}
		}

		private void Mute()
		{
			_muted = true;
			PlayerPrefs.SetInt("MUTE", 1);
			PlayerPrefs.Save();
			MonoBehaviour.print(string.Format("Mute: {0}", PlayerPrefs.GetInt("MUTE")));
			foreach (AudioSource audioSource in _audioSources)
			{
				audioSource.mute = _muted;
			}
			_mutedVisual.enabled = true;
			_unmutedVisual.enabled = false;
		}

		private void Unmute()
		{
			_muted = false;
			PlayerPrefs.SetInt("MUTE", 0);
			MonoBehaviour.print(string.Format("Mute: {0}", PlayerPrefs.GetInt("MUTE")));
			PlayerPrefs.Save();
			foreach (AudioSource audioSource in _audioSources)
			{
				audioSource.mute = _muted;
			}
			_mutedVisual.enabled = false;
			_unmutedVisual.enabled = true;
		}

		public void RegisterAudioSource(AudioSource audioSource)
		{
			_audioSources.Add(audioSource);
			audioSource.volume = _volume;
			audioSource.mute = _muted;
		}

		public void UnregisterAudioSource(AudioSource audioSource)
		{
			_audioSources.Remove(audioSource);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using BongoCat.Localizer;
using TMPro;
using UnityEngine;

namespace BongoCat
{
	public class ScaleSetting : MonoBehaviour
	{
		private int _index;

		private int _unappliedIndex;

		[SerializeField]
		private GameObject _applyButton;

		[SerializeField]
		private MainScreenSetting _mainScreenSetting;

		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private List<float> _scales;

		[SerializeField]
		private List<Scaler> _scalers;

		[SerializeField]
		private string KEY;

		public int Index => _index;

		private IEnumerator Start()
		{
			yield return null;
			_scales = RealWindowScale.Instance.GetScales(_scales);
			float num = PlayerPrefs.GetFloat(KEY, 1f);
			_index = _scales.IndexOf(num);
			if (_index == -1)
			{
				_index = 0;
				for (int i = 0; i < _scales.Count && !(_scales[i] >= num); i++)
				{
					_index = i;
				}
			}
			_unappliedIndex = _index;
			_mainScreenSetting.ScreenSwitched += UpdateScale;
			UpdateScale();
		}

		public void ToggleForward()
		{
			_unappliedIndex = (_unappliedIndex + 1) % _scales.Count;
			SetUnappliedScaleIndex(_unappliedIndex);
		}

		public void ToggleBackward()
		{
			_unappliedIndex = (_unappliedIndex - 1 + _scales.Count) % _scales.Count;
			SetUnappliedScaleIndex(_unappliedIndex);
		}

		public void Reset()
		{
			SetUnappliedScaleIndex(_index);
		}

		private void SetUnappliedScaleIndex(int newIndex)
		{
			_unappliedIndex = newIndex;
			_applyButton.SetActive(_unappliedIndex != _index);
			DisplayText(_scales[_unappliedIndex]);
		}

		public void UpdateScale()
		{
			_index = _unappliedIndex;
			float num = _scales[_index];
			foreach (Scaler scaler in _scalers)
			{
				scaler.SetScale(_index);
			}
			PlayerPrefs.SetFloat(KEY, num);
			PlayerPrefs.Save();
			DisplayText(num);
			_applyButton.SetActive(value: false);
		}

		private void DisplayText(float scale)
		{
			_text.text = $"{Loca.Instance.Get(KEY)} {scale:G3}x";
		}

		public float GetRealScaleFactor()
		{
			int val = (int)(450f * RealWindowScale.DpiScale * _scales[_index]);
			val = Math.Min(Screen.mainWindowDisplayInfo.height, val);
			float num = (float)val / 450f;
			if (!((double)num <= 0.01))
			{
				return num;
			}
			return 1f;
		}

		public void ResetScale()
		{
			_index = 2;
			UpdateScale();
		}

		public void UpdateIndex(int index)
		{
			_unappliedIndex = index;
			UpdateScale();
		}

		public void AddScaler(Scaler scaler)
		{
			_scalers.Add(scaler);
		}

		public void RemoveScaler(Scaler scaler)
		{
			_scalers.Remove(scaler);
		}

		public float GetScale()
		{
			return _scales[_index];
		}

		public List<float> GetAvailableScales()
		{
			return _scales;
		}
	}
}

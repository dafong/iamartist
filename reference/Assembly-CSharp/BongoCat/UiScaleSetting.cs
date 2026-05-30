using System;
using System.Collections;
using System.Collections.Generic;
using BongoCat.Localizer;
using TMPro;
using UnityEngine;
using Vfx;

namespace BongoCat
{
	public class UiScaleSetting : MonoBehaviour
	{
		[SerializeField]
		private List<float> _scales;

		private int _index;

		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private MainScreenSetting _mainScreenSetting;

		private List<UiScaler> _uiScalers = new List<UiScaler>();

		private const string KEY = "UiScale";

		public int Index => _index;

		private IEnumerator Start()
		{
			yield return null;
			_scales = RealWindowScale.Instance.GetScales(_scales);
			float num = PlayerPrefs.GetFloat("UiScale", 1f);
			_index = _scales.IndexOf(num);
			if (_index == -1)
			{
				_index = 0;
				for (int i = 0; i < _scales.Count && !(_scales[i] >= num); i++)
				{
					_index = i;
				}
			}
			_mainScreenSetting.ScreenSwitched += UpdateScale;
			UpdateScale();
		}

		public void ToggleForward()
		{
			_index = (_index + 1) % _scales.Count;
			UpdateScale();
		}

		public void ToggleBackward()
		{
			_index = (_index - 1 + _scales.Count) % _scales.Count;
			UpdateScale();
		}

		private void UpdateScale()
		{
			float num = _scales[_index];
			foreach (UiScaler uiScaler in _uiScalers)
			{
				uiScaler.SetScale(_index);
			}
			PlayerPrefs.SetFloat("UiScale", num);
			PlayerPrefs.Save();
			_text.text = string.Format("{0} {1:G3}x", Loca.Instance.Get("UiScale"), num);
		}

		public void ResetScale()
		{
			_index = 2;
			UpdateScale();
		}

		public void AddScaler(UiScaler scaler)
		{
			_uiScalers.Add(scaler);
		}

		public void RemoveScaler(UiScaler scaler)
		{
			_uiScalers.Remove(scaler);
		}

		public float GetRealScaleFactor()
		{
			int val = (int)(450f * (Screen.dpi / 96f) * _scales[_index]);
			val = Math.Min(Screen.mainWindowDisplayInfo.height, val);
			float num = (float)val / 450f;
			if (!((double)num <= 0.01))
			{
				return num;
			}
			return 1f;
		}

		public float GetScale()
		{
			return _scales[_index];
		}
	}
}

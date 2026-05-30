using System.Collections;
using BongoCat;
using BongoCat.Localizer;
using TMPro;
using UnityEngine;

namespace Vfx
{
	public class CatScaler : Scaler
	{
		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private bool useShortText;

		[SerializeField]
		private bool _isMainCat;

		private ScaleSetting _catScaleSetting;

		private PlayerPrefsToggle _scaleAllCats;

		private IEnumerator Start()
		{
			_catScaleSetting = SettingsManager.Instance.CatScaleSetting;
			_scaleAllCats = SettingsManager.Instance.ScaleAllCats;
			yield return null;
			yield return null;
			Index = _catScaleSetting.Index;
			UpdateScale();
			_scaleAllCats.OnToggleUpdated.AddListener(OnScaleAllCats);
			OnScaleAllCats(_scaleAllCats.Value);
		}

		private void OnScaleAllCats(bool toggle)
		{
			if (!_isMainCat)
			{
				if (toggle)
				{
					_catScaleSetting.AddScaler(this);
				}
				else
				{
					_catScaleSetting.RemoveScaler(this);
				}
			}
		}

		public void ToggleForward()
		{
			Index = (Index + 1) % _catScaleSetting.GetAvailableScales().Count;
			if (_scaleAllCats.Value)
			{
				_catScaleSetting.UpdateIndex(Index);
			}
			else
			{
				UpdateScale();
			}
		}

		public void ToggleBackward()
		{
			Index = (Index - 1) % _catScaleSetting.GetAvailableScales().Count;
			if (_scaleAllCats.Value)
			{
				_catScaleSetting.UpdateIndex(Index);
			}
			else
			{
				UpdateScale();
			}
		}

		protected override void UpdateScale()
		{
			float realScaleFactor = _catScaleSetting.GetRealScaleFactor();
			if ((bool)targetTransform)
			{
				int num = 1;
				if ((double)targetTransform.localScale.x < 0.1)
				{
					num = -1;
				}
				targetTransform.localScale = new Vector3((float)num * realScaleFactor, realScaleFactor, realScaleFactor);
			}
			if ((bool)_text)
			{
				_text.text = (useShortText ? realScaleFactor.ToString("0.00") : string.Format("{0} {1:G3}x", Loca.Instance.Get("Scale"), realScaleFactor));
			}
		}

		private void OnDestroy()
		{
			_scaleAllCats.OnToggleUpdated.RemoveListener(OnScaleAllCats);
		}
	}
}

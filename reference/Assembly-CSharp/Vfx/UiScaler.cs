using System.Collections;
using BongoCat;
using UnityEngine;

namespace Vfx
{
	public class UiScaler : Scaler
	{
		private Vector3 _originalScale = Vector3.negativeInfinity;

		public Vector3 OriginalScaleVector
		{
			get
			{
				if (_originalScale == Vector3.negativeInfinity)
				{
					_originalScale = targetTransform.localScale;
				}
				return _originalScale;
			}
		}

		private IEnumerator Start()
		{
			yield return null;
			yield return null;
			Index = SettingsManager.Instance.UIScaleSetting.Index;
			UpdateScale();
			SettingsManager.Instance.UIScaleSetting.AddScaler(this);
		}

		private void OnDestroy()
		{
			SettingsManager.Instance.UIScaleSetting.RemoveScaler(this);
		}

		protected override void UpdateScale()
		{
			float realScaleFactor = SettingsManager.Instance.UIScaleSetting.GetRealScaleFactor();
			targetTransform.localScale = new Vector3(realScaleFactor, realScaleFactor, realScaleFactor);
		}
	}
}

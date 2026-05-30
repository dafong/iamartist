using System.Collections;
using UnityEngine;

namespace BongoCat
{
	public class RenderOnEnable : MonoBehaviour
	{
		private bool _initialized;

		private IEnumerator Start()
		{
			yield return null;
			_initialized = true;
			OnDemandRenderHelper.Instance.ResumeRendering();
		}

		private void OnEnable()
		{
			if (_initialized)
			{
				OnDemandRenderHelper.Instance.ResumeRendering();
			}
		}

		private void OnDisable()
		{
			if (_initialized)
			{
				OnDemandRenderHelper.Instance.TryPauseRendering();
			}
		}
	}
}

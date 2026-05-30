using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace BongoCat
{
	public class OnDemandRenderHelper : MonoBehaviour
	{
		public static OnDemandRenderHelper Instance;

		private PlayerPrefsToggle[] _demandingConstantRendering;

		private Leaderboard _leaderboard;

		private int _renderDemand;

		private bool _canPauseRendering;

		private bool RenderConstantly
		{
			get
			{
				if (!_leaderboard.IsVisible)
				{
					return _demandingConstantRendering.Any((PlayerPrefsToggle toggle) => toggle.Value);
				}
				return true;
			}
		}

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			_leaderboard = Leaderboard.Instance;
			_demandingConstantRendering = new PlayerPrefsToggle[2]
			{
				SettingsManager.Instance.CatBobbing,
				SettingsManager.Instance.AlwaysShowChest
			};
			yield return new WaitUntil(() => CatInventory.Instance.WasLoadedFromSteam);
			TryPauseRendering();
			Leaderboard leaderboard = _leaderboard;
			leaderboard.OnShow = (Action<bool>)Delegate.Combine(leaderboard.OnShow, new Action<bool>(CheckConstantRenderDemand));
			PlayerPrefsToggle[] demandingConstantRendering = _demandingConstantRendering;
			for (int num = 0; num < demandingConstantRendering.Length; num++)
			{
				demandingConstantRendering[num].OnToggleUpdated.AddListener(CheckConstantRenderDemand);
			}
			if (RenderConstantly)
			{
				ResumeRendering();
			}
		}

		public void ResumeRenderingForDuration(float duration)
		{
			StartCoroutine(ResumeRenderingForDurationRoutine(duration));
		}

		private IEnumerator ResumeRenderingForDurationRoutine(float duration)
		{
			ResumeRendering();
			yield return new WaitForSeconds(duration);
			TryPauseRendering();
		}

		private void CheckConstantRenderDemand(bool toggle)
		{
			if (RenderConstantly)
			{
				ResumeRendering();
			}
		}

		public void TryPauseRendering()
		{
			if (_canPauseRendering)
			{
				_renderDemand = Mathf.Max(0, _renderDemand - 1);
				if (_renderDemand <= 0 && !RenderConstantly)
				{
					OnDemandRendering.renderFrameInterval = 100000;
				}
			}
		}

		public void ResumeRendering()
		{
			if (_canPauseRendering)
			{
				_renderDemand++;
				OnDemandRendering.renderFrameInterval = 1;
			}
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (!hasFocus)
			{
				_canPauseRendering = true;
			}
		}
	}
}

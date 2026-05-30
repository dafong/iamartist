using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BongoCat
{
	public class Cat : MonoBehaviour
	{
		[FormerlySerializedAs("_openDuration")]
		[SerializeField]
		private float _hitDuration;

		[SerializeField]
		private Image _catImage;

		[SerializeField]
		private GameObject _visuals;

		[SerializeField]
		private Transform _bobTransform;

		[SerializeField]
		private Image _catFront;

		[SerializeField]
		private Sprite _catNormalDefault;

		[SerializeField]
		private Sprite _catLeftDefault;

		[SerializeField]
		private Sprite _catNormalFrontDefault;

		[SerializeField]
		private Sprite _catRightFrontDefault;

		[SerializeField]
		private Sprite _catNormal;

		[SerializeField]
		private Sprite _catLeft;

		[SerializeField]
		private Sprite _catNormalFront;

		[SerializeField]
		private Sprite _catRightFront;

		[SerializeField]
		private GameObject _hatObject;

		[SerializeField]
		private FrameAnimation _popUpImage;

		private bool _wasLeft;

		private Coroutine _routineLeft;

		private Coroutine _routineRight;

		private Sequence _sequence;

		public UnityEvent<int> OnTap;

		private IEnumerator Start()
		{
			if ((bool)_popUpImage)
			{
				_catImage.gameObject.SetActive(value: false);
				_catFront.gameObject.SetActive(value: false);
				_hatObject.SetActive(value: false);
				_popUpImage.PlayAnimation(delegate
				{
					_catImage.gameObject.SetActive(value: true);
					_catFront.gameObject.SetActive(value: true);
					_hatObject.SetActive(value: true);
				}, forceAction: true);
			}
			else
			{
				_catImage.gameObject.SetActive(value: true);
				_catFront.gameObject.SetActive(value: true);
				_hatObject.SetActive(value: true);
			}
			yield return new WaitForSeconds(Random.Range(0f, 0.5f));
			BounceSequence();
			SettingsManager.Instance.CatBobbing.OnToggleUpdated.AddListener(ToggleBobbingAnimation);
			ToggleBobbingAnimation(SettingsManager.Instance.CatBobbing.Value);
		}

		public Sprite GetBaseSprite()
		{
			return _catNormal;
		}

		public Sprite GetFrontSprite()
		{
			return _catNormalFront;
		}

		public void SetSkin(string itemName)
		{
			if (string.IsNullOrEmpty(itemName))
			{
				_catNormal = _catNormalDefault;
				_catLeft = _catLeftDefault;
				_catNormalFront = _catNormalFrontDefault;
				_catRightFront = _catRightFrontDefault;
				_catImage.sprite = _catNormal;
				_catFront.sprite = _catNormalFront;
			}
			else
			{
				itemName = itemName.Replace(" ", "");
				_catNormal = MemoryImageCache.Instance.GetSprite(itemName + "Left");
				_catLeft = MemoryImageCache.Instance.GetSprite(itemName + "LeftPunch");
				_catNormalFront = MemoryImageCache.Instance.GetSprite(itemName + "Right");
				_catRightFront = MemoryImageCache.Instance.GetSprite(itemName + "RightPunch");
				_catImage.sprite = _catNormal;
				_catFront.sprite = _catNormalFront;
			}
		}

		private void BounceSequence()
		{
			if (_sequence == null)
			{
				_sequence = DOTween.Sequence();
				_sequence.Append(_bobTransform.transform.DOScaleY(0.975f, 0.5f));
				_sequence.Append(_bobTransform.transform.DOScaleY(1f, 0.5f));
				_sequence.SetLoops(-1);
				_sequence.Play();
			}
		}

		public void ToggleBobbingAnimation(bool toggle)
		{
			if (toggle)
			{
				BounceSequence();
				return;
			}
			_sequence.Kill();
			base.transform.localScale = new Vector3(base.transform.localScale.x, 1f, base.transform.localScale.z);
			_sequence = null;
		}

		public void Tap(int amount)
		{
			OnTap?.Invoke(amount);
			if (_wasLeft)
			{
				if (_routineRight == null)
				{
					_catFront.sprite = _catRightFront;
					if (base.gameObject.activeInHierarchy)
					{
						_routineRight = StartCoroutine(CatHitRight());
					}
				}
			}
			else if (_routineLeft == null)
			{
				_catImage.sprite = _catLeft;
				if (base.gameObject.activeInHierarchy)
				{
					_routineLeft = StartCoroutine(CatHitLeft());
				}
			}
			_wasLeft = !_wasLeft;
		}

		private IEnumerator CatHitLeft()
		{
			OnDemandRenderHelper.Instance.ResumeRendering();
			yield return new WaitForSeconds(_hitDuration);
			_catImage.sprite = _catNormal;
			_routineLeft = null;
			OnDemandRenderHelper.Instance.TryPauseRendering();
		}

		private IEnumerator CatHitRight()
		{
			OnDemandRenderHelper.Instance.ResumeRendering();
			yield return new WaitForSeconds(_hitDuration);
			_catFront.sprite = _catNormalFront;
			_routineRight = null;
			OnDemandRenderHelper.Instance.TryPauseRendering();
		}

		private void OnEnable()
		{
			_catFront.sprite = _catNormalFront;
			_routineRight = null;
			_catImage.sprite = _catNormal;
			_routineLeft = null;
		}

		private void OnDisable()
		{
			SettingsManager.Instance.CatBobbing.OnToggleUpdated.RemoveListener(ToggleBobbingAnimation);
		}
	}
}

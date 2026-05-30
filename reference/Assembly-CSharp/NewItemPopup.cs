using System.Collections;
using System.Collections.Generic;
using BongoCat;
using BongoCat.Localizer;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewItemPopup : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private Sprite _testSprite;

	[SerializeField]
	private Image _border;

	[SerializeField]
	private Image _eventBadge;

	[SerializeField]
	private EventBadges _eventBadges;

	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private QualityColors _qualityColors;

	[SerializeField]
	private PlayerPrefsToggle _animateChestPurchase;

	private Queue<(SteamItem, bool)> _queue;

	private bool _showing;

	public void ShowPopup(SteamItem item, bool alreadyOwned)
	{
		if (!base.gameObject)
		{
			return;
		}
		if (_queue == null)
		{
			_queue = new Queue<(SteamItem, bool)>();
		}
		if (_showing)
		{
			_queue.Enqueue((item, alreadyOwned));
			return;
		}
		if (item == null || !_image)
		{
			Debug.LogError($"NewItemPopup.ShowPopup called with null item! {item == null} {(bool)_image}");
			return;
		}
		_text.text = (alreadyOwned ? Loca.Instance.Get("DuplicateHat") : Loca.Instance.Get("NewItem"));
		_image.sprite = item.Icon;
		_border.color = _qualityColors.GetColor(item.QualityCategory);
		bool flag = item.EventTag != null;
		if (flag)
		{
			_eventBadge.sprite = _eventBadges.GetEventBadge(item.EventTag);
		}
		_eventBadge.gameObject.SetActive(flag && (bool)_eventBadge.sprite);
		StopAllCoroutines();
		base.transform.DOKill();
		_image.transform.DOKill();
		base.gameObject.SetActive(value: true);
		_showing = true;
		StartCoroutine(DoShowPopup());
	}

	private void OnDisable()
	{
		HidePopup();
	}

	public void HidePopup()
	{
		if (_queue == null)
		{
			_queue = new Queue<(SteamItem, bool)>();
		}
		_showing = false;
		StopAllCoroutines();
		base.transform.DOKill();
		_image.transform.DOKill();
		_eventBadge.gameObject.SetActive(value: false);
		if (_queue.Count > 0)
		{
			(SteamItem, bool) tuple = _queue.Dequeue();
			ShowPopup(tuple.Item1, tuple.Item2);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator DoShowPopup()
	{
		if ((bool)_animateChestPurchase && !_animateChestPurchase.Value)
		{
			yield return new WaitForSeconds(3f);
			_showing = false;
			if (_queue.Count > 0)
			{
				(SteamItem, bool) tuple = _queue.Dequeue();
				ShowPopup(tuple.Item1, tuple.Item2);
			}
			else
			{
				base.gameObject.SetActive(value: false);
			}
			yield break;
		}
		base.transform.localScale = Vector3.zero;
		yield return new WaitForSeconds(0.5f);
		_image.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
		_image.transform.DORotate(new Vector3(0f, 0f, 0f), 0.4f);
		_image.transform.localScale = Vector3.zero;
		float imageScale = 1.5f;
		_image.transform.DOScale(imageScale, 0.4f);
		imageScale = 1f;
		float endValue = 1f * SettingsManager.Instance.UIScaleSetting.GetRealScaleFactor();
		base.transform.DOScale(endValue, 0.2f);
		yield return new WaitForSeconds(0.4f);
		_image.transform.DOScale(imageScale, 0.25f);
		yield return new WaitForSeconds(2.3f);
		endValue = 1.1f;
		base.transform.DOScale(endValue, 0.2f);
		yield return new WaitForSeconds(0.2f);
		base.transform.DOScale(0f, 0.2f);
		yield return new WaitForSeconds(0.2f);
		base.transform.localScale = Vector3.one;
		_showing = false;
		if (_queue.Count > 0)
		{
			(SteamItem, bool) tuple2 = _queue.Dequeue();
			ShowPopup(tuple2.Item1, tuple2.Item2);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}
}

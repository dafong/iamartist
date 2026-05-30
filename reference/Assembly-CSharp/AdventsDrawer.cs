using System.Collections;
using System.Linq;
using BongoCat;
using BongoCat.SteamJsonParser;
using DG.Tweening;
using Steam;
using UnityEngine;
using UnityEngine.UI;

public class AdventsDrawer : MonoBehaviour
{
	[SerializeField]
	private int date;

	[SerializeField]
	private CanvasGroup door;

	[SerializeField]
	private SteamBundleExchange _exchangeToExecute;

	[SerializeField]
	private Image _image;

	private bool _isOpen;

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => CatInventory.Instance.IsInitialized);
		if (CatInventory.Instance.HasItem(_exchangeToExecute.Output.First((SteamItemUnity i) => !i.IsOtherToken).Id))
		{
			door.gameObject.SetActive(value: false);
			_isOpen = true;
		}
	}

	public void TryOpen()
	{
		if (!_isOpen)
		{
			if (!TokenExchanger.Instance.IsExchanging && TokenExchanger.Instance.CanExchange(_exchangeToExecute))
			{
				Open();
			}
			else
			{
				FailToOpen();
			}
		}
	}

	private void Open()
	{
		door.transform.DOComplete();
		door.transform.DOScale(1.1f, 0.3f);
		door.transform.DOMove(door.transform.position + Vector3.down * 20f, 0.3f);
		door.DOFade(0f, 0.3f).SetEase(Ease.InQuint).onComplete = delegate
		{
			door.gameObject.SetActive(value: false);
		};
		TokenExchanger.Instance.Exchange(_exchangeToExecute);
		_isOpen = true;
	}

	private void FailToOpen()
	{
		door.transform.DOComplete();
		door.transform.localPosition = Vector3.zero;
		Sequence s = DOTween.Sequence();
		s.Append(door.transform.DOMove(door.transform.position + Vector3.down * 20f, 0.1f));
		s.Append(door.transform.DOMove(door.transform.position, 0.1f));
	}
}

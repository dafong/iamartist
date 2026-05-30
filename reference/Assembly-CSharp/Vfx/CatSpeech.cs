using System.Collections;
using BongoCat.Multiplayer;
using Crosstales.BWF;
using Crosstales.BWF.Model.Enum;
using Crosstales.Common.Util;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Vfx
{
	public class CatSpeech : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _speechText;

		[SerializeField]
		private GameObject _speechBubble;

		[SerializeField]
		private float _hoverRadius;

		[SerializeField]
		private int _maxChars;

		[SerializeField]
		private Transform _speechButton;

		[SerializeField]
		private TMP_InputField _speechTextfield;

		private GameObject _speechTextfieldGo;

		private bool _clickedOnObject;

		private bool _isMainCat;

		private void Awake()
		{
			if (!(_hoverRadius <= 0f))
			{
				_isMainCat = true;
				_speechTextfieldGo = _speechTextfield.gameObject;
				_speechTextfield.onValueChanged.AddListener(CheckTextfield);
			}
		}

		private void Update()
		{
			if (!_isMainCat || _speechBubble.activeSelf || (!MultiplayerLobby.Instance.IsInLobby && !_speechBubble.activeSelf))
			{
				return;
			}
			if (_speechTextfieldGo.activeSelf)
			{
				if (Input.GetMouseButtonDown(0))
				{
					StartCoroutine(CheckClick());
				}
				return;
			}
			if (Input.GetKeyDown(KeyCode.Return))
			{
				ShowTextField();
			}
			bool flag = false;
			Vector2 a = Input.mousePosition;
			Vector2 b = RectTransformUtility.WorldToScreenPoint(null, _speechButton.position);
			if (Vector2.Distance(a, b) < _hoverRadius)
			{
				flag = true;
			}
			if (!_speechButton.gameObject.activeSelf == flag)
			{
				_speechButton.gameObject.SetActive(flag);
			}
		}

		private IEnumerator CheckClick()
		{
			yield return null;
			if (!_clickedOnObject)
			{
				_speechTextfieldGo.SetActive(value: false);
			}
			else
			{
				_clickedOnObject = false;
			}
		}

		public void ShowTextField()
		{
			_speechTextfieldGo.SetActive(value: true);
			_speechTextfield.ActivateInputField();
		}

		private void CheckTextfield(string speech)
		{
			if (!speech.Contains("\n"))
			{
				int length = Mathf.Min(speech.Length, _maxChars);
				_speechTextfield.text = speech.Substring(0, length);
			}
			else
			{
				_speechTextfield.text = speech.Replace("\n", "").Replace("\r", "");
				SendSpeech();
			}
		}

		public void SendSpeech()
		{
			if (!Singleton<BWFManager>.Instance.Contains(_speechTextfield.text, ManagerMask.All))
			{
				SteamMultiplayer.Instance.SendSpeech(_speechTextfield.text);
			}
			_speechText.text = (string.IsNullOrEmpty(_speechTextfield.text) ? SteamMultiplayer.Instance.MeowText : _speechTextfield.text);
			_speechTextfield.text = "";
			_speechButton.gameObject.SetActive(value: false);
			_speechTextfieldGo.SetActive(value: false);
			ShowSpeech();
		}

		public void ShowMultiplayerSpeech(string speech)
		{
			_speechText.text = speech;
			ShowSpeech();
		}

		public void ShowSpeech()
		{
			_speechBubble.SetActive(value: true);
			StopAllCoroutines();
			StartCoroutine(DoShowSpeech());
		}

		private void OnDisable()
		{
			_speechBubble.SetActive(value: false);
			if (_isMainCat)
			{
				_speechTextfieldGo.SetActive(value: false);
			}
		}

		private IEnumerator DoShowSpeech()
		{
			Transform bubbleTransform = _speechBubble.transform;
			bubbleTransform.DOKill();
			bubbleTransform.localScale = new Vector3(0.4f, 0.8f, 0.8f);
			bubbleTransform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutQuad);
			yield return new WaitForSeconds(2f + (float)_speechText.text.Length * 0.1f);
			bubbleTransform.DOScale(new Vector3(0.3f, 0.8f, 0.8f), 0.15f).SetEase(Ease.InQuad);
			yield return new WaitForSeconds(0.15f);
			_speechBubble.gameObject.SetActive(value: false);
		}

		public void ClickedOnObject()
		{
			_clickedOnObject = true;
		}
	}
}

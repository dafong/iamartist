using System;
using TMPro;
using UnityEngine;

namespace BongoCat.Localizer
{
	[RequireComponent(typeof(TMP_Text))]
	public class LocalizedText : MonoBehaviour
	{
		[SerializeField]
		private string _key;

		private TMP_Text _text;

		private void Start()
		{
			Loca instance = Loca.Instance;
			instance.OnLanguageChanged = (Action)Delegate.Combine(instance.OnLanguageChanged, new Action(UpdateText));
			UpdateText();
		}

		private void UpdateText()
		{
			if (!_text)
			{
				_text = GetComponent<TMP_Text>();
			}
			_text.text = Loca.Instance.Get(_key);
		}

		public void UpdateKey(string key)
		{
			_key = key;
			UpdateText();
		}

		private void OnDestroy()
		{
			if ((bool)Loca.Instance)
			{
				Loca instance = Loca.Instance;
				instance.OnLanguageChanged = (Action)Delegate.Remove(instance.OnLanguageChanged, new Action(UpdateText));
			}
		}
	}
}

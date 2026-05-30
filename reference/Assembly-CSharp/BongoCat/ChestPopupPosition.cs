using System;
using System.Collections.Generic;
using BongoCat.Localizer;
using TMPro;
using UnityEngine;

namespace BongoCat
{
	public class ChestPopupPosition : MonoBehaviour
	{
		[SerializeField]
		private List<Transform> _transformPresets;

		[SerializeField]
		private Transform _chestPopup;

		[SerializeField]
		private Transform _emojiChestPopup;

		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private string _locaBaseKey;

		private int _index;

		private const string CHEST_POSITION_KEY = "CHEST_POSITION";

		private void Awake()
		{
			_index = PlayerPrefs.GetInt("CHEST_POSITION", 0);
			_index = (_index + _transformPresets.Count) % _transformPresets.Count;
		}

		private void Start()
		{
			Loca instance = Loca.Instance;
			instance.OnLanguageChanged = (Action)Delegate.Combine(instance.OnLanguageChanged, new Action(UpdateText));
			UpdateText();
			UpdateSetting();
		}

		public void ToggleForward()
		{
			_index = (_index + 1) % _transformPresets.Count;
			UpdateSetting();
			UpdateText();
		}

		public void ToggleBackward()
		{
			_index = (_index - 1 + _transformPresets.Count) % _transformPresets.Count;
			UpdateSetting();
		}

		private void UpdateSetting()
		{
			_chestPopup.gameObject.SetActive(value: true);
			_chestPopup.position = _transformPresets[_index].position;
			int index = ((_index != _transformPresets.Count - 1) ? (_transformPresets.Count - 1) : 0);
			_emojiChestPopup.position = _transformPresets[index].position;
			UpdateText();
			PlayerPrefs.SetInt("CHEST_POSITION", _index);
			PlayerPrefs.Save();
		}

		private void UpdateText()
		{
			_text.text = $"{Loca.Instance.Get(_locaBaseKey)} {_index + 1}";
		}
	}
}

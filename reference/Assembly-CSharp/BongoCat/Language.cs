using System.Collections.Generic;
using BongoCat.Localizer;
using TMPro;
using UnityEngine;

namespace BongoCat
{
	public class Language : MonoBehaviour
	{
		private const string KEY = "LANGUAGE";

		[SerializeField]
		private List<string> _options;

		[SerializeField]
		private List<string> _optionsCode;

		private int _index;

		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private Loca _loca;

		private static readonly Dictionary<SystemLanguage, string> languageMap = new Dictionary<SystemLanguage, string>
		{
			{
				SystemLanguage.Afrikaans,
				"af"
			},
			{
				SystemLanguage.Arabic,
				"ar"
			},
			{
				SystemLanguage.Basque,
				"eu"
			},
			{
				SystemLanguage.Belarusian,
				"be"
			},
			{
				SystemLanguage.Bulgarian,
				"bg"
			},
			{
				SystemLanguage.Catalan,
				"ca"
			},
			{
				SystemLanguage.Chinese,
				"zh"
			},
			{
				SystemLanguage.Czech,
				"cs"
			},
			{
				SystemLanguage.Danish,
				"da"
			},
			{
				SystemLanguage.Dutch,
				"nl"
			},
			{
				SystemLanguage.English,
				"en"
			},
			{
				SystemLanguage.Estonian,
				"et"
			},
			{
				SystemLanguage.Faroese,
				"fo"
			},
			{
				SystemLanguage.Finnish,
				"fi"
			},
			{
				SystemLanguage.French,
				"fr"
			},
			{
				SystemLanguage.German,
				"de"
			},
			{
				SystemLanguage.Greek,
				"el"
			},
			{
				SystemLanguage.Hebrew,
				"he"
			},
			{
				SystemLanguage.Hungarian,
				"hu"
			},
			{
				SystemLanguage.Icelandic,
				"is"
			},
			{
				SystemLanguage.Indonesian,
				"id"
			},
			{
				SystemLanguage.Italian,
				"it"
			},
			{
				SystemLanguage.Japanese,
				"ja"
			},
			{
				SystemLanguage.Korean,
				"ko"
			},
			{
				SystemLanguage.Latvian,
				"lv"
			},
			{
				SystemLanguage.Lithuanian,
				"lt"
			},
			{
				SystemLanguage.Norwegian,
				"no"
			},
			{
				SystemLanguage.Polish,
				"pl"
			},
			{
				SystemLanguage.Portuguese,
				"pt"
			},
			{
				SystemLanguage.Romanian,
				"ro"
			},
			{
				SystemLanguage.Russian,
				"ru"
			},
			{
				SystemLanguage.SerboCroatian,
				"sh"
			},
			{
				SystemLanguage.Slovak,
				"sk"
			},
			{
				SystemLanguage.Slovenian,
				"sl"
			},
			{
				SystemLanguage.Spanish,
				"es"
			},
			{
				SystemLanguage.Swedish,
				"sv"
			},
			{
				SystemLanguage.Thai,
				"th"
			},
			{
				SystemLanguage.Turkish,
				"tr"
			},
			{
				SystemLanguage.Ukrainian,
				"uk"
			},
			{
				SystemLanguage.Vietnamese,
				"vi"
			},
			{
				SystemLanguage.ChineseSimplified,
				"zh-CN"
			},
			{
				SystemLanguage.ChineseTraditional,
				"zh-TW"
			},
			{
				SystemLanguage.Unknown,
				"unknown"
			}
		};

		private void Awake()
		{
			string text = PlayerPrefs.GetString("LANGUAGE", string.Empty);
			if (string.IsNullOrEmpty(text))
			{
				text = GetLanguageKey(Application.systemLanguage);
				Debug.Log("Default language code: " + text);
			}
			Debug.Log("Set language code to: " + text);
			_index = Mathf.Max(0, _optionsCode.IndexOf(text));
			UpdateValue();
		}

		public void ToggleForward()
		{
			_index = (_index + 1) % _options.Count;
			UpdateValue();
		}

		private void UpdateValue()
		{
			string text = _optionsCode[_index];
			PlayerPrefs.SetString("LANGUAGE", text);
			PlayerPrefs.Save();
			_text.text = _options[_index];
			_loca.SetLanguage(text);
		}

		public void ToggleBackward()
		{
			_index = (_index - 1 + _options.Count) % _options.Count;
			UpdateValue();
		}

		private string GetLanguageKey(SystemLanguage language)
		{
			if (!languageMap.TryGetValue(language, out var value))
			{
				return "en";
			}
			return value;
		}
	}
}

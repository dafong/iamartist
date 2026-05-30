using System;
using System.Collections.Generic;
using UnityEngine;

namespace BongoCat.Localizer
{
	public class Loca : MonoBehaviour
	{
		[Serializable]
		public class Language
		{
			public string Name;

			public string IsoCode;

			public List<Translation> Translations;
		}

		[Serializable]
		public struct Translation
		{
			public string Key;

			public string Value;
		}

		[SerializeField]
		public List<Language> _languages;

		private Dictionary<string, string> _fallback;

		private Dictionary<string, string> _currentSelectedLanguage;

		private const string LANGUAGE_KEY = "language";

		private static Loca _instance;

		public Action OnLanguageChanged;

		public static Loca Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = global::UnityEngine.Object.FindAnyObjectByType<Loca>();
					_instance?.Init();
				}
				return _instance;
			}
		}

		public string Get(string key)
		{
			if (_currentSelectedLanguage.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
			{
				return value;
			}
			return _fallback.GetValueOrDefault(key, key);
		}

		private void Init()
		{
			if (PlayerPrefs.HasKey("language"))
			{
				string language = PlayerPrefs.GetString("language");
				SetLanguage(language);
			}
			else
			{
				SetLanguage("en");
			}
		}

		public void SetLanguage(string isoCode)
		{
			foreach (Language language in _languages)
			{
				if (_fallback == null && language.IsoCode == "en")
				{
					_fallback = new Dictionary<string, string>();
					foreach (Translation translation in language.Translations)
					{
						_fallback[translation.Key] = translation.Value;
					}
				}
				if (!(language.IsoCode == isoCode))
				{
					continue;
				}
				_currentSelectedLanguage = new Dictionary<string, string>();
				foreach (Translation translation2 in language.Translations)
				{
					_currentSelectedLanguage[translation2.Key] = translation2.Value;
				}
				PlayerPrefs.SetString("language", isoCode);
				PlayerPrefs.Save();
				OnLanguageChanged?.Invoke();
			}
		}

		public string GetCurrentLanguageIsoCode()
		{
			return PlayerPrefs.GetString("language", "en");
		}

		public Dictionary<string, string> GetAllTranslationsForKey(string key)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (Language language in _languages)
			{
				foreach (Translation translation in language.Translations)
				{
					if (translation.Key == key)
					{
						dictionary.TryAdd(language.IsoCode, translation.Value);
					}
				}
			}
			return dictionary;
		}
	}
}

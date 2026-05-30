using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BongoCat.Localizer;
using TMPro;
using UnityEngine;

namespace BongoCat
{
	public class HatRandomizer : MonoBehaviour
	{
		[SerializeField]
		private List<float> _waitDurationInMinutes;

		[SerializeField]
		private CatInventory _catInventory;

		[SerializeField]
		private CatCosmetics _catCosmetics;

		[SerializeField]
		private PresetManager _presetManager;

		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private string _baseLocaKey;

		[SerializeField]
		private string _neverOptionLocaKey;

		[SerializeField]
		private string _minutesLocaKey;

		[SerializeField]
		private PlayerPrefsToggle _onlyRandomizeFavorites;

		[SerializeField]
		private PlayerPrefsToggle _onlyRandomizePresets;

		private int _index;

		private Coroutine _routine;

		private const string SETTING_KEY = "HAT_RANDOMIZER";

		private void Start()
		{
			_index = PlayerPrefs.GetInt("HAT_RANDOMIZER", 0);
			Loca instance = Loca.Instance;
			instance.OnLanguageChanged = (Action)Delegate.Combine(instance.OnLanguageChanged, new Action(UpdateText));
			UpdateWaitDuration();
		}

		public void ToggleForward()
		{
			_index = (_index + 1) % _waitDurationInMinutes.Count;
			UpdateWaitDuration();
		}

		public void ToggleBackward()
		{
			_index = (_index - 1 + _waitDurationInMinutes.Count) % _waitDurationInMinutes.Count;
			UpdateWaitDuration();
		}

		private void UpdateWaitDuration()
		{
			if (_routine != null)
			{
				StopCoroutine(_routine);
			}
			UpdateText();
			PlayerPrefs.SetInt("HAT_RANDOMIZER", _index);
			PlayerPrefs.Save();
			if (!(_waitDurationInMinutes[_index] <= 0f))
			{
				_routine = StartCoroutine(RandomizeSkinAndHatRoutine(_waitDurationInMinutes[_index] * 60f));
			}
		}

		private IEnumerator RandomizeSkinAndHatRoutine(float waitDuration)
		{
			while (true)
			{
				yield return new WaitForSeconds(waitDuration);
				RandomizeSkinAndHat();
			}
		}

		private void RandomizeSkinAndHat()
		{
			if (_onlyRandomizePresets.Value)
			{
				_presetManager.GetRandomPreset().Equip();
				return;
			}
			List<SteamItem> list = _catInventory.Items.Where((SteamItem item) => item.ItemSlot == "hat" && item.ItemAmount > 0 && !item.IsEquipped).ToList();
			List<SteamItem> list2 = _catInventory.Items.Where((SteamItem item) => item.ItemSlot == "skin" && item.ItemAmount > 0).ToList();
			if (_onlyRandomizeFavorites.Value)
			{
				list = list.Where((SteamItem item) => item.IsFavorite).ToList();
				list2 = list2.Where((SteamItem item) => item.IsFavorite && !item.IsEquipped).ToList();
			}
			if (list.Count > 0)
			{
				SteamItem steamItem = list[global::UnityEngine.Random.Range(0, list.Count)];
				_catCosmetics.EquipItem(steamItem);
			}
			if (list2.Count > 0)
			{
				SteamItem steamItem2 = list2[global::UnityEngine.Random.Range(0, list2.Count)];
				_catCosmetics.EquipItem(steamItem2, playAnimation: true, unequipIfSameItemIsEquipped: true);
			}
		}

		private void UpdateText()
		{
			if (_waitDurationInMinutes[_index] <= 0f)
			{
				_text.text = Loca.Instance.Get(_baseLocaKey) + " " + Loca.Instance.Get(_neverOptionLocaKey);
			}
			else
			{
				_text.text = $"{Loca.Instance.Get(_baseLocaKey)} {_waitDurationInMinutes[_index]}{Loca.Instance.Get(_minutesLocaKey)}";
			}
		}
	}
}

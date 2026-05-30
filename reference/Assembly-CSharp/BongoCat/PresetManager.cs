using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat
{
	public class PresetManager : MonoBehaviour
	{
		[SerializeField]
		private CatInventory _catInventory;

		[SerializeField]
		private CatCosmetics _catCosmetics;

		[SerializeField]
		private PlayerPrefsToggle _autoEquipPreset;

		private List<Preset> _presets = new List<Preset>();

		[SerializeField]
		private Transform _presetsRoot;

		[SerializeField]
		private Preset _presetPrefab;

		[SerializeField]
		private PlayerPrefsToggle _dynamicPresets;

		private const string PRESET_KEY = "PRESETS";

		private void Awake()
		{
			for (int i = 0; i < 5; i++)
			{
				CreateNewPreset();
			}
		}

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => _catInventory.WasLoadedFromSteam);
			Load();
			_dynamicPresets.OnToggleUpdated.AddListener(OnEnablePresetGrowthToggle);
			OnEnablePresetGrowthToggle(_dynamicPresets.Value);
		}

		private void Load()
		{
			if (!PlayerPrefs.HasKey("PRESETS"))
			{
				return;
			}
			List<Preset.PresetData> list = JsonConvert.DeserializeObject<List<Preset.PresetData>>(PlayerPrefs.GetString("PRESETS"));
			for (int i = 0; i < list.Count; i++)
			{
				if (i >= _presets.Count)
				{
					CreateNewPreset();
				}
				Preset.PresetData presetData = list[i];
				_presets[i].SetData(presetData.HatItemId, presetData.SkinItemId);
				_presets[i].LoadVisuals();
			}
			if (ShouldAddEmptyPreset())
			{
				CreateNewPreset();
			}
		}

		private bool ShouldAddEmptyPreset()
		{
			if (_presets.All((Preset preset) => preset.ContainsData))
			{
				return _dynamicPresets.Value;
			}
			return false;
		}

		private void CreateNewPreset()
		{
			Preset preset = Object.Instantiate(_presetPrefab, _presetsRoot);
			preset.Init(this, _catCosmetics);
			_presets.Add(preset);
		}

		public void Save()
		{
			if (ShouldAddEmptyPreset())
			{
				CreateNewPreset();
			}
			_presets = _presets.OrderByDescending((Preset preset) => preset.ContainsData ? 1 : 0).ToList();
			if (_presets.Count > 5 && _presets.Count((Preset preset) => !preset.ContainsData) >= 1)
			{
				int num = _presets.FindIndex((Preset preset) => !preset.ContainsData);
				for (int num2 = _presets.Count - 1; num2 > num; num2--)
				{
					Object.Destroy(_presets[num2].gameObject);
					_presets.RemoveAt(num2);
				}
				if (!_dynamicPresets.Value)
				{
					Object.Destroy(_presets[num].gameObject);
					_presets.RemoveAt(num);
				}
			}
			foreach (Preset preset in _presets)
			{
				preset.transform.SetAsLastSibling();
			}
			string value = JsonConvert.SerializeObject((from preset in _presets
				where preset.ContainsData
				select preset.Data).ToList());
			PlayerPrefs.SetString("PRESETS", value);
			if (_presets.Count % 5 == 1 || _presets.Count % 5 == 0)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(_presetsRoot as RectTransform);
			}
		}

		public Preset GetRandomPreset()
		{
			List<Preset> list = _presets.Where((Preset preset) => preset.ContainsData).ToList();
			return list[Random.Range(0, list.Count)];
		}

		private void OnEnablePresetGrowthToggle(bool toggle)
		{
			if (toggle && ShouldAddEmptyPreset())
			{
				CreateNewPreset();
			}
			else
			{
				if (toggle || _presets.Count <= 5)
				{
					return;
				}
				List<Preset> presets = _presets;
				if (!presets[presets.Count - 1].ContainsData)
				{
					List<Preset> presets2 = _presets;
					Preset preset = presets2[presets2.Count - 1];
					_presets.RemoveAt(_presets.Count - 1);
					Object.Destroy(preset.gameObject);
					if (_presets.Count % 5 == 1 || _presets.Count % 5 == 0)
					{
						LayoutRebuilder.ForceRebuildLayoutImmediate(_presetsRoot as RectTransform);
					}
				}
			}
		}
	}
}

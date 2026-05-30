using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BongoCat
{
	public class Preset : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		[Serializable]
		public struct PresetData
		{
			public int SkinItemId;

			public int HatItemId;
		}

		[SerializeField]
		private GameObject _deleteIcon;

		[SerializeField]
		private Image _hatPreview;

		[SerializeField]
		private Image _skinPreview;

		[SerializeField]
		private Image _pawPreview;

		[SerializeField]
		private Sprite _defaultSkin;

		[SerializeField]
		private Sprite _defaultPaw;

		[SerializeField]
		private GameObject _saveIcon;

		private PresetData _presetData;

		private PresetManager _presetManager;

		private CatInventory _catInventory;

		private CatCosmetics _catCosmetics;

		private bool _containsData;

		private bool _isHovering;

		public CatInventory CatInventory
		{
			get
			{
				if (!_catInventory)
				{
					_catInventory = global::UnityEngine.Object.FindAnyObjectByType<CatInventory>();
				}
				return _catInventory;
			}
		}

		public PresetData Data => _presetData;

		public bool ContainsData => _containsData;

		public void Init(PresetManager presetManager, CatCosmetics catCosmetics)
		{
			_presetManager = presetManager;
			_catCosmetics = catCosmetics;
		}

		public void OnClick()
		{
			if (_containsData)
			{
				Equip();
			}
			else
			{
				Save();
			}
		}

		private void Save()
		{
			SteamItem steamItem = _catCosmetics.EquippedItems.FirstOrDefault((SteamItem item) => item.ItemSlot == "hat");
			SteamItem steamItem2 = _catCosmetics.EquippedItems.FirstOrDefault((SteamItem item) => item.ItemSlot == "skin");
			int hatId = -1;
			int skinId = -1;
			if (steamItem != null)
			{
				hatId = steamItem.SteamItemDefId;
			}
			if (steamItem2 != null)
			{
				skinId = steamItem2.SteamItemDefId;
			}
			SetData(hatId, skinId);
			LoadVisuals();
			_presetManager.Save();
			if (_isHovering)
			{
				_deleteIcon.SetActive(_containsData);
			}
		}

		public void LoadVisuals()
		{
			SteamItem steamItem = CatInventory.Items.FirstOrDefault((SteamItem item) => item.SteamItemDefId == _presetData.HatItemId);
			SteamItem steamItem2 = CatInventory.Items.FirstOrDefault((SteamItem item) => item.SteamItemDefId == _presetData.SkinItemId);
			_skinPreview.gameObject.SetActive(value: true);
			_pawPreview.gameObject.SetActive(value: true);
			if (steamItem != null)
			{
				_hatPreview.sprite = steamItem.FullImage;
				_hatPreview.enabled = _hatPreview.sprite != null;
			}
			if (steamItem2 != null)
			{
				string text = steamItem2.ItemName.Replace(" ", "");
				_skinPreview.sprite = MemoryImageCache.Instance.GetSprite(text + "Left");
				_pawPreview.sprite = MemoryImageCache.Instance.GetSprite(text + "Right");
			}
			_saveIcon.SetActive(value: false);
		}

		public void SetData(int hatId, int skinId)
		{
			_presetData = default(PresetData);
			_presetData.HatItemId = hatId;
			_presetData.SkinItemId = skinId;
			_containsData = true;
		}

		public void Equip()
		{
			SteamItem steamItem = _catCosmetics.EquippedItems.FirstOrDefault((SteamItem item) => item.ItemSlot == "hat");
			bool unequipIfSameItemIsEquipped = true;
			if (_presetData.HatItemId > 0)
			{
				steamItem = CatInventory.Items.FirstOrDefault((SteamItem item) => item.SteamItemDefId == _presetData.HatItemId && item.ItemAmount > 0);
				unequipIfSameItemIsEquipped = false;
			}
			if (steamItem != null)
			{
				_catCosmetics.EquipItem(steamItem, playAnimation: false, unequipIfSameItemIsEquipped);
			}
			SteamItem steamItem2 = _catCosmetics.EquippedItems.FirstOrDefault((SteamItem item) => item.ItemSlot == "skin");
			unequipIfSameItemIsEquipped = true;
			if (_presetData.SkinItemId > 0)
			{
				unequipIfSameItemIsEquipped = false;
				steamItem2 = CatInventory.Items.FirstOrDefault((SteamItem item) => item.SteamItemDefId == _presetData.SkinItemId && item.ItemAmount > 0);
			}
			if (steamItem2 != null)
			{
				_catCosmetics.EquipItem(steamItem2, playAnimation: false, unequipIfSameItemIsEquipped);
			}
		}

		public void Delete()
		{
			_presetData = default(PresetData);
			_containsData = false;
			_deleteIcon.SetActive(value: false);
			_hatPreview.enabled = false;
			_skinPreview.gameObject.SetActive(value: false);
			_pawPreview.gameObject.SetActive(value: false);
			_skinPreview.sprite = _defaultSkin;
			_pawPreview.sprite = _defaultPaw;
			_hatPreview.sprite = null;
			_saveIcon.SetActive(value: true);
			_presetManager.Save();
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_isHovering = true;
			_deleteIcon.SetActive(_containsData);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_isHovering = false;
			_deleteIcon.SetActive(value: false);
		}
	}
}

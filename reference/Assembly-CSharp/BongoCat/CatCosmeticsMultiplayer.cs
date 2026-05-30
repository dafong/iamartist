using System.Collections.Generic;
using System.Linq;
using BongoCat.TapTapLootIntegration;
using UnityEngine;
using UnityEngine.UI;
using Vfx;

namespace BongoCat
{
	public class CatCosmeticsMultiplayer : MonoBehaviour
	{
		[SerializeField]
		private Image _hatImage;

		[SerializeField]
		private Cat _cat;

		[SerializeField]
		private MultiplayerCatFlipper _flipVisuals;

		private Dictionary<string, int> _equippedItemIds;

		public List<SteamItem> EquippedItems;

		private void Awake()
		{
			EquippedItems = new List<SteamItem>();
			_equippedItemIds = new Dictionary<string, int>
			{
				{ "hat", -1 },
				{ "skin", -1 }
			};
		}

		public void UpdateFlip()
		{
			bool flag = _flipVisuals.IsFlipped && MemoryImageCache.Instance.LetterSprites.Contains(_hatImage.sprite);
			_hatImage.transform.localScale = (flag ? new Vector3(-1f, 1f, 1f) : Vector3.one);
		}

		public void EquipItem(SteamItem steamItem, bool playAnimation = true)
		{
			if (_equippedItemIds.Values.Contains(steamItem.SteamItemDefId))
			{
				return;
			}
			string itemSlot = steamItem.ItemSlot;
			if (!(itemSlot == "hat"))
			{
				if (itemSlot == "skin")
				{
					_cat.SetSkin(steamItem.ItemName);
					steamItem.IsEquipped = true;
					steamItem.OnItemUpdated?.Invoke();
					_equippedItemIds["skin"] = steamItem.SteamItemDefId;
				}
			}
			else
			{
				_hatImage.sprite = steamItem.FullImage;
				_hatImage.enabled = _hatImage.sprite != null;
				bool flag = _flipVisuals.IsFlipped && MemoryImageCache.Instance.LetterSprites.Contains(_hatImage.sprite);
				_hatImage.transform.localScale = (flag ? new Vector3(-1f, 1f, 1f) : Vector3.one);
				if (playAnimation)
				{
					_hatImage.GetComponent<OpenScaleAnimation>()?.PlayAnimation(flag ? new Vector3(-1f, 1f, 1f) : Vector3.one);
				}
				steamItem.IsEquipped = true;
				steamItem.OnItemUpdated?.Invoke();
				_equippedItemIds["hat"] = steamItem.SteamItemDefId;
			}
			EquippedItems.RemoveAll((SteamItem i) => i.ItemSlot == steamItem.ItemSlot);
			EquippedItems.Add(steamItem);
			OnDemandRenderHelper.Instance.ResumeRenderingForDuration(0.2f);
			Ipc.Instance.UpdateBuffs();
		}

		public void UnequipItem(string itemSlot)
		{
			string text = itemSlot;
			if (!(text == "hat"))
			{
				if (text == "skin")
				{
					_cat.SetSkin(null);
					_equippedItemIds["skin"] = -1;
				}
			}
			else
			{
				_hatImage.sprite = null;
				_hatImage.enabled = _hatImage.sprite != null;
				_equippedItemIds["hat"] = -1;
			}
			EquippedItems.RemoveAll((SteamItem i) => i.ItemSlot == itemSlot);
			OnDemandRenderHelper.Instance.ResumeRenderingForDuration(0.2f);
			Ipc.Instance.UpdateBuffs();
		}
	}
}

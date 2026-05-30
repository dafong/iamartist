using BongoCat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Steam
{
	public class TrashSlot : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;

		private SteamItem _itemSlotted;

		[SerializeField]
		private Image _borderImage;

		[SerializeField]
		private QualityColors _colors;

		[SerializeField]
		private GameObject _counterIndicator;

		[SerializeField]
		private TMP_Text _counter;

		private TrashCan _trashCan;

		private void Awake()
		{
			_trashCan = Object.FindAnyObjectByType<TrashCan>();
		}

		public void SetItem(SteamItem item)
		{
			SteamItem itemSlotted = _itemSlotted;
			_itemSlotted = item;
			if (item == null)
			{
				_icon.sprite = null;
				_borderImage.color = Color.clear;
				_counterIndicator.SetActive(value: false);
				itemSlotted?.OnItemUpdated?.Invoke();
			}
			else
			{
				int itemAmount = item.ItemAmount;
				_counter.text = itemAmount.ToString();
				_counterIndicator.SetActive(itemAmount > 1);
				_counter.color = _colors.GetColor(item.QualityCategory);
				_icon.sprite = item.Icon;
				_borderImage.color = _colors.GetColor(item.QualityCategory);
			}
		}

		public void OnClick()
		{
			if (_itemSlotted != null)
			{
				_trashCan.RemoveFromTrash(_itemSlotted);
			}
		}
	}
}

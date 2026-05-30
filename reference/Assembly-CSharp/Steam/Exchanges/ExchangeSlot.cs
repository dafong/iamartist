using BongoCat;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Steam.Exchanges
{
	public class ExchangeSlot : MonoBehaviour
	{
		[SerializeField]
		private Image _icon;

		public SteamItem ItemSlotted;

		[SerializeField]
		private Image _borderImage;

		[SerializeField]
		private QualityColors _colors;

		[SerializeField]
		private GameObject _counterIndicator;

		[SerializeField]
		private TMP_Text _counter;

		private ItemExchange _exchange;

		private int _slottedAmount;

		public int SlottedAmount => _slottedAmount;

		private void Awake()
		{
			_exchange = Object.FindAnyObjectByType<ItemExchange>();
		}

		private void OnDisable()
		{
			SetItem(null);
			CatInventory.Instance.UpdateItemsUI();
		}

		public void SetItem(SteamItem item, int amount = 1)
		{
			SteamItem itemSlotted = ItemSlotted;
			ItemSlotted = item;
			if (item == null)
			{
				_icon.sprite = null;
				_borderImage.color = Color.clear;
				_counterIndicator.SetActive(value: false);
				if (itemSlotted != null)
				{
					if (itemSlotted.CurrentlyInExchangeSlot > 0)
					{
						itemSlotted.CurrentlyInExchangeSlot -= _slottedAmount;
					}
					itemSlotted.OnItemUpdated?.Invoke();
				}
			}
			else
			{
				_counter.text = amount.ToString();
				_counterIndicator.SetActive(amount > 1);
				_counter.color = _colors.GetColor(item.QualityCategory);
				_slottedAmount = amount;
				item.CurrentlyInExchangeSlot += amount;
				item.OnItemUpdated?.Invoke();
				_icon.sprite = item.Icon;
				_borderImage.color = _colors.GetColor(item.QualityCategory);
			}
		}

		public void OnClick()
		{
			if (ItemSlotted != null)
			{
				SetItem(null);
				_exchange.UpdateInteractable();
				CatInventory.Instance.UpdateItemsUI();
			}
		}
	}
}

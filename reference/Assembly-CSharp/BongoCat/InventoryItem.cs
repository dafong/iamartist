using System;
using System.Collections;
using Steam;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BongoCat
{
	public class InventoryItem : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		[SerializeField]
		private bool _isEmote;

		[SerializeField]
		private Color _equippedColor;

		[SerializeField]
		private Color _unequippedColor;

		[SerializeField]
		private Image _itemImage;

		[SerializeField]
		private TMP_Text _itemAmount;

		public SteamItem SteamItem;

		[SerializeField]
		private Graphic _borderImage;

		[SerializeField]
		private Graphic _number;

		[SerializeField]
		private Image _isNewIndicator;

		[SerializeField]
		private GameObject _counterIndicator;

		[SerializeField]
		private Image _background;

		[SerializeField]
		private Image _disabledOverlay;

		[SerializeField]
		private Image _unownedOverlay;

		[SerializeField]
		private Image _eventBadge;

		[SerializeField]
		private Image _chestIcon;

		[SerializeField]
		private Image _equippedIcon;

		[SerializeField]
		private QualityColors _colors;

		[SerializeField]
		private EventBadges _eventBadges;

		[SerializeField]
		private Image _favoriteImage;

		[SerializeField]
		private Image _favoriteHover;

		[SerializeField]
		private Image _marketIcon;

		[SerializeField]
		private InventoryItemMarketLinks _marketLinks;

		[SerializeField]
		private TooltipUiElement _nameTooltip;

		private WaitForSeconds _hoverDelay = new WaitForSeconds(0.5f);

		private bool _hovering;

		private Coroutine _hoverTooltipRoutine;

		[SerializeField]
		private GameObject _iconPreview;

		[SerializeField]
		private Image _iconPreviewIcon;

		private bool _initialized;

		private bool _isPartOfBongoDex;

		public void SetItem(SteamItem steamItem, bool isPartOfBongoDex)
		{
			SteamItem = steamItem;
			base.gameObject.name = steamItem.ItemName;
			_isPartOfBongoDex = isPartOfBongoDex;
			SteamItem steamItem2 = SteamItem;
			steamItem2.OnItemUpdated = (Action)Delegate.Combine(steamItem2.OnItemUpdated, new Action(OnItemUpdated));
			OnItemUpdated();
		}

		private void OnDisable()
		{
			OnPointerExit(null);
		}

		private void OnDestroy()
		{
			if (SteamItem != null)
			{
				SteamItem steamItem = SteamItem;
				steamItem.OnItemUpdated = (Action)Delegate.Remove(steamItem.OnItemUpdated, new Action(OnItemUpdated));
			}
		}

		private void OnItemUpdated()
		{
			if (_initialized && SteamItem.ItemAmount == 0 && !_isPartOfBongoDex)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			if (_initialized && _isPartOfBongoDex && SteamItem.HideUntilReceived && SteamItem.ItemAmount == 0)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			_itemImage.sprite = SteamItem.Icon;
			_iconPreviewIcon.sprite = SteamItem.Icon;
			int displayedItemAmount = SteamItem.DisplayedItemAmount;
			if (displayedItemAmount >= 1000)
			{
				_itemAmount.text = (displayedItemAmount / 1000).ToString("D") + "k";
			}
			else
			{
				_itemAmount.text = SteamItem.DisplayedItemAmount.ToString();
			}
			_borderImage.color = _colors.GetColor(SteamItem.QualityCategory);
			_number.color = _colors.GetColor(SteamItem.QualityCategory);
			bool flag = !Shop.OnceEquippedItems.Contains(SteamItem.SteamItemDefId) && SteamItem.ItemAmount > 0;
			_isNewIndicator.gameObject.SetActive(flag && !_isPartOfBongoDex);
			_counterIndicator.gameObject.SetActive((SteamItem.IsConsumable && SteamItem.DisplayedItemAmount > 0) || (!flag && SteamItem.DisplayedItemAmount > 1 && !_isPartOfBongoDex));
			bool flag2 = ItemExchange.Instance.IsItemSlottable(SteamItem);
			_background.color = (SteamItem.IsEquipped ? _equippedColor : _unequippedColor);
			if ((bool)_equippedIcon)
			{
				_equippedIcon.enabled = SteamItem.IsEquipped;
			}
			bool flag3 = TrashCan.Instance.IsItemInTrash(SteamItem);
			_disabledOverlay.enabled = !flag2 || flag3;
			_favoriteImage.enabled = SteamItem.IsFavorite && !_isPartOfBongoDex;
			_favoriteHover.enabled = false;
			_marketIcon.enabled = false;
			_chestIcon.enabled = false;
			bool flag4 = SteamItem.EventTag != null;
			if (flag4)
			{
				_eventBadge.preserveAspect = true;
				_eventBadge.sprite = _eventBadges.GetEventBadge(SteamItem.EventTag);
			}
			_eventBadge.gameObject.SetActive(flag4 && (bool)_eventBadge.sprite);
			bool active = SteamItem.ItemAmount == 0 && _isPartOfBongoDex;
			_unownedOverlay.gameObject.SetActive(active);
			if (SteamItem.ItemAmount == 0 && SteamItem.IsActivePromo && _marketLinks.HasCustomLink(SteamItem))
			{
				_chestIcon.enabled = true;
			}
			base.gameObject.SetActive(value: true);
			_initialized = true;
		}

		public void ToggleFavorite()
		{
			if (!ItemExchange.Instance.IsVisible)
			{
				SteamItem.SetFavorite(!SteamItem.IsFavorite);
				_favoriteImage.enabled = SteamItem.IsFavorite;
				CatInventory.Instance.SortItems();
			}
		}

		public void OpenMarket()
		{
			if (_marketLinks.HasCustomLink(SteamItem) && SteamItem.IsActivePromo)
			{
				Application.OpenURL(_marketLinks.GetMarketLink(SteamItem));
			}
			else if (SteamItem.EventTag == "itemstore")
			{
				Application.OpenURL($"steam://openurl/https://store.steampowered.com/itemstore/{3419430}/detail/{SteamItem.SteamItemDefId}/");
			}
			else
			{
				Application.OpenURL($"steam://openurl/https://steamcommunity.com/market/listings/{3419430}/{Uri.EscapeUriString(SteamItem.ItemName)}");
			}
		}

		public void OnClick()
		{
			if (_isPartOfBongoDex && SteamItem.ItemAmount == 0 && SteamItem.IsActivePromo && _marketLinks.HasCustomLink(SteamItem))
			{
				Application.OpenURL(_marketLinks.GetMarketLink(SteamItem));
			}
			else if (_isPartOfBongoDex)
			{
				if (!_isEmote)
				{
					CatCosmetics.Instance.EquipItem(SteamItem, playAnimation: true, unequipIfSameItemIsEquipped: true);
				}
				_isNewIndicator.gameObject.SetActive(value: false);
			}
			else
			{
				if (!SteamItem.IsReady || SteamItem.ItemAmount == 0)
				{
					return;
				}
				if (_disabledOverlay.enabled)
				{
					if (TrashCan.Instance.IsItemInTrash(SteamItem))
					{
						TrashCan.Instance.RemoveFromTrash(SteamItem);
					}
					return;
				}
				if (ItemExchange.Instance.IsVisible)
				{
					if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && SteamItem.ItemAmount - SteamItem.CurrentlyInExchangeSlot - 1 > 1)
					{
						ItemExchange.Instance.SlotItem(SteamItem, SteamItem.ItemAmount - SteamItem.CurrentlyInExchangeSlot - 1);
					}
					else
					{
						ItemExchange.Instance.SlotItem(SteamItem);
					}
					return;
				}
				if (TrashCan.Instance.InTrashMode)
				{
					if (!TrashCan.Instance.IsItemInTrash(SteamItem))
					{
						TrashCan.Instance.AddToTrash(SteamItem);
					}
					return;
				}
				if (_isEmote)
				{
					EmoteDonut.Instance.ToggleItem(SteamItem);
					OnItemUpdated();
				}
				else
				{
					CatCosmetics.Instance.EquipItem(SteamItem, playAnimation: true, unequipIfSameItemIsEquipped: true);
				}
				_isNewIndicator.gameObject.SetActive(value: false);
				_counterIndicator.gameObject.SetActive(SteamItem.ItemAmount > 1 || SteamItem.IsConsumable);
				Shop.OnceEquippedItems.Add(SteamItem.SteamItemDefId);
			}
		}

		private IEnumerator ShowNameTooltipRoutine()
		{
			yield return _hoverDelay;
			if (_hovering)
			{
				_nameTooltip.Show();
				_iconPreview.SetActive(SettingsManager.Instance.ShowIconPreview);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_hovering = true;
			if (_isPartOfBongoDex)
			{
				if (SteamItem.ItemAmount == 0 && (SteamItem.Marketable || SteamItem.EventTag == "itemstore"))
				{
					_marketIcon.enabled = true;
				}
				_nameTooltip.SetText(SteamItem.ItemName);
				_hoverTooltipRoutine = StartCoroutine(ShowNameTooltipRoutine());
			}
			else if (!ItemExchange.Instance.IsVisible && SteamItem.ItemAmount != 0)
			{
				_favoriteHover.enabled = true;
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_favoriteHover.enabled = false;
			_marketIcon.enabled = false;
			_hovering = false;
			if ((bool)_nameTooltip)
			{
				if (_hoverTooltipRoutine != null)
				{
					StopCoroutine(_hoverTooltipRoutine);
					_hoverTooltipRoutine = null;
				}
				_nameTooltip.Hide();
			}
		}
	}
}

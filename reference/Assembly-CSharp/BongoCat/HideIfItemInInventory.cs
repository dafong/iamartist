using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BongoCat.SteamJsonParser;
using Steam;
using UnityEngine;

namespace BongoCat
{
	public class HideIfItemInInventory : MonoBehaviour
	{
		[SerializeField]
		private GameObject _visual;

		[SerializeField]
		private List<SteamItemUnity> _items;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => CatInventory.Instance.WasLoadedFromSteam);
			PromoItemChecker instance = PromoItemChecker.Instance;
			instance.OnItemUpdated = (Action<SteamItem>)Delegate.Combine(instance.OnItemUpdated, new Action<SteamItem>(OnItemUpdated));
			OnItemUpdated(null);
		}

		private void OnItemUpdated(SteamItem _)
		{
			if (_items == null)
			{
				_items = new List<SteamItemUnity>();
			}
			if (!_visual)
			{
				return;
			}
			bool active = !_items.All((SteamItemUnity item) => CatInventory.Instance.Items.Any((SteamItem i) => item.Id == i.SteamItemDefId && i.ItemAmount > 0));
			_visual.SetActive(active);
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BongoCat.SteamJsonParser;
using Steam;
using UnityEngine;

namespace BongoCat
{
	public class ShowIfItemInInventory : MonoBehaviour
	{
		[SerializeField]
		private GameObject _visual;

		[SerializeField]
		private int _itemId;

		[SerializeField]
		private List<SteamItemUnity> _otherOptionalItems;

		[SerializeField]
		private List<SteamBundleExchange> _optionalExchanges;

		[SerializeField]
		private bool _invert;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => CatInventory.Instance.WasLoadedFromSteam);
			CatInventory instance = CatInventory.Instance;
			instance.OnOtherTokenReceived = (Action)Delegate.Combine(instance.OnOtherTokenReceived, new Action(OnTokenUpdated));
			OnTokenUpdated();
		}

		private void OnTokenUpdated()
		{
			if (_otherOptionalItems == null)
			{
				_otherOptionalItems = new List<SteamItemUnity>();
			}
			if (CatInventory.Instance.OtherTokens == null || !_visual)
			{
				return;
			}
			bool flag = CatInventory.Instance.OtherTokens.ContainsKey(_itemId) || _otherOptionalItems.Exists((SteamItemUnity item) => CatInventory.Instance.Items.Any((SteamItem i) => item.Id == i.SteamItemDefId && i.ItemAmount > 0)) || _optionalExchanges.Any((SteamBundleExchange exc) => TokenExchanger.Instance.CanExchange(exc));
			if (_invert)
			{
				flag = !flag;
			}
			_visual.SetActive(flag);
		}
	}
}

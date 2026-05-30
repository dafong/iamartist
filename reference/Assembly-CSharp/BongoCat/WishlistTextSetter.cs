using System;
using BongoCat.Localizer;
using BongoCat.SteamJsonParser;
using TMPro;
using UnityEngine;

namespace BongoCat
{
	public class WishlistTextSetter : MonoBehaviour
	{
		[SerializeField]
		private string _gameName;

		[SerializeField]
		private SteamItemUnity _steamItemReward;

		[SerializeField]
		private TMP_Text _wishlistText;

		private void Start()
		{
			Loca instance = Loca.Instance;
			instance.OnLanguageChanged = (Action)Delegate.Combine(instance.OnLanguageChanged, new Action(OnLocaUpdate));
			OnLocaUpdate();
		}

		private void OnLocaUpdate()
		{
			_wishlistText.text = string.Format(Loca.Instance.Get("WishlistFollowGeneric"), _steamItemReward.Name, _gameName);
		}
	}
}

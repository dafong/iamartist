using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using UnityEngine;

namespace BongoCat.Testing.Multiplayer
{
	public class CatBot : MonoBehaviour
	{
		private Cat _cat;

		private CatCosmeticsMultiplayer _catCosmeticsMultiplayer;

		private CatInventory _catInventory;

		private float _waitTime;

		private float _currentWaittime;

		private void Awake()
		{
			_catInventory = Object.FindAnyObjectByType<CatInventory>();
			_cat = GetComponent<Cat>();
			_catCosmeticsMultiplayer = GetComponent<CatCosmeticsMultiplayer>();
		}

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => _catInventory.IsInitialized);
			RandomizeAppearance();
			StartCoroutine(RandomTap());
		}

		private void RandomizeAppearance()
		{
			List<SteamItem> list = _catInventory.Items.Where((SteamItem item) => item.ItemSlot == "skin").ToList();
			_cat.SetSkin(list[Random.Range(0, list.Count)].ItemName);
			List<SteamItem> list2 = _catInventory.Items.Where((SteamItem item) => item.ItemSlot == "hat").ToList();
			_catCosmeticsMultiplayer.EquipItem(new SteamItem(new SteamItemDef_t(list2[Random.Range(0, list2.Count)].SteamItemDefId)));
		}

		private IEnumerator RandomTap()
		{
			while (true)
			{
				yield return new WaitForSeconds(Random.Range(0.5f, 4f));
				_cat.Tap(1);
			}
		}
	}
}

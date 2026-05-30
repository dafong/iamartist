using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BongoCat
{
	public class EventBannerPicker : MonoBehaviour
	{
		[SerializeField]
		private List<DisableIfItemsInInventory> _allBanner;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => _allBanner.All((DisableIfItemsInInventory b) => b._initialized));
			List<GameObject> list = (from b in _allBanner
				where b.gameObject.activeSelf
				select b.gameObject).ToList();
			if (list.Count > 0)
			{
				list[global::UnityEngine.Random.Range(0, list.Count)].SetActive(value: true);
				{
					foreach (DisableIfItemsInInventory item in _allBanner)
					{
						item.OnDisabled = (Action)Delegate.Combine(item.OnDisabled, (Action)delegate
						{
							if (_allBanner.All((DisableIfItemsInInventory b) => !b.gameObject.activeSelf) && base.gameObject.activeSelf)
							{
								base.gameObject.SetActive(value: false);
							}
						});
					}
					yield break;
				}
			}
			base.gameObject.SetActive(value: false);
		}
	}
}

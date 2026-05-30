using System.Collections;
using System.Collections.Generic;
using BongoCat.SteamJsonParser;
using Steam;
using UnityEngine;

namespace BongoCat
{
	public class AutoExchanger : MonoBehaviour
	{
		[SerializeField]
		private List<SteamBundleExchange> _exchangesToPerform;

		[SerializeField]
		private int _timer = 30;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => CatInventory.Instance.WasLoadedFromSteam);
			StartCoroutine(AutoExchangeRoutine());
		}

		private IEnumerator AutoExchangeRoutine()
		{
			while (true)
			{
				foreach (SteamBundleExchange item in _exchangesToPerform)
				{
					TokenExchanger.Instance.Exchange(item);
					yield return new WaitForSeconds(1f);
				}
				yield return new WaitForSeconds(_timer);
			}
		}
	}
}

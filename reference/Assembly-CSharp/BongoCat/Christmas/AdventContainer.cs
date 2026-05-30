using System.Collections.Generic;
using BongoCat.SteamJsonParser;
using UnityEngine;

namespace BongoCat.Christmas
{
	public class AdventContainer : MonoBehaviour
	{
		[SerializeField]
		private List<SteamBundleExchange> _exchangesOrdered;
	}
}

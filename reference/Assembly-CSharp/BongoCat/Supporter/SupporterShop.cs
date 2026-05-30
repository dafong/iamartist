using System.Collections;
using System.Globalization;
using System.Linq;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace BongoCat.Supporter
{
	public class SupporterShop : MonoBehaviour
	{
		public static SupporterShop Instance;

		private CallResult<SteamInventoryRequestPricesResult_t> _requestPricesCallResult;

		public string PriceSymbol;

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			_requestPricesCallResult = CallResult<SteamInventoryRequestPricesResult_t>.Create(RequestPricesCallback);
			SteamAPICall_t hAPICall = SteamInventory.RequestPrices();
			_requestPricesCallResult.Set(hAPICall);
		}

		private void RequestPricesCallback(SteamInventoryRequestPricesResult_t result, bool ioFailure)
		{
			if (result.m_result != EResult.k_EResultOK || ioFailure)
			{
				Debug.Log("Failed to get prices from Steam Inventory: " + result.m_result);
			}
			else if (!TryGetCurrencySymbol(result.m_rgchCurrency, out PriceSymbol))
			{
				PriceSymbol = result.m_rgchCurrency;
			}
		}

		private bool TryGetCurrencySymbol(string isoCurrencySymbol, out string symbol)
		{
			symbol = (from ri in (from c in CultureInfo.GetCultures(CultureTypes.AllCultures)
					where !c.IsNeutralCulture
					select c).Select(delegate(CultureInfo culture)
				{
					try
					{
						return new RegionInfo(culture.Name);
					}
					catch
					{
						return (RegionInfo)null;
					}
				})
				where ri != null && ri.ISOCurrencySymbol == isoCurrencySymbol
				select ri.CurrencySymbol).FirstOrDefault();
			return symbol != null;
		}
	}
}

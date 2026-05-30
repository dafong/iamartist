using System.Runtime.CompilerServices;

namespace BongoCat.SteamJsonParser
{
	public class SteamItemBackendBundle : SteamItemBackend
	{
		[CompilerGenerated]
		private bool _003Cuse_bundle_price_003Ek__BackingField;

		[CompilerGenerated]
		private int _003Cpurchase_bundle_discount_003Ek__BackingField;

		public bool use_bundle_price
		{
			[CompilerGenerated]
			set
			{
				_003Cuse_bundle_price_003Ek__BackingField = value;
			}
		}

		public int purchase_bundle_discount
		{
			[CompilerGenerated]
			set
			{
				_003Cpurchase_bundle_discount_003Ek__BackingField = value;
			}
		}
	}
}

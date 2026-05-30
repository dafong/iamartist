using BongoCat.SteamJsonParser;

namespace BongoCat
{
	public static class QualityConverter
	{
		public static QualityCategoryWithInfo ToQualityWithInfo(this QualityCategory quality, bool isEmote)
		{
			switch (quality)
			{
			case QualityCategory.Common:
				return isEmote ? QualityCategoryWithInfo.CommonEmote : QualityCategoryWithInfo.CommonCosmetic;
			case QualityCategory.Uncommon:
				return (!isEmote) ? QualityCategoryWithInfo.UncommonCosmetic : QualityCategoryWithInfo.UncommonEmote;
			case QualityCategory.Rare:
				return isEmote ? QualityCategoryWithInfo.RareEmote : QualityCategoryWithInfo.RareCosmetic;
			case QualityCategory.Epic:
				return isEmote ? QualityCategoryWithInfo.EpicEmote : QualityCategoryWithInfo.EpicCosmetic;
			case QualityCategory.Legendary:
				return isEmote ? QualityCategoryWithInfo.LegendaryEmote : QualityCategoryWithInfo.LegendaryCosmetic;
			case QualityCategory.Special:
				return isEmote ? QualityCategoryWithInfo.SpecialEmote : QualityCategoryWithInfo.SpecialCosmetic;
			default:
			{
				global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(quality);
				QualityCategoryWithInfo result = default(QualityCategoryWithInfo);
				return result;
			}
			}
		}

		public static QualityCategory ToQuality(this QualityCategoryWithInfo qualityWithInfo)
		{
			switch (qualityWithInfo)
			{
			case QualityCategoryWithInfo.CommonCosmetic:
				return QualityCategory.Common;
			case QualityCategoryWithInfo.UncommonCosmetic:
				return QualityCategory.Uncommon;
			case QualityCategoryWithInfo.RareCosmetic:
				return QualityCategory.Rare;
			case QualityCategoryWithInfo.EpicCosmetic:
				return QualityCategory.Epic;
			case QualityCategoryWithInfo.LegendaryCosmetic:
				return QualityCategory.Legendary;
			case QualityCategoryWithInfo.SpecialCosmetic:
				return QualityCategory.Special;
			case QualityCategoryWithInfo.CommonEmote:
				return QualityCategory.Common;
			case QualityCategoryWithInfo.UncommonEmote:
				return QualityCategory.Uncommon;
			case QualityCategoryWithInfo.RareEmote:
				return QualityCategory.Rare;
			case QualityCategoryWithInfo.EpicEmote:
				return QualityCategory.Epic;
			case QualityCategoryWithInfo.LegendaryEmote:
				return QualityCategory.Legendary;
			case QualityCategoryWithInfo.SpecialEmote:
				return QualityCategory.Special;
			default:
			{
				global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(qualityWithInfo);
				QualityCategory result = default(QualityCategory);
				return result;
			}
			}
		}
	}
}

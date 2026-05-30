using System;
using System.Collections.Generic;
using BongoCat.SteamJsonParser;
using UnityEngine;

namespace BongoCat
{
	public class QualityColors : ScriptableObject
	{
		[Serializable]
		private struct QualityColor
		{
			public QualityCategory Quality;

			public Color Color;
		}

		[SerializeField]
		private List<QualityColor> _qualityColors;

		private Dictionary<QualityCategory, Color> _dictionary;

		public Color GetColor(QualityCategory quality)
		{
			if (_dictionary == null)
			{
				_dictionary = new Dictionary<QualityCategory, Color>();
				foreach (QualityColor qualityColor in _qualityColors)
				{
					_dictionary.Add(qualityColor.Quality, qualityColor.Color);
				}
			}
			return _dictionary[quality];
		}
	}
}

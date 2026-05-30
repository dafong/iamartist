using System.Collections.Generic;
using UnityEngine;

namespace BongoCat
{
	public class MemoryImageCache : MonoBehaviour
	{
		[SerializeField]
		private List<Sprite> _sprites;

		public List<Sprite> LetterSprites;

		private Dictionary<string, Sprite> _spriteCache;

		public static MemoryImageCache Instance;

		private void Awake()
		{
			Instance = this;
			_spriteCache = new Dictionary<string, Sprite>();
			foreach (Sprite sprite in _sprites)
			{
				_spriteCache[sprite.name] = sprite;
			}
		}

		public Sprite GetSprite(string spriteName)
		{
			if (_spriteCache == null)
			{
				_spriteCache = new Dictionary<string, Sprite>();
				foreach (Sprite sprite in _sprites)
				{
					_spriteCache[sprite.name] = sprite;
				}
			}
			if (!_spriteCache.ContainsKey(spriteName))
			{
				return null;
			}
			return _spriteCache[spriteName];
		}
	}
}

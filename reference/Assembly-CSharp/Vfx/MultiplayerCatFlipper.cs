using BongoCat;
using UnityEngine;

namespace Vfx
{
	public class MultiplayerCatFlipper : MonoBehaviour
	{
		[SerializeField]
		private CatCosmeticsMultiplayer _cosmetics;

		[SerializeField]
		private Transform _cat;

		private bool _isFlipped;

		public bool IsFlipped => _isFlipped;

		public void Flip()
		{
			_isFlipped = !_isFlipped;
			_cosmetics.UpdateFlip();
			_cat.localScale = new Vector3(-1f * _cat.localScale.x, _cat.localScale.y, _cat.localScale.z);
		}
	}
}

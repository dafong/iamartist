using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat
{
	public class ImageSequence : MonoBehaviour
	{
		[SerializeField]
		private List<Sprite> _imageSequence;

		[SerializeField]
		private int _fps;

		private int _index;

		[SerializeField]
		private Image _image;

		private IEnumerator Start()
		{
			while (true)
			{
				_image.sprite = _imageSequence[_index];
				_index = (_index + 1) % _imageSequence.Count;
				yield return new WaitForSeconds(1f / (float)_fps);
			}
		}
	}
}

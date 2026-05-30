using System.Collections.Generic;
using UnityEngine;

namespace BongoCat
{
	public class HideDangerZone : MonoBehaviour
	{
		[SerializeField]
		private List<GameObject> _settingsOptions;

		[SerializeField]
		private RectTransform _optionsRoot;

		private float _baseHeight = 48f;

		public void UpdateToggle(bool hide)
		{
			if (hide)
			{
				Vector2 sizeDelta = _optionsRoot.sizeDelta;
				sizeDelta.y = _baseHeight;
				_optionsRoot.sizeDelta = sizeDelta;
				{
					foreach (GameObject settingsOption in _settingsOptions)
					{
						settingsOption.SetActive(value: false);
					}
					return;
				}
			}
			Vector2 sizeDelta2 = _optionsRoot.sizeDelta;
			sizeDelta2.y = (float)_settingsOptions.Count * 28f + _baseHeight;
			_optionsRoot.sizeDelta = sizeDelta2;
			foreach (GameObject settingsOption2 in _settingsOptions)
			{
				settingsOption2.SetActive(value: true);
			}
		}
	}
}

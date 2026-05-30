using TMPro;
using UnityEngine;

namespace BongoCat
{
	public class TooltipUiElement : MonoBehaviour
	{
		[SerializeField]
		private TMP_Text _text;

		public void Show()
		{
			base.gameObject.SetActive(value: true);
		}

		public void Hide()
		{
			base.gameObject.SetActive(value: false);
		}

		public void SetText(string text)
		{
			_text.text = text;
		}
	}
}

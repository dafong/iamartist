using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat.Supporter
{
	public class SupporterDlc : MonoBehaviour
	{
		[SerializeField]
		private string _dlcName;

		[SerializeField]
		private string _dlcId;

		[SerializeField]
		private Image _image;

		[SerializeField]
		private TMP_Text _text;

		public void OnBuyButtonClicked()
		{
			Application.OpenURL("steam://openurl/https://store.steampowered.com/app/" + _dlcId + "/");
		}
	}
}

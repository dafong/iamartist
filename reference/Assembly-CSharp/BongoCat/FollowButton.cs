using Steam;
using UnityEngine;

namespace BongoCat
{
	public class FollowButton : MonoBehaviour
	{
		[SerializeField]
		private ErrorMessage _popupMessage;

		public void OpenSteamPage()
		{
			ErrorMessageHandler.Instance.SetErrorMessage(_popupMessage, delegate
			{
				Application.OpenURL("steam://openurl/https://store.steampowered.com/app/3419430?utm_source=game&utm_medium=followButton");
				SteamGroupHelper.Instance.ClickedFollowButton(Global.GroupId.m_SteamID);
			}, delegate
			{
				ErrorMessageHandler.Instance.CloseErrorPopup();
			});
		}
	}
}

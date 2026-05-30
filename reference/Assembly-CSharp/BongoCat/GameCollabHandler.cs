using UnityEngine;
using UnityEngine.UI;

namespace BongoCat
{
	public class GameCollabHandler : MonoBehaviour
	{
		[SerializeField]
		private RuntimeCollabData _collabData;

		[SerializeField]
		private GameObject _collabUi;

		[SerializeField]
		private Image _banner;

		private void Awake()
		{
			if ((bool)_collabData)
			{
				_collabUi.SetActive(value: true);
				_banner.sprite = _collabData.Banner;
			}
			else
			{
				_collabUi.SetActive(value: false);
			}
		}

		public void OpenGameLink()
		{
			Application.OpenURL(_collabData.GameLink);
		}

		public void OpenAnnouncementLink()
		{
			Application.OpenURL(_collabData.AnnouncementLink);
		}
	}
}

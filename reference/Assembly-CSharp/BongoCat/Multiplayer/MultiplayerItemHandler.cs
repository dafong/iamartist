using Steamworks;
using UnityEngine;

namespace BongoCat.Multiplayer
{
	public class MultiplayerItemHandler : MonoBehaviour
	{
		[SerializeField]
		private GameObject _chestReady;

		[SerializeField]
		private NewItemPopup _newItemPopup;

		private void Start()
		{
			SettingsManager.Instance.MultiplayerPopups.OnToggleUpdated.AddListener(OnMultiplayerPopupsToggle);
			OnMultiplayerPopupsToggle(SettingsManager.Instance.MultiplayerPopups.Value);
		}

		private void OnMultiplayerPopupsToggle(bool toogle)
		{
			if (!toogle)
			{
				_chestReady.SetActive(value: false);
				_newItemPopup.HidePopup();
			}
		}

		public void UpdateChestToggle(bool toggle)
		{
			_chestReady.SetActive(toggle);
		}

		public void DisplayReceivedItem(int itemId)
		{
			SteamItem item = new SteamItem(new SteamItemDef_t(itemId));
			_newItemPopup.ShowPopup(item, alreadyOwned: true);
		}

		private void OnDestroy()
		{
			SettingsManager.Instance.MultiplayerPopups.OnToggleUpdated.RemoveListener(OnMultiplayerPopupsToggle);
		}
	}
}

using UnityEngine;

namespace BongoCat
{
	public class SettingsManager : MonoBehaviour
	{
		public static SettingsManager Instance;

		[SerializeField]
		private MainScreenSetting _mainScreenSetting;

		[SerializeField]
		private PlayerPrefsToggle _catBobbing;

		[SerializeField]
		private PlayerPrefsToggle _snapping;

		[SerializeField]
		private PlayerPrefsToggle _lockPosition;

		[SerializeField]
		private ScaleSetting _catScaleSetting;

		[SerializeField]
		private ScaleSetting _uiScaleSetting;

		public PlayerPrefsToggle AfkIndicator;

		[SerializeField]
		private PlayerPrefsToggle _showIconPreview;

		[SerializeField]
		private PlayerPrefsToggle _alwaysShowChest;

		[SerializeField]
		private PlayerPrefsToggle _snapCats;

		[SerializeField]
		private PlayerPrefsToggle _showPlayerNames;

		[SerializeField]
		private PlayerPrefsToggle _scaleAllCats;

		[SerializeField]
		private PlayerPrefsToggle _multiplayerPopups;

		[SerializeField]
		private PlayerPrefsToggle _hideEmotesSetting;

		[SerializeField]
		private SelectionMenu _privateLobby;

		[SerializeField]
		private PlayerPrefsToggle _autoHideCats;

		[SerializeField]
		private PlayerPrefsToggle _muteChats;

		[SerializeField]
		private PlayerPrefsToggle _autoEquipDrops;

		public MainScreenSetting MainScreenSetting => _mainScreenSetting;

		public PlayerPrefsToggle CatBobbing => _catBobbing;

		public PlayerPrefsToggle Snapping => _snapping;

		public PlayerPrefsToggle LockPosition => _lockPosition;

		public ScaleSetting CatScaleSetting => _catScaleSetting;

		public ScaleSetting UIScaleSetting => _uiScaleSetting;

		public bool ShowIconPreview => _showIconPreview.Value;

		public PlayerPrefsToggle AlwaysShowChest => _alwaysShowChest;

		public PlayerPrefsToggle SnapCats => _snapCats;

		public PlayerPrefsToggle ShowPlayerNames => _showPlayerNames;

		public PlayerPrefsToggle ScaleAllCats => _scaleAllCats;

		public PlayerPrefsToggle MultiplayerPopups => _multiplayerPopups;

		public PlayerPrefsToggle HideEmotesSetting => _hideEmotesSetting;

		public SelectionMenu PrivateLobby => _privateLobby;

		public PlayerPrefsToggle AutoHideCats => _autoHideCats;

		public PlayerPrefsToggle MuteChats => _muteChats;

		public bool AutoEquipDrops => _autoEquipDrops.Value;

		private void Awake()
		{
			if (Instance != null)
			{
				Object.Destroy(this);
			}
			Instance = this;
		}
	}
}

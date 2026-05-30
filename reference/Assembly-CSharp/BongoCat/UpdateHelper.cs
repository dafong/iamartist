using System;
using System.Collections;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace BongoCat
{
	public class UpdateHelper : MonoBehaviour
	{
		public static UpdateHelper Instance;

		[SerializeField]
		private ErrorMessage _updateError;

		private const string LAST_FORCED_UPDATE = "LastUpdate";

		private const int LAST_FORCED_UPDATE_WAIT_HOURS = 3;

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			Pets pets = global::UnityEngine.Object.FindAnyObjectByType<Pets>();
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			yield return new WaitUntil(() => pets.StatsInitialized);
		}

		public void ForceUpdate()
		{
			SteamApps.MarkContentCorrupt(bMissingFilesOnly: false);
			PlayerPrefs.SetString("LastUpdate", DateTime.UtcNow.ToString());
			RestartApplication.RestartGameViaSteam();
		}

		public void TryUpdate()
		{
			if (PlayerPrefs.HasKey("LastUpdate"))
			{
				DateTime dateTime = DateTime.Parse(PlayerPrefs.GetString("LastUpdate"));
				if (DateTime.UtcNow < dateTime.AddHours(3.0))
				{
					return;
				}
			}
			SteamApps.MarkContentCorrupt(bMissingFilesOnly: false);
			PlayerPrefs.SetString("LastUpdate", DateTime.UtcNow.ToString());
			ErrorMessageHandler.Instance.SetErrorMessage(_updateError, RestartApplication.RestartGameViaSteam, ErrorMessageHandler.Instance.CloseErrorPopup);
		}
	}
}

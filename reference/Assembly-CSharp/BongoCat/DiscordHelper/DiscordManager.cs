using System;
using Discord.Sdk;
using Steamworks;
using UnityEngine;

namespace BongoCat.DiscordHelper
{
	public class DiscordManager : MonoBehaviour
	{
		public Client Client;

		public static DiscordManager Instance;

		[SerializeField]
		private PlayerPrefsToggle _setting;

		private void Awake()
		{
			Instance = this;
			_setting.OnToggleUpdated.AddListener(ToggleUpdated);
		}

		private void Start()
		{
			if (_setting.Value)
			{
				SetDefaultActivity();
			}
		}

		private void Init()
		{
			if (Client == null)
			{
				Client = new Client();
				Client.SetApplicationId(1480978256919265390uL);
				Client.RegisterLaunchSteamApplication(1480978256919265390uL, (uint)Global.SteamID);
				Client.SetActivityJoinCallback(delegate(string secret)
				{
					SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse(secret)));
				});
			}
		}

		private void ToggleUpdated(bool value)
		{
			if (value)
			{
				SetDefaultActivity();
			}
			else
			{
				Client?.ClearRichPresence();
			}
		}

		public void SetDefaultActivity()
		{
			if (_setting.Value)
			{
				Init();
				Activity activity = new Activity();
				activity.SetType(ActivityTypes.Playing);
				activity.SetState("Typing very hard!");
				activity.SetDetails("I'm working, trust me...");
				Client.UpdateRichPresence(activity, delegate
				{
				});
			}
		}

		public void SetMultiplayerActivity(string secret, int numLobbyMembers)
		{
			if (_setting.Value)
			{
				Init();
				Activity activity = new Activity();
				activity.SetType(ActivityTypes.Playing);
				activity.SetState("Tapping together");
				activity.SetDetails("Tap tap tap..");
				ActivityParty activityParty = new ActivityParty();
				activityParty.SetId(default(Guid).ToString());
				activityParty.SetCurrentSize(numLobbyMembers);
				activityParty.SetMaxSize(250);
				activity.SetParty(activityParty);
				ActivitySecrets activitySecrets = new ActivitySecrets();
				activitySecrets.SetJoin(secret);
				activity.SetSecrets(activitySecrets);
				activity.SetSupportedPlatforms(ActivityGamePlatforms.Desktop);
				Client.UpdateRichPresence(activity, delegate
				{
				});
			}
		}
	}
}

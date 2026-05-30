using System;
using System.Collections;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace Steam
{
	public class OnlineStatusHandler : MonoBehaviour
	{
		public Action<EPersonaState> PlayerStatusChanged;

		private Callback<PersonaStateChange_t> _personaStateChange;

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SteamManager.Initialized);
			if (_personaStateChange == null)
			{
				_personaStateChange = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
			}
		}

		private void OnPersonaStateChange(PersonaStateChange_t changedPersona)
		{
			CSteamID steamID = SteamUser.GetSteamID();
			if (changedPersona.m_ulSteamID == steamID.m_SteamID)
			{
				EPersonaState friendPersonaState = SteamFriends.GetFriendPersonaState(steamID);
				Debug.Log($"Persona state changed to {friendPersonaState} at {DateTime.Now:t}");
				PlayerStatusChanged?.Invoke(friendPersonaState);
			}
		}
	}
}

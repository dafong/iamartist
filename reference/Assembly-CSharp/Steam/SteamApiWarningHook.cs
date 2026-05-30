using System.Collections;
using System.Text;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace Steam
{
	public class SteamApiWarningHook : MonoBehaviour
	{
		private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

		private static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
		{
			Debug.Log(pchDebugText.ToString());
		}

		private IEnumerator Start()
		{
			yield return new WaitUntil(() => SteamManager.s_EverInitialized);
			if (m_SteamAPIWarningMessageHook == null)
			{
				Debug.Log("Setting up Steam API warning message hook");
				m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
				SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
			}
		}
	}
}

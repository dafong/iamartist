using System;
using System.Text;
using AOT;
using BongoCat;
using Steamworks;
using UnityEngine;

namespace IroxGames.StoreFronts.Steam
{
	[DisallowMultipleComponent]
	public class SteamManager : MonoBehaviour
	{
		public static bool s_EverInitialized;

		public static bool s_ShuttingDown;

		protected static SteamManager s_instance;

		protected bool m_bInitialized;

		protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

		protected static SteamManager Instance
		{
			get
			{
				if (s_instance == null && !s_EverInitialized)
				{
					return new GameObject("SteamManager").AddComponent<SteamManager>();
				}
				return s_instance;
			}
		}

		public static bool Initialized => Instance.m_bInitialized;

		public static bool ShuttingDown => s_ShuttingDown;

		[MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
		protected static void SteamAPIDebugTextHook(int nSeverity, StringBuilder pchDebugText)
		{
			Debug.LogWarning(pchDebugText);
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void InitOnPlayMode()
		{
			s_EverInitialized = false;
			s_instance = null;
		}

		protected virtual void Awake()
		{
			if (s_instance != null)
			{
				UnityEngine.Object.Destroy(base.gameObject);
				return;
			}
			s_instance = this;
			if (s_EverInitialized)
			{
				throw new Exception("Tried to Initialize the SteamAPI twice in one session!");
			}
			if (!Packsize.Test())
			{
				Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
			}
			if (!DllCheck.Test())
			{
				Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
			}
			try
			{
				if (SteamAPI.RestartAppIfNecessary(Global.SteamID))
				{
					Debug.Log("[Steamworks.NET] Shutting down because RestartAppIfNecessary returned true. Steam will restart the application.");
					Application.Quit();
					return;
				}
			}
			catch (DllNotFoundException ex)
			{
				Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + ex, this);
				Application.Quit();
				return;
			}
			if (!SteamAPI.IsSteamRunning())
			{
				Debug.LogError("[Steamworks.NET] Steam client is not running.");
			}
			string OutSteamErrMsg;
			ESteamAPIInitResult eSteamAPIInitResult = SteamAPI.InitEx(out OutSteamErrMsg);
			m_bInitialized = eSteamAPIInitResult == ESteamAPIInitResult.k_ESteamAPIInitResult_OK;
			if (!m_bInitialized)
			{
				Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed with:\n" + OutSteamErrMsg + "\nRefer to Valve's documentation or the comment above this line for more information.", this);
				return;
			}
			s_EverInitialized = true;
			Debug.Log("[Steamworks.NET] SteamAPI initialized.");
		}

		protected virtual void OnEnable()
		{
			if (s_instance == null)
			{
				s_instance = this;
			}
			if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
			{
				m_SteamAPIWarningMessageHook = SteamAPIDebugTextHook;
				SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
			}
		}

		protected virtual void OnDestroy()
		{
			if (!(s_instance != this))
			{
				s_instance = null;
				if (m_bInitialized)
				{
					SteamAPI.Shutdown();
				}
			}
		}

		protected virtual void Update()
		{
			if (m_bInitialized && !s_ShuttingDown)
			{
				SteamAPI.RunCallbacks();
			}
		}

		private void OnApplicationQuit()
		{
			s_ShuttingDown = true;
		}
	}
}

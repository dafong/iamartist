using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using BongoCat.IPC;
using BongoCat.Multiplayer;
using BongoCat.OSSpecific;
using Newtonsoft.Json;
using Steamworks;
using UnityEngine;

namespace BongoCat.TapTapLootIntegration
{
	public class Ipc : MonoBehaviour
	{
		public static Ipc Instance;

		private Thread _t;

		private Thread _farmerReplacedThread;

		private bool _isRunning;

		private bool _isRunningFarmerReplaced;

		private List<TapTapLootBuff> _buffs = new List<TapTapLootBuff>();

		private bool _buffsChanged;

		private readonly object _lock = new object();

		private int _taps;

		private BongoCat.OSSpecific.GlobalKeyHook _globalKeyHook;

		private void Awake()
		{
			Instance = this;
			_globalKeyHook = global::UnityEngine.Object.FindAnyObjectByType<BongoCat.OSSpecific.GlobalKeyHook>(FindObjectsInactive.Include);
		}

		public void Start()
		{
			if (!Application.isEditor)
			{
				_isRunning = true;
				_t = new Thread(BongoCatServerThread)
				{
					IsBackground = true
				};
				_t.Start();
				_isRunningFarmerReplaced = true;
				_farmerReplacedThread = new Thread(TheFarmerWasReplacedThread)
				{
					IsBackground = true
				};
				_farmerReplacedThread.Start();
			}
		}

		private void OnDestroy()
		{
			_isRunning = false;
			if (_t != null)
			{
				try
				{
					_t.Interrupt();
					_t.Abort();
				}
				catch
				{
				}
				_t = null;
			}
			_isRunningFarmerReplaced = false;
			if (_farmerReplacedThread != null)
			{
				try
				{
					_farmerReplacedThread.Interrupt();
					_farmerReplacedThread.Abort();
				}
				catch
				{
				}
				_farmerReplacedThread = null;
			}
		}

		private void Update()
		{
			if (_taps > 0)
			{
				_globalKeyHook.OnKeyPressed?.Invoke(_taps);
				_taps = 0;
			}
		}

		public void UpdateBuffs()
		{
			Dictionary<string, float> dictionary = new Dictionary<string, float>();
			if (CatCosmetics.Instance != null && CatCosmetics.Instance.EquippedItems != null)
			{
				foreach (SteamItem item in CatCosmetics.Instance.EquippedItems.Distinct())
				{
					if (item.TapTapLootBuffs == null)
					{
						continue;
					}
					foreach (TapTapLootBuff tapTapLootBuff in item.TapTapLootBuffs)
					{
						if (dictionary.ContainsKey(tapTapLootBuff.Name))
						{
							dictionary[tapTapLootBuff.Name] += tapTapLootBuff.Value;
						}
						else
						{
							dictionary.Add(tapTapLootBuff.Name, tapTapLootBuff.Value);
						}
					}
				}
			}
			if (MultiplayerLobby.Instance != null && MultiplayerLobby.Instance.LobbyMembers != null)
			{
				foreach (LobbyMember lobbyMember in MultiplayerLobby.Instance.LobbyMembers)
				{
					if (lobbyMember.SteamId == SteamUser.GetSteamID() || lobbyMember.IsHidden || !lobbyMember.CatActivated)
					{
						continue;
					}
					CatCosmeticsMultiplayer catCosmetics = lobbyMember.CatCosmetics;
					if (!catCosmetics || catCosmetics.EquippedItems == null)
					{
						continue;
					}
					foreach (SteamItem item2 in catCosmetics.EquippedItems.Distinct())
					{
						if (item2.TapTapLootBuffs == null)
						{
							continue;
						}
						foreach (TapTapLootBuff tapTapLootBuff2 in item2.TapTapLootBuffs)
						{
							if (dictionary.ContainsKey(tapTapLootBuff2.Name))
							{
								dictionary[tapTapLootBuff2.Name] += tapTapLootBuff2.Value * 0.25f;
							}
							else
							{
								dictionary.Add(tapTapLootBuff2.Name, tapTapLootBuff2.Value * 0.25f);
							}
						}
					}
				}
			}
			lock (_lock)
			{
				_buffs = dictionary.Select((KeyValuePair<string, float> kv) => new TapTapLootBuff(kv.Key, kv.Value)).ToList();
				_buffsChanged = true;
			}
		}

		private void TheFarmerWasReplacedThread()
		{
			while (_isRunningFarmerReplaced)
			{
				try
				{
					using NamedPipeClientStream namedPipeClientStream = new NamedPipeClientStream(".", "BongoCatxTheFarmerWasReplaced", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Anonymous);
					namedPipeClientStream.Connect(1000);
					StreamString streamString = new StreamString(namedPipeClientStream);
					while (_isRunningFarmerReplaced && namedPipeClientStream.IsConnected)
					{
						Thread.Sleep(90);
						string text = streamString.ReadString();
						if (!string.IsNullOrEmpty(text))
						{
							_taps += int.Parse(text);
						}
					}
				}
				catch (Exception)
				{
				}
				if (_isRunningFarmerReplaced)
				{
					Thread.Sleep(1000);
				}
			}
		}

		private void BongoCatServerThread()
		{
			while (_isRunning)
			{
				try
				{
					using NamedPipeServerStream namedPipeServerStream = new NamedPipeServerStream("TapTapLootxBongoCat", PipeDirection.InOut, 1);
					namedPipeServerStream.WaitForConnection();
					StreamString streamString = new StreamString(namedPipeServerStream);
					bool flag = true;
					while (_isRunning && namedPipeServerStream.IsConnected)
					{
						List<TapTapLootBuff> list = null;
						lock (_lock)
						{
							if (flag || _buffsChanged)
							{
								list = _buffs;
								flag = false;
								_buffsChanged = false;
							}
						}
						if (list != null)
						{
							streamString.WriteString(JsonConvert.SerializeObject(list));
						}
						Thread.Sleep(500);
					}
				}
				catch (Exception)
				{
				}
				if (_isRunning)
				{
					Thread.Sleep(1000);
				}
			}
		}
	}
}

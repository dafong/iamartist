using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BongoCat.Localizer;
using Crosstales.BWF;
using Crosstales.BWF.Model.Enum;
using Crosstales.Common.Util;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;

namespace BongoCat.Multiplayer
{
	public class SteamMultiplayer : MonoBehaviour
	{
		[SerializeField]
		private PlayerPrefsToggle _multiplayerPopups;

		private int _tapsToSend;

		[SerializeField]
		private GameObject _catPrefab;

		[SerializeField]
		private Transform _root;

		[SerializeField]
		private Cat _mainCat;

		private Leaderboard _leaderboard;

		private Coroutine _sendTapsRoutine;

		private Coroutine _receiveMessagesRoutine;

		private Callback<LobbyDataUpdate_t> _lobbyDataCallback;

		private const string SPEECH_LOCA_KEY = "Meow";

		private Dictionary<string, string> _meowLocaDict;

		private const string HAT_ITEMSLOT_KEY = "hat";

		private const string SKIN_ITEMSLOT_KEY = "skin";

		public static SteamMultiplayer Instance;

		public const int k_nSteamNetworkingSend_Unreliable = 0;

		public const int k_nSteamNetworkingSend_NoNagle = 1;

		public const int k_nSteamNetworkingSend_UnreliableNoNagle = 1;

		public const int k_nSteamNetworkingSend_NoDelay = 4;

		public const int k_nSteamNetworkingSend_UnreliableNoDelay = 5;

		public const int k_nSteamNetworkingSend_Reliable = 8;

		public const int k_nSteamNetworkingSend_ReliableNoNagle = 9;

		public const int k_nSteamNetworkingSend_AutoRestartBrokenSession = 32;

		public Dictionary<string, string> MeowLocaDict
		{
			get
			{
				if (_meowLocaDict == null)
				{
					_meowLocaDict = Loca.Instance.GetAllTranslationsForKey("Meow");
				}
				return _meowLocaDict;
			}
		}

		public string MeowText => MeowLocaDict.GetValueOrDefault(Loca.Instance.GetCurrentLanguageIsoCode(), "Meow Meow");

		public Transform Root => _root;

		private void Awake()
		{
			Instance = this;
		}

		private IEnumerator Start()
		{
			_leaderboard = Leaderboard.Instance;
			MultiplayerLobby.Instance.EnteredLobby += OnLobbyEntered;
			MultiplayerLobby.Instance.LeftLobby += OnLobbyLeft;
			_mainCat.OnTap.AddListener(AddTaps);
			yield return new WaitUntil(() => SteamManager.Initialized);
			_lobbyDataCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
		}

		private void OnLobbyDataUpdate(LobbyDataUpdate_t lobbyDataUpdate)
		{
			if (lobbyDataUpdate.m_bSuccess != 0 && lobbyDataUpdate.m_ulSteamIDLobby != lobbyDataUpdate.m_ulSteamIDMember)
			{
				CSteamID cSteamID = new CSteamID(lobbyDataUpdate.m_ulSteamIDMember);
				MultiplayerLobby.Instance.GetMember(cSteamID)?.FetchCosmetics(MultiplayerLobby.Instance.LobbyId);
				_leaderboard.UpdateMemberTaps(cSteamID);
			}
		}

		private void OnDestroy()
		{
			MultiplayerLobby.Instance.EnteredLobby -= OnLobbyEntered;
			MultiplayerLobby.Instance.LeftLobby -= OnLobbyLeft;
			_mainCat.OnTap.RemoveListener(AddTaps);
		}

		private void OnLobbyEntered()
		{
			_tapsToSend = 0;
			_sendTapsRoutine = StartCoroutine(SendingTapsRoutine());
			_receiveMessagesRoutine = StartCoroutine(ReceiveMultiplayerMessagesRoutine());
		}

		private void OnLobbyLeft()
		{
			StopMessagingRoutines();
		}

		private void StopMessagingRoutines()
		{
			if (_sendTapsRoutine != null)
			{
				StopCoroutine(_sendTapsRoutine);
				_sendTapsRoutine = null;
			}
			if (_receiveMessagesRoutine != null)
			{
				StopCoroutine(_receiveMessagesRoutine);
				_receiveMessagesRoutine = null;
			}
		}

		private IEnumerator ReceiveMultiplayerMessagesRoutine()
		{
			while (true)
			{
				yield return null;
				ReadTaps();
				ReadSpeech();
				ReadEmote();
				ReadChestReady();
				ReadItemReceived();
				ReadResetLeaderboard();
			}
		}

		private void ReadOnChannel(int channel, Action<string, SteamNetworkingIdentity> onMessageReceived)
		{
			if (SteamManager.ShuttingDown)
			{
				return;
			}
			IntPtr[] array = new IntPtr[15];
			SteamNetworkingMessages.ReceiveMessagesOnChannel(channel, array, 15);
			IntPtr[] array2 = array;
			foreach (IntPtr intPtr in array2)
			{
				if (!(intPtr == IntPtr.Zero))
				{
					SteamNetworkingIdentity sender;
					string message = GetMessage(intPtr, out sender);
					onMessageReceived(message, sender);
					SteamNetworkingMessage_t.Release(intPtr);
				}
			}
		}

		private static string GetMessage(IntPtr msgPtr, out SteamNetworkingIdentity sender)
		{
			SteamNetworkingMessage_t steamNetworkingMessage_t = SteamNetworkingMessage_t.FromIntPtr(msgPtr);
			int num = Math.Min(50, steamNetworkingMessage_t.m_cbSize);
			byte[] array = new byte[num];
			Marshal.Copy(steamNetworkingMessage_t.m_pData, array, 0, num);
			string result = Encoding.UTF8.GetString(array);
			sender = steamNetworkingMessage_t.m_identityPeer;
			return result;
		}

		private void ReadTaps()
		{
			ReadOnChannel(2, HandleTapsMessage);
			void HandleTapsMessage(string message, SteamNetworkingIdentity sender)
			{
				CSteamID steamID = sender.GetSteamID();
				LobbyMember member = MultiplayerLobby.Instance.GetMember(steamID);
				if ((bool)member && !member.IsHidden && int.TryParse(message, out var result) && member.CatActivated)
				{
					member.Cat.Tap(result);
					_leaderboard.UpdateMemberTapsDirty(steamID, result);
				}
			}
		}

		private void ReadSpeech()
		{
			ReadOnChannel(3, HandleSpeechMessage);
			void HandleSpeechMessage(string codedMessage, SteamNetworkingIdentity sender)
			{
				if (!string.IsNullOrEmpty(codedMessage) && !SettingsManager.Instance.MuteChats.Value)
				{
					LobbyMember member = MultiplayerLobby.Instance.GetMember(sender.GetSteamID());
					if ((bool)member && !member.IsHidden && member.CatActivated)
					{
						string valueOrDefault = MeowLocaDict.GetValueOrDefault(codedMessage, codedMessage);
						if (!Singleton<BWFManager>.Instance.Contains(valueOrDefault, ManagerMask.All))
						{
							member.CatSpeech.ShowMultiplayerSpeech(valueOrDefault);
						}
					}
				}
			}
		}

		private void ReadEmote()
		{
			if (SteamManager.ShuttingDown)
			{
				return;
			}
			IntPtr[] array = new IntPtr[1];
			SteamNetworkingMessages.ReceiveMessagesOnChannel(6, array, 1);
			IntPtr[] array2 = array;
			foreach (IntPtr intPtr in array2)
			{
				if (intPtr == IntPtr.Zero)
				{
					continue;
				}
				SteamNetworkingIdentity sender;
				string message = GetMessage(intPtr, out sender);
				if (!string.IsNullOrEmpty(message))
				{
					LobbyMember member = MultiplayerLobby.Instance.GetMember(sender.GetSteamID());
					if ((bool)member && !member.IsHidden)
					{
						member.EmoteSpawner.SpawnMultiplayerEmote(message);
					}
				}
				SteamNetworkingMessage_t.Release(intPtr);
			}
		}

		private void ReadChestReady()
		{
			if (_multiplayerPopups.Value)
			{
				ReadOnChannel(4, HandleChestReadyMessage);
			}
			static void HandleChestReadyMessage(string message, SteamNetworkingIdentity sender)
			{
				if (!string.IsNullOrEmpty(message))
				{
					LobbyMember member = MultiplayerLobby.Instance.GetMember(sender.GetSteamID());
					if ((bool)member && !member.IsHidden && int.TryParse(message, out var result))
					{
						member.MultiplayerItemHandler.UpdateChestToggle(result == 1);
					}
				}
			}
		}

		private void ReadItemReceived()
		{
			if (_multiplayerPopups.Value)
			{
				ReadOnChannel(5, HandleItemReceivedMessage);
			}
			static void HandleItemReceivedMessage(string message, SteamNetworkingIdentity sender)
			{
				if (!string.IsNullOrEmpty(message))
				{
					LobbyMember member = MultiplayerLobby.Instance.GetMember(sender.GetSteamID());
					if ((bool)member && !member.IsHidden && member.CatActivated && int.TryParse(message, out var result))
					{
						member.MultiplayerItemHandler.DisplayReceivedItem(result);
					}
				}
			}
		}

		private void ReadResetLeaderboard()
		{
			ReadOnChannel(7, HandleResetMessage);
			static void HandleResetMessage(string message, SteamNetworkingIdentity sender)
			{
				Leaderboard.Instance.ResetOwnTaps();
			}
		}

		public void SendChestReady(bool ready)
		{
			string message = (ready ? "1" : "0");
			SendMessageToLobby(message, reliable: true, 4);
		}

		public void SendReceivedItem(int itemId)
		{
			SendMessageToLobby(itemId.ToString(), reliable: true, 5);
		}

		public void SendSpeech(string text)
		{
			text = (string.IsNullOrEmpty(text) ? Loca.Instance.GetCurrentLanguageIsoCode() : text);
			SendMessageToLobby(text, reliable: false, 3);
		}

		public void SendEmote(SteamItem emote)
		{
			SendMessageToLobby(emote.SteamItemDefId.ToString(), reliable: true, 6);
		}

		public void SendResetLeaderboard()
		{
			SendMessageToLobby("Reset", reliable: true, 7);
		}

		public void ClientEquippedCosmetic(string itemSlot, int itemId)
		{
			MultiplayerLobby.Instance.SetClientLobbyMemberData(itemSlot, itemId.ToString());
		}

		private void SendMessageToLobby(string message, bool reliable, int channel)
		{
			if (!MultiplayerLobby.Instance.IsInLobby || SteamManager.ShuttingDown)
			{
				return;
			}
			foreach (LobbyMember member in MultiplayerLobby.Instance.GetMembers())
			{
				if (!(member.SteamId == SteamUser.GetSteamID()))
				{
					SteamMultiplayerHelper.SendMessage(message, member.NetworkingIdentity, reliable, channel);
					_ = 1;
				}
			}
		}

		public IEnumerator SetCosmeticsData()
		{
			yield return new WaitUntil(() => CatCosmetics.Instance.initialized);
			if (CatCosmetics.Instance.EquippedItems.All((SteamItem i) => i.ItemSlot != "hat"))
			{
				MultiplayerLobby.Instance.SetClientLobbyMemberData("hat", "-1");
			}
			if (CatCosmetics.Instance.EquippedItems.All((SteamItem i) => i.ItemSlot != "skin"))
			{
				MultiplayerLobby.Instance.SetClientLobbyMemberData("skin", "-1");
				yield break;
			}
			foreach (SteamItem equippedItem in CatCosmetics.Instance.EquippedItems)
			{
				if (equippedItem.ItemSlot == "hat")
				{
					MultiplayerLobby.Instance.SetClientLobbyMemberData("hat", equippedItem.SteamItemDefId.ToString());
				}
				else if (equippedItem.ItemSlot == "skin")
				{
					MultiplayerLobby.Instance.SetClientLobbyMemberData("skin", equippedItem.SteamItemDefId.ToString());
				}
			}
		}

		private IEnumerator SendingTapsRoutine()
		{
			while (true)
			{
				yield return null;
				if (_tapsToSend > 0)
				{
					if (_tapsToSend >= 8)
					{
						_tapsToSend = 0;
						continue;
					}
					SendMessageToLobby(_tapsToSend.ToString(), reliable: false, 2);
					_leaderboard.UpdateOwnTaps(_tapsToSend);
					_tapsToSend = 0;
				}
			}
		}

		private void AddTaps(int amount)
		{
			_tapsToSend += amount;
		}

		private void OnApplicationQuit()
		{
			StopMessagingRoutines();
		}
	}
}

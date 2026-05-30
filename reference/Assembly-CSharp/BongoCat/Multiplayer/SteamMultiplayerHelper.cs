using System;
using System.Runtime.InteropServices;
using System.Text;
using Steamworks;

namespace BongoCat.Multiplayer
{
	public static class SteamMultiplayerHelper
	{
		public static EResult SendMessage(string message, SteamNetworkingIdentity playerIdentity, bool reliable, int channel = 0)
		{
			if (message.Length == 0)
			{
				return EResult.k_EResultFail;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(message);
			IntPtr intPtr = SteamNetworkingUtils.AllocateMessage(bytes.Length);
			Marshal.Copy(bytes, 0, intPtr, bytes.Length);
			return SteamNetworkingMessages.SendMessageToUser(ref playerIdentity, intPtr, (uint)bytes.Length, reliable ? 8 : 0, channel);
		}
	}
}

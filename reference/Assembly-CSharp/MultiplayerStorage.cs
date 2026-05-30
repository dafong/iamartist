using System.Collections.Generic;
using BongoCat.Multiplayer;
using Steamworks;
using UnityEngine;

public class MultiplayerStorage : MonoBehaviour
{
	private const string HIDDEN_KEY = "playerHidden_";

	private static Dictionary<CSteamID, Vector2> _positions;

	private static Dictionary<CSteamID, int> _rotations;

	private void Awake()
	{
		_positions = new Dictionary<CSteamID, Vector2>();
		_rotations = new Dictionary<CSteamID, int>();
	}

	private void Start()
	{
		MultiplayerLobby.Instance.LeftLobby += ClearDictionaries;
	}

	private static void ClearDictionaries()
	{
		_positions.Clear();
		_rotations.Clear();
	}

	public static void HidePlayer(CSteamID playerId, bool hide)
	{
		if (hide)
		{
			CSteamID cSteamID = playerId;
			PlayerPrefs.SetInt("playerHidden_" + cSteamID.ToString(), 0);
		}
		else
		{
			CSteamID cSteamID = playerId;
			PlayerPrefs.DeleteKey("playerHidden_" + cSteamID.ToString());
		}
		PlayerPrefs.Save();
	}

	public static bool IsPlayerHidden(CSteamID steamID)
	{
		CSteamID cSteamID = steamID;
		return PlayerPrefs.HasKey("playerHidden_" + cSteamID.ToString());
	}

	public static void SavePlayerPos(CSteamID playerId, Vector3 pos)
	{
		Vector2 value = new Vector2(pos.x / (float)ScreenSize.FullWidth, pos.y / (float)ScreenSize.FullHeight);
		_positions[playerId] = value;
	}

	public static bool IsPlayerPosSaved(CSteamID steamID)
	{
		return _positions.ContainsKey(steamID);
	}

	public static Vector3 GetPlayerPos(CSteamID steamID)
	{
		if (!_positions.TryGetValue(steamID, out var value))
		{
			return Vector3.zero;
		}
		return new Vector3(value.x * (float)ScreenSize.FullWidth, value.y * (float)ScreenSize.FullHeight);
	}

	public static void SavePlayerRot(CSteamID playerId, int state)
	{
		_rotations[playerId] = state;
	}

	public static int GetPlayerRot(CSteamID steamID)
	{
		_rotations.TryGetValue(steamID, out var value);
		return value;
	}
}

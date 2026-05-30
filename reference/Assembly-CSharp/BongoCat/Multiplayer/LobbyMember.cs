using System;
using System.Collections;
using System.Collections.Generic;
using IroxGames.StoreFronts.Steam;
using Steamworks;
using UnityEngine;
using Vfx;

namespace BongoCat.Multiplayer
{
	public class LobbyMember : MonoBehaviour
	{
		private CSteamID _steamId;

		private SteamNetworkingIdentity _networkingIdentity;

		private LobbyListEntry _lobbyListEntry;

		[SerializeField]
		private Cat _cat;

		[SerializeField]
		private CatCosmeticsMultiplayer _cosmetics;

		[SerializeField]
		private CatSpeech _catSpeech;

		[SerializeField]
		private EmoteSpawner _emoteSpawner;

		[SerializeField]
		private PlayerNameTag _nameTag;

		[SerializeField]
		private MultiplayerItemHandler _multiplayerItemHandler;

		[SerializeField]
		private LobbyListEntry _lobbyListEntryPrefab;

		[SerializeField]
		private CatRotator _catRotator;

		[SerializeField]
		private Draggable _draggable;

		private const int CAT_VISUAL_TRANSFORM_WIDTH = 156;

		private const string HAT_ITEMSLOT_KEY = "hat";

		private const string SKIN_ITEMSLOT_KEY = "skin";

		private bool _isHidden;

		public bool IsHidden
		{
			get
			{
				return _isHidden;
			}
			set
			{
				_isHidden = value;
				_cat.gameObject.SetActive(!_isHidden);
				MultiplayerStorage.HidePlayer(_steamId, value);
			}
		}

		public bool CatActivated
		{
			get
			{
				if ((bool)_cat)
				{
					return _cat.gameObject.activeSelf;
				}
				return false;
			}
		}

		public Cat Cat => _cat;

		public CatCosmeticsMultiplayer CatCosmetics => _cosmetics;

		public CatSpeech CatSpeech => _catSpeech;

		public EmoteSpawner EmoteSpawner => _emoteSpawner;

		public CSteamID SteamId => _steamId;

		public ulong SteamId64 => _steamId.m_SteamID;

		public SteamNetworkingIdentity NetworkingIdentity => _networkingIdentity;

		public LobbyListEntry LobbyListEntry => _lobbyListEntry;

		public MultiplayerItemHandler MultiplayerItemHandler => _multiplayerItemHandler;

		public void Init(CSteamID steamId, Transform lobbyListEntryRoot)
		{
			if (steamId.IsValid())
			{
				_steamId = steamId;
				_networkingIdentity = default(SteamNetworkingIdentity);
				_networkingIdentity.SetSteamID(_steamId);
				_lobbyListEntry = global::UnityEngine.Object.Instantiate(_lobbyListEntryPrefab, lobbyListEntryRoot);
				_lobbyListEntry.SetData(steamId, isOwner: false);
				LobbyListEntry lobbyListEntry = _lobbyListEntry;
				lobbyListEntry.OnToggleVisibility = (Action<bool>)Delegate.Combine(lobbyListEntry.OnToggleVisibility, new Action<bool>(OnToggleVisibility));
				_nameTag.Init(_steamId);
				_draggable.OnDragFinished.AddListener(SavePos);
				_catRotator.SetPlayerId(steamId);
				StartCoroutine(HideCatDelayed());
			}
		}

		private IEnumerator HideCatDelayed()
		{
			yield return null;
			yield return null;
			yield return null;
			_lobbyListEntry.SetVisibility(!MultiplayerStorage.IsPlayerHidden(_steamId) && !SettingsManager.Instance.AutoHideCats.Value);
		}

		public void FetchCosmetics(CSteamID lobbyId)
		{
			string lobbyMemberData = SteamMatchmaking.GetLobbyMemberData(lobbyId, SteamId, "hat");
			string lobbyMemberData2 = SteamMatchmaking.GetLobbyMemberData(lobbyId, SteamId, "skin");
			if (!string.IsNullOrWhiteSpace(lobbyMemberData) && !string.IsNullOrWhiteSpace(lobbyMemberData2))
			{
				if (int.TryParse(lobbyMemberData, out var result))
				{
					EquipCosmetic("hat", result);
				}
				if (int.TryParse(lobbyMemberData2, out var result2))
				{
					EquipCosmetic("skin", result2);
				}
			}
		}

		private void EquipCosmetic(string itemSlot, int itemId)
		{
			if (!CatActivated && !IsHidden)
			{
				ActivateCat();
			}
			if (itemId == -1)
			{
				CatCosmetics.UnequipItem(itemSlot);
			}
			else
			{
				CatCosmetics.EquipItem(new SteamItem(new SteamItemDef_t(itemId)));
			}
		}

		public void ReparentAndPositionCat(Transform parent)
		{
			if (MultiplayerStorage.IsPlayerPosSaved(_steamId))
			{
				_cat.transform.SetParent(parent);
				_draggable.transform.position = MultiplayerStorage.GetPlayerPos(_steamId);
				_draggable.FetchRelativePos();
				return;
			}
			List<RectTransform> list = new List<RectTransform>();
			foreach (Transform item in parent)
			{
				list.Add(item as RectTransform);
			}
			_cat.transform.SetParent(parent);
			DisplayInfo mainWindowDisplayInfo = Screen.mainWindowDisplayInfo;
			int num = Mathf.CeilToInt((float)(mainWindowDisplayInfo.width / 156) * SettingsManager.Instance.CatScaleSetting.GetRealScaleFactor());
			if (list.Count > num)
			{
				PositionCatRandomly();
				return;
			}
			MainCat mainCat = global::UnityEngine.Object.FindAnyObjectByType<MainCat>();
			RectTransform rectTransform = mainCat.transform as RectTransform;
			if (!rectTransform)
			{
				PositionCatRandomly();
				return;
			}
			int num2 = Mathf.CeilToInt(156f * SettingsManager.Instance.CatScaleSetting.GetRealScaleFactor());
			int num3 = Mathf.CeilToInt(156f * SettingsManager.Instance.CatScaleSetting.GetRealScaleFactor());
			Vector3 position = mainCat.transform.position;
			Queue<Vector2Int> queue = new Queue<Vector2Int>();
			queue.Enqueue(new Vector2Int((int)position.x, (int)position.y));
			HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
			RectTransform rectTransform2 = _cat.transform as RectTransform;
			Rect rect = default(Rect);
			Rect other = default(Rect);
			Vector2Int result;
			while (queue.TryDequeue(out result))
			{
				rect.Set(result.x, result.y, num2, num3);
				bool flag = false;
				foreach (RectTransform item2 in list)
				{
					other.Set(item2.position.x, item2.position.y, num2, num3);
					if (rect.Overlaps(other))
					{
						flag = true;
						break;
					}
				}
				other.Set(rectTransform.position.x, rectTransform.position.y, num2, num3);
				if (rect.Overlaps(other))
				{
					flag = true;
				}
				if (!flag)
				{
					rectTransform2.position = new Vector3(result.x, result.y);
					_draggable.FetchRelativePos();
					return;
				}
				Vector2Int vector2Int = result + Vector2Int.left * num2;
				if (mainWindowDisplayInfo.workArea.Contains(vector2Int) && !hashSet.Contains(vector2Int))
				{
					queue.Enqueue(vector2Int);
				}
				Vector2Int vector2Int2 = result + Vector2Int.right * num2;
				if (mainWindowDisplayInfo.workArea.Contains(vector2Int2) && !hashSet.Contains(vector2Int2))
				{
					queue.Enqueue(vector2Int2);
				}
				hashSet.Add(result);
			}
			PositionCatRandomly();
		}

		private void PositionCatRandomly()
		{
			DisplayInfo mainWindowDisplayInfo = Screen.mainWindowDisplayInfo;
			_cat.transform.position = new Vector3(global::UnityEngine.Random.Range(mainWindowDisplayInfo.workArea.xMin, mainWindowDisplayInfo.workArea.xMax), global::UnityEngine.Random.Range(mainWindowDisplayInfo.workArea.yMin, mainWindowDisplayInfo.workArea.yMax), 0f);
			_draggable.FetchRelativePos();
			SavePos();
		}

		private void SavePos()
		{
			MultiplayerStorage.SavePlayerPos(_steamId, _draggable.transform.position);
		}

		public void ActivateCat()
		{
			_cat.gameObject.SetActive(value: true);
		}

		private void OnToggleVisibility(bool isVisible)
		{
			IsHidden = !isVisible;
		}

		private void OnDestroy()
		{
			if (SteamManager.ShuttingDown)
			{
				return;
			}
			LobbyListEntry lobbyListEntry = _lobbyListEntry;
			lobbyListEntry.OnToggleVisibility = (Action<bool>)Delegate.Remove(lobbyListEntry.OnToggleVisibility, new Action<bool>(OnToggleVisibility));
			if ((bool)_lobbyListEntry)
			{
				global::UnityEngine.Object.Destroy(_lobbyListEntry.gameObject);
			}
			if ((bool)_cat)
			{
				if ((bool)BattleRoyale.Instance && (BattleRoyale.Instance.InBattle || BattleRoyale.Instance.HasWon))
				{
					BattleRoyale.Instance.KillCat(_cat.transform);
				}
				else
				{
					global::UnityEngine.Object.Destroy(_cat.gameObject);
				}
			}
			if ((bool)_draggable)
			{
				_draggable.OnDragFinished.RemoveListener(SavePos);
			}
		}
	}
}

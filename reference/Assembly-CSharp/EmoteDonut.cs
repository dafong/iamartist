using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BongoCat;
using BongoCat.OSSpecific;
using UnityEngine;

public class EmoteDonut : MonoBehaviour
{
	[SerializeField]
	private int maxEmoteCount;

	[SerializeField]
	private float radius;

	[SerializeField]
	private EmoteDonutEntry emoteEntryPrefab;

	[SerializeField]
	private EmoteSpawner emoteSpawner;

	[SerializeField]
	private Transform emoteEntryParent;

	[SerializeField]
	private GameObject _menu;

	private Dictionary<int, EmoteDonutEntry> IDtoEmoteEntry = new Dictionary<int, EmoteDonutEntry>();

	private bool isOpen;

	public static EmoteDonut Instance;

	private const string EQUIPPED_EMOTES_KEY = "EQUIPPED_EMOTES";

	[SerializeField]
	private float _interactionDistance;

	private ScaleSetting _scaleSetting;

	private void Awake()
	{
		Instance = this;
		_scaleSetting = SettingsManager.Instance.UIScaleSetting;
	}

	private IEnumerator Start()
	{
		yield return new WaitUntil(() => CatInventory.Instance.WasLoadedFromSteam);
		if (!PlayerPrefs.HasKey("EQUIPPED_EMOTES") || string.IsNullOrEmpty(PlayerPrefs.GetString("EQUIPPED_EMOTES")))
		{
			yield break;
		}
		List<int> ids = PlayerPrefs.GetString("EQUIPPED_EMOTES").Split(',').Select(int.Parse)
			.ToList();
		List<SteamItem> emotesToEquip = CatInventory.Instance.Items.Where((SteamItem item) => ids.Contains(item.SteamItemDefId) && item.ItemAmount > 0).ToList();
		yield return new WaitUntil(() => emotesToEquip.All((SteamItem item) => item.IsReady));
		foreach (SteamItem item in emotesToEquip)
		{
			AddEmote(item);
		}
	}

	private void Update()
	{
		bool flag = false;
		Vector2 a = Input.mousePosition;
		foreach (Transform item in emoteEntryParent)
		{
			Vector2 b = RectTransformUtility.WorldToScreenPoint(null, item.position);
			if (Vector2.Distance(a, b) < _interactionDistance * _scaleSetting.GetScale())
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			Open();
		}
		else
		{
			Close();
		}
	}

	private void AddEmote(SteamItem steamItem)
	{
		EmoteDonutEntry emoteDonutEntry = UnityEngine.Object.Instantiate(emoteEntryPrefab, emoteEntryParent);
		emoteDonutEntry.SetItem(steamItem, emoteSpawner, this);
		IDtoEmoteEntry.Add(steamItem.SteamItemDefId, emoteDonutEntry);
		if (!isOpen)
		{
			emoteDonutEntry.gameObject.SetActive(value: false);
		}
		RearrangeEmotes();
		steamItem.IsEquipped = true;
		SaveToPlayerPrefs();
		steamItem.OnItemUpdated?.Invoke();
	}

	private void RemoveEmote(SteamItem steamItem)
	{
		UnityEngine.Object.Destroy(IDtoEmoteEntry[steamItem.SteamItemDefId].gameObject);
		IDtoEmoteEntry.Remove(steamItem.SteamItemDefId);
		steamItem.IsEquipped = false;
		SaveToPlayerPrefs();
		RearrangeEmotes();
		steamItem.OnItemUpdated?.Invoke();
	}

	public void Validate()
	{
		if (!PlayerPrefs.HasKey("EQUIPPED_EMOTES"))
		{
			return;
		}
		string text = PlayerPrefs.GetString("EQUIPPED_EMOTES");
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		List<int> ids = text.Split(',').Select(int.Parse).ToList();
		List<SteamItem> source = CatInventory.Instance.Items.Where((SteamItem item) => ids.Contains(item.SteamItemDefId) && item.ItemAmount > 0 && (item.IsEmote || item.IsConsumable)).ToList();
		foreach (int id in IDtoEmoteEntry.Keys.ToList())
		{
			if (!source.Any((SteamItem item) => item.SteamItemDefId == id))
			{
				SteamItem steamItem = CatInventory.Instance.Items.FirstOrDefault((SteamItem item) => item.SteamItemDefId == id);
				if (steamItem != null)
				{
					RemoveEmote(steamItem);
				}
			}
		}
	}

	private void SaveToPlayerPrefs()
	{
		PlayerPrefs.SetString("EQUIPPED_EMOTES", string.Join(",", IDtoEmoteEntry.Keys));
		PlayerPrefs.Save();
	}

	private void RearrangeEmotes()
	{
		float num = 20 * IDtoEmoteEntry.Count;
		List<EmoteDonutEntry> list = IDtoEmoteEntry.Values.ToList();
		for (int i = 0; i < list.Count; i++)
		{
			EmoteDonutEntry emoteDonutEntry = list[i];
			float num2 = (float)i / (float)(list.Count - 1) * num * ((float)Math.PI / 180f);
			num2 += (0f - num) / 2f * ((float)Math.PI / 180f);
			if (list.Count < 2)
			{
				num2 = 0f;
			}
			emoteDonutEntry.transform.localPosition = new Vector3(Mathf.Sin(num2), Mathf.Cos(num2), 0f) * radius;
		}
	}

	private void Open()
	{
		if (isOpen || TransparentWindow.Instance.GamingModeEnabled || _menu.gameObject.activeSelf)
		{
			return;
		}
		isOpen = true;
		foreach (EmoteDonutEntry value in IDtoEmoteEntry.Values)
		{
			value.gameObject.SetActive(value: true);
		}
	}

	private void Close()
	{
		if (!isOpen)
		{
			return;
		}
		isOpen = false;
		foreach (EmoteDonutEntry value in IDtoEmoteEntry.Values)
		{
			value.gameObject.SetActive(value: false);
		}
	}

	public void ToggleItem(SteamItem steamItem)
	{
		if (IDtoEmoteEntry.Keys.Contains(steamItem.SteamItemDefId))
		{
			RemoveEmote(steamItem);
		}
		else if (maxEmoteCount > IDtoEmoteEntry.Count)
		{
			AddEmote(steamItem);
		}
	}
}

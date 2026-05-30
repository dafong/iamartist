using System;
using BongoCat;
using BongoCat.Achievements;
using BongoCat.Multiplayer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmoteDonutEntry : MonoBehaviour
{
	[SerializeField]
	private Image image;

	[SerializeField]
	private GameObject counter;

	[SerializeField]
	private TMP_Text itemAmount;

	private EmoteSpawner _emoteSpawner;

	private EmoteDonut _emoteDonut;

	private SteamItem _steamItem;

	public void SetItem(SteamItem steamItem, EmoteSpawner emoteSpawner, EmoteDonut emoteDonut)
	{
		_steamItem = steamItem;
		_emoteSpawner = emoteSpawner;
		_emoteDonut = emoteDonut;
		image.sprite = _steamItem.Icon;
		if (_steamItem.IsConsumable)
		{
			SteamItem steamItem2 = _steamItem;
			steamItem2.OnItemUpdated = (Action)Delegate.Combine(steamItem2.OnItemUpdated, new Action(SetItemAmount));
			counter.SetActive(value: true);
			SetItemAmount();
		}
	}

	private void OnDestroy()
	{
		if (_steamItem.IsConsumable)
		{
			SteamItem steamItem = _steamItem;
			steamItem.OnItemUpdated = (Action)Delegate.Remove(steamItem.OnItemUpdated, new Action(SetItemAmount));
		}
	}

	public void SpawnEmoteParticle()
	{
		if (_steamItem != null)
		{
			_emoteSpawner.SpawnEmoteParticle(_steamItem);
			SteamMultiplayer.Instance.SendEmote(_steamItem);
			AchievementStats.IncrementEmotesUsed();
			if (_steamItem.IsConsumable)
			{
				Consume();
			}
		}
	}

	private void Consume()
	{
		_steamItem.Consumed++;
		SetItemAmount();
		_steamItem.OnItemUpdated?.Invoke();
		ItemConsumer.Instance.ConsumeItem(_steamItem);
	}

	private void SetItemAmount()
	{
		int displayedItemAmount = _steamItem.DisplayedItemAmount;
		if (displayedItemAmount <= 0)
		{
			SteamItem steamItem = _steamItem;
			steamItem.OnItemUpdated = (Action)Delegate.Remove(steamItem.OnItemUpdated, new Action(SetItemAmount));
			_emoteDonut.ToggleItem(_steamItem);
			CatInventory.Instance.UpdateVisuals();
		}
		else if (displayedItemAmount >= 1000)
		{
			itemAmount.text = (displayedItemAmount / 1000).ToString("D") + "k";
		}
		else
		{
			itemAmount.text = displayedItemAmount.ToString();
		}
	}
}

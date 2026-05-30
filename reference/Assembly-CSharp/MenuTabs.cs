using System;
using Steam;
using UnityEngine;
using UnityEngine.UI;

public class MenuTabs : MonoBehaviour
{
	public enum MenuTab
	{
		Inventory = 0,
		Bongodecks = 1,
		Settings = 2,
		Exchange = 3,
		Multiplayer = 4,
		TrashBin = 5,
		Supporter = 6
	}

	[SerializeField]
	private GameObject[] _inventory;

	[SerializeField]
	private GameObject[] _bongodecks;

	[SerializeField]
	private GameObject[] _settings;

	[SerializeField]
	private GameObject[] _exchange;

	[SerializeField]
	private GameObject[] _multiplayer;

	[SerializeField]
	private GameObject[] _supporter;

	[SerializeField]
	private GameObject[] _presets;

	[SerializeField]
	private GameObject[] _trashBin;

	[SerializeField]
	private Button _inventoryButton;

	[SerializeField]
	private Button _bongodecksButton;

	[SerializeField]
	private Button _settingsButton;

	[SerializeField]
	private Button _exchangeButton;

	[SerializeField]
	private Button _multiplayerButton;

	[SerializeField]
	private Button _supporterButton;

	[SerializeField]
	private Color selectedColor;

	[SerializeField]
	private Color unselectedColor;

	private MenuTab _currentTab;

	public Action OnCloseBongoDecks;

	public static Action OnCloseExchange;

	public void OpenMenuTab(MenuTab menuTab)
	{
		if (_currentTab == MenuTab.Bongodecks && menuTab != MenuTab.Bongodecks)
		{
			OnCloseBongoDecks?.Invoke();
		}
		if (_currentTab == MenuTab.Exchange && menuTab != MenuTab.Exchange)
		{
			OnCloseExchange?.Invoke();
		}
		_currentTab = menuTab;
		GameObject[] inventory = _inventory;
		for (int i = 0; i < inventory.Length; i++)
		{
			inventory[i].SetActive(menuTab == MenuTab.Inventory || menuTab == MenuTab.Exchange || menuTab == MenuTab.TrashBin);
		}
		inventory = _bongodecks;
		for (int i = 0; i < inventory.Length; i++)
		{
			inventory[i].SetActive(menuTab == MenuTab.Bongodecks);
		}
		inventory = _settings;
		for (int i = 0; i < inventory.Length; i++)
		{
			inventory[i].SetActive(menuTab == MenuTab.Settings);
		}
		inventory = _exchange;
		for (int i = 0; i < inventory.Length; i++)
		{
			inventory[i].SetActive(menuTab == MenuTab.Exchange);
		}
		inventory = _multiplayer;
		for (int i = 0; i < inventory.Length; i++)
		{
			inventory[i].SetActive(menuTab == MenuTab.Multiplayer);
		}
		inventory = _supporter;
		for (int i = 0; i < inventory.Length; i++)
		{
			inventory[i].SetActive(menuTab == MenuTab.Supporter);
		}
		inventory = _presets;
		for (int i = 0; i < inventory.Length; i++)
		{
			inventory[i].SetActive(menuTab == MenuTab.Inventory);
		}
		inventory = _trashBin;
		for (int i = 0; i < inventory.Length; i++)
		{
			inventory[i].SetActive(menuTab == MenuTab.TrashBin);
		}
		_inventoryButton.GetComponent<Image>().color = ((menuTab == MenuTab.Inventory || menuTab == MenuTab.TrashBin) ? selectedColor : unselectedColor);
		_bongodecksButton.GetComponent<Image>().color = ((menuTab == MenuTab.Bongodecks) ? selectedColor : unselectedColor);
		_settingsButton.GetComponent<Image>().color = ((menuTab == MenuTab.Settings) ? selectedColor : unselectedColor);
		_exchangeButton.GetComponent<Image>().color = ((menuTab == MenuTab.Exchange) ? selectedColor : unselectedColor);
		_multiplayerButton.GetComponent<Image>().color = ((menuTab == MenuTab.Multiplayer) ? selectedColor : unselectedColor);
		_supporterButton.GetComponent<Image>().color = ((menuTab == MenuTab.Supporter) ? selectedColor : unselectedColor);
		GetComponentInChildren<ScrollRect>().normalizedPosition = new Vector2(0f, 1f);
		if (menuTab != MenuTab.TrashBin)
		{
			TrashCan.Instance.RestoreAllItems();
		}
	}

	public void OpenMenuTab(int menuTab)
	{
		OpenMenuTab((MenuTab)menuTab);
	}

	private void OnEnable()
	{
		GetComponentInChildren<ScrollRect>().normalizedPosition = new Vector2(0f, 1f);
	}

	private void OnDisable()
	{
		if (_currentTab == MenuTab.Bongodecks)
		{
			OnCloseBongoDecks?.Invoke();
		}
	}
}

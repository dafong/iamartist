using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemScaleSetting : MonoBehaviour
{
	public static ItemScaleSetting Instance;

	private const string KEY = "ITEMS_SCALE";

	private List<ItemScaler> _itemScalers;

	[SerializeField]
	private Slider[] itemScaleSliders;

	[SerializeField]
	private TMP_Text[] itemScaleTexts;

	public int Count { get; private set; }

	private void Awake()
	{
		Instance = this;
		_itemScalers = new List<ItemScaler>();
		Slider[] array = itemScaleSliders;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].onValueChanged.AddListener(ChangeMenuItemScale);
		}
		Count = PlayerPrefs.GetInt("ITEMS_SCALE", 5);
	}

	private IEnumerator Start()
	{
		yield return null;
		ChangeMenuItemScale(Count);
	}

	public void AddScaler(ItemScaler scaler)
	{
		_itemScalers.Add(scaler);
	}

	public void ChangeMenuItemScale(float newCount)
	{
		Count = (int)newCount;
		PlayerPrefs.SetInt("ITEMS_SCALE", Count);
		PlayerPrefs.Save();
		foreach (ItemScaler itemScaler in _itemScalers)
		{
			itemScaler.ChangeCellCount();
		}
		Slider[] array = itemScaleSliders;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].value = newCount;
		}
		TMP_Text[] array2 = itemScaleTexts;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i].text = newCount.ToString(CultureInfo.CurrentCulture);
		}
	}
}

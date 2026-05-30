using UnityEngine;
using UnityEngine.UI;

public class ItemScaler : MonoBehaviour
{
	private const int WIDTH = 200;

	private GridLayoutGroup _grid;

	private ItemScaleSetting _scaleSetting;

	private void Awake()
	{
		_grid = GetComponent<GridLayoutGroup>();
	}

	private void Start()
	{
		_scaleSetting = ItemScaleSetting.Instance;
		_scaleSetting.AddScaler(this);
		ChangeCellCount();
	}

	private void OnEnable()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.parent as RectTransform);
	}

	public void ChangeCellCount()
	{
		int count = _scaleSetting.Count;
		float num = 200f / (float)count;
		_grid.cellSize = new Vector2(num, num);
		_grid.constraintCount = count;
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform.parent as RectTransform);
	}
}

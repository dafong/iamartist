using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BongoCat.Testing.Multiplayer
{
	public class BotManager : MonoBehaviour
	{
		[SerializeField]
		private Transform _root;

		[SerializeField]
		private int _botNumber;

		[SerializeField]
		private CatBot _catBotPrefab;

		private List<CatBot> _catBots = new List<CatBot>();

		[ContextMenu("Spawn Cats")]
		public void SpawnCats()
		{
			StartCoroutine(SpawnCatsRoutine());
		}

		private IEnumerator SpawnCatsRoutine()
		{
			for (int i = 0; i < _botNumber; i++)
			{
				CatBot catBot = Object.Instantiate(_catBotPrefab);
				ReparentAndPositionCat(catBot, _root);
				_catBots.Add(catBot);
				yield return new WaitForSeconds(0.1f);
			}
		}

		public void ReparentAndPositionCat(CatBot cat, Transform parent)
		{
			List<RectTransform> list = new List<RectTransform>();
			foreach (Transform item in parent)
			{
				list.Add(item as RectTransform);
			}
			cat.transform.SetParent(parent);
			DisplayInfo mainWindowDisplayInfo = Screen.mainWindowDisplayInfo;
			if (list.Count > 0)
			{
				cat.transform.position = new Vector3(Random.Range(mainWindowDisplayInfo.workArea.xMin, mainWindowDisplayInfo.workArea.xMax), Random.Range(mainWindowDisplayInfo.workArea.yMin, mainWindowDisplayInfo.workArea.yMax), 0f);
			}
			MainCat mainCat = Object.FindAnyObjectByType<MainCat>();
			RectTransform rectTransform = mainCat.transform as RectTransform;
			if (!rectTransform)
			{
				cat.transform.position = new Vector3(Random.Range(mainWindowDisplayInfo.workArea.xMin, mainWindowDisplayInfo.workArea.xMax), Random.Range(mainWindowDisplayInfo.workArea.yMin, mainWindowDisplayInfo.workArea.yMax), 0f);
				return;
			}
			int num = (int)(rectTransform.rect.width * SettingsManager.Instance.CatScaleSetting.GetRealScaleFactor());
			int num2 = (int)(rectTransform.rect.height * SettingsManager.Instance.CatScaleSetting.GetRealScaleFactor());
			Vector3 position = mainCat.transform.position;
			Queue<Vector2Int> queue = new Queue<Vector2Int>();
			queue.Enqueue(new Vector2Int((int)position.x, (int)position.y));
			HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
			RectTransform rectTransform2 = cat.transform as RectTransform;
			Rect rect = default(Rect);
			Rect other = default(Rect);
			Vector2Int result;
			while (queue.TryDequeue(out result))
			{
				rect.Set(result.x, result.y, num, num2);
				bool flag = false;
				foreach (RectTransform item2 in list)
				{
					other.Set(item2.position.x, item2.position.y, num, num2);
					if (rect.Overlaps(other))
					{
						flag = true;
						break;
					}
				}
				other.Set(rectTransform.position.x, rectTransform.position.y, num, num2);
				if (rect.Overlaps(other))
				{
					flag = true;
				}
				if (!flag)
				{
					rectTransform2.position = new Vector3(result.x, result.y);
					return;
				}
				Vector2Int vector2Int = result + Vector2Int.left * num;
				if (mainWindowDisplayInfo.workArea.Contains(vector2Int) && !hashSet.Contains(vector2Int))
				{
					queue.Enqueue(vector2Int);
				}
				Vector2Int vector2Int2 = result + Vector2Int.right * num;
				if (mainWindowDisplayInfo.workArea.Contains(vector2Int2) && !hashSet.Contains(vector2Int2))
				{
					queue.Enqueue(vector2Int2);
				}
				hashSet.Add(result);
			}
			cat.transform.position = new Vector3(Random.Range(mainWindowDisplayInfo.workArea.xMin, mainWindowDisplayInfo.workArea.xMax), Random.Range(mainWindowDisplayInfo.workArea.yMin, mainWindowDisplayInfo.workArea.yMax), 0f);
		}
	}
}

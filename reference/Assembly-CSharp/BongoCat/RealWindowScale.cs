using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BongoCat
{
	public class RealWindowScale : MonoBehaviour
	{
		private int _windowHeight = 450;

		[NonSerialized]
		public float Scaling = 1f;

		[NonSerialized]
		public float RealScaling = 1f;

		public static RealWindowScale Instance;

		public static float DpiScale
		{
			get
			{
				float num = 96f;
				return Screen.dpi / num;
			}
		}

		private void Awake()
		{
			Instance = this;
		}

		public List<float> GetScales(List<float> originalScales)
		{
			List<float> list = new List<float>();
			foreach (float originalScale in originalScales)
			{
				if ((int)(450f * DpiScale * originalScale) <= Screen.mainWindowDisplayInfo.height)
				{
					list.Add(originalScale);
				}
			}
			if (list.Count == 0)
			{
				list.Add(1f);
			}
			list = list.OrderBy((float x) => x).ToList();
			float num = (float)Screen.mainWindowDisplayInfo.height / (650f * DpiScale);
			List<float> list2 = list;
			if (num - list2[list2.Count - 1] > 0.1f)
			{
				list.Add(num);
			}
			return list;
		}

		public void SetScale(float scaling)
		{
			Scaling = scaling;
			_windowHeight = (int)(450f * DpiScale * Scaling);
			_windowHeight = Math.Min(Screen.mainWindowDisplayInfo.height, _windowHeight);
		}
	}
}

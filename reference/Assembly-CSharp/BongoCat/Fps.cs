using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BongoCat
{
	public class Fps : MonoBehaviour
	{
		private const string KEY = "FPS";

		private List<int> _options = new List<int> { 15, 30, 60 };

		private int _index;

		[SerializeField]
		private TMP_Text _text;

		private void Awake()
		{
			Application.runInBackground = true;
			int item = PlayerPrefs.GetInt("FPS", 60);
			_index = Mathf.Max(0, _options.IndexOf(item));
			UpdateValue();
		}

		public void ToggleForward()
		{
			_index = (_index + 1) % _options.Count;
			UpdateValue();
		}

		private void UpdateValue()
		{
			int num = _options[_index];
			PlayerPrefs.SetInt("FPS", num);
			PlayerPrefs.Save();
			_text.text = $"FPS: {num}";
			Application.targetFrameRate = num;
		}

		public void ToggleBackward()
		{
			_index = (_index - 1 + _options.Count) % _options.Count;
			UpdateValue();
		}
	}
}

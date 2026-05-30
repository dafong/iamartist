using System;
using System.Collections.Generic;
using UnityEngine;

namespace BongoCat
{
	public class ToggleObjectsWithKey : MonoBehaviour
	{
		[SerializeField]
		private List<GameObject> _objectsToToggle;

		private bool _showObjects = true;

		public bool ShowObjects => _showObjects;

		public event Action<bool> OnToggleWithKey;

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F2))
			{
				ToggleObjects();
			}
		}

		private void ToggleObjects()
		{
			_showObjects = !_showObjects;
			foreach (GameObject item in _objectsToToggle)
			{
				item.SetActive(_showObjects);
			}
			this.OnToggleWithKey?.Invoke(_showObjects);
		}
	}
}

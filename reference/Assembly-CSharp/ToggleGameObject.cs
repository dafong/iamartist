using System;
using UnityEngine;

public class ToggleGameObject : MonoBehaviour
{
	public event Action OnToggle;

	public void Toggle()
	{
		bool active = !base.gameObject.activeSelf;
		base.gameObject.SetActive(active);
		this.OnToggle?.Invoke();
	}

	public void SetActive(bool active)
	{
		base.gameObject.SetActive(active);
	}
}

using System.Collections;
using BongoCat;
using UnityEngine;

public class OnDefocusCallback : MonoBehaviour
{
	[SerializeField]
	private GameObject _gameObject;

	[SerializeField]
	private float _delaySecondsOnStartup;

	[SerializeField]
	private bool _isShop;

	private bool _canHideObjects;

	private IEnumerator Start()
	{
		yield return new WaitForSecondsRealtime(_delaySecondsOnStartup);
		_canHideObjects = true;
		OnApplicationFocus(Application.isFocused);
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if ((!_isShop || !SettingsManager.Instance.AlwaysShowChest.Value) && !hasFocus && _canHideObjects && _gameObject.activeSelf)
		{
			_gameObject.SetActive(value: false);
		}
	}
}

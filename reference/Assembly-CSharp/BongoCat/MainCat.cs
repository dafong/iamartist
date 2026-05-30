using System;
using System.Collections;
using System.Linq;
using BongoCat.OSSpecific;
using IroxGames.StoreFronts.Steam;
using TMPro;
using UnityEngine;

namespace BongoCat
{
	public class MainCat : MonoBehaviour
	{
		[SerializeField]
		private Draggable _draggable;

		[SerializeField]
		private PlayerPrefsToggle _lockPosition;

		[SerializeField]
		private Cat _cat;

		private Pets _pets;

		private const string MENU_POS_X_REL = "BongoMenuPosXRel";

		private const string MENU_POS_Y_REL = "BongoMenuPosYRel";

		private IEnumerator Start()
		{
			_pets = Pets.Instance;
			BongoCat.OSSpecific.GlobalKeyHook globalKeyHook = global::UnityEngine.Object.FindAnyObjectByType<BongoCat.OSSpecific.GlobalKeyHook>();
			globalKeyHook.OnKeyPressed = (Action<int>)Delegate.Combine(globalKeyHook.OnKeyPressed, new Action<int>(_cat.Tap));
			_cat.OnTap.AddListener(HandleTap);
			yield return null;
			if (PlayerPrefs.HasKey("BongoMenuPosXRel") && PlayerPrefs.HasKey("BongoMenuPosYRel"))
			{
				float value = PlayerPrefs.GetFloat("BongoMenuPosXRel");
				value = Mathf.Clamp(value, 0f, 1f);
				float value2 = PlayerPrefs.GetFloat("BongoMenuPosYRel");
				value2 = Mathf.Clamp(value2, 0f, 1f);
				_draggable.transform.position = new Vector3(value * (float)ScreenSize.FullWidth, value2 * (float)ScreenSize.FullHeight);
			}
			else
			{
				ResetPosition();
			}
			_draggable.FetchRelativePos();
			CheckBoundaries();
			SavePosition();
			_draggable.OnDragFinished.AddListener(SavePosition);
			StartCoroutine(CheckCatIsOnScreenRoutine());
		}

		private void CheckBoundaries()
		{
			_draggable.OutOfBoundsFix();
		}

		private IEnumerator CheckCatIsOnScreenRoutine()
		{
			WaitForSeconds waitFor30Seconds = new WaitForSeconds(30f);
			while (true)
			{
				yield return waitFor30Seconds;
			}
		}

		private void HandleTap(int amount)
		{
			_pets.AddPet(amount);
		}

		private void SavePosition()
		{
			PlayerPrefs.SetFloat("BongoMenuPosXRel", _draggable.transform.position.x / (float)ScreenSize.FullWidth);
			PlayerPrefs.SetFloat("BongoMenuPosYRel", _draggable.transform.position.y / (float)ScreenSize.FullHeight);
			PlayerPrefs.Save();
		}

		private void OnDestroy()
		{
			_draggable.OnDragFinished.RemoveListener(SavePosition);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F1))
			{
				ResetPosition();
				SettingsManager.Instance.CatScaleSetting.ResetScale();
				SettingsManager.Instance.UIScaleSetting.ResetScale();
			}
		}

		private void ResetPosition()
		{
			PlayerPrefs.DeleteKey("BongoMenuPosXRel");
			PlayerPrefs.DeleteKey("BongoMenuPosYRel");
			_lockPosition.Value = false;
			_draggable.transform.position = new Vector2((float)ScreenSize.FullWidth / 2f, (float)ScreenSize.FullHeight / 2f);
			_draggable.FetchRelativePos();
			Debug.Log("Resetting position to center of screen: " + _draggable.transform.position.ToString());
		}

		public void OnApplicationQuit()
		{
			if (SteamManager.s_EverInitialized)
			{
				SavePosition();
			}
		}

		public static bool FocusingInputfield()
		{
			return global::UnityEngine.Object.FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None).Any((TMP_InputField inputField) => inputField.isFocused);
		}
	}
}

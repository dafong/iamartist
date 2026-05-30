using System.Collections;
using System.IO;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace BongoCat.OSSpecific
{
	public class ScreenshotHelper : MonoBehaviour
	{
		[SerializeField]
		private Camera _camera;

		[SerializeField]
		private GameObject _gameCanvas;

		[SerializeField]
		private GameObject _multiplayerCanvas;

		[SerializeField]
		private GameObject _screenshotCanvas;

		[SerializeField]
		private Image _hat;

		[SerializeField]
		private Image _base;

		[SerializeField]
		private Image _front;

		private CatCosmetics _catCosmetics;

		private Cat _cat;

		private void Awake()
		{
			_catCosmetics = Object.FindAnyObjectByType<CatCosmetics>(FindObjectsInactive.Include);
			_cat = Object.FindAnyObjectByType<Cat>(FindObjectsInactive.Include);
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F12))
			{
				StartCoroutine(CaptureScreenshotRoutine());
			}
		}

		public void CaptureScreenshot()
		{
			StartCoroutine(CaptureScreenshotRoutine());
		}

		private IEnumerator CaptureScreenshotRoutine()
		{
			_hat.sprite = _catCosmetics.GetHatSprite();
			_hat.enabled = _hat.sprite != null;
			_base.sprite = _cat.GetBaseSprite();
			_front.sprite = _cat.GetFrontSprite();
			RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
			_camera.targetTexture = renderTexture;
			RenderTexture.active = renderTexture;
			_gameCanvas.SetActive(value: false);
			_multiplayerCanvas.SetActive(value: false);
			_screenshotCanvas.SetActive(value: true);
			_camera.Render();
			yield return new WaitForEndOfFrame();
			Texture2D texture2D = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, mipChain: false);
			texture2D.ReadPixels(new Rect(0f, 0f, renderTexture.width, renderTexture.height), 0, 0);
			texture2D.Apply();
			ColorUtility.TryParseHtmlString("#929292", out var color);
			texture2D = texture2D.CropTextureBackground(color);
			_camera.targetTexture = null;
			RenderTexture.active = null;
			byte[] bytes = texture2D.EncodeToPNG();
			string text = Application.dataPath + "/../Screenshots/";
			string text2 = text + "Screenshot.png";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			File.WriteAllBytes(text2, bytes);
			SteamScreenshots.AddScreenshotToLibrary(text2, null, texture2D.width, texture2D.height);
			Object.Destroy(renderTexture);
			Object.Destroy(texture2D);
			_gameCanvas.SetActive(value: true);
			_multiplayerCanvas.SetActive(value: true);
			_screenshotCanvas.SetActive(value: false);
		}
	}
}

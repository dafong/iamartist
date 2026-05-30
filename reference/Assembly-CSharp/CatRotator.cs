using BongoCat;
using DG.Tweening;
using Steamworks;
using UnityEngine;

public class CatRotator : MonoBehaviour
{
	[SerializeField]
	private Transform cat;

	[SerializeField]
	private OnHoverCallback hoverCallback;

	[SerializeField]
	private string KEY;

	private CSteamID _playerId;

	private bool _isMainCat;

	private int _rotationState;

	public bool RotatedNormally => _rotationState == 0;

	public bool UpsideDown => _rotationState == 2;

	private void Awake()
	{
		_isMainCat = !string.IsNullOrEmpty(KEY);
		if (_isMainCat)
		{
			_rotationState = PlayerPrefs.GetInt(KEY, 0);
			cat.localRotation = Quaternion.Euler(0f, 0f, _rotationState * 90);
		}
	}

	private void Update()
	{
		if (hoverCallback.Hovering && Input.GetKeyDown(KeyCode.R) && !MainCat.FocusingInputfield())
		{
			Rotate();
		}
	}

	public void Rotate(bool left = true)
	{
		OnDemandRenderHelper.Instance.ResumeRendering();
		DOTween.Kill(cat);
		_rotationState = (left ? (++_rotationState % 4) : (--_rotationState % 4));
		cat.DOLocalRotate(new Vector3(0f, 0f, _rotationState * 90), 0.2f).OnComplete(delegate
		{
			OnDemandRenderHelper.Instance.TryPauseRendering();
		});
		if (_isMainCat)
		{
			PlayerPrefs.SetInt(KEY, _rotationState);
			PlayerPrefs.Save();
		}
		else
		{
			MultiplayerStorage.SavePlayerRot(_playerId, _rotationState);
		}
	}

	public void SetPlayerId(CSteamID playerId)
	{
		_playerId = playerId;
		_rotationState = MultiplayerStorage.GetPlayerRot(_playerId);
		cat.localRotation = Quaternion.Euler(0f, 0f, _rotationState * 90);
	}
}

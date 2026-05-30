using BongoCat;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class OnHoverCallback : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	public UnityEvent onHoverEnter;

	public UnityEvent onHoverExit;

	[SerializeField]
	private bool renderOnHover = true;

	private bool _hovering;

	public bool Hovering => _hovering;

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (renderOnHover)
		{
			OnDemandRenderHelper.Instance.ResumeRendering();
		}
		_hovering = true;
		onHoverEnter?.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (renderOnHover)
		{
			OnDemandRenderHelper.Instance.TryPauseRendering();
		}
		_hovering = false;
		onHoverExit?.Invoke();
	}
}

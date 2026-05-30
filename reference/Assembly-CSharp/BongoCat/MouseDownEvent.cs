using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace BongoCat
{
	public class MouseDownEvent : MonoBehaviour, IPointerDownHandler, IEventSystemHandler
	{
		public UnityEvent onMouseDown;

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Left)
			{
				onMouseDown.Invoke();
			}
		}
	}
}

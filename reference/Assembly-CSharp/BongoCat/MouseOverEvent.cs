using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace BongoCat
{
	public class MouseOverEvent : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		[SerializeField]
		private UnityEvent _onMouseEnter;

		[SerializeField]
		private UnityEvent _onMouseExit;

		public void OnPointerEnter(PointerEventData eventData)
		{
			_onMouseEnter.Invoke();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_onMouseExit.Invoke();
		}
	}
}

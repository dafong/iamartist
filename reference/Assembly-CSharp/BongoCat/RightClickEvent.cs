using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace BongoCat
{
	public class RightClickEvent : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
	{
		[SerializeField]
		private UnityEvent _onRightClick;

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button == PointerEventData.InputButton.Right)
			{
				_onRightClick.Invoke();
			}
		}
	}
}

using UnityEngine;
using UnityEngine.EventSystems;

namespace BongoCat
{
	public class HoverTooltip : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
	{
		[SerializeField]
		private GameObject _tooltipGameObject;

		[SerializeField]
		private bool _instantly;

		private float _timeToHover = 0.5f;

		private bool _hovering;

		private float _timeToHoverLeft;

		public void OnPointerEnter(PointerEventData eventData)
		{
			_hovering = true;
			_timeToHoverLeft = _timeToHover;
		}

		private void Update()
		{
			if (_hovering)
			{
				_timeToHoverLeft -= Time.deltaTime;
				if (_timeToHoverLeft <= 0f || _instantly)
				{
					_tooltipGameObject.SetActive(value: true);
					_hovering = false;
				}
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_tooltipGameObject.SetActive(value: false);
			_hovering = false;
		}
	}
}

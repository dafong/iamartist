using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace BongoCat.UnityEngine.EventSystems
{
	public class CustomInputModule : PointerInputModule
	{
		private float m_PrevActionTime;

		private Vector2 m_LastMoveVector;

		private int m_ConsecutiveMoveCount;

		private Vector2 m_LastMousePosition;

		private Vector2 m_MousePosition;

		private GameObject m_CurrentFocusedGameObject;

		private readonly Dictionary<int, PointerEventData> m_InputPointerEvents = new Dictionary<int, PointerEventData>();

		[SerializeField]
		[FormerlySerializedAs("m_AllowActivationOnMobileDevice")]
		private bool m_ForceModuleActive;

		public static CustomInputModule Instance;

		protected override void Awake()
		{
			base.Awake();
			Instance = this;
		}

		private bool ShouldIgnoreEventsOnNoFocus()
		{
			return true;
		}

		public override void UpdateModule()
		{
			if (!base.eventSystem.isFocused && ShouldIgnoreEventsOnNoFocus())
			{
				ReleasePointerDrags();
				return;
			}
			m_LastMousePosition = m_MousePosition;
			m_MousePosition = base.input.mousePosition;
		}

		private void ReleasePointerDrags()
		{
			List<int> value;
			using (CollectionPool<List<int>, int>.Get(out value))
			{
				foreach (int key in m_InputPointerEvents.Keys)
				{
					value.Add(key);
				}
				foreach (int item in value)
				{
					if (m_InputPointerEvents.TryGetValue(item, out var value2) && value2 != null && value2.pointerDrag != null && value2.dragging)
					{
						ReleaseMouse(value2, value2.pointerCurrentRaycast.gameObject);
					}
				}
			}
			m_InputPointerEvents.Clear();
		}

		private void ReleaseMouse(PointerEventData pointerEvent, GameObject currentOverGo)
		{
			ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
			GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(currentOverGo);
			if (pointerEvent.pointerClick == eventHandler && pointerEvent.eligibleForClick)
			{
				ExecuteEvents.Execute(pointerEvent.pointerClick, pointerEvent, ExecuteEvents.pointerClickHandler);
			}
			if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.ExecuteHierarchy(currentOverGo, pointerEvent, ExecuteEvents.dropHandler);
			}
			pointerEvent.eligibleForClick = false;
			pointerEvent.pointerPress = null;
			pointerEvent.rawPointerPress = null;
			pointerEvent.pointerClick = null;
			if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
			{
				ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
			}
			pointerEvent.dragging = false;
			pointerEvent.pointerDrag = null;
			if (currentOverGo != pointerEvent.pointerEnter)
			{
				HandlePointerExitAndEnter(pointerEvent, null);
				HandlePointerExitAndEnter(pointerEvent, currentOverGo);
			}
			m_InputPointerEvents[pointerEvent.pointerId] = pointerEvent;
		}

		public override bool ShouldActivateModule()
		{
			if (!base.ShouldActivateModule())
			{
				return false;
			}
			bool forceModuleActive = m_ForceModuleActive;
			forceModuleActive |= (m_MousePosition - m_LastMousePosition).sqrMagnitude > 0f;
			forceModuleActive |= base.input.GetMouseButtonDown(0);
			if (base.input.touchCount > 0)
			{
				forceModuleActive = true;
			}
			return forceModuleActive;
		}

		public override void ActivateModule()
		{
			if (base.eventSystem.isFocused || !ShouldIgnoreEventsOnNoFocus())
			{
				base.ActivateModule();
				m_MousePosition = base.input.mousePosition;
				m_LastMousePosition = base.input.mousePosition;
				GameObject gameObject = base.eventSystem.currentSelectedGameObject;
				if (gameObject == null)
				{
					gameObject = base.eventSystem.firstSelectedGameObject;
				}
				base.eventSystem.SetSelectedGameObject(gameObject, GetBaseEventData());
			}
		}

		public override void DeactivateModule()
		{
			base.DeactivateModule();
			ClearSelection();
		}

		public override void Process()
		{
			if (base.eventSystem.isFocused || !ShouldIgnoreEventsOnNoFocus())
			{
				bool flag = SendUpdateEventToSelectedObject();
				if (!ProcessTouchEvents() && base.input.mousePresent)
				{
					ProcessMouseEvent();
				}
				if (base.eventSystem.sendNavigationEvents && !flag)
				{
					SendSubmitEventToSelectedObject();
				}
			}
		}

		private bool ProcessTouchEvents()
		{
			for (int i = 0; i < base.input.touchCount; i++)
			{
				Touch touch = base.input.GetTouch(i);
				if (touch.type != TouchType.Indirect)
				{
					bool pressed;
					bool released;
					PointerEventData touchPointerEventData = GetTouchPointerEventData(touch, out pressed, out released);
					ProcessTouchPress(touchPointerEventData, pressed, released);
					if (!released)
					{
						ProcessMove(touchPointerEventData);
						ProcessDrag(touchPointerEventData);
					}
					else
					{
						RemovePointerData(touchPointerEventData);
					}
				}
			}
			return base.input.touchCount > 0;
		}

		protected void ProcessTouchPress(PointerEventData pointerEvent, bool pressed, bool released)
		{
			GameObject gameObject = pointerEvent.pointerCurrentRaycast.gameObject;
			if (pressed)
			{
				pointerEvent.eligibleForClick = true;
				pointerEvent.delta = Vector2.zero;
				pointerEvent.dragging = false;
				pointerEvent.useDragThreshold = true;
				pointerEvent.pressPosition = pointerEvent.position;
				pointerEvent.pointerPressRaycast = pointerEvent.pointerCurrentRaycast;
				DeselectIfSelectionChanged(gameObject, pointerEvent);
				if (pointerEvent.pointerEnter != gameObject)
				{
					HandlePointerExitAndEnter(pointerEvent, gameObject);
					pointerEvent.pointerEnter = gameObject;
				}
				pointerEvent.clickCount = 0;
				GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, pointerEvent, ExecuteEvents.pointerDownHandler);
				GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				if (gameObject2 == null)
				{
					gameObject2 = eventHandler;
				}
				float unscaledTime = Time.unscaledTime;
				pointerEvent.clickCount = 1;
				if (gameObject2 == pointerEvent.lastPress)
				{
					pointerEvent.clickTime = unscaledTime;
				}
				pointerEvent.pointerPress = gameObject2;
				pointerEvent.rawPointerPress = gameObject;
				pointerEvent.pointerClick = eventHandler;
				pointerEvent.clickTime = unscaledTime;
				pointerEvent.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
				if (pointerEvent.pointerDrag != null)
				{
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.initializePotentialDrag);
				}
			}
			if (released)
			{
				ExecuteEvents.Execute(pointerEvent.pointerPress, pointerEvent, ExecuteEvents.pointerUpHandler);
				GameObject eventHandler2 = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				if (pointerEvent.pointerClick == eventHandler2 && pointerEvent.eligibleForClick)
				{
					ExecuteEvents.Execute(pointerEvent.pointerClick, pointerEvent, ExecuteEvents.pointerClickHandler);
				}
				if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
				{
					ExecuteEvents.ExecuteHierarchy(gameObject, pointerEvent, ExecuteEvents.dropHandler);
				}
				pointerEvent.eligibleForClick = false;
				pointerEvent.pointerPress = null;
				pointerEvent.rawPointerPress = null;
				pointerEvent.pointerClick = null;
				if (pointerEvent.pointerDrag != null && pointerEvent.dragging)
				{
					ExecuteEvents.Execute(pointerEvent.pointerDrag, pointerEvent, ExecuteEvents.endDragHandler);
				}
				pointerEvent.dragging = false;
				pointerEvent.pointerDrag = null;
				ExecuteEvents.ExecuteHierarchy(pointerEvent.pointerEnter, pointerEvent, ExecuteEvents.pointerExitHandler);
				pointerEvent.pointerEnter = null;
			}
			m_InputPointerEvents[pointerEvent.pointerId] = pointerEvent;
		}

		protected bool SendSubmitEventToSelectedObject()
		{
			if (base.eventSystem.currentSelectedGameObject == null)
			{
				return false;
			}
			return GetBaseEventData().used;
		}

		protected void ProcessMouseEvent()
		{
			ProcessMouseEvent(0);
		}

		protected void ProcessMouseEvent(int id)
		{
			MouseState mousePointerEventData = GetMousePointerEventData(id);
			MouseButtonEventData eventData = mousePointerEventData.GetButtonState(PointerEventData.InputButton.Left).eventData;
			m_CurrentFocusedGameObject = eventData.buttonData.pointerCurrentRaycast.gameObject;
			ProcessMousePress(eventData);
			ProcessMove(eventData.buttonData);
			ProcessDrag(eventData.buttonData);
			ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData);
			ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Right).eventData.buttonData);
			ProcessMousePress(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData);
			ProcessDrag(mousePointerEventData.GetButtonState(PointerEventData.InputButton.Middle).eventData.buttonData);
			if (!Mathf.Approximately(eventData.buttonData.scrollDelta.sqrMagnitude, 0f))
			{
				ExecuteEvents.ExecuteHierarchy(ExecuteEvents.GetEventHandler<IScrollHandler>(eventData.buttonData.pointerCurrentRaycast.gameObject), eventData.buttonData, ExecuteEvents.scrollHandler);
			}
		}

		protected bool SendUpdateEventToSelectedObject()
		{
			if (base.eventSystem.currentSelectedGameObject == null)
			{
				return false;
			}
			BaseEventData baseEventData = GetBaseEventData();
			ExecuteEvents.Execute(base.eventSystem.currentSelectedGameObject, baseEventData, ExecuteEvents.updateSelectedHandler);
			return baseEventData.used;
		}

		protected void ProcessMousePress(MouseButtonEventData data)
		{
			PointerEventData buttonData = data.buttonData;
			GameObject gameObject = buttonData.pointerCurrentRaycast.gameObject;
			if (data.PressedThisFrame())
			{
				buttonData.eligibleForClick = true;
				buttonData.delta = Vector2.zero;
				buttonData.dragging = false;
				buttonData.useDragThreshold = true;
				buttonData.pressPosition = buttonData.position;
				buttonData.pointerPressRaycast = buttonData.pointerCurrentRaycast;
				DeselectIfSelectionChanged(gameObject, buttonData);
				buttonData.clickCount = 0;
				GameObject gameObject2 = ExecuteEvents.ExecuteHierarchy(gameObject, buttonData, ExecuteEvents.pointerDownHandler);
				GameObject eventHandler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(gameObject);
				if (gameObject2 == null)
				{
					gameObject2 = eventHandler;
				}
				float unscaledTime = Time.unscaledTime;
				buttonData.clickCount = 1;
				if (gameObject2 == buttonData.lastPress)
				{
					buttonData.clickTime = unscaledTime;
				}
				buttonData.pointerPress = gameObject2;
				buttonData.rawPointerPress = gameObject;
				buttonData.pointerClick = eventHandler;
				buttonData.clickTime = unscaledTime;
				buttonData.pointerDrag = ExecuteEvents.GetEventHandler<IDragHandler>(gameObject);
				if (buttonData.pointerDrag != null)
				{
					ExecuteEvents.Execute(buttonData.pointerDrag, buttonData, ExecuteEvents.initializePotentialDrag);
				}
				m_InputPointerEvents[buttonData.pointerId] = buttonData;
			}
			if (data.ReleasedThisFrame())
			{
				ReleaseMouse(buttonData, gameObject);
			}
		}

		public GameObject GetCurrentFocusedGameObject()
		{
			return m_CurrentFocusedGameObject;
		}
	}
}

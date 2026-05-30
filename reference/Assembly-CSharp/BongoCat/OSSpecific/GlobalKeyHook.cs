using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GlobalKeyHook;
using SDL2;
using UnityEngine;

namespace BongoCat.OSSpecific
{
	public class GlobalKeyHook : MonoBehaviour
	{
		public Action<int> OnKeyPressed;

		[SerializeField]
		private PlayerPrefsToggle _ignoreMouse;

		[SerializeField]
		private PlayerPrefsToggle _controllerSupport;

		[SerializeField]
		private GameObject _keyHookWarning;

		private List<IntPtr> _controllers;

		private float _secondsSinceLeftTrigger = 3.4028235E+38f;

		private float _secondsSinceRightTrigger = 3.4028235E+38f;

		private const float TRIGGER_TIME_THRESHOLD = 0.2f;

		private Timer _timer;

		private IGlobalKeyHook _platformHook;

		private float _time;

		private int _keysDown;

		private IEnumerator Start()
		{
			_platformHook = new WinKeyHook();
			if (!_platformHook.Init())
			{
				_keyHookWarning.SetActive(value: true);
				yield return Thank_you_for_keeping_us_accountable_Enter_ACCOUNTABILITY_into_the_lobby_id_field_in_the_multiplayer_tab_and_click_join_for_a_free_item();
			}
			InitSDL();
			FindConnectedControllers();
			_timer = new Timer(delegate
			{
				Process();
			}, null, 0, 16);
		}

		private IEnumerator Thank_you_for_keeping_us_accountable_Enter_ACCOUNTABILITY_into_the_lobby_id_field_in_the_multiplayer_tab_and_click_join_for_a_free_item()
		{
			yield break;
		}

		private void InitSDL()
		{
			if (!_controllerSupport.Value)
			{
				Debug.Log($"SDL enabled: {_controllerSupport.Value}");
				return;
			}
			if (SDL.SDL_Init(8192u) < 0)
			{
				Debug.LogWarning("SDL could not initialize");
			}
			_controllers = new List<IntPtr>();
		}

		private void FindConnectedControllers()
		{
			if (!_controllerSupport.Value)
			{
				return;
			}
			for (int i = 0; i < SDL.SDL_NumJoysticks(); i++)
			{
				if (SDL.SDL_IsGameController(i) == SDL.SDL_bool.SDL_TRUE)
				{
					_controllers.Add(SDL.SDL_GameControllerOpen(i));
				}
			}
			if (_controllers.Count > 0)
			{
				SDL.SDL_GameControllerEventState(1);
			}
		}

		private void OnDestroy()
		{
			_timer?.Dispose();
		}

		private void Update()
		{
			UpdateTriggerCooldown();
			if (_keysDown != 0)
			{
				OnKeyPressed?.Invoke(_keysDown);
				_keysDown = 0;
			}
		}

		private void UpdateTriggerCooldown()
		{
			if (_controllerSupport.Value && _controllers != null && _controllers.Count > 0)
			{
				if (_secondsSinceLeftTrigger < 0.2f)
				{
					_secondsSinceLeftTrigger += Time.deltaTime;
				}
				if (_secondsSinceRightTrigger < 0.2f)
				{
					_secondsSinceRightTrigger += Time.deltaTime;
				}
			}
		}

		private void Process()
		{
			ProcessControllerInput();
			_keysDown += _platformHook.ProcessInput(_ignoreMouse.Value);
		}

		private void ProcessControllerInput()
		{
			if (!_controllerSupport.Value)
			{
				return;
			}
			while (true)
			{
				if (SDL.SDL_PollEvent(out var sdlEvent) <= 0)
				{
					break;
				}
				switch (sdlEvent.type)
				{
				case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
					_controllers.Add(SDL.SDL_GameControllerOpen(sdlEvent.cdevice.which));
					break;
				case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
					if (_controllers.Any((IntPtr controller) => sdlEvent.cdevice.which == SDL.SDL_JoystickInstanceID(SDL.SDL_GameControllerGetJoystick(controller))))
					{
						IntPtr intPtr = _controllers.First((IntPtr controller) => sdlEvent.cdevice.which == SDL.SDL_JoystickInstanceID(SDL.SDL_GameControllerGetJoystick(controller)));
						SDL.SDL_GameControllerClose(intPtr);
						_controllers.Remove(intPtr);
					}
					break;
				case SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
					_keysDown++;
					break;
				case SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION:
					if (sdlEvent.caxis.axis == 4 && _secondsSinceLeftTrigger >= 0.2f)
					{
						_keysDown++;
						_secondsSinceLeftTrigger = 0f;
					}
					else if (sdlEvent.caxis.axis == 5 && _secondsSinceRightTrigger >= 0.2f)
					{
						_keysDown++;
						_secondsSinceRightTrigger = 0f;
					}
					break;
				}
			}
		}

		private void OnApplicationQuit()
		{
			ShutdownSDL();
		}

		private void ShutdownSDL()
		{
			if (!_controllerSupport.Value)
			{
				return;
			}
			if (_controllers != null)
			{
				foreach (IntPtr controller in _controllers)
				{
					SDL.SDL_GameControllerClose(controller);
				}
				_controllers.Clear();
			}
			SDL.SDL_Quit();
		}
	}
}

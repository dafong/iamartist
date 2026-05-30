using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace GlobalKeyHook
{
	public class WinKeyHook : IGlobalKeyHook
	{
		private static int[] BUTTONS;

		private static bool[] WasPressed;

		private static bool[] IsDown;

		[DllImport("user32.dll")]
		private static extern short GetAsyncKeyState(int virtualKeyCode);

		public bool Init()
		{
			Array values = Enum.GetValues(typeof(WinKeys));
			BUTTONS = new int[values.Length];
			for (int i = 0; i < values.Length; i++)
			{
				BUTTONS[i] = (int)values.GetValue(i);
			}
			WasPressed = new bool[BUTTONS.Length];
			IsDown = new bool[BUTTONS.Length];
			return true;
		}

		private static bool AnyButtonPressed()
		{
			for (int i = 0; i < BUTTONS.Length; i++)
			{
				if (IsDown[i])
				{
					return true;
				}
			}
			return false;
		}

		public int ProcessInput(bool ignoreMouse)
		{
			for (int i = 0; i < BUTTONS.Length; i++)
			{
				IsDown[i] = false;
				if (!ignoreMouse || (BUTTONS[i] != 1 && BUTTONS[i] != 2 && BUTTONS[i] != 4 && BUTTONS[i] != 6 && BUTTONS[i] != 5))
				{
					short asyncKeyState = GetAsyncKeyState(BUTTONS[i]);
					if (!WasPressed[i] && asyncKeyState == -32768)
					{
						WasPressed[i] = true;
						IsDown[i] = true;
					}
					else if (WasPressed[i] && asyncKeyState == 0)
					{
						WasPressed[i] = false;
					}
				}
			}
			if (!AnyButtonPressed())
			{
				return 0;
			}
			return IsDown.Count((bool x) => x);
		}
	}
}

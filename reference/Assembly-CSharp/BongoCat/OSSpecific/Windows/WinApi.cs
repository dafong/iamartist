using System;
using System.Runtime.InteropServices;

namespace BongoCat.OSSpecific.Windows
{
	public class WinApi
	{
		public struct WinRect
		{
			public int Left;

			public int Top;

			public int Right;

			public int Bottom;

			public int Width => Right - Left;

			public int Height => Bottom - Top;
		}

		public struct Margins
		{
			public int cxLeftWidth;

			public int cxRightWidth;

			public int cyTopHeight;

			public int cyBottomHeight;
		}

		public delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref WinRect lprcMonitor, IntPtr dwData);

		public static readonly int GWL_STYLE = -16;

		public static readonly int GWL_EXSTYLE = -20;

		public static readonly int SW_HIDE = 0;

		public static readonly int SW_MAXIMIZE = 3;

		public static readonly int SW_MINIMIZE = 6;

		public static readonly int SW_RESTORE = 9;

		public static readonly int SW_SHOW = 5;

		public static readonly uint SWP_REFRESH = 567u;

		public static readonly uint SWP_NOSIZE = 1u;

		public static readonly uint SWP_NOMOVE = 2u;

		public static readonly uint SWP_NOZORDER = 4u;

		public static readonly uint SWP_NOACTIVATE = 16u;

		public static readonly uint SWP_FRAMECHANGED = 32u;

		public static readonly uint SWP_SHOWWINDOW = 64u;

		public static readonly uint SWP_NOCOPYBITS = 256u;

		public static readonly uint SWP_NOOWNERZORDER = 512u;

		public static readonly uint SWP_NOREPOSITION = 512u;

		public static readonly uint SWP_NOSENDCHANGING = 1024u;

		public static readonly uint SWP_ASYNCWINDOWPOS = 16384u;

		public static readonly ulong WS_BORDER = 8388608uL;

		public static readonly ulong WS_VISIBLE = 268435456uL;

		public static readonly ulong WS_OVERLAPPED = 0uL;

		public static readonly ulong WS_CAPTION = 12582912uL;

		public static readonly ulong WS_SYSMENU = 524288uL;

		public static readonly ulong WS_THICKFRAME = 262144uL;

		public static readonly ulong WS_ICONIC = 536870912uL;

		public static readonly ulong WS_MINIMIZE = 536870912uL;

		public static readonly ulong WS_MAXIMIZE = 16777216uL;

		public static readonly ulong WS_MINIMIZEBOX = 131072uL;

		public static readonly ulong WS_MAXIMIZEBOX = 65536uL;

		public static readonly ulong WS_POPUP = 2147483648uL;

		public static readonly ulong WS_OVERLAPPEDWINDOW = 13565952uL;

		public static readonly ulong WS_EX_TOOLWINDOW = 128uL;

		public static readonly ulong WS_EX_TRANSPARENT = 32uL;

		public static readonly ulong WS_EX_LAYERED = 524288uL;

		public static readonly ulong WS_EX_TOPMOST = 8uL;

		public static readonly ulong WS_EX_OVERLAPPEDWINDOW = 768uL;

		public static readonly ulong WS_EX_ACCEPTFILES = 16uL;

		public static readonly IntPtr HWND_TOP = new IntPtr(0);

		public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

		public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

		public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

		public static readonly uint GA_PARENT = 1u;

		public static readonly uint GA_ROOT = 2u;

		public static readonly uint GA_ROOTOWNER = 3u;

		public static readonly uint GW_HWNDFIRST = 0u;

		public static readonly uint GW_HWNDLAST = 1u;

		public static readonly uint GW_HWNDNEXT = 2u;

		public static readonly uint GW_HWNDPREV = 3u;

		public static readonly uint GW_OWNER = 4u;

		public static readonly uint GW_CHILD = 5u;

		public static readonly uint WM_IME_CHAR = 646u;

		public static readonly uint WM_SETTEXT = 12u;

		public static readonly uint WM_NCDESTROY = 130u;

		public static readonly uint WM_WINDOWPOSCHANGING = 70u;

		public static readonly uint WM_DROPFILES = 563u;

		public static readonly uint WM_COPYDATA = 74u;

		public static readonly uint WM_COPYGLOBALDATA = 73u;

		public static readonly uint MSGFLT_ALLOW = 1u;

		public static readonly uint MSGFLT_DISALLOW = 2u;

		public static readonly uint MSGFLT_RESET = 0u;

		public static readonly uint MSGFLTINFO_NONE = 0u;

		public static readonly uint MSGFLTINFO_ALLOWED_HIGHER = 3u;

		public static readonly uint MSGFLTINFO_ALREADYALLOWED_FORWND = 1u;

		public static readonly uint MSGFLTINFO_ALREADYDISALLOWED_FORWND = 2u;

		public static readonly uint ULW_COLORKEY = 1u;

		public static readonly uint ULW_ALPHA = 2u;

		public static readonly uint ULW_OPAQUE = 4u;

		public static readonly uint LWA_COLORKEY = 1u;

		public static readonly uint LWA_ALPHA = 2u;

		public static readonly uint RDW_INVALIDATE = 1u;

		public static readonly uint RDW_UPDATENOW = 256u;

		[DllImport("dwmapi.dll", SetLastError = true)]
		public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMarInset);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

		[DllImport("user32.dll")]
		public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(IntPtr hWnd, out WinRect rect);

		[DllImport("user32.dll")]
		public static extern ulong SetWindowLong(IntPtr hWnd, int nIndex, ulong value);

		[DllImport("user32.dll")]
		public static extern ulong GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		[DllImport("user32.dll")]
		public static extern IntPtr GetActiveWindow();
	}
}

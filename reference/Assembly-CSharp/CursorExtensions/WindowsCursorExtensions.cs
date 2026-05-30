using System;
using System.Runtime.InteropServices;

namespace CursorExtensions
{
	public static class WindowsCursorExtensions
	{
		private struct PointStruct
		{
			public int x;

			public int y;
		}

		private struct CursorInfoStruct
		{
			public int cbSize;

			public int flags;

			public IntPtr hCursor;

			public PointStruct pt;
		}

		[DllImport("user32.dll")]
		private static extern bool GetCursorInfo(ref CursorInfoStruct pci);

		public static bool IsVisible()
		{
			CursorInfoStruct pci = new CursorInfoStruct
			{
				cbSize = Marshal.SizeOf(typeof(CursorInfoStruct))
			};
			GetCursorInfo(ref pci);
			return (pci.flags & 1) != 0;
		}
	}
}

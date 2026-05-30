using BongoCat.OSSpecific.Windows;

namespace Transparency
{
	public interface ITransparentWindow
	{
		void SetClickthrough(bool isClickthrough);

		void OnTaskbarIconToggleUpdated(bool iconEnabled);

		void SetTopMost(bool topMost);

		void MoveWindowToMonitor(int monitorIndex);

		WinApi.WinRect GetCurrentWindowBounds();
	}
}

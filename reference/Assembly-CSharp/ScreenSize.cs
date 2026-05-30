using UnityEngine;

public class ScreenSize : MonoBehaviour
{
	public static int FullWidth => Screen.mainWindowDisplayInfo.width;

	public static int FullHeight => Screen.mainWindowDisplayInfo.height;

	public static int XMin => Screen.mainWindowDisplayInfo.workArea.xMin;

	public static int XMax => Screen.mainWindowDisplayInfo.workArea.xMax;

	public static int YMin => FullHeight - Screen.mainWindowDisplayInfo.workArea.yMax;

	public static int YMax => FullHeight - Screen.mainWindowDisplayInfo.workArea.yMin;
}

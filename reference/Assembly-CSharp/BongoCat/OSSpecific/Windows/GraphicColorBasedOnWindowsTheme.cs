using UnityEngine;
using UnityEngine.UI;

namespace BongoCat.OSSpecific.Windows
{
	[RequireComponent(typeof(Graphic))]
	public class GraphicColorBasedOnWindowsTheme : MonoBehaviour
	{
		private Graphic _graphic;

		private static Color _colorToUseInDarkMode = Color.white;

		private static Color _colorToUseInLightMode = Color.black;

		private void Awake()
		{
			_graphic = GetComponent<Graphic>();
		}
	}
}

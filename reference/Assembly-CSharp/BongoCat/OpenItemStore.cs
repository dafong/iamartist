using BongoCat.SteamJsonParser;
using UnityEngine;

namespace BongoCat
{
	public class OpenItemStore : MonoBehaviour
	{
		[SerializeField]
		private SteamItemUnity _item;

		public void Open()
		{
			Application.OpenURL($"steam://openurl/https://store.steampowered.com/itemstore/{3419430}/detail/{_item.Id}/");
		}
	}
}

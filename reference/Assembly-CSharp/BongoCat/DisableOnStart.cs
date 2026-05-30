using UnityEngine;

namespace BongoCat
{
	public class DisableOnStart : MonoBehaviour
	{
		private void Start()
		{
			base.gameObject.SetActive(value: false);
		}
	}
}

using UnityEngine;

namespace BongoCat
{
	public class FlipVisuals : MonoBehaviour
	{
		[SerializeField]
		private Transform _cat;

		[SerializeField]
		private PlayerPrefsToggle _toggle;

		public bool IsFlipped;

		private CatCosmetics _catCosmetics;

		private void Awake()
		{
			_catCosmetics = Object.FindAnyObjectByType<CatCosmetics>();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.F) && !MainCat.FocusingInputfield())
			{
				_toggle.Toggle();
			}
		}

		public void Flip(bool val)
		{
			IsFlipped = val;
			_catCosmetics.UpdateFlip();
			_cat.localScale = new Vector3((!val) ? 1 : (-1), _cat.localScale.y, _cat.localScale.z);
		}
	}
}

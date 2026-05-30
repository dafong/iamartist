using BongoCat.SteamJsonParser;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace BongoCat
{
	public class SelectedItemUi : MonoBehaviour
	{
		[SerializeField]
		private SteamBundleExchange _bundleToSelect;

		[FormerlySerializedAs("multiplayerItemReward")]
		[FormerlySerializedAs("_itemSelection")]
		[SerializeField]
		private TokenItemReward tokenItemReward;

		[SerializeField]
		private Image _backgroundImage;

		[SerializeField]
		private Color _selectedColor;

		private Color _defaultColor;

		private void Awake()
		{
			_defaultColor = _backgroundImage.color;
		}

		public void Select()
		{
			tokenItemReward.SetSelectedItem(_bundleToSelect);
			_backgroundImage.color = _selectedColor;
		}

		public void Deselect()
		{
			_backgroundImage.color = _defaultColor;
		}
	}
}

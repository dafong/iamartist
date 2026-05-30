using UnityEngine;
using UnityEngine.Events;

namespace BongoCat
{
	public class OnDisableCallback : MonoBehaviour
	{
		public UnityEvent OnDisableEvent;

		private bool _quitting;

		private void OnApplicationQuit()
		{
			_quitting = true;
		}

		private void OnDisable()
		{
			if (!_quitting)
			{
				OnDisableEvent?.Invoke();
			}
		}
	}
}

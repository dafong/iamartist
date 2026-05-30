using UnityEngine;
using UnityEngine.Events;

namespace BongoCat
{
	public class OnEnableForwarder : MonoBehaviour
	{
		public UnityEvent OnEnableEvent;

		private void OnEnable()
		{
			OnEnableEvent?.Invoke();
		}
	}
}

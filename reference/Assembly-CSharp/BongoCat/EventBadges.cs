using System;
using System.Collections.Generic;
using UnityEngine;

namespace BongoCat
{
	public class EventBadges : ScriptableObject
	{
		[Serializable]
		private struct EventBadge
		{
			public string EventTag;

			public Sprite Badge;
		}

		[SerializeField]
		private List<EventBadge> _eventBadges;

		private Dictionary<string, Sprite> _eventBadgeDict;

		public Sprite GetEventBadge(string eventTag)
		{
			if (_eventBadgeDict == null)
			{
				_eventBadgeDict = new Dictionary<string, Sprite>();
				foreach (EventBadge eventBadge in _eventBadges)
				{
					_eventBadgeDict.Add(eventBadge.EventTag, eventBadge.Badge);
				}
			}
			if (!_eventBadgeDict.ContainsKey(eventTag))
			{
				return null;
			}
			return _eventBadgeDict[eventTag];
		}
	}
}

using System.Collections.Generic;
using UnityEngine;

namespace BongoCat.Emotes
{
	public class ConsumableBehaviorList : ScriptableObject
	{
		[SerializeField]
		private List<ConsumableBehaviorListEntry> _consumableBehaviors;

		private Dictionary<int, EmoteBehaviorOverride> _consumableBehaviorDict;

		private Dictionary<int, EmoteBehaviorOverride> ConsumableBehaviorDict
		{
			get
			{
				if (_consumableBehaviorDict == null)
				{
					_consumableBehaviorDict = new Dictionary<int, EmoteBehaviorOverride>();
					foreach (ConsumableBehaviorListEntry consumableBehavior in _consumableBehaviors)
					{
						_consumableBehaviorDict.TryAdd(consumableBehavior.Consumable.Id, consumableBehavior.Behavior);
					}
				}
				return _consumableBehaviorDict;
			}
		}

		public EmoteBehaviorOverride GetBehaviorOverride(int consumableId)
		{
			return ConsumableBehaviorDict.GetValueOrDefault(consumableId, EmoteBehaviorOverride.None);
		}
	}
}

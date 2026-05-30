using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BongoCat
{
	public class CollectionNames : ScriptableObject
	{
		[Serializable]
		public struct CollectionName
		{
			public string Key;

			public List<string> OtherKeys;

			public string DisplayName;
		}

		[FormerlySerializedAs("_collections")]
		[SerializeField]
		public List<CollectionName> Collections;

		private Dictionary<string, string> _collectionsDict;

		public string GetDisplayName(string key)
		{
			if (_collectionsDict == null)
			{
				_collectionsDict = new Dictionary<string, string>();
				foreach (CollectionName collection in Collections)
				{
					_collectionsDict.Add(collection.Key, collection.DisplayName);
					foreach (string otherKey in collection.OtherKeys)
					{
						_collectionsDict.Add(otherKey, collection.DisplayName);
					}
				}
			}
			if (_collectionsDict.TryGetValue(key, out var value))
			{
				return value;
			}
			Debug.LogWarning("[CollectionNames] Key " + key + " not found. Using key as name.");
			return key;
		}

		public int GetIndex(string displayName)
		{
			return Collections.FindIndex((CollectionName c) => c.DisplayName == displayName);
		}
	}
}

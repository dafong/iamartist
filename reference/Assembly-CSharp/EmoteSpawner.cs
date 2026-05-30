using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BongoCat;
using BongoCat.Emotes;
using UnityEngine;

public class EmoteSpawner : MonoBehaviour
{
	[SerializeField]
	private EmoteParticle emoteParticlePrefab;

	[SerializeField]
	private ConsumableBehaviorList _consumableBehaviorList;

	[SerializeField]
	private uint particlePoolSize;

	[SerializeField]
	[Tooltip("Seconds to wait before restoring the original poolsize if pool was extended due to spamming emotes")]
	private float secondsToRestorePoolSize;

	private Queue<EmoteParticle> _emoteParticlePool;

	private int _excessEmotes;

	private Coroutine _restoringPoolSize;

	private WaitForSeconds _waitingToRestorePoolSize;

	private void Awake()
	{
		_emoteParticlePool = new Queue<EmoteParticle>();
		for (int i = 0; i < particlePoolSize; i++)
		{
			AddParticleToPool();
		}
		_waitingToRestorePoolSize = new WaitForSeconds(secondsToRestorePoolSize);
	}

	public void SpawnEmoteParticle(SteamItem steamItem)
	{
		EmoteBehaviorOverride behaviorOverride = _consumableBehaviorList.GetBehaviorOverride(steamItem.SteamItemDefId);
		SpawnEmoteParticle(steamItem.Icon, behaviorOverride);
	}

	public void SpawnEmoteParticle(Sprite icon, EmoteBehaviorOverride behavior)
	{
		if (_emoteParticlePool.Count == 0)
		{
			_excessEmotes++;
			AddParticleToPool();
		}
		if (_restoringPoolSize != null)
		{
			StopCoroutine(_restoringPoolSize);
		}
		EmoteParticle emoteParticle = _emoteParticlePool.Dequeue();
		emoteParticle.transform.position = base.transform.position + new Vector3(Random.Range(-25, 25), 0f, 0f);
		emoteParticle.gameObject.SetActive(value: true);
		emoteParticle.StartEmote(icon, behavior);
		if (_excessEmotes > 0)
		{
			_restoringPoolSize = StartCoroutine(RestorePoolSize());
		}
	}

	public void SpawnMultiplayerEmote(string emoteID)
	{
		if (!SettingsManager.Instance.HideEmotesSetting.Value && base.gameObject.activeInHierarchy && int.TryParse(emoteID, out var id))
		{
			SteamItem steamItem = CatInventory.Instance.Items.FirstOrDefault((SteamItem item) => item.SteamItemDefId == id);
			if (steamItem != null)
			{
				EmoteBehaviorOverride behaviorOverride = _consumableBehaviorList.GetBehaviorOverride(id);
				SpawnEmoteParticle(steamItem.Icon, behaviorOverride);
			}
		}
	}

	private void AddParticleToPool()
	{
		EmoteParticle emoteParticle = Object.Instantiate(emoteParticlePrefab, base.transform);
		emoteParticle.Init(this);
		emoteParticle.gameObject.SetActive(value: false);
		_emoteParticlePool.Enqueue(emoteParticle);
	}

	public void ReturnParticleToPool(EmoteParticle particle)
	{
		particle.gameObject.SetActive(value: false);
		_emoteParticlePool.Enqueue(particle);
	}

	private IEnumerator RestorePoolSize()
	{
		yield return _waitingToRestorePoolSize;
		while (_excessEmotes > 0)
		{
			yield return new WaitUntil(() => _emoteParticlePool.Count > 0);
			Object.Destroy(_emoteParticlePool.Dequeue().gameObject);
			_excessEmotes--;
		}
		_restoringPoolSize = null;
	}
}

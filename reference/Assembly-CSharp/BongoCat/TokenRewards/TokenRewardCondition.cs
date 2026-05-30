using UnityEngine;

namespace BongoCat.TokenRewards
{
	public abstract class TokenRewardCondition : MonoBehaviour
	{
		public abstract bool CheckRewardConditions();
	}
}

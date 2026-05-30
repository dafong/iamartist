using UnityEngine;

public abstract class Scaler : MonoBehaviour
{
	protected int Index;

	[SerializeField]
	protected Transform targetTransform;

	protected abstract void UpdateScale();

	public void SetScale(int index)
	{
		Index = index;
		UpdateScale();
	}
}

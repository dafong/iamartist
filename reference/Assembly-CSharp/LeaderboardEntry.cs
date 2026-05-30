using TMPro;
using UnityEngine;

public class LeaderboardEntry : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _rank;

	[SerializeField]
	private TMP_Text _playerName;

	[SerializeField]
	private TMP_Text _tapAmount;

	public void Init(string playerName)
	{
		_playerName.text = playerName;
	}

	public void UpdateValues(int rank, int tapAmount)
	{
		_rank.text = $"{rank}.";
		_tapAmount.text = tapAmount.ToString();
	}
}

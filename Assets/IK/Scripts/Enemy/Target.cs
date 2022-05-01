using UnityEngine;

public class Target : MonoBehaviour
{
	[Header("Target")] [SerializeField] [ReadOnly]
	private Player _player;

	[HideInInspector] public Transform selfTarget;

	public Player player
	{
		get => _player;
		set
		{
			if (_player != value)
			{
				OnPlayerChanged(value);
			}

			_player = value;
		}
	}

	protected virtual void Awake()
	{
		selfTarget = this.transform;
	}

	protected virtual void OnDestroy()
	{
		if (player)
		{
			player.nearTargets.Remove(this);
		}
	}

	protected virtual void OnPlayerChanged(Player newPlayer)
	{
		this.enabled = newPlayer;
	}
}
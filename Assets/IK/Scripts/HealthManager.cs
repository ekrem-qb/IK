using System;
using ARP.APR.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
	private const float MinHealth = 0;
	private const float MaxHealth = 100;

	[Range(MinHealth, MaxHealth)] [SerializeField]
	private float _health = 100;

	public GameObject particlePrefab;
	public Color particleColor = Color.white;
	public Text textHealth;

	private APRController _aprController;
	private Enemy _enemy;
	private PathFollower _pathFollower;
	private Player _player;
	private WeaponManager _weaponManager;
	public Action Hit = () => { };

	public float health
	{
		get => _health;
		set
		{
			if (value < _health)
			{
				Hit();
			}

			_health = Mathf.Clamp(value, MinHealth, MaxHealth);

			if (textHealth)
			{
				textHealth.text = _health.ToString();
			}

			if (_health == 0)
			{
				Death();
			}
		}
	}

	private void Awake()
	{
		if (textHealth)
		{
			textHealth.text = _health.ToString();
		}

		_aprController = this.GetComponent<APRController>();
		if (!_aprController.root.transform)
		{
			_aprController.PlayerSetup();
		}

		_player = _aprController.root.transform.GetComponent<Player>();
		_enemy = _aprController.root.transform.GetComponent<Enemy>();
		_pathFollower = _aprController.root.transform.GetComponent<PathFollower>();
		_weaponManager = _aprController.COMP.GetComponent<WeaponManager>();
	}

	protected virtual void Death()
	{
		if (_player)
		{
			Destroy(_player);
		}

		if (_pathFollower)
		{
			Destroy(_pathFollower);
		}

		if (_enemy)
		{
			if (_enemy is Mover mover)
			{
				StartCoroutine(mover.Drop());
			}

			Destroy(_enemy);
		}

		if (_weaponManager)
		{
			_weaponManager.DropWeapon();
			Destroy(_weaponManager);
		}

		_aprController.ActivateRagdoll();
		_aprController.autoGetUpWhenPossible = false;
		_aprController.useControls = false;
		_aprController.useStepPrediction = false;
	}
}
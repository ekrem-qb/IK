using System.Collections;
using ARP.APR.Scripts;
using UnityEngine;

public class Ped : Enemy
{
	[Header("Ped")] public float attackInterval = 0.5f;

	private HealthManager _healthManager;
	protected WeaponManager weaponManager;

	protected override void Awake()
	{
		base.Awake();
		weaponManager = aprController.COMP.GetComponent<WeaponManager>();
		_healthManager = this.transform.root.GetComponent<HealthManager>();
		_healthManager.Hit += Annoy;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		_healthManager.Hit -= Annoy;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.isTrigger)
		{
			if (other.transform.root != this.transform.root)
			{
				APRController playerApr = other.transform.root.GetComponent<APRController>();
				if (playerApr)
				{
					Player player = playerApr.root.transform.GetComponent<Player>();
					if (player)
					{
						Annoy();
					}
				}
			}
		}
	}

	protected override void OnPlayerChanged(Player newPlayer)
	{
		if (!newPlayer)
		{
			pathFollower.enabled = true;
			this.enabled = false;
		}
	}

	protected virtual void Annoy()
	{
		if (player)
		{
			pathFollower.enabled = false;
			this.enabled = true;
		}
	}

	protected override IEnumerator Attack()
	{
		isAttacking = true;

		if (weaponManager.weapon)
		{
			weaponManager.weapon.Attack();
		}

		yield return new WaitForSeconds(attackInterval);
		isAttacking = false;
	}
}
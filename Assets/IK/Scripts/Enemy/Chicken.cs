using System.Collections;
using UnityEngine;

public class Chicken : Enemy
{
	[Header("Chicken")] public float explosionForce = 40;

	private ParticleSystem _particle;

	protected override void Awake()
	{
		base.Awake();
		_particle = this.GetComponentInChildren<ParticleSystem>();
	}

	protected override IEnumerator Attack()
	{
		Collider[] colliders = Physics.OverlapSphere(this.transform.position, attackDistance);
		for (int i = 0; i < colliders.Length; i++)
		{
			HealthManager enemy = colliders[i].transform.root.GetComponent<HealthManager>();
			if (enemy)
			{
				enemy.health = 0;
			}

			if (colliders[i].attachedRigidbody)
			{
				colliders[i].attachedRigidbody.velocity = Vector3.zero;
				colliders[i].attachedRigidbody.AddExplosionForce(explosionForce, this.transform.position, attackDistance, 0, ForceMode.VelocityChange);
			}
		}

		if (_particle)
		{
			_particle.transform.SetParent(null);
			_particle.Play();
		}

		Destroy(this.transform.root.gameObject);
		yield return null;
	}

	protected override void OnPlayerChanged(Player newPlayer)
	{
		this.enabled = newPlayer;
	}
}
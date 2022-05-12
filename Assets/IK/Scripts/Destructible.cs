using System;
using UnityEngine;

public class Destructible : HealthManager
{
	public Collider[] parts;
	public float requiredFallSpeedToDestroy = 25;
	public float explosionForce = 500;
	public float explosionRadius = 5;

	public GameObject spawnPrefab;

	// Common collider for all parts of this destructible object
	private Collider _collider;
	private Rigidbody _rigidbody;
	private Target _target;

	private void Awake()
	{
		_rigidbody = this.GetComponent<Rigidbody>();
		_collider = this.GetComponent<Collider>();
		_target = this.GetComponent<Target>();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.root != this.transform.root)
		{
			if (collision.relativeVelocity.y > requiredFallSpeedToDestroy)
			{
				Death();
			}
		}
	}

	protected override void Death()
	{
		if (parts.Length > 0)
		{
			for (int i = 0; i < parts.Length; i++)
			{
				Rigidbody partRigidbody = parts[i].gameObject.AddComponent<Rigidbody>();
				partRigidbody.AddExplosionForce(explosionForce, this.transform.position, explosionRadius);
			}

			parts = Array.Empty<Collider>();

			if (spawnPrefab)
			{
				Instantiate(spawnPrefab, this.transform.position, Quaternion.identity);
			}

			Destroy(_rigidbody);
			Destroy(_collider);
			Destroy(_target);
			Destroy(this);
		}
	}
}
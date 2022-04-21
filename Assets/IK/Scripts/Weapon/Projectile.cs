using UnityEngine;

public class Projectile : MonoBehaviour
{
	[Header("Projectile")] [HideInInspector]
	public Transform owner;

	public float speed = 100;
	public float damage = 10;
	protected new Rigidbody rigidbody;

	protected virtual void Awake()
	{
		rigidbody = this.GetComponent<Rigidbody>();
	}

	protected virtual void Start()
	{
		rigidbody.AddForce(this.transform.forward * speed, ForceMode.Impulse);
	}

	private void OnCollisionEnter(Collision collision) => Hit(collision.collider);

	private void OnTriggerEnter(Collider other) => Hit(other);

	protected virtual void Hit(Collider collider)
	{
		if (!collider.isTrigger)
		{
			if (owner != collider.transform.root)
			{
				HealthManager enemy = collider.transform.root.GetComponent<HealthManager>();
				if (enemy)
				{
					enemy.health -= damage;
					if (collider.attachedRigidbody)
					{
						collider.attachedRigidbody.AddForce(rigidbody.velocity, ForceMode.VelocityChange);
					}

					if (enemy.particlePrefab)
					{
						ParticleSystem[] particles = Instantiate(enemy.particlePrefab, collider.ClosestPointOnBounds(this.transform.position), Quaternion.identity).GetComponentsInChildren<ParticleSystem>();

						for (int i = 0; i < particles.Length; i++)
						{
							ParticleSystem.MainModule main = particles[i].main;
							main.startColor = enemy.particleColor;
						}

						particles[0].transform.SetParent(null);
						particles[0].transform.rotation = Quaternion.LookRotation(particles[0].transform.position - collider.transform.position);
						particles[0].Play();
					}
				}

				Destroy(this.gameObject);
			}
		}
	}
}
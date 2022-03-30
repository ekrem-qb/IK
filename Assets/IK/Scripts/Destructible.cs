using System;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : HealthManager
{
    public Collider[] parts;
    public float requiredFallSpeedToDestroy = 25;
    public float explosionForce = 500;
    public float explosionRadius = 5;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private Target _target;

    private void Awake()
    {
        _rigidbody = this.GetComponent<Rigidbody>();
        _collider = this.GetComponent<Collider>();
        _target = this.GetComponent<Target>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (parts.Length > 0)
        {   
            if (collision.transform.root != this.transform.root)
            {
                if (collision.relativeVelocity.y > requiredFallSpeedToDestroy)
                {
                    Death();
                }
            }
        }
    }

    protected override void Death()
    {
        for (int i = 0; i < parts.Length; i++)
        {
            Rigidbody partRigidbody = parts[i].gameObject.AddComponent<Rigidbody>();
            partRigidbody.AddExplosionForce(explosionForce, this.transform.position, explosionRadius);
        }
        parts = Array.Empty<Collider>();
        
        Destroy(_rigidbody);
        Destroy(_collider);
        Destroy(_target);
        Destroy(this);
    }
}
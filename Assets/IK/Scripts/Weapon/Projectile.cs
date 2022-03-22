using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5;
    public float rotationSpeed = 25;
    [HideInInspector] public Transform owner;
    [HideInInspector] public Vector3 target;
    Rigidbody rb;
    Transform child;

    void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
        child = this.transform.GetChild(0);
    }

    void Start()
    {
        rb.AddForce((target - this.transform.position) * speed, ForceMode.Impulse);
    }

    void Update()
    {
        child.Rotate(transform.right * rotationSpeed * 100 * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (owner != collision.transform.root)
        {
            HealthManager enemy = collision.transform.root.GetComponent<HealthManager>();
            if (enemy)
            {
                enemy.health -= 10;
                if (collision.rigidbody)
                {
                    collision.rigidbody.AddForce(this.transform.forward * speed, ForceMode.Impulse);
                }

                child.transform.SetParent(collision.transform);
                Destroy(this.gameObject);
            }
        }
    }
}
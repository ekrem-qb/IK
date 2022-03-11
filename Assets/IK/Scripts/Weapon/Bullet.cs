using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody rb;
    public float force = 50;
    public float timeout = 15;
    public LayerMask enemiesLayerMask;
    [HideInInspector]
    public Transform owner;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        rb.AddForce(this.transform.forward * force, ForceMode.Impulse);
        StartCoroutine(SelfDestroy());
    }

    IEnumerator SelfDestroy()
    {
        yield return new WaitForSeconds(timeout);
        Destroy(this.gameObject);
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
                    collision.rigidbody.AddForce(this.transform.forward * force, ForceMode.Impulse);
                }
                Destroy(this.gameObject);
            }
        }
    }
}
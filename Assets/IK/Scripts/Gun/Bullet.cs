using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody rb;
    public float force = 50;
    public float timeout = 15;
    public LayerMask enemiesLayerMask;

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
        if (enemiesLayerMask.Contains(collision.gameObject.layer))
        {
            Destroy(collision.transform.root.gameObject);
        }
    }
}
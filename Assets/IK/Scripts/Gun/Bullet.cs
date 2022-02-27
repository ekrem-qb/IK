using UnityEngine;

public class Bullet : MonoBehaviour
{
    Rigidbody rb;
    public float force = 50;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        rb.AddForce(this.transform.forward * force, ForceMode.Impulse);
    }
}

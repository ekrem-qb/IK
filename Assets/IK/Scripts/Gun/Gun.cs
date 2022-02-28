using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab;
    public KeyCode fireKey = KeyCode.Mouse0;
    public Vector3 holdPosition, holdAngles;
    Rigidbody rb;
    [HideInInspector]
    public bool canShoot = false;
    SphereCollider pickupTrigger;

    void Awake()
    {
        pickupTrigger = this.GetComponent<SphereCollider>();
        rb = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(fireKey) && canShoot)
        {
            GameObject bullet = Instantiate(bulletPrefab, this.transform.position, this.transform.rotation);
        }
    }

    void OnEnable()
    {
        pickupTrigger.enabled = false;
        Destroy(rb);
    }

    void OnDisable()
    {
        pickupTrigger.enabled = true;
        rb = this.gameObject.AddComponent<Rigidbody>();
    }
}
using UnityEngine;

public class Knife : Projectile
{
    public float rotationSpeed = 25;
    [HideInInspector] public Vector3 target;
    private Transform _mesh;

    protected override void Awake()
    {
        base.Awake();
        _mesh = this.GetComponentInChildren<MeshFilter>().transform;
    }

    protected override void Start()
    {
        rigidbody.AddForce((target - this.transform.position) * speed, ForceMode.Impulse);
    }

    private void Update()
    {
        _mesh.Rotate(transform.right * rotationSpeed * 100 * Time.deltaTime);
    }

    protected override void Hit(Collider collider)
    {
        base.Hit(collider);
        if (!collider.isTrigger)
        {
            if (owner != collider.transform.root)
            {
                _mesh.transform.SetParent(collider.transform);
            }
        }
    }
}
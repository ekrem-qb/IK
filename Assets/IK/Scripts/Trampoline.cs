using UnityEngine;

public class Trampoline : MonoBehaviour
{
    public float springForce = 50;
    public Vector3 direction = Vector3.up;
    private Animation _animation;
    private Gizmos _gizmos;

    private void Awake()
    {
        _animation = this.GetComponent<Animation>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        _gizmos.DrawArrow(this.transform.position, this.transform.rotation * direction * springForce / 10);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            _animation.Play();

            Rigidbody[] rigidbodies = other.transform.root.GetComponentsInChildren<Rigidbody>();
            if (rigidbodies.Length > 0)
            {
                for (int i = 0; i < rigidbodies.Length; i++)
                {
                    rigidbodies[i].isKinematic = true;
                    rigidbodies[i].isKinematic = false;
                    if (i == 0)
                    {
                        rigidbodies[i].AddForce(this.transform.rotation * direction * springForce, ForceMode.VelocityChange);
                    }
                }
            }
        }
    }
}
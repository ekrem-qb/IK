using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public float speed = 18;
    public Vector3 direction = Vector3.forward;
    [Range(0, 1)] public float antiInertia = 0.5f;
    Gizmos gizmos = new Gizmos();

    void OnCollisionStay(Collision collision)
    {
        if (collision.rigidbody)
        {
            collision.rigidbody.velocity = Vector3.Slerp(collision.rigidbody.velocity, this.transform.rotation * direction * collision.rigidbody.velocity.magnitude, antiInertia);
            collision.rigidbody.AddForce(this.transform.rotation * direction * speed, ForceMode.Force);
        }
    }

    void OnDrawGizmos()
    {
        if (this.transform.rotation * direction != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            gizmos.DrawArrow(this.transform.position, this.transform.rotation * direction * speed / 2);
        }
    }
}
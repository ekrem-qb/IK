using System;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public float speed = 18;
    public Vector3 direction = Vector3.forward;
    Gizmos gizmos = new Gizmos();

    private void OnCollisionStay(Collision collision)
    {
        if (collision.rigidbody)
        {
            collision.rigidbody.AddForce(this.transform.rotation * direction * speed, ForceMode.Force);
        }
    }

    private void OnDrawGizmos()
    {
        if (this.transform.rotation * direction != Vector3.zero)
        {
            Gizmos.color = Color.magenta;
            gizmos.DrawArrow(this.transform.position, this.transform.rotation * direction * speed / 2);
        }
    }
}
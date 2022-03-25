using System;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    public float speed = 18;
    public Vector3 direction = Vector3.forward;
    [Range(0, 1)] public float antiInertia = 0.5f;
    public bool scrollTexture;
    Material material;
    Renderer meshRenderer;
    Gizmos gizmos = new Gizmos();

    void Awake()
    {
        if (scrollTexture)
        {
            meshRenderer = this.GetComponent<Renderer>();
            if (meshRenderer)
            {
                material = meshRenderer.material;
            }
        }
    }

    void Update()
    {
        if (scrollTexture)
        {
            if (material)
            {
                Vector2 offset = material.mainTextureOffset;
                
                if (offset.x >= 1 || offset.x <= -1)
                {
                    offset.x = 0;
                }

                if (offset.y >= 1 || offset.y <= -1)
                {
                    offset.y = 0;
                }

                offset.x += direction.x * speed * 0.1f * Time.deltaTime;
                offset.y += direction.z * speed * 0.1f * Time.deltaTime;
                
                material.mainTextureOffset = offset;
            }
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.rigidbody)
        {
            collision.rigidbody.velocity = Vector3.Slerp(collision.rigidbody.velocity, this.transform.rotation * direction * collision.rigidbody.velocity.magnitude, antiInertia);
            collision.rigidbody.AddForce(this.transform.rotation * direction * speed, ForceMode.Acceleration);
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
using UnityEngine;

public class Enemy : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        Bullet bullet = collision.transform.GetComponent<Bullet>();
        if (bullet)
        {
            Destroy(this.gameObject);
        }
    }
}

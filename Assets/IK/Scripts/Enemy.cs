using UnityEngine;

public class Enemy : MonoBehaviour
{
    AutoAim player;

    void Awake()
    {
        player = GameObject.FindObjectOfType<AutoAim>();
    }

    void OnCollisionEnter(Collision collision)
    {
        Bullet bullet = collision.transform.GetComponent<Bullet>();
        if (bullet)
        {
            player.enemyList.Remove(this);
            Destroy(this.gameObject);
        }
    }
}

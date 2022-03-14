using UnityEngine;

public class Gun : Weapon
{
    public GameObject bulletPrefab;
    [HideInInspector] public bool canShoot = false;

    public override void Attack()
    {
        if (canShoot)
        {
            Bullet bullet = Instantiate(bulletPrefab, this.transform.position, this.transform.rotation).GetComponent<Bullet>();
            bullet.owner = this.transform.root;
        }
    }
}
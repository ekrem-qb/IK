using UnityEngine;

public class Gun : Weapon
{
    public GameObject bulletPrefab;
    [HideInInspector] public bool canShoot = false;

    public override void Attack()
    {
        if (canShoot)
        {
            Projectile projectile = Instantiate(bulletPrefab, this.transform.position, this.transform.rotation).GetComponent<Projectile>();
            projectile.owner = this.transform.root;
            if (particle)
            {
                particle.Play();
            }
        }
    }
}
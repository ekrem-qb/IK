using UnityEngine;

public class Gun : Weapon
{
    public GameObject bulletPrefab;
    public float recoilForce = 50;

    public override void Attack()
    {
        hand.rigidbody.AddForce(-this.transform.forward * recoilForce, ForceMode.Impulse);
        Projectile projectile = Instantiate(bulletPrefab, this.transform.position, this.transform.rotation).GetComponent<Projectile>();
        projectile.owner = this.transform.root;
        if (particle)
        {
            particle.Play();
        }
    }
}
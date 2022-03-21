using System.Collections;
using UnityEngine;

public class Chicken : Enemy
{
    public float explosionForce = 4000;

    protected override IEnumerator Attack()
    {
        Collider[] colliders = Physics.OverlapSphere(this.transform.position, attackDistance);
        foreach (Collider collider in colliders)
        {
            HealthManager enemy = collider.transform.root.GetComponent<HealthManager>();
            if (enemy)
            {
                enemy.health = 0;
            }

            if (collider.attachedRigidbody)
            {
                collider.attachedRigidbody.velocity = Vector3.zero;
                collider.attachedRigidbody.AddExplosionForce(explosionForce, this.transform.position, attackDistance);
            }
        }

        Destroy(this.transform.root.gameObject);
        yield return null;
    }
    
    protected override void OnPlayerChanged(Player newPlayer)
    {
        this.enabled = newPlayer;
    }
}
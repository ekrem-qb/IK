using UnityEngine;

public class Chicken : Enemy
{
    public float explosionForce = 5000;

    void FixedUpdate()
    {
        if (player)
        {
            Vector3 target = Vector3.zero;
            target = player.transform.position;
            target.y = this.transform.position.y;

            rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(target - rootJoint.transform.position));

            if (Vector3.Distance(this.transform.position, target) > attackDistance)
            {
                Vector3 direction = APR.Root.transform.forward;
                direction.y = 0f;

                if (player)
                {
                    direction *= APR.moveSpeed;
                }
                else
                {
                    direction *= APR.moveSpeed / 4;
                }

                rootRB.velocity = Vector3.Lerp(rootRB.velocity, direction + new Vector3(0, rootRB.velocity.y, 0), Time.fixedDeltaTime * 10);

                if (APR.balanced)
                {
                    if (!APR.WalkForward && !APR.moveAxisUsed)
                    {
                        APR.WalkForward = true;
                        APR.moveAxisUsed = true;
                        APR.isKeyDown = true;
                    }
                }
            }
            else
            {
                if (APR.WalkForward && APR.moveAxisUsed)
                {
                    APR.WalkForward = false;
                    APR.moveAxisUsed = false;
                    APR.isKeyDown = false;
                }

                Attack();
            }
        }
        else if (APR.WalkForward && APR.moveAxisUsed)
        {
            APR.WalkForward = false;
            APR.moveAxisUsed = false;
            APR.isKeyDown = false;
        }
    }

    void Attack()
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
                collider.attachedRigidbody.AddExplosionForce(explosionForce, this.transform.position, attackDistance);
            }
        }
        Destroy(this.transform.root.gameObject);
    }
}
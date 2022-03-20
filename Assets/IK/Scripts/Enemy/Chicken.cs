using UnityEngine;

public class Chicken : Enemy
{
    public float explosionForce = 4000;

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
                Vector3 direction = aprController.Root.transform.forward;
                direction.y = 0f;

                if (player)
                {
                    direction *= aprController.moveSpeed;
                }
                else
                {
                    direction *= aprController.moveSpeed / 4;
                }

                rootRB.velocity = Vector3.Lerp(rootRB.velocity, direction + new Vector3(0, rootRB.velocity.y, 0), Time.fixedDeltaTime * 10);

                if (aprController.balanced)
                {
                    if (!aprController.WalkForward && !aprController.moveAxisUsed)
                    {
                        aprController.WalkForward = true;
                        aprController.moveAxisUsed = true;
                        aprController.isKeyDown = true;
                    }
                }
            }
            else
            {
                if (aprController.WalkForward && aprController.moveAxisUsed)
                {
                    aprController.WalkForward = false;
                    aprController.moveAxisUsed = false;
                    aprController.isKeyDown = false;
                }

                Attack();
            }
        }
        else if (aprController.WalkForward && aprController.moveAxisUsed)
        {
            aprController.WalkForward = false;
            aprController.moveAxisUsed = false;
            aprController.isKeyDown = false;
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
                collider.attachedRigidbody.velocity = Vector3.zero;
                collider.attachedRigidbody.AddExplosionForce(explosionForce, this.transform.position, attackDistance);
            }
        }

        Destroy(this.transform.root.gameObject);
    }
}
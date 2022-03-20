using System.Collections;
using UnityEngine;

public class Gangster : Enemy
{
    public float attackInterval = 0.5f;
    bool isAttacking;
    WeaponManager weaponManager;
    PathFollower pathFollower;

    void Start()
    {
        weaponManager = aprController.COMP.GetComponent<WeaponManager>();
        pathFollower = this.GetComponent<PathFollower>();
    }

    void FixedUpdate()
    {
        if (player)
        {
            Vector3 target = player.transform.position;
            target.y = this.transform.position.y;

            rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(target - rootJoint.transform.position));

            if (Vector3.Distance(this.transform.position, target) > attackDistance)
            {
                Vector3 direction = aprController.Root.transform.forward;
                direction.y = 0f;
                direction *= aprController.moveSpeed;

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

                if (aprController.balanced)
                {
                    if (!isAttacking)
                    {
                        StartCoroutine(Attack());
                    }
                }
            }
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        if (weaponManager.weaponLeft)
        {
            weaponManager.weaponLeft.Attack();
        }

        if (weaponManager.weaponRight)
        {
            weaponManager.weaponRight.Attack();
        }

        yield return new WaitForSeconds(attackInterval);
        isAttacking = false;
    }

    public override void OnPlayerChanged(Player newPlayer)
    {
        pathFollower.enabled = !newPlayer;
    }
}
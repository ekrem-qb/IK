using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector]
    public ARP.APR.Scripts.APRController APR;
    ConfigurableJoint rootJoint;
    Rigidbody rootRB;
    AutoAim player;
    WeaponManager weaponManager;
    public float attackDistance = 2;
    public float attackInterval = 0.5f;
    bool isAttacking;

    void Awake()
    {
        APR = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        rootJoint = APR.Root.GetComponent<ConfigurableJoint>();
        rootRB = APR.Root.GetComponent<Rigidbody>();
        weaponManager = APR.COMP.GetComponent<WeaponManager>();
        player = GameObject.FindObjectOfType<AutoAim>();
    }

    void FixedUpdate()
    {
        if (player)
        {
            rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(player.transform.position - rootJoint.transform.position));

            if (Vector3.Distance(this.transform.position, player.transform.position) > attackDistance)
            {
                Vector3 direction = APR.Root.transform.forward;
                direction.y = 0f;
                rootRB.velocity = Vector3.Lerp(rootRB.velocity, (direction * APR.moveSpeed) + new Vector3(0, rootRB.velocity.y, 0), Time.fixedDeltaTime * 10);

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
                if (APR.balanced)
                {
                    if (!isAttacking)
                    {
                        StartCoroutine(Attack());
                    }
                }
            }
        }
        else if (APR.WalkForward && APR.moveAxisUsed)
        {
            APR.WalkForward = false;
            APR.moveAxisUsed = false;
            APR.isKeyDown = false;
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, attackDistance);
    }

    void OnDestroy()
    {
        if (player)
        {
            player.nearEnemies.Remove(this);
        }
    }
}
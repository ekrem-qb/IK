using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR;
    ConfigurableJoint rootJoint;
    ConfigurableJoint armLeft, armRight;
    ConfigurableJoint armLeftLow, armRightLow;
    Rigidbody rootRB;
    AutoAim player;
    public float attackDistance = 2;
    public float attackInterval = 0.5f;
    bool isAttacking;

    void Awake()
    {
        APR = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        rootJoint = APR.Root.GetComponent<ConfigurableJoint>();
        rootRB = APR.Root.GetComponent<Rigidbody>();
        armLeft = APR.UpperLeftArm.GetComponent<ConfigurableJoint>();
        armRight = APR.UpperRightArm.GetComponent<ConfigurableJoint>();
        armLeftLow = APR.LowerLeftArm.GetComponent<ConfigurableJoint>();
        armRightLow = APR.LowerRightArm.GetComponent<ConfigurableJoint>();
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
                if (!isAttacking && APR.balanced)
                {
                    StartCoroutine(Attack());
                }
            }
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        armRight.targetRotation = new Quaternion(-0.62f, 0.51f, 0.02f, 1);
        armRightLow.targetRotation = new Quaternion(1.31f, -0.5f, -0.5f, 1);

        yield return new WaitForSeconds(0.25f);
        armRight.targetRotation = new Quaternion(0.3f, 0.64f, -0.3f, -0.5f);
        armRightLow.targetRotation = new Quaternion(0.2f, 0, 0, 1);

        yield return new WaitForSeconds(0.25f);
        armRight.targetRotation = APR.UpperRightArmTarget;
        armRightLow.targetRotation = APR.LowerRightArmTarget;

        yield return new WaitForSeconds(attackInterval);
        isAttacking = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        Bullet bullet = collision.transform.GetComponent<Bullet>();
        if (bullet)
        {
            Destroy(this.transform.root.gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, attackDistance);
    }
}
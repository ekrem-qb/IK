using System.Collections;
using UnityEngine;

public class Melee : Weapon
{
    ARP.APR.Scripts.APRController APR;
    ConfigurableJoint arm, armLow, hand;
    public float force = 50;
    bool isInHook;
    bool isAttacking;

    void Start()
    {
        ReassignToNewOwner();
    }

    void OnTransformParentChanged()
    {
        ReassignToNewOwner();
    }

    void ReassignToNewOwner()
    {
        if (this.transform.parent)
        {
            APR = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
            if (isLeft)
            {
                arm = APR.UpperLeftArm.GetComponent<ConfigurableJoint>();
                armLow = APR.LowerLeftArm.GetComponent<ConfigurableJoint>();
                hand = APR.LeftHand.GetComponent<ConfigurableJoint>();
            }
            else
            {
                arm = APR.UpperRightArm.GetComponent<ConfigurableJoint>();
                armLow = APR.LowerRightArm.GetComponent<ConfigurableJoint>();
                hand = APR.RightHand.GetComponent<ConfigurableJoint>();
            }
        }
    }

    public override void Attack()
    {
        if (!isInHook)
        {
            StartCoroutine(Hook());
        }
    }

    IEnumerator Hook()
    {
        isInHook = true;

        if (isLeft)
        {
            arm.targetRotation = new Quaternion(-0.62f, -0.51f, -0.02f, 1);
            armLow.targetRotation = new Quaternion(-1.31f, -0.5f, 0.5f, 1);
        }
        else
        {
            arm.targetRotation = new Quaternion(-0.62f, 0.51f, 0.02f, 1);
            armLow.targetRotation = new Quaternion(1.31f, -0.5f, -0.5f, 1);
        }

        yield return new WaitForSeconds(0.25f);

        if (isLeft)
        {
            arm.targetRotation = new Quaternion(0.3f, -0.64f, 0.3f, -0.5f);
            armLow.targetRotation = new Quaternion(-0.2f, 0, 0, 1);
            hand.targetRotation = new Quaternion(0.75f, -0.04f, 0.1f, 1);
        }
        else
        {
            arm.targetRotation = new Quaternion(0.3f, 0.64f, -0.3f, -0.5f);
            armLow.targetRotation = new Quaternion(0.2f, 0, 0, 1);
            hand.targetRotation = new Quaternion(-0.75f, -0.04f, -0.1f, 1);
        }
        isAttacking = true;

        yield return new WaitForSeconds(0.25f);
        if (isLeft)
        {
            arm.targetRotation = APR.UpperLeftArmTarget;
            armLow.targetRotation = APR.LowerLeftArmTarget;
            hand.targetRotation = APR.LeftHandTarget;
        }
        else
        {
            arm.targetRotation = APR.UpperRightArmTarget;
            armLow.targetRotation = APR.LowerRightArmTarget;
            hand.targetRotation = APR.RightHandTarget;
        }
        isAttacking = false;
        isInHook = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isAttacking)
        {
            if (this.enabled)
            {
                if (!other.isTrigger)
                {
                    if (this.transform.root != other.transform.root)
                    {
                        HealthManager enemy = other.transform.root.GetComponent<HealthManager>();
                        if (enemy)
                        {
                            enemy.health -= 10;
                            Rigidbody rb = other.GetComponent<Rigidbody>();
                            if (rb)
                            {
                                rb.AddForce(this.transform.right * force, ForceMode.Impulse);
                            }
                        }
                    }
                }
            }
        }
    }
}

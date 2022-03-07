using System.Collections;
using UnityEngine;

public class Melee : Weapon
{
    ARP.APR.Scripts.APRController APR;
    ConfigurableJoint arm;
    ConfigurableJoint armLow;
    public float force = 50;
    bool isInHook;
    bool isAttacking;

    void Start()
    {
        APR = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        if (isLeft)
        {
            arm = APR.UpperLeftArm.GetComponent<ConfigurableJoint>();
            armLow = APR.LowerLeftArm.GetComponent<ConfigurableJoint>();
        }
        else
        {
            arm = APR.UpperRightArm.GetComponent<ConfigurableJoint>();
            armLow = APR.LowerRightArm.GetComponent<ConfigurableJoint>();
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

        arm.targetRotation = new Quaternion(-0.62f, 0.51f, 0.02f, 1);
        armLow.targetRotation = new Quaternion(1.31f, -0.5f, -0.5f, 1);

        yield return new WaitForSeconds(0.25f);
        arm.targetRotation = new Quaternion(0.3f, 0.64f, -0.3f, -0.5f);
        armLow.targetRotation = new Quaternion(0.2f, 0, 0, 1);
        isAttacking = true;

        yield return new WaitForSeconds(0.25f);
        arm.targetRotation = APR.UpperRightArmTarget;
        armLow.targetRotation = APR.LowerRightArmTarget;
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
                        Rigidbody rb = other.GetComponent<Rigidbody>();
                        if (rb)
                        {
                            rb.AddForce(this.transform.right * force, ForceMode.Impulse);
                        }
                        HealthManager enemy = other.transform.root.GetComponent<HealthManager>();
                        if (enemy)
                        {
                            enemy.health -= 10;
                        }
                    }
                }
            }
        }
    }
}

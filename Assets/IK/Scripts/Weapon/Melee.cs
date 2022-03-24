using System;
using System.Collections;
using UnityEngine;

public class Melee : Weapon
{
    ARP.APR.Scripts.APRController aprController;
    protected ConfigurableJoint arm, armLow, hand;
    public float force = 50;
    [HideInInspector] public bool isAttacking;
    private bool _isInHook;
    protected Action<bool> IsInHookChanged = newIsInHook => { };

    protected bool isInHook
    {
        get => _isInHook;
        set
        {
            if (_isInHook != value)
            {
                IsInHookChanged(value);
            }

            _isInHook = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();
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
            aprController = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
            if (isLeft)
            {
                arm = aprController.UpperLeftArm.GetComponent<ConfigurableJoint>();
                armLow = aprController.LowerLeftArm.GetComponent<ConfigurableJoint>();
                hand = aprController.LeftHand.GetComponent<ConfigurableJoint>();
            }
            else
            {
                arm = aprController.UpperRightArm.GetComponent<ConfigurableJoint>();
                armLow = aprController.LowerRightArm.GetComponent<ConfigurableJoint>();
                hand = aprController.RightHand.GetComponent<ConfigurableJoint>();
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

    protected IEnumerator Hook()
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
            if (!(this is Fist))
            {
                hand.targetRotation = new Quaternion(0.75f, -0.04f, 0.1f, 1);
            }
        }
        else
        {
            arm.targetRotation = new Quaternion(0.3f, 0.64f, -0.3f, -0.5f);
            armLow.targetRotation = new Quaternion(0.2f, 0, 0, 1);
            if (!(this is Fist))
            {
                hand.targetRotation = new Quaternion(-0.75f, -0.04f, -0.1f, 1);
            }
        }

        isAttacking = true;

        yield return new WaitForSeconds(0.25f);
        if (isLeft)
        {
            arm.targetRotation = aprController.UpperLeftArmTarget;
            armLow.targetRotation = aprController.LowerLeftArmTarget;
            hand.targetRotation = aprController.LeftHandTarget;
        }
        else
        {
            arm.targetRotation = aprController.UpperRightArmTarget;
            armLow.targetRotation = aprController.LowerRightArmTarget;
            hand.targetRotation = aprController.RightHandTarget;
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
                            enemy.health -= damage;
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
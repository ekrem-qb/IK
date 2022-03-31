using System;
using System.Collections;
using UnityEngine;

public class Melee : Weapon
{
    public float force = 50;
    [HideInInspector] public bool isAttacking;
    private ARP.APR.Scripts.APRController _aprController;
    private ConfigurableJoint _arm, _armLow;
    private bool _isInHook;
    protected ConfigurableJoint hand;
    protected Action<bool> isInHookChanged = newIsInHook => { };

    protected bool isInHook
    {
        get => _isInHook;
        set
        {
            if (_isInHook != value)
            {
                isInHookChanged(value);
            }

            _isInHook = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        ReassignToNewOwner();
    }

    private void OnTransformParentChanged()
    {
        ReassignToNewOwner();
    }

    private void OnTriggerEnter(Collider other)
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

                        if (particle)
                        {
                            particle.Play();
                        }
                    }
                }
            }
        }
    }

    private void ReassignToNewOwner()
    {
        if (this.transform.parent)
        {
            _aprController = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
            if (isLeft)
            {
                _arm = _aprController.UpperLeftArm.GetComponent<ConfigurableJoint>();
                _armLow = _aprController.LowerLeftArm.GetComponent<ConfigurableJoint>();
                hand = _aprController.LeftHand.GetComponent<ConfigurableJoint>();
            }
            else
            {
                _arm = _aprController.UpperRightArm.GetComponent<ConfigurableJoint>();
                _armLow = _aprController.LowerRightArm.GetComponent<ConfigurableJoint>();
                hand = _aprController.RightHand.GetComponent<ConfigurableJoint>();
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
            _arm.targetRotation = new Quaternion(-0.62f, -0.51f, -0.02f, 1);
            _armLow.targetRotation = new Quaternion(-1.31f, -0.5f, 0.5f, 1);
        }
        else
        {
            _arm.targetRotation = new Quaternion(-0.62f, 0.51f, 0.02f, 1);
            _armLow.targetRotation = new Quaternion(1.31f, -0.5f, -0.5f, 1);
        }

        yield return new WaitForSeconds(0.25f);

        if (isLeft)
        {
            _arm.targetRotation = new Quaternion(0.3f, -0.64f, 0.3f, -0.5f);
            _armLow.targetRotation = new Quaternion(-0.2f, 0, 0, 1);
            if (!(this is Fist))
            {
                hand.targetRotation = new Quaternion(0.75f, -0.04f, 0.1f, 1);
            }
        }
        else
        {
            _arm.targetRotation = new Quaternion(0.3f, 0.64f, -0.3f, -0.5f);
            _armLow.targetRotation = new Quaternion(0.2f, 0, 0, 1);
            if (!(this is Fist))
            {
                hand.targetRotation = new Quaternion(-0.75f, -0.04f, -0.1f, 1);
            }
        }

        isAttacking = true;

        yield return new WaitForSeconds(0.25f);
        if (isLeft)
        {
            _arm.targetRotation = _aprController.UpperLeftArmTarget;
            _armLow.targetRotation = _aprController.LowerLeftArmTarget;
            hand.targetRotation = _aprController.LeftHandTarget;
        }
        else
        {
            _arm.targetRotation = _aprController.UpperRightArmTarget;
            _armLow.targetRotation = _aprController.LowerRightArmTarget;
            hand.targetRotation = _aprController.RightHandTarget;
        }

        isAttacking = false;
        isInHook = false;
    }
}
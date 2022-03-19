using System;
using System.Collections;
using UnityEngine;

public class Mover : Enemy
{
    public float pickUpDelay = 0.065f;
    PathFollower pathFollower;
    GameObject box;
    SphereCollider trigger;
    FixedJoint jointLeft, jointRight;
    Transform target;

    void OnDrawGizmosSelected()
    {
    }

    private void Start()
    {
        pathFollower = this.GetComponent<PathFollower>();
        trigger = this.GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!box)
        {
            if (other.name == "Box")
            {
                StartCoroutine(PickUp(other));
            }
        }
        else
        {
            Conveyor conveyor = other.GetComponent<Conveyor>();
            if (conveyor)
            {
                StartCoroutine(Drop(conveyor));
            }
        }
    }

    IEnumerator PickUp(Collider coll)
    {
        pathFollower.isWaiting = true;

        rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(coll.transform.position - this.transform.position));

        yield return new WaitForSeconds(pickUpDelay * 10);

        if (APR.MouseYAxisArms <= 1.2f && APR.MouseYAxisArms >= -1.2f)
        {
            APR.MouseYAxisArms = APR.MouseYAxisArms + (Input.GetAxis("Mouse Y") / APR.reachSensitivity);
        }
        else if (APR.MouseYAxisArms > 1.2f)
        {
            APR.MouseYAxisArms = 1.2f;
        }
        else if (APR.MouseYAxisArms < -1.2f)
        {
            APR.MouseYAxisArms = -1.2f;
        }

        if (!APR.reachLeftAxisUsed)
        {
            //Adjust Left Arm joint strength
            APR.UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.ReachStiffness;
            APR.UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.ReachStiffness;
            APR.LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.ReachStiffness;
            APR.LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.ReachStiffness;

            //Adjust body joint strength
            APR.Body.GetComponent<ConfigurableJoint>().angularXDrive = APR.CoreStiffness;
            APR.Body.GetComponent<ConfigurableJoint>().angularYZDrive = APR.CoreStiffness;

            APR.reachLeftAxisUsed = true;
        }

        //upper  left arm pose
        APR.UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(-0.88f - APR.MouseYAxisArms, 0.58f + APR.MouseYAxisArms, -0.8f, 1);

        if (!APR.reachRightAxisUsed)
        {
            //Adjust Right Arm joint strength
            APR.UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.ReachStiffness;
            APR.UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.ReachStiffness;
            APR.LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.ReachStiffness;
            APR.LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.ReachStiffness;

            //Adjust body joint strength
            APR.Body.GetComponent<ConfigurableJoint>().angularXDrive = APR.CoreStiffness;
            APR.Body.GetComponent<ConfigurableJoint>().angularYZDrive = APR.CoreStiffness;

            APR.reachRightAxisUsed = true;
        }

        //upper  left arm pose
        APR.UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Inverse(new Quaternion(0.88f - APR.MouseYAxisArms, 0.58f + APR.MouseYAxisArms, -0.8f, 1));

        yield return new WaitForSeconds(pickUpDelay);

        jointLeft = APR.LeftHand.gameObject.AddComponent<FixedJoint>();
        jointLeft.breakForce = Mathf.Infinity;
        jointLeft.connectedBody = coll.attachedRigidbody;

        jointRight = APR.RightHand.gameObject.AddComponent<FixedJoint>();
        jointRight.breakForce = Mathf.Infinity;
        jointRight.connectedBody = coll.attachedRigidbody;

        box = coll.gameObject;
        trigger.radius = 2;

        pathFollower.nextPoint++;
        pathFollower.isWaiting = false;
    }

    IEnumerator Drop(Conveyor conveyor)
    {
        pathFollower.isWaiting = true;

        rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(conveyor.transform.position - this.transform.position));

        yield return new WaitForSeconds(pickUpDelay * 10);

        if (APR.reachLeftAxisUsed)
        {
            if (APR.balanced)
            {
                APR.UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.PoseOn;
                APR.UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.PoseOn;
                APR.LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.PoseOn;
                APR.LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.PoseOn;

                APR.Body.GetComponent<ConfigurableJoint>().angularXDrive = APR.PoseOn;
                APR.Body.GetComponent<ConfigurableJoint>().angularYZDrive = APR.PoseOn;
            }
            else if (!APR.balanced)
            {
                APR.UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.DriveOff;
                APR.UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.DriveOff;
                APR.LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.DriveOff;
                APR.LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.DriveOff;
            }

            APR.ResetPlayerPose();
            APR.reachLeftAxisUsed = false;
        }

        yield return new WaitForSeconds(pickUpDelay);

        Destroy(jointLeft);
        Destroy(jointRight);

        box = null;
        trigger.radius = 1;

        pathFollower.nextPoint = 0;
        pathFollower.isWaiting = false;
    }
}
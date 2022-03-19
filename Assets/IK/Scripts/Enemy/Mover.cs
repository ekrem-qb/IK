using System;
using System.Collections;
using UnityEngine;

public class Mover : Enemy
{
    PathFollower pathFollower;
    public float pickUpDelay;
    GameObject box;

    void OnDrawGizmosSelected()
    {
    }

    private void Start()
    {
        pathFollower = this.GetComponent<PathFollower>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!box)
        {
            if (other.CompareTag("CanBeGrabbed"))
            {
                if (other.transform.root != this.transform.root)
                {
                    StartCoroutine(PickUp(other));
                }
            }
        }
    }

    IEnumerator PickUp(Collider coll)
    {
        pathFollower.isWaiting = true;

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

        FixedJoint joint = APR.LeftHand.gameObject.AddComponent<FixedJoint>();
        joint.breakForce = Mathf.Infinity;
        joint.connectedBody = coll.attachedRigidbody;

        box = coll.gameObject;

        pathFollower.nextPoint++;
        pathFollower.isWaiting = false;
    }
}
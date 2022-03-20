using System;
using System.Collections;
using UnityEngine;

public class Mover : Enemy
{
    public float pickUpDelay = 0.065f;
    public Conveyor conveyor;
    public BoxManager boxManager;
    PathFollower pathFollower;
    Box _box;

    Box box
    {
        get => _box;
        set
        {
            if (_box)
            {
                pathFollower.path.Remove(_box.transform);
            }

            if (value)
            {
                if (!pathFollower.path.Contains(value.transform))
                {
                    pathFollower.path.Add(value.transform);
                }
            }

            _box = value;
        }
    }

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
        boxManager.boxes.CountChanged += OnBoxesCountChanged;
    }

    void OnBoxesCountChanged(object sender, EventArgs e)
    {
        if (boxManager.boxes.Count > 0)
        {
            boxManager.boxes.SortByDistanceTo(this.transform.position);
            box = boxManager.boxes[0];
        }
        else
        {
            box = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pathFollower.path.Count > 0)
        {
            if (pathFollower.path[0] == conveyor.transform)
            {
                if (other.transform == conveyor.transform)
                {
                    StartCoroutine(Drop());
                }
            }
            else if (box)
            {
                if (pathFollower.path[0] == box.transform)
                {
                    if (other.transform == box.transform)
                    {
                        if (!jointLeft && !jointRight)
                        {
                            StartCoroutine(PickUp(other));
                        }
                    }
                }
            }
        }
    }

    IEnumerator PickUp(Collider coll)
    {
        pathFollower.isWaiting = true;
        box = null;

        Vector3 targetPos = coll.transform.position;
        targetPos.y = this.transform.position.y;
        rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(targetPos - this.transform.position));
        bodyJoint.targetRotation = new Quaternion(APR.MouseYAxisBody, 0, 0, 1);

        yield return new WaitForSeconds(pickUpDelay * 5);

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

        bodyJoint.targetRotation = new Quaternion(0, 0, 0, 1);

        yield return new WaitForSeconds(pickUpDelay * 5);

        if (!pathFollower.path.Contains(conveyor.transform))
        {
            pathFollower.path.Insert(0, conveyor.transform);
        }

        pathFollower.isWaiting = false;
        trigger.radius = 1.5f;
    }

    IEnumerator Drop()
    {
        pathFollower.isWaiting = true;

        Vector3 targetPos = conveyor.transform.position;
        targetPos.y = this.transform.position.y;
        rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(targetPos - this.transform.position));

        yield return new WaitForSeconds(pickUpDelay * 5);

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

        if (APR.reachRightAxisUsed)
        {
            if (APR.balanced)
            {
                APR.UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.PoseOn;
                APR.UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.PoseOn;
                APR.LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.PoseOn;
                APR.LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.PoseOn;

                APR.Body.GetComponent<ConfigurableJoint>().angularXDrive = APR.PoseOn;
                APR.Body.GetComponent<ConfigurableJoint>().angularYZDrive = APR.PoseOn;
            }
            else if (!APR.balanced)
            {
                APR.UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.DriveOff;
                APR.UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.DriveOff;
                APR.LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.DriveOff;
                APR.LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.DriveOff;
            }

            APR.ResetPlayerPose();
            APR.reachRightAxisUsed = false;
        }

        yield return new WaitForSeconds(pickUpDelay);

        Destroy(jointLeft);
        Destroy(jointRight);

        pathFollower.isWaiting = false;
        pathFollower.path.Remove(conveyor.transform);

        trigger.radius = 1;
    }

    private void OnDrawGizmos()
    {
        if (pathFollower)
        {
            if (pathFollower.path.Count > 0)
            {
                if (pathFollower.path[0])
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawLine(this.transform.position, pathFollower.path[0].position);
                }
            }
        }
    }
}
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

    void OnDrawGizmosSelected()
    {
    }

    private void Start()
    {
        pathFollower = this.GetComponent<PathFollower>();
        trigger = this.GetComponent<SphereCollider>();
        boxManager.boxes.CountChanged += OnBoxesCountChanged;
    }

    void OnBoxesCountChanged(int count)
    {
        if (count > 0)
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
        bodyJoint.targetRotation = new Quaternion(aprController.MouseYAxisBody, 0, 0, 1);

        yield return new WaitForSeconds(pickUpDelay * 5);

        if (!aprController.reachLeftAxisUsed)
        {
            //Adjust Left Arm joint strength
            aprController.UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.ReachStiffness;
            aprController.UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.ReachStiffness;
            aprController.LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.ReachStiffness;
            aprController.LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.ReachStiffness;

            //Adjust body joint strength
            aprController.Body.GetComponent<ConfigurableJoint>().angularXDrive = aprController.CoreStiffness;
            aprController.Body.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.CoreStiffness;

            aprController.reachLeftAxisUsed = true;
        }

        //upper  left arm pose
        aprController.UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = new Quaternion(-0.88f - aprController.MouseYAxisArms, 0.58f + aprController.MouseYAxisArms, -0.8f, 1);

        if (!aprController.reachRightAxisUsed)
        {
            //Adjust Right Arm joint strength
            aprController.UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.ReachStiffness;
            aprController.UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.ReachStiffness;
            aprController.LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.ReachStiffness;
            aprController.LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.ReachStiffness;

            //Adjust body joint strength
            aprController.Body.GetComponent<ConfigurableJoint>().angularXDrive = aprController.CoreStiffness;
            aprController.Body.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.CoreStiffness;

            aprController.reachRightAxisUsed = true;
        }

        //upper  left arm pose
        aprController.UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.Inverse(new Quaternion(0.88f - aprController.MouseYAxisArms, 0.58f + aprController.MouseYAxisArms, -0.8f, 1));

        yield return new WaitForSeconds(pickUpDelay);

        jointLeft = aprController.LeftHand.gameObject.AddComponent<FixedJoint>();
        jointLeft.breakForce = Mathf.Infinity;
        jointLeft.connectedBody = coll.attachedRigidbody;

        jointRight = aprController.RightHand.gameObject.AddComponent<FixedJoint>();
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

        if (aprController.reachLeftAxisUsed)
        {
            if (aprController.balanced)
            {
                aprController.UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.PoseOn;
                aprController.UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.PoseOn;
                aprController.LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.PoseOn;
                aprController.LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.PoseOn;

                aprController.Body.GetComponent<ConfigurableJoint>().angularXDrive = aprController.PoseOn;
                aprController.Body.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.PoseOn;
            }
            else if (!aprController.balanced)
            {
                aprController.UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.DriveOff;
                aprController.UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.DriveOff;
                aprController.LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.DriveOff;
                aprController.LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.DriveOff;
            }

            aprController.ResetPlayerPose();
            aprController.reachLeftAxisUsed = false;
        }

        if (aprController.reachRightAxisUsed)
        {
            if (aprController.balanced)
            {
                aprController.UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.PoseOn;
                aprController.UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.PoseOn;
                aprController.LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.PoseOn;
                aprController.LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.PoseOn;

                aprController.Body.GetComponent<ConfigurableJoint>().angularXDrive = aprController.PoseOn;
                aprController.Body.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.PoseOn;
            }
            else if (!aprController.balanced)
            {
                aprController.UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.DriveOff;
                aprController.UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.DriveOff;
                aprController.LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.DriveOff;
                aprController.LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.DriveOff;
            }

            aprController.ResetPlayerPose();
            aprController.reachRightAxisUsed = false;
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
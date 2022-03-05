using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR;
    Transform handLeft, handRight;
    public List<Gun> nearGuns = new List<Gun>();
    public Gun gunLeft, gunRight;
    public KeyCode keyPickUp = KeyCode.E;
    public KeyCode keyDrop = KeyCode.Q;

    void Awake()
    {
        APR = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        handLeft = APR.LeftHand.transform.GetChild(0);
        handRight = APR.RightHand.transform.GetChild(0);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            Gun gun = other.GetComponent<Gun>();
            if (gun)
            {
                if (!nearGuns.Contains(gun))
                {
                    nearGuns.Add(gun);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
        {
            Gun gun = other.GetComponent<Gun>();
            if (gun)
            {
                nearGuns.Remove(gun);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(keyPickUp) && (!gunLeft || !gunRight) && nearGuns.Count > 0)
        {
            APR.ResetPlayerPose();

            if (!gunLeft)
            {
                gunLeft = nearGuns[0];
                gunLeft.isLeft = true;
                gunLeft.transform.SetParent(handLeft);
                gunLeft.enabled = true;
            }
            else if (!gunRight)
            {
                gunRight = nearGuns[0];
                gunRight.isLeft = false;
                gunRight.transform.SetParent(handRight);
                gunRight.enabled = true;
            }
            nearGuns.Remove(nearGuns[0]);
        }
        if (Input.GetKeyDown(keyDrop) && (gunLeft || gunRight))
        {
            if (gunRight)
            {
                gunRight.transform.SetParent(null);
                gunRight.enabled = false;
                gunRight = null;
                APR.UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.PoseOn;
                APR.UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.PoseOn;
                APR.LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.PoseOn;
                APR.LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.PoseOn;
                APR.UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = APR.UpperRightArmTarget;
                APR.LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = APR.LowerRightArmTarget;
            }
            else if (gunLeft)
            {
                gunLeft.transform.SetParent(null);
                gunLeft.enabled = false;
                gunLeft = null;
                APR.UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.PoseOn;
                APR.UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.PoseOn;
                APR.LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR.PoseOn;
                APR.LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR.PoseOn;
                APR.UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = APR.UpperLeftArmTarget;
                APR.LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation = APR.LowerLeftArmTarget;
            }
        }
    }
}
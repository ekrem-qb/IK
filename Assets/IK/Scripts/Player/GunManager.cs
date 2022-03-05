using System.Collections.Generic;
using UnityEngine;

public class GunManager : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR_Player;
    Transform handLeft, handRight;
    public List<Gun> nearGuns = new List<Gun>();
    public Gun gunLeft, gunRight;
    public KeyCode keyPickUp = KeyCode.E;
    public KeyCode keyDrop = KeyCode.Q;

    void Awake()
    {
        APR_Player = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        handLeft = APR_Player.LeftHand.transform.GetChild(0);
        handRight = APR_Player.RightHand.transform.GetChild(0);
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
            APR_Player.ResetPlayerPose();

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
                APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR_Player.PoseOn;
                APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR_Player.PoseOn;
                APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR_Player.PoseOn;
                APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR_Player.PoseOn;
                APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = APR_Player.UpperRightArmTarget;
                APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = APR_Player.LowerRightArmTarget;
            }
            else if (gunLeft)
            {
                gunLeft.transform.SetParent(null);
                gunLeft.enabled = false;
                gunLeft = null;
                APR_Player.UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR_Player.PoseOn;
                APR_Player.UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR_Player.PoseOn;
                APR_Player.LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR_Player.PoseOn;
                APR_Player.LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR_Player.PoseOn;
                APR_Player.UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = APR_Player.UpperLeftArmTarget;
                APR_Player.LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation = APR_Player.LowerLeftArmTarget;
            }
        }
    }
}
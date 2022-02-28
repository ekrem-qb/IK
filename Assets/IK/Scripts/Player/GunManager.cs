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
            if (gun != null)
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
            if (gun != null)
            {
                nearGuns.Remove(gun);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(keyPickUp) && (gunLeft == null || gunRight == null) && nearGuns.Count > 0)
        {
            APR_Player.ResetPlayerPose();

            if (gunLeft == null)
            {
                gunLeft = nearGuns[0];
                gunLeft.enabled = true;
                gunLeft.fireKey = KeyCode.Mouse0;
                gunLeft.transform.SetParent(handLeft);
                gunLeft.transform.localPosition = gunLeft.holdPosition;
                gunLeft.transform.localEulerAngles = gunLeft.holdAngles;
            }
            else if (gunRight == null)
            {
                gunRight = nearGuns[0];
                gunRight.enabled = true;
                gunRight.fireKey = KeyCode.Mouse1;
                gunRight.transform.SetParent(handRight);
                gunRight.transform.localPosition = new Vector3(-gunRight.holdPosition.x, gunRight.holdPosition.y, gunRight.holdPosition.z);
                gunRight.transform.localEulerAngles = new Vector3(gunRight.holdAngles.x, gunRight.holdAngles.y, gunRight.holdAngles.z + 180);
            }
            nearGuns.Remove(nearGuns[0]);
        }
        if (Input.GetKeyDown(keyDrop) && (gunLeft != null || gunRight != null))
        {
            if (gunRight != null)
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
            else if (gunLeft != null)
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
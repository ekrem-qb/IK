using UnityEngine;

public class GunManager : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR_Player;
    Transform handLeft, handRight;
    public Gun gunLeft, gunRight;
    public KeyCode keyPickUp = KeyCode.E;
    public KeyCode keyDrop = KeyCode.Q;
    public GameObject pickupPrefab;

    void Awake()
    {
        APR_Player = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        handLeft = APR_Player.LeftHand.transform.GetChild(0);
        handRight = APR_Player.RightHand.transform.GetChild(0);
    }

    void OnTriggerStay(Collider other)
    {
        if (Input.GetKey(keyPickUp))
        {
            Pickup pickup = other.GetComponent<Pickup>();
            if (pickup != null)
            {
                APR_Player.ResetPlayerPose();

                if (gunLeft == null)
                {
                    gunLeft = pickup.pickup.GetComponent<Gun>();
                    gunLeft.transform.SetParent(handLeft);
                    gunLeft.transform.localPosition = pickup.originalPosition;
                    gunLeft.transform.localEulerAngles = pickup.originalAngles;
                    gunLeft.enabled = true;
                }
                else if (gunRight == null)
                {
                    gunRight = pickup.pickup.GetComponent<Gun>();
                    gunRight.transform.SetParent(handRight);
                    gunRight.transform.localPosition = new Vector3(-pickup.originalPosition.x, pickup.originalPosition.y, pickup.originalPosition.z);
                    gunRight.transform.localEulerAngles = new Vector3(pickup.originalAngles.x, pickup.originalAngles.y, -pickup.originalAngles.z);
                    gunRight.fireButton = Gun.FireButton.Fire2;
                    gunRight.enabled = true;
                }
                Destroy(pickup.gameObject);
            }
        }
    }

    void Update()
    {
        if (Input.GetKey(keyDrop) && (gunLeft != null || gunRight != null))
        {
            Pickup pickup = Instantiate(pickupPrefab).GetComponent<Pickup>();

            if (gunLeft != null)
            {
                pickup.transform.position = gunLeft.transform.position;
                pickup.pickup = gunLeft.gameObject;
                gunLeft.enabled = false;
                gunLeft = null;
                APR_Player.UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR_Player.PoseOn;
                APR_Player.UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR_Player.PoseOn;
                APR_Player.LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = APR_Player.PoseOn;
                APR_Player.LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR_Player.PoseOn;
                APR_Player.UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = APR_Player.UpperLeftArmTarget;
                APR_Player.LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation = APR_Player.LowerLeftArmTarget;
            }
            else if (gunRight != null)
            {
                pickup.transform.position = gunRight.transform.position;
                pickup.pickup = gunRight.gameObject;
                gunRight.enabled = false;
                gunRight = null;
                APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR_Player.PoseOn;
                APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR_Player.PoseOn;
                APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR_Player.PoseOn;
                APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR_Player.PoseOn;
                APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = APR_Player.UpperRightArmTarget;
                APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = APR_Player.LowerRightArmTarget;
            }
        }
    }
}

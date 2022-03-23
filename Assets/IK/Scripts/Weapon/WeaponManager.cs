using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    ARP.APR.Scripts.APRController aprController;
    Transform handLeft, handRight;
    Player player;
    public List<Weapon> nearWeapons = new List<Weapon>();
    [HideInInspector] public Weapon weaponLeft, weaponRight;
    public KeyCode keyPickUp = KeyCode.E;
    public KeyCode keyDrop = KeyCode.Q;

    void Awake()
    {
        aprController = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        player = aprController.Root.GetComponent<Player>();
        handLeft = aprController.LeftHand.transform.GetChild(0);
        handRight = aprController.RightHand.transform.GetChild(0);
        Weapon hasWeaponOnLeft = handLeft.GetComponentInChildren<Weapon>();
        if (hasWeaponOnLeft)
        {
            weaponLeft = hasWeaponOnLeft;
            weaponLeft.player = player;
        }

        Weapon hasWeaponOnRight = handRight.GetComponentInChildren<Weapon>();
        if (hasWeaponOnRight)
        {
            weaponRight = hasWeaponOnRight;
            weaponRight.player = player;
        }

        this.enabled = player != null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            Weapon weapon = other.GetComponent<Weapon>();
            if (weapon)
            {
                if (!weapon.enabled)
                {
                    if (!nearWeapons.Contains(weapon))
                    {
                        nearWeapons.Add(weapon);
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
        {
            Weapon weapon = other.GetComponent<Weapon>();
            if (weapon)
            {
                nearWeapons.Remove(weapon);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(keyPickUp) && (!weaponLeft || !weaponRight) && nearWeapons.Count > 0)
        {
            aprController.ResetPlayerPose();

            if (!weaponLeft)
            {
                weaponLeft = nearWeapons[0];
                weaponLeft.isLeft = true;
                weaponLeft.transform.SetParent(handLeft);
                weaponLeft.enabled = true;
                weaponLeft.player = player;
                if (weaponLeft is Gun)
                {
                    (weaponLeft as Gun).canShoot = player.nearEnemies.Count > 0;
                    player.enabled = false;
                    player.enabled = player.nearEnemies.Count > 0;
                }
            }
            else if (!weaponRight)
            {
                weaponRight = nearWeapons[0];
                weaponRight.isLeft = false;
                weaponRight.transform.SetParent(handRight);
                weaponRight.enabled = true;
                weaponRight.player = player;
                if (weaponRight is Gun)
                {
                    (weaponRight as Gun).canShoot = player.nearEnemies.Count > 0;
                    player.enabled = false;
                    player.enabled = player.nearEnemies.Count > 0;
                }
            }

            nearWeapons.Remove(nearWeapons[0]);
        }

        if (Input.GetKeyDown(keyDrop) && (weaponLeft || weaponRight))
        {
            if (weaponRight)
            {
                Drop(weaponRight);
            }
            else if (weaponLeft)
            {
                Drop(weaponLeft);
            }
        }
    }

    void Drop(Weapon weapon)
    {
        weapon.transform.SetParent(null);
        weapon.enabled = false;

        if (weapon == weaponRight)
        {
            weaponRight = null;
            aprController.UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.PoseOn;
            aprController.UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.PoseOn;
            aprController.LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.PoseOn;
            aprController.LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.PoseOn;
            aprController.UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = aprController.UpperRightArmTarget;
            aprController.LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = aprController.LowerRightArmTarget;
        }
        else if (weapon == weaponLeft)
        {
            weaponLeft = null;
            aprController.UpperLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.PoseOn;
            aprController.UpperLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.PoseOn;
            aprController.LowerLeftArm.GetComponent<ConfigurableJoint>().angularXDrive = aprController.PoseOn;
            aprController.LowerLeftArm.GetComponent<ConfigurableJoint>().angularYZDrive = aprController.PoseOn;
            aprController.UpperLeftArm.GetComponent<ConfigurableJoint>().targetRotation = aprController.UpperLeftArmTarget;
            aprController.LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation = aprController.LowerLeftArmTarget;
        }
    }

    public void DropAllWeapons()
    {
        if (weaponRight)
        {
            Drop(weaponRight);
        }

        if (weaponLeft)
        {
            Drop(weaponLeft);
        }
    }
}
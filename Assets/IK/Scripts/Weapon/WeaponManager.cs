using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR_Player;
    Transform handLeft, handRight;
    AutoAim player;
    public List<Weapon> nearWeapons = new List<Weapon>();
    public Weapon weaponLeft, weaponRight;
    public KeyCode keyPickUp = KeyCode.E;
    public KeyCode keyDrop = KeyCode.Q;

    void Awake()
    {
        APR_Player = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        player = APR_Player.Root.GetComponent<AutoAim>();
        handLeft = APR_Player.LeftHand.transform.GetChild(0);
        handRight = APR_Player.RightHand.transform.GetChild(0);
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
            APR_Player.ResetPlayerPose();

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
                weaponRight.transform.SetParent(null);
                weaponRight.enabled = false;
                weaponRight = null;
                APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR_Player.PoseOn;
                APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR_Player.PoseOn;
                APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>().angularXDrive = APR_Player.PoseOn;
                APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>().angularYZDrive = APR_Player.PoseOn;
                APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>().targetRotation = APR_Player.UpperRightArmTarget;
                APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = APR_Player.LowerRightArmTarget;
            }
            else if (weaponLeft)
            {
                weaponLeft.transform.SetParent(null);
                weaponLeft.enabled = false;
                weaponLeft = null;
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
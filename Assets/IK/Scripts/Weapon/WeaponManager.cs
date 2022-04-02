using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public List<Weapon> nearWeapons = new List<Weapon>();
    [HideInInspector] public Weapon weaponLeft, weaponRight;
    public KeyCode keyPickUp = KeyCode.E;
    public KeyCode keyDrop = KeyCode.Q;
    ARP.APR.Scripts.APRController aprController;
    Transform handLeft, handRight;
    Player player;

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
            }
            else if (!weaponRight)
            {
                weaponRight = nearWeapons[0];
                weaponRight.isLeft = false;
                weaponRight.transform.SetParent(handRight);
            }

            nearWeapons[0].enabled = true;
            nearWeapons[0].player = player;
            if (nearWeapons[0] is Gun)
            {
                player.enabled = false;
                player.enabled = player.nearEnemies.Count > 0;
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

    void Drop(Weapon weapon)
    {
        if (!(weapon is Fist))
        {
            weapon.transform.SetParent(null);
            weapon.enabled = false;

            if (weapon == weaponRight)
            {
                weaponRight = null;
            }
            else if (weapon == weaponLeft)
            {
                weaponLeft = null;
            }
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
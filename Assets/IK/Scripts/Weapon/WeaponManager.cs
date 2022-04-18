using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    public Control pickUp = new Control()
    {
        key = KeyCode.E
    };

    public Control drop = new Control()
    {
        key = KeyCode.Q
    };

    public Control attackGun = new Control()
    {
        key = KeyCode.KeypadPlus
    };

    public Control attackMelee = new Control()
    {
        key = KeyCode.Keypad0
    };

    public Text ammoCountLeft, ammoCountRight;
    private ARP.APR.Scripts.APRController _aprController;
    private Transform _handLeft, _handRight;
    private List<Weapon> _nearWeapons = new List<Weapon>();
    private Player _player;

    private Weapon _weaponLeft, _weaponRight;

    public Weapon weaponLeft
    {
        get => _weaponLeft;
        private set
        {
            if (_weaponLeft != value)
            {
                if (value)
                {
                    if (drop.button)
                    {
                        drop.button.gameObject.SetActive(true);
                    }

                    if (value is Gun gun)
                    {
                        if (attackGun.button)
                        {
                            attackGun.button.transform.gameObject.SetActive(true);
                        }

                        if (ammoCountLeft)
                        {
                            ammoCountLeft.transform.parent.gameObject.SetActive(true);
                            ammoCountLeft.text = gun.ammo.ToString();
                        }
                    }
                    else
                    {
                        if (attackMelee.button)
                        {
                            attackMelee.button.transform.gameObject.SetActive(true);
                        }
                    }
                }
                else
                {
                    if (!weaponRight)
                    {
                        if (drop.button)
                        {
                            drop.button.gameObject.SetActive(false);
                        }
                    }

                    if (weaponLeft is Gun)
                    {
                        if (ammoCountLeft)
                        {
                            ammoCountLeft.transform.parent.gameObject.SetActive(false);
                        }

                        if (!(weaponRight is Gun))
                        {
                            if (attackGun.button)
                            {
                                attackGun.button.transform.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }

            _weaponLeft = value;
        }
    }

    public Weapon weaponRight
    {
        get => _weaponRight;
        private set
        {
            if (_weaponRight != value)
            {
                if (value)
                {
                    if (drop.button)
                    {
                        drop.button.gameObject.SetActive(true);
                    }

                    if (value is Gun gun)
                    {
                        if (attackGun.button)
                        {
                            attackGun.button.transform.gameObject.SetActive(true);
                        }

                        if (ammoCountRight)
                        {
                            ammoCountRight.transform.parent.gameObject.SetActive(true);
                            ammoCountRight.text = gun.ammo.ToString();
                        }
                    }
                    else
                    {
                        if (attackMelee.button)
                        {
                            attackMelee.button.transform.gameObject.SetActive(true);
                        }
                    }
                }
                else
                {
                    if (!weaponLeft)
                    {
                        if (drop.button)
                        {
                            drop.button.gameObject.SetActive(false);
                        }
                    }

                    if (weaponRight is Gun)
                    {
                        if (ammoCountRight)
                        {
                            ammoCountRight.transform.parent.gameObject.SetActive(false);
                        }

                        if (!(weaponLeft is Gun))
                        {
                            if (attackGun.button)
                            {
                                attackGun.button.transform.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }

            _weaponRight = value;
        }
    }

    private void Awake()
    {
        _aprController = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        _player = _aprController.Root.GetComponent<Player>();
        _handLeft = _aprController.LeftHand.transform.GetChild(0);
        _handRight = _aprController.RightHand.transform.GetChild(0);

        if (attackGun.button)
        {
            attackGun.button.onClick.AddListener(AttackWithGun);
            attackGun.button.gameObject.SetActive(false);
        }

        if (attackMelee.button)
        {
            attackMelee.button.onClick.AddListener(AttackWithMelee);
            attackMelee.button.gameObject.SetActive(false);
        }

        if (pickUp.button)
        {
            pickUp.button.onClick.AddListener(PickUp);
        }

        if (drop.button)
        {
            drop.button.onClick.AddListener(DropOneWeapon);
            drop.button.gameObject.SetActive(false);
        }

        Weapon hasWeaponOnLeft = _handLeft.GetComponentInChildren<Weapon>();
        if (hasWeaponOnLeft)
        {
            weaponLeft = hasWeaponOnLeft;
            weaponLeft.player = _player;
        }

        Weapon hasWeaponOnRight = _handRight.GetComponentInChildren<Weapon>();
        if (hasWeaponOnRight)
        {
            weaponRight = hasWeaponOnRight;
            weaponRight.player = _player;
        }

        this.enabled = _player != null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(attackGun.key))
        {
            AttackWithGun();
        }
        else if (Input.GetKeyDown(attackMelee.key))
        {
            AttackWithMelee();
        }
        else if (Input.GetKeyDown(pickUp.key))
        {
            PickUp();
        }
        else if (Input.GetKeyDown(drop.key))
        {
            DropOneWeapon();
        }
    }

    private void OnDestroy()
    {
        if (attackGun.button)
        {
            attackGun.button.onClick.RemoveListener(AttackWithGun);
        }

        if (attackMelee.button)
        {
            attackMelee.button.onClick.RemoveListener(AttackWithMelee);
        }

        if (pickUp.button)
        {
            pickUp.button.onClick.AddListener(PickUp);
        }

        if (drop.button)
        {
            drop.button.onClick.AddListener(DropOneWeapon);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            Weapon weapon = other.GetComponent<Weapon>();
            if (weapon)
            {
                if (!weapon.enabled)
                {
                    if (weapon is Gun gun)
                    {
                        if (gun.ammo <= 0)
                        {
                            return;
                        }
                    }

                    if (!_nearWeapons.Contains(weapon))
                    {
                        _nearWeapons.Add(weapon);
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
        {
            Weapon weapon = other.GetComponent<Weapon>();
            if (weapon)
            {
                _nearWeapons.Remove(weapon);
            }
        }
    }

    private void Drop(Weapon weapon)
    {
        switch (weapon)
        {
            case Fist:
            {
                return;
            }
            case Melee:
            {
                if (attackMelee.button)
                {
                    attackMelee.button.transform.gameObject.SetActive(false);
                }

                break;
            }
            case Gun gun:
            {
                gun.AmmoCountChanged -= OnAmmoCountChanged;
                gun.Relax();

                break;
            }
        }

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

    private void PickUp()
    {
        if (!weaponLeft || !weaponRight)
        {
            for (int i = 0; i < _nearWeapons.Count; i++)
            {
                if (_nearWeapons[i] is Melee)
                {
                    if (weaponLeft is Melee || weaponRight is Melee)
                    {
                        continue;
                    }
                }

                _aprController.ResetPlayerPose();

                if (!weaponLeft)
                {
                    weaponLeft = _nearWeapons[i];
                    weaponLeft.isLeft = true;
                    weaponLeft.transform.SetParent(_handLeft);
                }
                else if (!weaponRight)
                {
                    weaponRight = _nearWeapons[i];
                    weaponRight.isLeft = false;
                    weaponRight.transform.SetParent(_handRight);
                }

                _nearWeapons[i].enabled = true;
                _nearWeapons[i].player = _player;

                if (_nearWeapons[i] is Gun gun)
                {
                    if (_player.enabled)
                    {
                        gun.Strain();
                    }

                    gun.AmmoCountChanged += OnAmmoCountChanged;
                }

                _nearWeapons.Remove(_nearWeapons[i]);
                break;
            }
        }
    }

    private void DropOneWeapon()
    {
        if (weaponRight)
        {
            Drop(weaponRight);
            return;
        }

        if (weaponLeft)
        {
            Drop(weaponLeft);
        }
    }

    private void AttackWithGun()
    {
        if (weaponLeft is Gun)
        {
            weaponLeft.Attack();
        }

        if (weaponRight is Gun)
        {
            weaponRight.Attack();
        }
    }

    private void AttackWithMelee()
    {
        if (weaponLeft is Melee)
        {
            weaponLeft.Attack();
        }
        else if (weaponRight is Melee)
        {
            weaponRight.Attack();
        }
    }

    private void OnAmmoCountChanged(Gun gun, int count)
    {
        if (ammoCountLeft)
        {
            if (gun == weaponLeft)
            {
                ammoCountLeft.text = count.ToString();
            }
        }

        if (ammoCountRight)
        {
            if (gun == weaponRight)
            {
                ammoCountRight.text = count.ToString();
            }
        }

        if (count <= 0)
        {
            Drop(gun);
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

    [Serializable]
    public struct Control
    {
        public KeyCode key;
        public Button button;
    }
}
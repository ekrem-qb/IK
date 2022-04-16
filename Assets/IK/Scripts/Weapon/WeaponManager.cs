using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    public KeyCode keyPickUp = KeyCode.E;
    public KeyCode keyDrop = KeyCode.Q;
    public KeyCode keyAttackGun = KeyCode.KeypadPlus;
    public KeyCode keyAttackMelee = KeyCode.Keypad0;
    public Button buttonAttackGun;
    public Button buttonAttackMelee;
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
                    if (value is Gun gun)
                    {
                        if (buttonAttackGun)
                        {
                            buttonAttackGun.transform.gameObject.SetActive(true);
                        }

                        if (ammoCountLeft)
                        {
                            ammoCountLeft.transform.parent.gameObject.SetActive(true);
                            ammoCountLeft.text = gun.ammo.ToString();
                        }
                    }
                    else
                    {
                        if (buttonAttackMelee)
                        {
                            buttonAttackMelee.transform.gameObject.SetActive(true);
                        }

                        if (ammoCountLeft)
                        {
                            ammoCountLeft.transform.parent.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (weaponLeft is Melee)
                    {
                        if (buttonAttackMelee)
                        {
                            buttonAttackMelee.transform.gameObject.SetActive(false);
                        }
                    }
                    else if (weaponLeft is Gun)
                    {
                        if (!(weaponRight is Gun))
                        {
                            if (buttonAttackGun)
                            {
                                buttonAttackGun.transform.gameObject.SetActive(false);
                            }
                        }
                    }

                    if (ammoCountLeft)
                    {
                        ammoCountLeft.transform.parent.gameObject.SetActive(false);
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
                    if (value is Gun gun)
                    {
                        if (buttonAttackGun)
                        {
                            buttonAttackGun.transform.gameObject.SetActive(true);
                        }

                        if (ammoCountRight)
                        {
                            ammoCountRight.transform.parent.gameObject.SetActive(true);
                            ammoCountRight.text = gun.ammo.ToString();
                        }
                    }
                    else
                    {
                        if (buttonAttackMelee)
                        {
                            buttonAttackMelee.transform.gameObject.SetActive(true);
                        }

                        if (ammoCountRight)
                        {
                            ammoCountRight.transform.parent.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (weaponRight is Melee)
                    {
                        if (buttonAttackMelee)
                        {
                            buttonAttackMelee.transform.gameObject.SetActive(false);
                        }
                    }
                    else if (weaponRight is Gun)
                    {
                        if (!(weaponLeft is Gun))
                        {
                            if (buttonAttackGun)
                            {
                                buttonAttackGun.transform.gameObject.SetActive(false);
                            }
                        }
                    }

                    if (ammoCountRight)
                    {
                        ammoCountRight.transform.parent.gameObject.SetActive(false);
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

        if (buttonAttackGun)
        {
            buttonAttackGun.onClick.AddListener(AttackWithGun);
        }

        if (buttonAttackMelee)
        {
            buttonAttackMelee.onClick.AddListener(AttackWithMelee);
        }

        this.enabled = _player != null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(keyAttackGun))
        {
            AttackWithGun();
        }
        else if (Input.GetKeyDown(keyAttackMelee))
        {
            AttackWithMelee();
        }
        else if (Input.GetKeyDown(keyPickUp) && (!weaponLeft || !weaponRight) && _nearWeapons.Count > 0)
        {
            if (_nearWeapons[0] is Melee)
            {
                if (weaponLeft is Melee || weaponRight is Melee)
                {
                    return;
                }
            }

            _aprController.ResetPlayerPose();

            if (!weaponLeft)
            {
                weaponLeft = _nearWeapons[0];
                weaponLeft.isLeft = true;
                weaponLeft.transform.SetParent(_handLeft);
            }
            else if (!weaponRight)
            {
                weaponRight = _nearWeapons[0];
                weaponRight.isLeft = false;
                weaponRight.transform.SetParent(_handRight);
            }

            _nearWeapons[0].enabled = true;
            _nearWeapons[0].player = _player;
            if (_nearWeapons[0] is Gun gun)
            {
                _player.enabled = false;
                _player.enabled = _player.nearEnemies.Count > 0;
                gun.AmmoCountChanged += OnAmmoCountChanged;
            }

            _nearWeapons.Remove(_nearWeapons[0]);
        }
        else if (Input.GetKeyDown(keyDrop) && (weaponLeft || weaponRight))
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

    private void OnDestroy()
    {
        if (buttonAttackGun)
        {
            buttonAttackGun.onClick.RemoveListener(AttackWithGun);
        }

        if (buttonAttackMelee)
        {
            buttonAttackMelee.onClick.RemoveListener(AttackWithMelee);
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
                    if (weapon is Gun && (weapon as Gun).ammo <= 0)
                    {
                        return;
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

    private void Drop(Weapon weapon)
    {
        if (!(weapon is Fist))
        {
            weapon.transform.SetParent(null);
            weapon.enabled = false;
            if (weapon is Gun gun)
            {
                gun.AmmoCountChanged -= OnAmmoCountChanged;
            }

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
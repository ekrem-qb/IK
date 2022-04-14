using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    public KeyCode keyPickUp = KeyCode.E;
    public KeyCode keyDrop = KeyCode.Q;
    public KeyCode keyAttackLeft = KeyCode.Keypad1;
    public KeyCode keyAttackRight = KeyCode.Keypad3;
    public Button buttonAttackLeft, buttonAttackRight;
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
                    if (buttonAttackLeft)
                    {
                        buttonAttackLeft.transform.gameObject.SetActive(true);
                    }

                    if (value is Gun)
                    {
                        if (ammoCountLeft)
                        {
                            ammoCountLeft.transform.parent.gameObject.SetActive(true);
                            ammoCountLeft.text = (value as Gun).ammo.ToString();
                        }
                    }
                    else
                    {
                        if (ammoCountLeft)
                        {
                            ammoCountLeft.transform.parent.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (buttonAttackLeft)
                    {
                        buttonAttackLeft.transform.gameObject.SetActive(false);
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
                    if (buttonAttackRight)
                    {
                        buttonAttackRight.transform.gameObject.SetActive(true);
                    }

                    if (value is Gun)
                    {
                        if (ammoCountRight)
                        {
                            ammoCountRight.transform.parent.gameObject.SetActive(true);
                            ammoCountRight.text = (value as Gun).ammo.ToString();
                        }
                    }
                    else
                    {
                        if (ammoCountRight)
                        {
                            ammoCountRight.transform.parent.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    if (buttonAttackRight)
                    {
                        buttonAttackRight.transform.gameObject.SetActive(false);
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

        if (buttonAttackLeft)
        {
            buttonAttackLeft.onClick.AddListener(AttackLeft);
        }

        if (buttonAttackRight)
        {
            buttonAttackRight.onClick.AddListener(AttackRight);
        }

        this.enabled = _player != null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(keyAttackLeft))
        {
            AttackLeft();
        }
        else if (Input.GetKeyDown(keyAttackRight))
        {
            AttackRight();
        }
        else if (Input.GetKeyDown(keyPickUp) && (!weaponLeft || !weaponRight) && _nearWeapons.Count > 0)
        {
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
            if (_nearWeapons[0] is Gun)
            {
                _player.enabled = false;
                _player.enabled = _player.nearEnemies.Count > 0;
                (_nearWeapons[0] as Gun).AmmoCountChanged += OnAmmoCountChanged;
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
        if (buttonAttackLeft)
        {
            buttonAttackLeft.onClick.RemoveListener(AttackLeft);
        }

        if (buttonAttackRight)
        {
            buttonAttackRight.onClick.RemoveListener(AttackRight);
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

    private void AttackLeft()
    {
        if (weaponLeft)
        {
            weaponLeft.Attack();
        }
    }

    private void AttackRight()
    {
        if (weaponRight)
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
            if (weapon is Gun)
            {
                (weapon as Gun).AmmoCountChanged -= OnAmmoCountChanged;
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
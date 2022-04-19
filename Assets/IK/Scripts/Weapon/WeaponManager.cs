using System;
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

    public Control attack = new Control()
    {
        key = KeyCode.Keypad0
    };

    public Text ammoCount;
    private ARP.APR.Scripts.APRController _aprController;
    private Transform _handRight;
    private ObservableList<Weapon> _nearWeapons = new ObservableList<Weapon>();
    private Player _player;
    private Weapon _weapon;

    public Weapon weapon
    {
        get => _weapon;
        private set
        {
            if (_weapon != value)
            {
                if (value)
                {
                    value.isLeft = false;
                    value.transform.SetParent(_handRight);
                    value.enabled = true;
                    value.player = _player;

                    if (drop.button)
                    {
                        drop.button.gameObject.SetActive(true);
                    }

                    if (attack.button)
                    {
                        attack.button.transform.gameObject.SetActive(true);
                        attack.button.image.sprite = value.icon;
                    }

                    if (value is Gun gun)
                    {
                        if (ammoCount)
                        {
                            ammoCount.transform.parent.gameObject.SetActive(true);
                            ammoCount.text = gun.ammo.ToString();
                        }

                        if (_player.enabled)
                        {
                            gun.Strain();
                        }

                        gun.AmmoCountChanged += OnAmmoCountChanged;
                    }
                }
            }

            _weapon = value;
        }
    }

    private void Awake()
    {
        _aprController = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        _player = _aprController.Root.GetComponent<Player>();
        _handRight = _aprController.RightHand.transform.GetChild(0);

        if (attack.button)
        {
            attack.button.onClick.AddListener(Attack);
            attack.button.gameObject.SetActive(false);
        }

        if (pickUp.button)
        {
            pickUp.button.onClick.AddListener(PickUp);
            pickUp.button.gameObject.SetActive(false);
        }

        if (drop.button)
        {
            drop.button.onClick.AddListener(Drop);
            drop.button.gameObject.SetActive(false);
        }

        Weapon hasWeapon = _handRight.GetComponentInChildren<Weapon>();
        if (hasWeapon)
        {
            weapon = hasWeapon;
            weapon.player = _player;
        }

        _nearWeapons.CountChanged += OnNearWeaponsCountChanged;

        this.enabled = _player != null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(attack.key))
        {
            Attack();
        }
        else if (Input.GetKeyDown(pickUp.key))
        {
            PickUp();
        }
        else if (Input.GetKeyDown(drop.key))
        {
            Drop();
        }
    }

    private void OnDestroy()
    {
        if (attack.button)
        {
            attack.button.onClick.RemoveListener(Attack);
        }

        if (pickUp.button)
        {
            pickUp.button.onClick.AddListener(PickUp);
        }

        if (drop.button)
        {
            drop.button.onClick.AddListener(Drop);
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

    private void OnNearWeaponsCountChanged(int count)
    {
        if (pickUp.button)
        {
            pickUp.button.gameObject.SetActive(count > 0 && !weapon);
        }
    }

    public void Drop()
    {
        switch (weapon)
        {
            case null:
            {
                return;
            }
            case Fist:
            {
                return;
            }
            case Gun gun:
            {
                gun.AmmoCountChanged -= OnAmmoCountChanged;
                gun.Relax();

                if (ammoCount)
                {
                    ammoCount.transform.parent.gameObject.SetActive(false);
                }

                break;
            }
        }

        if (drop.button)
        {
            drop.button.gameObject.SetActive(false);
        }

        if (attack.button)
        {
            attack.button.transform.gameObject.SetActive(false);
        }

        weapon.transform.SetParent(null);
        weapon.enabled = false;

        weapon = null;
    }

    private void PickUp()
    {
        if (_nearWeapons.Count > 0 && !weapon)
        {
            _aprController.ResetPlayerPose();

            weapon = _nearWeapons[0];

            _nearWeapons.Remove(_nearWeapons[0]);
        }
    }

    private void Attack()
    {
        if (weapon)
        {
            weapon.Attack();
        }
    }

    private void OnAmmoCountChanged(Gun gun, int count)
    {
        if (ammoCount)
        {
            if (gun == weapon)
            {
                ammoCount.text = count.ToString();
            }
        }

        if (count <= 0)
        {
            Drop();
        }
    }

    [Serializable]
    public struct Control
    {
        public KeyCode key;
        public Button button;
    }
}
using System;
using ARP.APR.Scripts;
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
	private APRController _aprController;
	private Transform _handRight;
	private ObservableList<Weapon> _nearWeapons = new ObservableList<Weapon>();
	private Player _player;
	private Weapon _weapon;

	public Action WeaponChanged = () => { };

	public Weapon weapon
	{
		get => _weapon;
		private set
		{
			if (_weapon != value)
			{
				if (value)
				{
					value.PickUp(_aprController);

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
							_aprController.StrainArms(APRController.Arms.Both);
						}

						gun.AmmoCountChanged += OnAmmoCountChanged;
					}
				}

				_weapon = value;
				WeaponChanged();
			}
		}
	}

	private void Awake()
	{
		_aprController = this.transform.root.GetComponent<APRController>();
		_player = _aprController.Root.GetComponent<Player>();
		_handRight = _aprController.RightHand.transform.GetChild(0);

		if (attack.button)
		{
			attack.button.onClick.AddListener(Attack);
			attack.button.gameObject.SetActive(false);
		}

		if (pickUp.button)
		{
			pickUp.button.onClick.AddListener(PickUpNearestWeapon);
			pickUp.button.gameObject.SetActive(false);
		}

		if (drop.button)
		{
			drop.button.onClick.AddListener(DropWeapon);
			drop.button.gameObject.SetActive(false);
		}

		Weapon hasWeapon = _handRight.GetComponentInChildren<Weapon>();
		if (hasWeapon)
		{
			weapon = hasWeapon;
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
			PickUpNearestWeapon();
		}
		else if (Input.GetKeyDown(drop.key))
		{
			DropWeapon();
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
			pickUp.button.onClick.AddListener(PickUpNearestWeapon);
		}

		if (drop.button)
		{
			drop.button.onClick.AddListener(DropWeapon);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.isTrigger)
		{
			if (other.transform.root != this.transform.root)
			{
				Weapon nearWeapon = other.GetComponent<Weapon>();
				if (nearWeapon)
				{
					if (!nearWeapon.isPickedUp)
					{
						if (nearWeapon is Gun gun)
						{
							if (gun.ammo <= 0)
							{
								return;
							}
						}

						if (!_nearWeapons.Contains(nearWeapon))
						{
							_nearWeapons.Add(nearWeapon);
						}
					}
				}
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.isTrigger)
		{
			Weapon nearWeapon = other.GetComponent<Weapon>();
			if (nearWeapon)
			{
				_nearWeapons.Remove(nearWeapon);
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

	public void DropWeapon()
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
				_aprController.RelaxArms(APRController.Arms.Both);

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

		weapon.Drop();

		weapon = null;
	}

	private void PickUpNearestWeapon()
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
			DropWeapon();
		}
	}

	[Serializable]
	public struct Control
	{
		public KeyCode key;
		public Button button;
	}
}
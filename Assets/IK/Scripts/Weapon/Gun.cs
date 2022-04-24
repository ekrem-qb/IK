using System;
using System.Collections;
using ARP.APR.Scripts;
using UnityEngine;

public class Gun : Weapon
{
	[Header("Gun")] public GameObject bulletPrefab;

	public float recoilForce = 50;
	[SerializeField] private int _ammo = 10;
	public WeaponPart left, right;
	private BoxCollider _commonCollider;

	public Action<Gun, int> AmmoCountChanged = (gun, i) => { };

	public int ammo
	{
		get => _ammo;
		set
		{
			if (_ammo != value)
			{
				AmmoCountChanged(this, value);
			}

			_ammo = value;
		}
	}

	protected override void Awake()
	{
		_commonCollider = this.GetComponent<BoxCollider>();
		left.particle = left.transform.GetComponentInChildren<ParticleSystem>();
		right.particle = right.transform.GetComponentInChildren<ParticleSystem>();
		base.Awake();
	}

	public override void PickUp(APRController newAprController)
	{
		_commonCollider.enabled = false;

		left.hand = newAprController.handLeft;

		left.transform.SetParent(left.hand.transform.GetChild(0));

		right.hand = newAprController.handRight;

		right.transform.SetParent(right.hand.transform.GetChild(0));

		base.PickUp(newAprController);
	}

	public override void Drop()
	{
		base.Drop();
		this.transform.position = Vector3.Slerp(left.transform.position, right.transform.position, 0.5f);

		_commonCollider.enabled = true;

		left.transform.SetParent(this.transform);
		right.transform.SetParent(this.transform);
		StartCoroutine(TransitionToDrop());
	}

	private IEnumerator TransitionToDrop()
	{
		while (Vector3.Distance(left.transform.localPosition, left.dropPosition) > 0.05f || Quaternion.Angle(left.transform.localRotation, left.dropRotation) > 0.05f)
		{
			left.transform.localPosition = Vector3.Lerp(left.transform.localPosition, left.dropPosition, Time.deltaTime * transitionSpeed);
			left.transform.localRotation = Quaternion.Lerp(left.transform.localRotation, left.dropRotation, Time.deltaTime * transitionSpeed);
			yield return null;
		}

		while (Vector3.Distance(right.transform.localPosition, right.dropPosition) > 0.05f || Quaternion.Angle(right.transform.localRotation, right.dropRotation) > 0.05f)
		{
			right.transform.localPosition = Vector3.Lerp(right.transform.localPosition, right.dropPosition, Time.deltaTime * transitionSpeed);
			right.transform.localRotation = Quaternion.Lerp(right.transform.localRotation, right.dropRotation, Time.deltaTime * transitionSpeed);
			yield return null;
		}
	}

	protected override IEnumerator TransitionToHold(Vector3 targetPosition, Quaternion targetRotation)
	{
		Vector3 targetPositionLeft = new Vector3(-targetPosition.x, targetPosition.y, targetPosition.z);
		Quaternion targetRotationLeft = new Quaternion(targetRotation.x, -targetRotation.y, -targetRotation.z, targetRotation.w);

		while (Vector3.Distance(left.transform.localPosition, targetPositionLeft) > 0.05f || Quaternion.Angle(left.transform.localRotation, targetRotationLeft) > 0.05f)
		{
			left.transform.localPosition = Vector3.Lerp(left.transform.localPosition, targetPositionLeft, Time.deltaTime * transitionSpeed);
			left.transform.localRotation = Quaternion.Lerp(left.transform.localRotation, targetRotationLeft, Time.deltaTime * transitionSpeed);
			yield return null;
		}

		while (Vector3.Distance(right.transform.localPosition, targetPosition) > 0.05f || Quaternion.Angle(right.transform.localRotation, targetRotation) > 0.05f)
		{
			right.transform.localPosition = Vector3.Lerp(right.transform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
			right.transform.localRotation = Quaternion.Lerp(right.transform.localRotation, targetRotation, Time.deltaTime * transitionSpeed);
			yield return null;
		}
	}

	public override void Attack()
	{
		if (ammo > 0)
		{
			left.hand.rigidbody.AddForce(-left.transform.forward * recoilForce, ForceMode.Impulse);
			Projectile bulletLeft = Instantiate(bulletPrefab, left.transform.position, left.transform.rotation).GetComponent<Projectile>();
			bulletLeft.owner = left.transform.root;
			if (left.particle)
			{
				left.particle.Play();
			}

			right.hand.rigidbody.AddForce(-right.transform.forward * recoilForce, ForceMode.Impulse);
			Projectile bulletRight = Instantiate(bulletPrefab, right.transform.position, right.transform.rotation).GetComponent<Projectile>();
			bulletRight.owner = right.transform.root;
			if (right.particle)
			{
				right.particle.Play();
			}

			ammo--;
		}
	}

	[Serializable]
	public struct WeaponPart
	{
		public Transform transform;
		public Vector3 dropPosition;
		public Quaternion dropRotation;
		[HideInInspector] public ParticleSystem particle;
		public BodyPart hand;
	}
}
using System.Collections;
using ARP.APR.Scripts;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
	[Header("Weapon")] public Sprite icon;
	public Vector3 holdPosition;
	public Quaternion holdRotation;
	public float transitionSpeed = 15;
	public float damage = 10;
	[HideInInspector] public bool isPickedUp;
	private SphereCollider _pickupTrigger;
	private Rigidbody _rigidbody;
	protected APRController aprController;
	protected BodyPart arm, armLow, hand;
	protected ParticleSystem particle;

	protected virtual void Awake()
	{
		_pickupTrigger = this.GetComponent<SphereCollider>();
		_rigidbody = this.GetComponent<Rigidbody>();
		particle = this.GetComponentInChildren<ParticleSystem>();
		if (this.transform.parent)
		{
			PickUp(this.transform.root.GetComponent<APRController>());
		}
		else
		{
			Drop();
		}
	}

	private void OnTransformParentChanged()
	{
		if (this.transform.parent)
		{
			PickUp(this.transform.root.GetComponent<APRController>());
		}
		else
		{
			Drop();
		}
	}

	public virtual void PickUp(APRController newAprController)
	{
		aprController = newAprController;
		arm = newAprController.armRight;
		armLow = newAprController.armRightLow;
		hand = newAprController.handRight;

		this.transform.SetParent(aprController.handRight.transform.GetChild(0));

		if (_pickupTrigger)
		{
			_pickupTrigger.enabled = false;
		}

		if (_rigidbody)
		{
			Destroy(_rigidbody);
		}

		if (!(this is Fist))
		{
			StartCoroutine(TransitionToHold(new Vector3(-holdPosition.x, holdPosition.y, holdPosition.z), new Quaternion(holdRotation.x, -holdRotation.y, -holdRotation.z, holdRotation.w)));
		}

		isPickedUp = true;
	}

	public virtual void Drop()
	{
		this.transform.SetParent(null);
		if (_pickupTrigger)
		{
			_pickupTrigger.enabled = true;
		}

		if (!_rigidbody)
		{
			_rigidbody = this.gameObject.AddComponent<Rigidbody>();
		}

		isPickedUp = false;
	}

	public abstract void Attack();

	protected virtual IEnumerator TransitionToHold(Vector3 targetPosition, Quaternion targetRotation)
	{
		while (Vector3.Distance(this.transform.localPosition, targetPosition) > 0.05f || Quaternion.Angle(this.transform.localRotation, targetRotation) > 0.05f)
		{
			this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
			this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, targetRotation, Time.deltaTime * transitionSpeed);
			yield return null;
		}
	}
}
using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
	[HideInInspector] public bool isPickedUp;
	public Sprite icon;
	public Vector3 holdPosition;
	public Quaternion holdRotation;
	public float transitionSpeed = 15;
	public float damage = 10;
	private SphereCollider _pickupTrigger;
	private Rigidbody _rigidbody;
	protected ARP.APR.Scripts.APRController aprController;
	protected BodyPart arm, armLow, hand;
	protected ParticleSystem particle;

	protected virtual void Awake()
	{
		_pickupTrigger = this.GetComponent<SphereCollider>();
		_rigidbody = this.GetComponent<Rigidbody>();
		particle = this.GetComponentInChildren<ParticleSystem>();
		if (this.transform.parent)
		{
			PickUp(this.transform.root.GetComponent<ARP.APR.Scripts.APRController>());
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
			PickUp(this.transform.root.GetComponent<ARP.APR.Scripts.APRController>());
		}
		else
		{
			Drop();
		}
	}

	public void PickUp(ARP.APR.Scripts.APRController newAprController)
	{
		aprController = newAprController;
		arm.joint = aprController.UpperRightArm.GetComponent<ConfigurableJoint>();
		arm.rigidbody = aprController.UpperRightArm.GetComponent<Rigidbody>();
		arm.originalRotation = aprController.upperRightArmTarget;
		armLow.joint = aprController.LowerRightArm.GetComponent<ConfigurableJoint>();
		armLow.rigidbody = aprController.LowerRightArm.GetComponent<Rigidbody>();
		armLow.originalRotation = aprController.lowerRightArmTarget;
		hand.joint = aprController.RightHand.GetComponent<ConfigurableJoint>();
		hand.rigidbody = aprController.RightHand.GetComponent<Rigidbody>();
		hand.originalRotation = aprController.rightHandTarget;

		this.transform.SetParent(hand.rigidbody.transform.GetChild(0));

		if (_pickupTrigger)
		{
			_pickupTrigger.enabled = false;
		}

		Destroy(_rigidbody);
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

	private IEnumerator TransitionToHold(Vector3 targetPosition, Quaternion targetRotation)
	{
		while (Vector3.Distance(this.transform.localPosition, targetPosition) > 0.05f || Quaternion.Angle(this.transform.localRotation, targetRotation) > 0.05f)
		{
			this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
			this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, targetRotation, Time.deltaTime * transitionSpeed);
			yield return null;
		}
	}

	protected struct BodyPart
	{
		public ConfigurableJoint joint;
		public Rigidbody rigidbody;
		public Quaternion originalRotation;
	}
}
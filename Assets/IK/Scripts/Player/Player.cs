using ARP.APR.Scripts;
using UnityEngine;

public class Player : MonoBehaviour
{
	public readonly ObservableList<Target> nearTargets = new ObservableList<Target>();
	private APRController _aprController;

	// Trigger for detecting near targets and enemies
	private SphereCollider _trigger;
	private WeaponManager _weaponManager;

	private void Awake()
	{
		_aprController = this.transform.root.GetComponent<APRController>();
		if (!_aprController.root.transform)
		{
			_aprController.PlayerSetup();
		}

		_weaponManager = _aprController.COMP.GetComponent<WeaponManager>();
		_trigger = this.GetComponent<SphereCollider>();
		nearTargets.CountChanged += count => this.enabled = count > 0;
	}

	private void FixedUpdate()
	{
		if (_aprController.useControls)
		{
			if (_weaponManager.weapon is Gun)
			{
				// Aiming guns towards enemies

				Vector3 bodyBendingFactor = new Vector3(_aprController.body.transform.eulerAngles.x, _aprController.root.transform.localEulerAngles.y, 0);

				nearTargets.SortByDistanceTo(_aprController.armLeft.transform.position);
				Target nearestToArmLeft = nearTargets[0];

				Debug.DrawLine(nearestToArmLeft.selfTarget.position, _aprController.armLeft.transform.position, Color.red);

				Quaternion targetRotationLeft = Quaternion.LookRotation(nearestToArmLeft.selfTarget.position - _aprController.armLeft.transform.position);
				Vector3 anglesLeft = targetRotationLeft.eulerAngles;
				anglesLeft.y -= 270;
				// Hands bending fix when aiming backwards relative to Player
				if (Mathf.Abs(Quaternion.Dot(Quaternion.LookRotation(_aprController.root.transform.forward), targetRotationLeft)) < 0.5)
				{
					anglesLeft.x *= -1;
				}

				anglesLeft -= bodyBendingFactor;
				_aprController.armLeft.joint.targetRotation = Quaternion.Euler(anglesLeft);


				nearTargets.SortByDistanceTo(_aprController.armRight.transform.position);
				Target nearestToArmRight = nearTargets[0];

				if (nearTargets.Count > 1)
				{
					if (nearestToArmRight == nearestToArmLeft)
					{
						nearestToArmRight = nearTargets[1];
					}
				}

				Debug.DrawLine(nearestToArmRight.selfTarget.position, _aprController.armRight.transform.position, Color.yellow);

				Quaternion targetRotationRight = Quaternion.LookRotation(nearestToArmRight.selfTarget.position - _aprController.armRight.transform.position);
				Vector3 anglesRight = targetRotationRight.eulerAngles;
				anglesRight.y -= 90;
				// Hands bending fix when aiming backwards relative to Player
				if (Mathf.Abs(Quaternion.Dot(Quaternion.LookRotation(_aprController.root.transform.forward), targetRotationRight)) < 0.5)
				{
					anglesRight.x *= -1;
				}

				anglesRight -= bodyBendingFactor;
				_aprController.armRight.joint.targetRotation = Quaternion.Euler(anglesRight);
			}
		}
	}

	private void OnEnable()
	{
		if (_weaponManager.weapon is Gun)
		{
			_aprController.StrainArms(APRController.Arms.Both);
		}
	}

	private void OnDisable()
	{
		if (_weaponManager.weapon is Gun)
		{
			_aprController.RelaxArms(APRController.Arms.Both);
		}
	}

	private void OnDestroy()
	{
		nearTargets.ForEach(target => target.player = null);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.isTrigger)
		{
			if (other.TryGetComponent(out Target target))
			{
				if (!nearTargets.Contains(target))
				{
					// Setting self as a near Player for Target
					target.player = this;
					nearTargets.Add(target);
				}
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.isTrigger)
		{
			if (_trigger.radius <= Vector3.Distance(this.transform.position, other.transform.position))
			{
				if (other.TryGetComponent(out Target target))
				{
					// Setting null as a near Player for Target, when going far away from it
					target.player = null;
					nearTargets.Remove(target);
				}
			}
		}
	}
}
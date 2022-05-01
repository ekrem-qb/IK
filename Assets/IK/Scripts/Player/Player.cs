using ARP.APR.Scripts;
using UnityEngine;

public class Player : MonoBehaviour
{
	public readonly ObservableList<Target> nearTargets = new ObservableList<Target>();
	private APRController _aprController;
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
				Vector3 bodyBendingFactor = new Vector3(_aprController.body.transform.eulerAngles.x, _aprController.root.transform.localEulerAngles.y, 0);

				nearTargets.SortByDistanceTo(_aprController.armLeft.transform.position);
				Target nearestToArmLeft = nearTargets[0];

				Debug.DrawLine(nearestToArmLeft.selfTarget.position, _aprController.armLeft.transform.position, Color.red);

				Vector3 anglesLeft = Quaternion.LookRotation(nearestToArmLeft.selfTarget.position - _aprController.armLeft.transform.position).eulerAngles;
				anglesLeft -= bodyBendingFactor;
				_aprController.armLeft.joint.targetRotation = Quaternion.Euler(anglesLeft.x, anglesLeft.y - 270, anglesLeft.z);

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

				Vector3 angles = Quaternion.LookRotation(nearestToArmRight.selfTarget.position - _aprController.armRight.transform.position).eulerAngles;
				angles -= bodyBendingFactor;
				_aprController.armRight.joint.targetRotation = Quaternion.Euler(angles.x, angles.y - 90, angles.z);
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
					target.player = null;
					nearTargets.Remove(target);
				}
			}
		}
	}
}
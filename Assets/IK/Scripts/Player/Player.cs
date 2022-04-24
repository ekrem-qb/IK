using ARP.APR.Scripts;
using UnityEngine;

public class Player : MonoBehaviour
{
	private APRController _aprController;
	private SphereCollider _trigger;
	private WeaponManager _weaponManager;
	public ObservableList<Enemy> nearEnemies = new ObservableList<Enemy>();

	private void Awake()
	{
		_aprController = this.transform.root.GetComponent<APRController>();
		_weaponManager = _aprController.COMP.GetComponent<WeaponManager>();
		_trigger = this.GetComponent<SphereCollider>();
		nearEnemies.CountChanged += count => this.enabled = count > 0;
	}

	private void FixedUpdate()
	{
		if (_aprController.useControls)
		{
			if (_weaponManager.weapon is Gun)
			{
				Vector3 bodyBendingFactor = new Vector3(_aprController.body.transform.eulerAngles.x, _aprController.root.transform.localEulerAngles.y, 0);

				nearEnemies.SortByDistanceTo(_aprController.armLeft.transform.position);
				Enemy nearestToArmLeft = nearEnemies[0];

				Debug.DrawLine(nearestToArmLeft.selfTarget.position, _aprController.armLeft.transform.position, Color.red);

				Vector3 anglesLeft = Quaternion.LookRotation(nearestToArmLeft.selfTarget.position - _aprController.armLeft.transform.position).eulerAngles;
				anglesLeft -= bodyBendingFactor;
				_aprController.armLeft.joint.targetRotation = Quaternion.Euler(anglesLeft.x, anglesLeft.y - 270, anglesLeft.z);

				nearEnemies.SortByDistanceTo(_aprController.armLeft.transform.position);
				Enemy nearestToArmRight = nearEnemies[0];

				if (nearEnemies.Count > 1)
				{
					if (nearestToArmRight == nearestToArmLeft)
					{
						nearestToArmRight = nearEnemies[1];
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
		nearEnemies.ForEach(enemy => enemy.player = null);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.isTrigger)
		{
			Enemy enemy = other.GetComponent<Enemy>();
			if (enemy)
			{
				if (!nearEnemies.Contains(enemy))
				{
					enemy.player = this;
					nearEnemies.Add(enemy);
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
				Enemy enemy = other.GetComponent<Enemy>();
				if (enemy)
				{
					enemy.player = null;
					nearEnemies.Remove(enemy);
				}
			}
		}
	}
}
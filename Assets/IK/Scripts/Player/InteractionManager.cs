using System.Collections;
using ARP.APR.Scripts;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
	public InputControl interact = new InputControl()
	{
		key = KeyCode.F
	};

	public float pressingDelay = 1;
	[ReadOnly] [SerializeField] private Transform _currentTarget;
	[ReadOnly] public bool canInteract;
	private APRController _aprController;
	private ConfigurableJoint _armLeft, _armRight;
	private WeaponManager _weaponManager;

	public Transform currentTarget
	{
		get => _currentTarget;
		set
		{
			if (_currentTarget != value)
			{
				_currentTarget = value;
				CheckInteractionAvailability();
			}
		}
	}

	private void Awake()
	{
		_aprController = this.transform.root.GetComponent<APRController>();
		_armLeft = _aprController.UpperLeftArm.GetComponent<ConfigurableJoint>();
		_weaponManager = _aprController.COMP.GetComponent<WeaponManager>();
		_weaponManager.WeaponChanged += CheckInteractionAvailability;
		CheckInteractionAvailability();
		interact.button.onClick.AddListener(Interact);
	}

	private void Update()
	{
		if (Input.GetKeyDown(interact.key))
		{
			Interact();
		}
	}

	private void OnDestroy()
	{
		_weaponManager.WeaponChanged += CheckInteractionAvailability;
		interact.button.onClick.RemoveListener(Interact);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.root != this.transform.root)
		{
			if (other.GetComponent<RedButton>())
			{
				currentTarget = other.transform;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.transform == currentTarget)
		{
			currentTarget = null;
		}
	}

	private void CheckInteractionAvailability()
	{
		if (currentTarget && _aprController.isBalanced && !_aprController.isGrabbing && !_aprController.isGrabbed && (!_weaponManager.weapon || (currentTarget.GetComponent<RedButton>() && _weaponManager.weapon is Melee)))
		{
			canInteract = true;
		}
		else
		{
			canInteract = false;
		}

		if (interact.button)
		{
			interact.button.gameObject.SetActive(canInteract);
		}
	}

	private IEnumerator PressButton(RedButton button)
	{
		_aprController.StrainArms(APRController.Arms.Left);

		Vector3 bodyBendingFactor = new Vector3(_aprController.Body.transform.eulerAngles.x, _aprController.Root.transform.localEulerAngles.y, 0);

		Vector3 anglesLeft = Quaternion.LookRotation(_currentTarget.transform.position - _armLeft.transform.position).eulerAngles;
		anglesLeft -= bodyBendingFactor;
		_armLeft.targetRotation = Quaternion.Euler(anglesLeft.x, anglesLeft.y - 270, anglesLeft.z);

		yield return new WaitForSeconds(pressingDelay);

		button.isOn = !button.isOn;

		_aprController.RelaxArms(APRController.Arms.Left);
	}

	private void Interact()
	{
		if (canInteract)
		{
			if (_currentTarget)
			{
				RedButton button = _currentTarget.GetComponent<RedButton>();
				if (button)
				{
					StopCoroutine(PressButton(button));
					StartCoroutine(PressButton(button));
				}
			}
			else
			{
				canInteract = false;
				CheckInteractionAvailability();
			}
		}
	}
}
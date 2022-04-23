using System.Collections;
using ARP.APR.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
	public KeyCode interactKey = KeyCode.F;
	public Button interactButton;
	public float pressingDelay = 1;
	[ReadOnly] public Transform currentTarget;
	[ReadOnly] public bool canInteract;
	private APRController _aprController;
	private ConfigurableJoint _armLeft, _armRight;
	private WeaponManager _weaponManager;

	private Transform _currentTarget
	{
		get => currentTarget;
		set
		{
			if (currentTarget != value)
			{
				currentTarget = value;
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
		interactButton.onClick.AddListener(Interact);
	}

	private void Update()
	{
		if (Input.GetKeyDown(interactKey))
		{
			Interact();
		}
	}

	private void OnDestroy()
	{
		_weaponManager.WeaponChanged += CheckInteractionAvailability;
		interactButton.onClick.RemoveListener(Interact);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.transform.root != this.transform.root)
		{
			if (other.GetComponent<RedButton>() || (!other.isTrigger && other.CompareTag("CanBeGrabbed")))
			{
				_currentTarget = other.transform;
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.transform == _currentTarget)
		{
			_currentTarget = null;
		}
	}

	private void CheckInteractionAvailability()
	{
		if (_aprController.IsGrabbing)
		{
			canInteract = true;
		}
		else if (_currentTarget && _aprController.isBalanced && (!_weaponManager.weapon || (_currentTarget.GetComponent<RedButton>() && _weaponManager.weapon is Melee)))
		{
			canInteract = true;
		}
		else
		{
			canInteract = false;
		}

		if (interactButton)
		{
			interactButton.gameObject.SetActive(canInteract);
		}
	}

	private IEnumerator PressButton(RedButton button)
	{
		_aprController.StrainArms(APRController.Arms.Left);

		Vector3 bodyBendingFactor = new Vector3(_aprController.Body.transform.eulerAngles.x, _aprController.Root.transform.localEulerAngles.y, 0);

		Vector3 anglesLeft = Quaternion.LookRotation(currentTarget.transform.position - _armLeft.transform.position).eulerAngles;
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
			if (currentTarget)
			{
				if (currentTarget.CompareTag("CanBeGrabbed"))
				{
					_aprController.IsGrabbing = !_aprController.IsGrabbing;
				}
				else
				{
					RedButton button = currentTarget.GetComponent<RedButton>();
					if (button)
					{
						StopCoroutine(PressButton(button));
						StartCoroutine(PressButton(button));
					}
				}
			}
			else
			{
				_aprController.IsGrabbing = !_aprController.IsGrabbing;
				canInteract = false;
				CheckInteractionAvailability();
			}
		}
	}
}
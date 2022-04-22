using System.Collections;
using ARP.APR.Scripts;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
	public KeyCode interactKey = KeyCode.F;
	public Button interactButton;
	public float pressingDelay = 1;
	[ReadOnly] public Toggler currentToggler;
	[ReadOnly] public bool canInteract;
	private APRController _aprController;
	private ConfigurableJoint _armLeft, _armRight;
	private WeaponManager _weaponManager;

	private Toggler _currentToggler
	{
		get => currentToggler;
		set
		{
			if (currentToggler != value)
			{
				currentToggler = value;
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
		Toggler toggler = other.GetComponent<Toggler>();
		if (toggler)
		{
			_currentToggler = toggler;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Toggler toggler = other.GetComponent<Toggler>();
		if (toggler == _currentToggler)
		{
			_currentToggler = null;
		}
	}

	private void CheckInteractionAvailability()
	{
		if (_currentToggler)
		{
			if (!_weaponManager.weapon || (_weaponManager.weapon && _weaponManager.weapon is Melee))
			{
				canInteract = true;
			}
			else
			{
				canInteract = false;
			}
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

	private IEnumerator Interaction()
	{
		_aprController.StrainArms(APRController.Arms.Left);

		Vector3 bodyBendingFactor = new Vector3(_aprController.Body.transform.eulerAngles.x, _aprController.Root.transform.localEulerAngles.y, 0);

		Debug.DrawLine(currentToggler.transform.position, _armLeft.transform.position, Color.red);

		Vector3 anglesLeft = Quaternion.LookRotation(currentToggler.transform.position - _armLeft.transform.position).eulerAngles;
		anglesLeft -= bodyBendingFactor;
		_armLeft.targetRotation = Quaternion.Euler(anglesLeft.x, anglesLeft.y - 270, anglesLeft.z);

		yield return new WaitForSeconds(pressingDelay);

		if (currentToggler)
		{
			currentToggler.isOn = !currentToggler.isOn;
		}

		_aprController.RelaxArms(APRController.Arms.Left);
	}

	private void Interact()
	{
		if (canInteract)
		{
			StopCoroutine(Interaction());
			StartCoroutine(Interaction());
		}
	}
}
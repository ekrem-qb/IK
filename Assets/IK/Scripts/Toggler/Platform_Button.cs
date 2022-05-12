using UnityEngine;

public class Platform_Button : Toggler
{
	[Tooltip("Object type/class/component required for pressing this button, button will for work with any object if this field is null")]
	public MonoBehaviour requiredObjectClass;

	private Transform _currentObject;

	private void OnCollisionEnter(Collision collision)
	{
		if (!_currentObject)
		{
			if (requiredObjectClass)
			{
				if (collision.transform.GetComponent(requiredObjectClass.GetType()))
				{
					isOn = true;
					_currentObject = collision.transform.root;
				}
			}
			else
			{
				isOn = true;
				_currentObject = collision.transform.root;
			}
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (_currentObject)
		{
			if (_currentObject == collision.transform.root)
			{
				isOn = false;
				_currentObject = null;
			}
		}
	}
}
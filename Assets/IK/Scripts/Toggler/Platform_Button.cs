using UnityEngine;

public class Platform_Button : Toggler
{
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
using UnityEngine;

public class Platform_Button : Switcher
{
    public MonoBehaviour requiredObjectClass;
    internal override void FixedUpdate()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (requiredObjectClass)
        {
            if (collision.transform.GetComponent(requiredObjectClass.GetType()))
            {
                isOn = true;
            }
        }
        else
        {
            isOn = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (requiredObjectClass)
        {
            if (collision.transform.GetComponent(requiredObjectClass.GetType()))
            {
                isOn = false;
            }
        }
        else
        {
            isOn = false;
        }
    }
}

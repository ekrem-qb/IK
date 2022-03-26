using UnityEngine;

public class Platform_Button : Toggler
{
    public MonoBehaviour requiredObjectClass;

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
using UnityEngine;

public class Switcher : Toggler
{
    public HingeJoint handle;

    private void FixedUpdate()
    {
        for (int axis = 0; axis < 3; axis++)
        {
            if (handle.axis[axis] >= 1)
            {
                isOn = handle.transform.eulerAngles[axis] > 180;
            }
            else if (handle.axis[axis] <= -1)
            {
                isOn = handle.transform.eulerAngles[axis] < 180;
            }
        }
    }
}
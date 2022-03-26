using System;
using UnityEngine;

public class Switcher : MonoBehaviour
{
    public bool isOn
    {
        get => _isOn;
        set
        {
            if (_isOn != value)
            {
                Toggle(value);
            }

            _isOn = value;
        }
    }

    public HingeJoint handle;
    [SerializeField] bool _isOn;
    public Action<bool> Toggle = b => { };

    private void Awake()
    {
        Toggle(isOn);
    }

    internal virtual void FixedUpdate()
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
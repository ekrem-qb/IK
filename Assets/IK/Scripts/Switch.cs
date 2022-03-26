using System;
using UnityEngine;

public class Switch : MonoBehaviour
{
    [SerializeField] bool _isOn;

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
    public Action<bool> Toggle = b => { };

    private void Awake()
    {
        Toggle(isOn);
    }

    void FixedUpdate()
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
using System;
using UnityEngine;

public class Toggler : MonoBehaviour
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

    [SerializeField] [ReadOnly] bool _isOn;
    public Action<bool> Toggle = b => { };

    private void Awake()
    {
        Toggle(isOn);
    }
}
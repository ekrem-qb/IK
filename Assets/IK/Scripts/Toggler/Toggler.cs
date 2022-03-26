using System;
using UnityEngine;

public class Toggler : MonoBehaviour
{
    public bool isOn
    {
        get => _isOn;
        set
        {
            if (invert)
            {
                if (_isOn != !value)
                {
                    Toggle(!value);
                }
    
                _isOn = !value;
            }
            else
            {
                if (_isOn != value)
                {
                    Toggle(value);
                }
    
                _isOn = value;
            }
        }
    }

    [SerializeField] [ReadOnly] bool _isOn;
    public bool invert;
    public Action<bool> Toggle = b => { };

    private void Awake()
    {
        Toggle(isOn);
    }
}
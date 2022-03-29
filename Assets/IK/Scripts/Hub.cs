using System;
using UnityEngine;

public class Hub : MonoBehaviour
{
    [Serializable]
    public struct ConveyorDirection
    {
        public Vector3 on, off;
    }

    public Toggler toggler;
    public Conveyor conveyor;
    public ConveyorDirection conveyorDirection;
    public Transform movingPart;
    public Vector3 movingPartToggleOffset;
    public float transitionSpeed = 15;
    private Vector3 _originalPosition;
    private Vector3 _targetPosition;

    private void Awake()
    {
        toggler.Toggle += OnSwitchToggle;
        _originalPosition = movingPart.localPosition;
    }

    private void OnSwitchToggle(bool isOn)
    {
        this.enabled = true;
        if (isOn)
        {
            conveyor.direction = conveyorDirection.on;
            _targetPosition = _originalPosition + movingPartToggleOffset;
        }
        else
        {
            _targetPosition = _originalPosition;
            conveyor.direction = conveyorDirection.off;
        }
    }

    private void FixedUpdate()
    {
        if (Vector3.Distance(movingPart.localPosition, _targetPosition) > 0.05f)
        {
            movingPart.localPosition = Vector3.Lerp(movingPart.localPosition, _targetPosition, transitionSpeed * Time.fixedDeltaTime);
        }
        else
        {
            this.enabled = false;
        }
    }
}
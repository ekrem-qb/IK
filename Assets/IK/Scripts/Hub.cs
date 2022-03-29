using System;
using UnityEngine;

public class Hub : Slidable
{
    [Serializable]
    public struct ConveyorDirection
    {
        public Vector3 on, off;
    }

    public Conveyor conveyor;
    public ConveyorDirection conveyorDirection;

    internal override void OnSwitchToggle(bool isOn)
    {
        base.OnSwitchToggle(isOn);
        if (isOn)
        {
            conveyor.direction = conveyorDirection.on;
        }
        else
        {
            conveyor.direction = conveyorDirection.off;
        }
    }
}
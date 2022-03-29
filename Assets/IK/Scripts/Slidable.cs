using UnityEngine;

public class Slidable : MonoBehaviour
{
    public Toggler toggler;
    public ConfigurableJoint movingPart;
    public Vector3 movingPartToggleOffset;

    private void Awake()
    {
        toggler.Toggle += OnSwitchToggle;
        SoftJointLimit limit = movingPart.linearLimit;
        limit.limit = Mathf.Max(movingPartToggleOffset.x, movingPartToggleOffset.y, movingPartToggleOffset.z);
        movingPart.linearLimit = limit;
    }

    internal virtual void OnSwitchToggle(bool isOn)
    {
        if (isOn)
        {
            movingPart.targetPosition -= movingPartToggleOffset;
        }
        else
        {
            movingPart.targetPosition = Vector3.zero;
        }
    }

    private void OnDestroy()
    {
        toggler.Toggle -= OnSwitchToggle;
    }
}
using System;
using System.Collections;
using UnityEngine;

public class Hub : MonoBehaviour
{
    [Serializable]
    public struct State
    {
        public Vector3 hubPosition, conveyorDirection;
    }

    public Toggler toggler;
    public Conveyor conveyor;
    public float transitionSpeed = 15;
    public State on;
    public State off;
    private bool _isInTransition;

    private void Awake()
    {
        toggler.Toggle += OnSwitchToggle;
    }

    private void OnSwitchToggle(bool isOn)
    {
        if (isOn)
        {
            conveyor.direction = on.conveyorDirection;
            StartCoroutine(TransitionToPosition(on.hubPosition));
        }
        else
        {
            conveyor.direction = off.conveyorDirection;
            StartCoroutine(TransitionToPosition(off.hubPosition));
        }
    }

    private IEnumerator TransitionToPosition(Vector3 targetPosition)
    {
        if (!_isInTransition)
        {
            _isInTransition = true;

            while (Vector3.Distance(this.transform.localPosition, targetPosition) > 0.05f)
            {
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, targetPosition, transitionSpeed * Time.deltaTime);
                yield return null;
            }

            _isInTransition = false;
        }
    }

    private void OnDestroy()
    {
        toggler.Toggle -= OnSwitchToggle;
    }
}
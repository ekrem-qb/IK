using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoldTrigger : EventTrigger
{
	public UnityEvent onPress = new UnityEvent();
	public UnityEvent onHold = new UnityEvent();
	public UnityEvent onRelease = new UnityEvent();
	public bool isPressed;

	public void Update()
	{
		if (isPressed)
		{
			onHold.Invoke();
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);

		onPress.Invoke();
		isPressed = true;
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);

		onRelease.Invoke();
		isPressed = false;
	}
}
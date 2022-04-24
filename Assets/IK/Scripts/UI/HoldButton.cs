using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : Selectable
{
	public UnityEvent onPress = new UnityEvent();
	public UnityEvent onHold = new UnityEvent();
	public UnityEvent onRelease = new UnityEvent();

	public void Update()
	{
		if (IsPressed())
		{
			onHold.Invoke();
		}
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);

		onPress.Invoke();
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);

		onRelease.Invoke();
	}
}
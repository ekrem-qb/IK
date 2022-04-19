using UnityEngine;

public class Fist : Melee
{
	private Vector3 _originalScale = Vector3.zero;

	protected override void Awake()
	{
		base.Awake();
		isInHookChanged += ResizeFist;
		_originalScale = hand.rigidbody.transform.localScale;
	}

	public override void Drop()
	{
	}

	private void ResizeFist(bool isInHook)
	{
		if (isInHook)
		{
			hand.rigidbody.transform.localScale = _originalScale * 2;
		}
		else
		{
			hand.rigidbody.transform.localScale = _originalScale;
		}
	}
}
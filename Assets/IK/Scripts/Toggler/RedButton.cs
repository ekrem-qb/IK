using UnityEngine;

public class RedButton : Toggler
{
	public Color offColor = Color.red;
	public Color onColor = Color.green;
	private Material _material;

	private void Awake()
	{
		Toggle += OnToggle;
		_material = this.GetComponent<Renderer>().material;
	}

	private void OnToggle(bool isOn)
	{
		if (isOn)
		{
			_material.color = onColor;
		}
		else
		{
			_material.color = offColor;
		}
	}
}
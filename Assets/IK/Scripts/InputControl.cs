using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct InputControl
{
	public KeyCode key;
	public Button button;
	public HoldButton holdButton;
}
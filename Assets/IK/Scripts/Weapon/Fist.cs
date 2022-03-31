using UnityEngine;

public class Fist : Melee
{
    Vector3 originalScale = Vector3.zero;

    protected override void Awake()
    {
        base.Awake();
        isInHookChanged += ResizeFist;
        originalScale = hand.transform.localScale;
    }

    protected override void OnEnable()
    {
    }

    protected override void OnDisable()
    {
    }

    void ResizeFist(bool isInHook)
    {
        if (isInHook)
        {
            hand.transform.localScale = originalScale * 2;
        }
        else
        {
            hand.transform.localScale = originalScale;
        }
    }
}
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public float rotationSpeed = 25;
    public GameObject pickup;
    [HideInInspector]
    public Vector3 originalPosition, originalAngles;

    void Start()
    {
        originalPosition = pickup.transform.localPosition;
        originalAngles = pickup.transform.localEulerAngles;
        if (pickup.scene.name == null)
        {
            pickup = Instantiate(pickup);
        }
        pickup.transform.SetParent(this.transform);
        pickup.transform.localPosition = Vector3.zero;
        pickup.transform.localEulerAngles = Vector3.zero;
    }

    void Update()
    {
        pickup.transform.eulerAngles = Vector3.Slerp(pickup.transform.eulerAngles, pickup.transform.eulerAngles + (Vector3.up * rotationSpeed), Time.deltaTime * 10);
    }
}

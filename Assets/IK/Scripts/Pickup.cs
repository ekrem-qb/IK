using UnityEngine;

public class Pickup : MonoBehaviour
{
    public float rotationSpeed = 25;
    public GameObject pickupPrefab;
    GameObject pickup;

    void Start()
    {
        pickup = Instantiate(pickupPrefab, this.transform.position, this.transform.rotation);
        pickup.transform.SetParent(this.transform);
    }

    void Update()
    {
        pickup.transform.eulerAngles = Vector3.Slerp(pickup.transform.eulerAngles, pickup.transform.eulerAngles + (Vector3.up * rotationSpeed), Time.deltaTime * 10);
    }
}

using UnityEngine;

public class GunManager : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR_Player;
    Transform handLeft, handRight;
    public Gun gunLeft, gunRight;

    void Awake()
    {
        APR_Player = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        handLeft = APR_Player.LeftHand.transform.GetChild(0);
        handRight = APR_Player.RightHand.transform.GetChild(0);
    }

    void OnTriggerEnter(Collider other)
    {
        Pickup pickup = other.GetComponent<Pickup>();
        if (pickup != null)
        {
            if (gunLeft == null)
            {
                gunLeft = Instantiate(pickup.pickupPrefab, handLeft).GetComponent<Gun>();
            }
            else if (gunRight == null)
            {
                gunRight = Instantiate(pickup.pickupPrefab, handRight).GetComponent<Gun>();
                gunRight.transform.localPosition = new Vector3(-gunRight.transform.localPosition.x, gunRight.transform.localPosition.y, gunRight.transform.localPosition.z);
                gunRight.transform.localEulerAngles = new Vector3(gunRight.transform.localEulerAngles.x, gunRight.transform.localEulerAngles.y, -gunRight.transform.localEulerAngles.z);
                gunRight.fireButton = Gun.FireButton.Fire2;
            }
        }
    }
}

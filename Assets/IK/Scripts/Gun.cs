using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab;
    public enum FireButton
    {
        Fire1,
        Fire2,
    }
    public FireButton fireButton;

    void Update()
    {
        if (Input.GetButtonUp(fireButton.ToString()))
        {
            GameObject bullet = Instantiate(bulletPrefab, this.transform.position, this.transform.rotation);
        }
    }
}
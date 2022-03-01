using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Vector3 holdPosition;
    public Quaternion holdRotation;
    public float transitionSpeed = 5;
    [HideInInspector]
    public bool canShoot = false;
    [HideInInspector]
    public bool isLeft;
    SphereCollider pickupTrigger;
    Rigidbody rb;
    KeyCode fireKey;

    void Awake()
    {
        pickupTrigger = this.GetComponent<SphereCollider>();
        rb = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (Input.GetKeyDown(fireKey) && canShoot)
        {
            GameObject bullet = Instantiate(bulletPrefab, this.transform.position, this.transform.rotation);
        }
    }

    void OnEnable()
    {
        pickupTrigger.enabled = false;
        Destroy(rb);
        if (isLeft)
        {
            fireKey = KeyCode.Mouse0;
            StartCoroutine(TransitionToHold(holdPosition, holdRotation));
        }
        else
        {
            fireKey = KeyCode.Mouse1;
            StartCoroutine(TransitionToHold(new Vector3(-holdPosition.x, holdPosition.y, holdPosition.z), new Quaternion(holdRotation.x, -holdRotation.y, -holdRotation.z, holdRotation.w)));
        }
    }

    void OnDisable()
    {
        pickupTrigger.enabled = true;
        rb = this.gameObject.AddComponent<Rigidbody>();
    }

    public IEnumerator TransitionToHold(Vector3 targetPosition, Quaternion targetRotation)
    {
        while (this.enabled && (Vector3.Distance(this.transform.localPosition, targetPosition) > 0.05f || Quaternion.Angle(this.transform.localRotation, targetRotation) > 0.05f))
        {
            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, targetRotation, Time.deltaTime * transitionSpeed);
            yield return null;
        }
    }
}
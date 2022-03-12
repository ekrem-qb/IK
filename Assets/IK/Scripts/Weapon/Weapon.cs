using System.Collections;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Vector3 holdPosition;
    public Quaternion holdRotation;
    public float transitionSpeed = 15;
    public bool isLeft = true;
    [HideInInspector]
    public Player player;
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
        if (player)
        {
            if (Input.GetKeyDown(fireKey))
            {
                Attack();
            }
        }
    }

    public virtual void Attack() { }

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
using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    public Vector3 holdPosition;
    public Quaternion holdRotation;
    public float transitionSpeed = 15;
    public bool isLeft = true;
    public float damage = 10;
    [HideInInspector] public Player player;
    private KeyCode _fireKey;
    private SphereCollider _pickupTrigger;
    private Rigidbody _rigidbody;
    protected ParticleSystem particle;

    protected virtual void Awake()
    {
        _pickupTrigger = this.GetComponent<SphereCollider>();
        _rigidbody = this.GetComponent<Rigidbody>();
        particle = this.GetComponentInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (player)
        {
            if (Input.GetKeyDown(_fireKey))
            {
                Attack();
            }
        }
    }

    protected virtual void OnEnable()
    {
        _pickupTrigger.enabled = false;
        Destroy(_rigidbody);
        if (isLeft)
        {
            _fireKey = KeyCode.Mouse0;
            StartCoroutine(TransitionToHold(holdPosition, holdRotation));
        }
        else
        {
            _fireKey = KeyCode.Mouse1;
            StartCoroutine(TransitionToHold(new Vector3(-holdPosition.x, holdPosition.y, holdPosition.z), new Quaternion(holdRotation.x, -holdRotation.y, -holdRotation.z, holdRotation.w)));
        }
    }

    protected virtual void OnDisable()
    {
        _pickupTrigger.enabled = true;
        _rigidbody = this.gameObject.AddComponent<Rigidbody>();
    }

    public abstract void Attack();

    private IEnumerator TransitionToHold(Vector3 targetPosition, Quaternion targetRotation)
    {
        while (this.enabled && (Vector3.Distance(this.transform.localPosition, targetPosition) > 0.05f || Quaternion.Angle(this.transform.localRotation, targetRotation) > 0.05f))
        {
            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, targetPosition, Time.deltaTime * transitionSpeed);
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, targetRotation, Time.deltaTime * transitionSpeed);
            yield return null;
        }
    }
}
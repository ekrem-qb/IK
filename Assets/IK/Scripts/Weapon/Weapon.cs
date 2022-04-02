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
    protected ARP.APR.Scripts.APRController aprController;
    protected BodyPart arm, armLow, hand;
    protected ParticleSystem particle;

    protected virtual void Awake()
    {
        _pickupTrigger = this.GetComponent<SphereCollider>();
        _rigidbody = this.GetComponent<Rigidbody>();
        particle = this.GetComponentInChildren<ParticleSystem>();
        ReassignToNewOwner();
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

    private void OnTransformParentChanged()
    {
        ReassignToNewOwner();
    }

    private void ReassignToNewOwner()
    {
        if (this.transform.parent)
        {
            aprController = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
            if (isLeft)
            {
                arm.joint = aprController.UpperLeftArm.GetComponent<ConfigurableJoint>();
                arm.rigidbody = aprController.UpperLeftArm.GetComponent<Rigidbody>();
                armLow.joint = aprController.LowerLeftArm.GetComponent<ConfigurableJoint>();
                armLow.rigidbody = aprController.LowerLeftArm.GetComponent<Rigidbody>();
                hand.joint = aprController.LeftHand.GetComponent<ConfigurableJoint>();
                hand.rigidbody = aprController.LeftHand.GetComponent<Rigidbody>();
            }
            else
            {
                arm.joint = aprController.UpperRightArm.GetComponent<ConfigurableJoint>();
                arm.rigidbody = aprController.UpperRightArm.GetComponent<Rigidbody>();
                armLow.joint = aprController.LowerRightArm.GetComponent<ConfigurableJoint>();
                armLow.rigidbody = aprController.LowerRightArm.GetComponent<Rigidbody>();
                hand.joint = aprController.RightHand.GetComponent<ConfigurableJoint>();
                hand.rigidbody = aprController.RightHand.GetComponent<Rigidbody>();
            }
        }
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

    protected struct BodyPart
    {
        public ConfigurableJoint joint;
        public Rigidbody rigidbody;
    }
}
using System;
using PathCreation;
using UnityEngine;

public class Car : MonoBehaviour
{
    [Serializable]
    public struct Wheels
    {
        public Rigidbody frontLeft;
        public Rigidbody frontRight;
        public Rigidbody backLeft;
        public Rigidbody backRight;
    }

    public Wheels wheels;
    public PathCreator path;
    public EndOfPathInstruction endOfPathInstruction;
    public float speed = 15;
    
    private Rigidbody _rigidbody;
    private float _distanceTravelled;
    private const float MaxSpeed = 100;

    private void Awake()
    {
        _rigidbody = this.GetComponent<Rigidbody>();
        Vector3 centerOfMass = _rigidbody.centerOfMass;
        centerOfMass.y /= 2;
        _rigidbody.centerOfMass = centerOfMass;
        _rigidbody.maxAngularVelocity = MaxSpeed;

        wheels.frontLeft.maxAngularVelocity = MaxSpeed;
        wheels.frontRight.maxAngularVelocity = MaxSpeed;
        wheels.backLeft.maxAngularVelocity = MaxSpeed;
        wheels.backRight.maxAngularVelocity = MaxSpeed;

        path.pathUpdated += OnPathChanged;
    }

    private void FixedUpdate()
    {
        _distanceTravelled += speed * Time.deltaTime;
        this.transform.position = path.path.GetPointAtDistance(_distanceTravelled, endOfPathInstruction);
        this.transform.rotation = path.path.GetRotationAtDistance(_distanceTravelled, endOfPathInstruction);
    }

    private void OnPathChanged()
    {
        _distanceTravelled = path.path.GetClosestDistanceAlongPath(this.transform.position);
    }

    private void OnEnable()
    {
        _distanceTravelled = path.path.GetClosestDistanceAlongPath(this.transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (this.enabled)
        {
            if (this.transform.root != collision.transform.root)
            {
                HealthManager enemy = collision.transform.root.GetComponent<HealthManager>();
                if (enemy)
                {
                    enemy.health = 0;
                    if (collision.rigidbody)
                    {
                        collision.rigidbody.AddForce(this.transform.forward * speed * 2, ForceMode.Impulse);
                    }
                }
            }
        }
    }
}
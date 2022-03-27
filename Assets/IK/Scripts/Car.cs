using System;
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

    public Rigidbody body;
    public Wheels wheels;
    public float speed = 10;
    private const float MaxSpeed = 100;

    private void Awake()
    {
        body.maxAngularVelocity = MaxSpeed;
        
        wheels.frontLeft.maxAngularVelocity = MaxSpeed;
        wheels.frontRight.maxAngularVelocity = MaxSpeed;
        wheels.backLeft.maxAngularVelocity = MaxSpeed;
        wheels.backRight.maxAngularVelocity = MaxSpeed;
    }

    private void FixedUpdate()
    {
        wheels.frontLeft.AddTorque(wheels.frontLeft.transform.right * speed * 100 * Time.fixedDeltaTime);
        wheels.frontRight.AddTorque(wheels.frontRight.transform.right * speed * 100 * Time.fixedDeltaTime);
        wheels.backLeft.AddTorque(wheels.backLeft.transform.right * speed * 100 * Time.fixedDeltaTime);
        wheels.backRight.AddTorque(wheels.backRight.transform.right * speed * 100 * Time.fixedDeltaTime);
    }
}
using UnityEngine;

public class RotationControl : MonoBehaviour
{
    ARP.APR.Scripts.APRController aprController;
    ConfigurableJoint rootJoint;

    void Awake()
    {
        aprController = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        rootJoint = aprController.Root.GetComponent<ConfigurableJoint>();
    }

    void FixedUpdate()
    {
        if (aprController.useControls)
        {
            Vector3 targetDirection = Vector3.zero;

            if (Input.GetAxisRaw(aprController.leftRight) != 0)
            {
                targetDirection += Vector3.left * Input.GetAxisRaw(aprController.leftRight);
            }

            if (Input.GetAxisRaw(aprController.forwardBackward) != 0)
            {
                targetDirection += Vector3.forward * Input.GetAxisRaw(aprController.forwardBackward);
            }

            if (aprController.joystick)
            {
                if (aprController.joystick.Horizontal != 0)
                {
                    targetDirection += Vector3.left * aprController.joystick.Horizontal;
                }

                if (aprController.joystick.Vertical != 0)
                {
                    targetDirection += Vector3.forward * aprController.joystick.Vertical;
                }
            }

            if (targetDirection != Vector3.zero)
            {
                rootJoint.targetRotation = Quaternion.LookRotation(targetDirection);
            }
        }
    }
}
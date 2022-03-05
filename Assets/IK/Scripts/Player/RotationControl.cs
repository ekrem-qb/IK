using UnityEngine;

public class RotationControl : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR;
    ConfigurableJoint rootJoint;

    void Awake()
    {
        APR = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        rootJoint = APR.Root.GetComponent<ConfigurableJoint>();
    }

    void FixedUpdate()
    {
        if (APR.useControls)
        {
            Vector3 targetPosition = Vector3.zero;

            if (Input.GetAxisRaw(APR.leftRight) != 0)
            {
                targetPosition += Vector3.right * Input.GetAxisRaw(APR.leftRight) * 100;
            }
            if (Input.GetAxisRaw(APR.forwardBackward) != 0)
            {
                targetPosition += Vector3.back * Input.GetAxisRaw(APR.forwardBackward) * 100;
            }

            if (targetPosition != Vector3.zero)
            {
                rootJoint.targetRotation = Quaternion.LookRotation(rootJoint.transform.position - targetPosition);
            }
        }
    }
}

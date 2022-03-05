using UnityEngine;

public class RotationControl : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR_Player;
    ConfigurableJoint rootJoint;

    void Awake()
    {
        APR_Player = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        rootJoint = APR_Player.Root.GetComponent<ConfigurableJoint>();
    }

    void FixedUpdate()
    {
        if (APR_Player.useControls)
        {
            Vector3 targetPosition = Vector3.zero;

            if (Input.GetAxisRaw(APR_Player.leftRight) != 0)
            {
                targetPosition += Vector3.right * Input.GetAxisRaw(APR_Player.leftRight) * 100;
            }
            if (Input.GetAxisRaw(APR_Player.forwardBackward) != 0)
            {
                targetPosition += Vector3.back * Input.GetAxisRaw(APR_Player.forwardBackward) * 100;
            }

            if (targetPosition != Vector3.zero)
            {
                rootJoint.targetRotation = Quaternion.LookRotation(rootJoint.transform.position - targetPosition);
            }
        }
    }
}

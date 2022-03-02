using UnityEngine;

public class Enemy : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR;
    ConfigurableJoint rootJoint;
    AutoAim player;

    void Awake()
    {
        APR = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        rootJoint = APR.Root.GetComponent<ConfigurableJoint>();
        player = GameObject.FindObjectOfType<AutoAim>();
    }

    void FixedUpdate()
    {
        if (player)
        {
            rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(rootJoint.transform.position - player.transform.position));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet)
        {
            Destroy(this.transform.root.gameObject);
        }
    }
}

using UnityEngine;

public class Enemy : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR;
    ConfigurableJoint rootJoint;
    Rigidbody rootRB;
    AutoAim player;
    public float playerAttackDistance = 2;

    void Awake()
    {
        APR = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        rootJoint = APR.Root.GetComponent<ConfigurableJoint>();
        rootRB = APR.Root.GetComponent<Rigidbody>();
        player = GameObject.FindObjectOfType<AutoAim>();
    }

    void FixedUpdate()
    {
        if (player)
        {
            rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(rootJoint.transform.position - player.transform.position));

            if (Vector3.Distance(this.transform.position, player.transform.position) > playerAttackDistance)
            {
                Vector3 direction = APR.Root.transform.forward;
                direction.y = 0f;
                rootRB.velocity = Vector3.Lerp(rootRB.velocity, (direction * APR.moveSpeed) + new Vector3(0, rootRB.velocity.y, 0), Time.fixedDeltaTime * 10);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Bullet bullet = collision.transform.GetComponent<Bullet>();
        if (bullet)
        {
            Destroy(this.transform.root.gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, playerAttackDistance);
    }
}
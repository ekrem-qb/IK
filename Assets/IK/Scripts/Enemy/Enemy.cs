using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public ARP.APR.Scripts.APRController APR;
    [HideInInspector] public ConfigurableJoint rootJoint;
    [HideInInspector] public Rigidbody rootRB;
    [HideInInspector] public Player player;
    public float attackDistance = 2.5f;

    void Awake()
    {
        APR = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        rootJoint = APR.Root.GetComponent<ConfigurableJoint>();
        rootRB = APR.Root.GetComponent<Rigidbody>();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(this.transform.position, attackDistance);
    }

    void OnDestroy()
    {
        if (player)
        {
            player.nearEnemies.Remove(this);
        }
    }
}
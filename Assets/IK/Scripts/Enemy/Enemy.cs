using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public ARP.APR.Scripts.APRController aprController;
    [HideInInspector] public ConfigurableJoint rootJoint;
    [HideInInspector] public ConfigurableJoint bodyJoint;
    [HideInInspector] public Rigidbody rootRB;
    [SerializeField] Player _player;

    public Player player
    {
        get => _player;
        set
        {
            if (_player != value)
            {
                OnPlayerChanged(value);
            }

            _player = value;
        }
    }

    public float attackDistance = 2.5f;

    void Awake()
    {
        aprController = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        rootJoint = aprController.Root.GetComponent<ConfigurableJoint>();
        bodyJoint = aprController.Body.GetComponent<ConfigurableJoint>();
        rootRB = aprController.Root.GetComponent<Rigidbody>();
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

    public virtual void OnPlayerChanged(Player newPlayer)
    {
    }
}
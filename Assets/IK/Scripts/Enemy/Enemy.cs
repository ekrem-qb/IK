using System;
using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public ARP.APR.Scripts.APRController aprController;
    [HideInInspector] public ConfigurableJoint rootJoint;
    [HideInInspector] public Rigidbody rootRB;
    [SerializeField] Player _player;
    protected ConfigurableJoint bodyJoint;
    protected PathFollower pathFollower;
    protected bool isAttacking;
    public float attackDistance = 2.5f;

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

    protected virtual void Awake()
    {
        aprController = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        rootJoint = aprController.Root.GetComponent<ConfigurableJoint>();
        bodyJoint = aprController.Body.GetComponent<ConfigurableJoint>();
        rootRB = aprController.Root.GetComponent<Rigidbody>();
        pathFollower = this.GetComponent<PathFollower>();
        this.enabled = false;
    }

    void FixedUpdate()
    {
        Vector3 target = player.transform.position;
        target.y = this.transform.position.y;

        rootJoint.targetRotation = Quaternion.Inverse(Quaternion.LookRotation(target - rootJoint.transform.position));

        if (Vector3.Distance(this.transform.position, target) > attackDistance)
        {
            Vector3 direction = aprController.Root.transform.forward;
            direction.y = 0f;
            direction *= aprController.moveSpeed;

            rootRB.velocity = Vector3.Lerp(rootRB.velocity, direction + new Vector3(0, rootRB.velocity.y, 0), Time.fixedDeltaTime * 10);

            if (aprController.balanced)
            {
                if (!aprController.WalkForward && !aprController.moveAxisUsed)
                {
                    aprController.WalkForward = true;
                    aprController.moveAxisUsed = true;
                    aprController.isKeyDown = true;
                }
            }
        }
        else
        {
            if (aprController.WalkForward && aprController.moveAxisUsed)
            {
                aprController.WalkForward = false;
                aprController.moveAxisUsed = false;
                aprController.isKeyDown = false;
            }

            if (aprController.balanced)
            {
                if (!isAttacking)
                {
                    StartCoroutine(Attack());
                }
            }
        }
    }

    protected virtual IEnumerator Attack()
    {
        yield return null;
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

    protected virtual void OnPlayerChanged(Player newPlayer)
    {
        pathFollower.enabled = !newPlayer;
        this.enabled = newPlayer;
    }

    void OnDisable()
    {
        if (aprController.WalkForward && aprController.moveAxisUsed)
        {
            aprController.WalkForward = false;
            aprController.moveAxisUsed = false;
            aprController.isKeyDown = false;
        }
    }
}
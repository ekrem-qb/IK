using System;
using System.Collections;
using UnityEngine;

public class Ped : Enemy
{
    public float attackInterval = 0.5f;
    WeaponManager weaponManager;
    HealthManager healthManager;

    protected override void Awake()
    {
        base.Awake();
        weaponManager = aprController.COMP.GetComponent<WeaponManager>();
        healthManager = this.transform.root.GetComponent<HealthManager>();
        healthManager.Hit += Annoy;
    }

    protected override void OnPlayerChanged(Player newPlayer)
    {
        if (!newPlayer)
        {
            pathFollower.enabled = true;
            this.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger)
        {
            if (other.transform.root != this.transform.root) 
            {
                ARP.APR.Scripts.APRController playerApr = other.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
                if (playerApr)
                {
                    Player player = playerApr.Root.GetComponent<Player>();
                    if (player)
                    {
                        Annoy();
                    } 
                }
            }
        }
    }

    void Annoy()
    {
        print("Annoyed");
        if (player)
        {
            pathFollower.enabled = false;
            this.enabled = true;
        }
    }

    protected override IEnumerator Attack()
    {
        isAttacking = true;

        if (weaponManager.weaponLeft)
        {
            weaponManager.weaponLeft.Attack();
        }

        yield return new WaitForSeconds(attackInterval);

        if (weaponManager.weaponRight)
        {
            weaponManager.weaponRight.Attack();
        }

        yield return new WaitForSeconds(attackInterval);
        isAttacking = false;
    }
}
using System.Collections;
using UnityEngine;

public class Gangster : Enemy
{
    public float attackInterval = 0.5f;
    WeaponManager weaponManager;

    protected override void Awake()
    {
        base.Awake();
        weaponManager = aprController.COMP.GetComponent<WeaponManager>();
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
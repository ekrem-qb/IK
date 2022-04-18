using System;
using UnityEngine;

public class Gun : Weapon
{
    public GameObject bulletPrefab;
    public float recoilForce = 50;
    [SerializeField] private int _ammo = 10;

    public Action<Gun, int> AmmoCountChanged = (gun, i) => { };

    public int ammo
    {
        get => _ammo;
        set
        {
            if (_ammo != value)
            {
                AmmoCountChanged(this, value);
            }

            _ammo = value;
        }
    }

    public override void Attack()
    {
        if (ammo > 0)
        {
            hand.rigidbody.AddForce(-this.transform.forward * recoilForce, ForceMode.Impulse);
            Projectile projectile = Instantiate(bulletPrefab, this.transform.position, this.transform.rotation).GetComponent<Projectile>();
            projectile.owner = this.transform.root;
            if (particle)
            {
                particle.Play();
            }

            ammo--;
        }
    }

    public void Strain()
    {
        arm.joint.angularXDrive = aprController.ReachStiffness;
        arm.joint.angularYZDrive = aprController.ReachStiffness;
        armLow.joint.angularXDrive = aprController.ReachStiffness;
        armLow.joint.angularYZDrive = aprController.ReachStiffness;
        armLow.joint.targetRotation = Quaternion.identity;
    }

    public void Relax()
    {
        arm.joint.angularXDrive = aprController.DriveOff;
        arm.joint.angularYZDrive = aprController.DriveOff;
        arm.joint.targetRotation = arm.originalRotation;
        armLow.joint.angularXDrive = aprController.DriveOff;
        armLow.joint.angularYZDrive = aprController.DriveOff;
        armLow.joint.targetRotation = armLow.originalRotation;
    }
}
using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    ARP.APR.Scripts.APRController aprController;
    WeaponManager weaponManager;
    ConfigurableJoint armLeft, armRight;
    ConfigurableJoint armLeftLow, armRightLow;
    SphereCollider trigger;
    public ObservableList<Enemy> nearEnemies = new ObservableList<Enemy>();

    void Awake()
    {
        aprController = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        armLeft = aprController.UpperLeftArm.GetComponent<ConfigurableJoint>();
        armRight = aprController.UpperRightArm.GetComponent<ConfigurableJoint>();
        armLeftLow = aprController.LowerLeftArm.GetComponent<ConfigurableJoint>();
        armRightLow = aprController.LowerRightArm.GetComponent<ConfigurableJoint>();
        weaponManager = aprController.COMP.GetComponent<WeaponManager>();
        trigger = this.GetComponent<SphereCollider>();
        nearEnemies.CountChanged += count => this.enabled = count > 0;
    }

    void FixedUpdate()
    {
        if (aprController.useControls)
        {
            if (weaponManager.weaponLeft is Gun || weaponManager.weaponRight is Gun)
            {
                Vector3 bodyBendingFactor = new Vector3(aprController.Body.transform.eulerAngles.x, aprController.Root.transform.localEulerAngles.y, 0);
                Enemy nearestToArmLeft = null;

                if (weaponManager.weaponLeft is Gun)
                {
                    nearEnemies.SortByDistanceTo(armLeft.transform.position);
                    nearestToArmLeft = nearEnemies[0];

                    Debug.DrawLine(nearestToArmLeft.target.position, armLeft.transform.position, Color.red);

                    Vector3 angles = Quaternion.LookRotation(nearestToArmLeft.target.position - armLeft.transform.position).eulerAngles;
                    angles -= bodyBendingFactor;
                    Quaternion newRot = Quaternion.Euler(angles.x, angles.y - 270, angles.z);

                    armLeft.targetRotation = newRot;
                }

                if (weaponManager.weaponRight is Gun)
                {
                    nearEnemies.SortByDistanceTo(armRight.transform.position);
                    Enemy nearestToArmRight = nearEnemies[0];
                    if (nearEnemies.Count > 1)
                    {
                        if (nearestToArmRight == nearestToArmLeft)
                        {
                            nearestToArmRight = nearEnemies[1];
                        }
                    }

                    Debug.DrawLine(nearestToArmRight.target.position, armRight.transform.position, Color.yellow);

                    Vector3 angles = Quaternion.LookRotation(nearestToArmRight.target.position - armRight.transform.position).eulerAngles;
                    angles -= bodyBendingFactor;
                    Quaternion newRot = Quaternion.Euler(angles.x, angles.y - 90, angles.z);

                    armRight.targetRotation = newRot;
                }
            }
        }
    }

    void OnEnable()
    {
        if (weaponManager.weaponLeft is Gun)
        {
            if (!aprController.punchingLeft)
            {
                armLeft.angularXDrive = aprController.ReachStiffness;
                armLeft.angularYZDrive = aprController.ReachStiffness;
                armLeftLow.angularXDrive = aprController.ReachStiffness;
                armLeftLow.angularYZDrive = aprController.ReachStiffness;
                armLeftLow.targetRotation = Quaternion.identity;

                (weaponManager.weaponLeft as Gun).canShoot = true;
            }
        }

        if (weaponManager.weaponRight is Gun)
        {
            if (!aprController.punchingRight)
            {
                armRight.angularXDrive = aprController.ReachStiffness;
                armRight.angularYZDrive = aprController.ReachStiffness;
                armRightLow.angularXDrive = aprController.ReachStiffness;
                armRightLow.angularYZDrive = aprController.ReachStiffness;
                armRightLow.targetRotation = Quaternion.identity;

                (weaponManager.weaponRight as Gun).canShoot = true;
            }
        }
    }

    void OnDisable()
    {
        if (weaponManager.weaponLeft is Gun)
        {
            armLeft.angularXDrive = aprController.PoseOn;
            armLeft.angularYZDrive = aprController.PoseOn;
            armLeftLow.angularXDrive = aprController.PoseOn;
            armLeftLow.angularYZDrive = aprController.PoseOn;
            armLeft.targetRotation = aprController.UpperLeftArmTarget;
            armLeftLow.targetRotation = aprController.LowerLeftArmTarget;

            (weaponManager.weaponLeft as Gun).canShoot = false;
        }

        if (weaponManager.weaponRight is Gun)
        {
            armRight.angularXDrive = aprController.PoseOn;
            armRight.angularYZDrive = aprController.PoseOn;
            armRightLow.angularXDrive = aprController.PoseOn;
            armRightLow.angularYZDrive = aprController.PoseOn;
            armRight.targetRotation = aprController.UpperRightArmTarget;
            armRightLow.targetRotation = aprController.LowerRightArmTarget;

            (weaponManager.weaponRight as Gun).canShoot = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy)
            {
                if (!nearEnemies.Contains(enemy))
                {
                    enemy.player = this;
                    nearEnemies.Add(enemy);
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
        {
            if (trigger.radius <= Vector3.Distance(this.transform.position, other.transform.position))
            {
                Enemy enemy = other.GetComponent<Enemy>();
                if (enemy)
                {
                    enemy.player = null;
                    nearEnemies.Remove(enemy);
                }
            }
        }
    }

    private void OnDestroy()
    {
        nearEnemies.ForEach(enemy => enemy.player = null);
    }
}
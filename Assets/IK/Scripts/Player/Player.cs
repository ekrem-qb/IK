using UnityEngine;

public class Player : MonoBehaviour
{
    ARP.APR.Scripts.APRController aprController;
    ConfigurableJoint armLeft, armRight;
    ConfigurableJoint armLeftLow, armRightLow;
    public ObservableList<Enemy> nearEnemies = new ObservableList<Enemy>();
    SphereCollider trigger;
    WeaponManager weaponManager;

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
            }
        }
    }

    void OnDisable()
    {
        if (weaponManager.weaponLeft is Gun)
        {
            armLeft.angularXDrive = aprController.DriveOff;
            armLeft.angularYZDrive = aprController.DriveOff;
            armLeftLow.angularXDrive = aprController.DriveOff;
            armLeftLow.angularYZDrive = aprController.DriveOff;
        }

        if (weaponManager.weaponRight is Gun)
        {
            armRight.angularXDrive = aprController.DriveOff;
            armRight.angularYZDrive = aprController.DriveOff;
            armRightLow.angularXDrive = aprController.DriveOff;
            armRightLow.angularYZDrive = aprController.DriveOff;
        }
    }

    private void OnDestroy()
    {
        nearEnemies.ForEach(enemy => enemy.player = null);
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
}
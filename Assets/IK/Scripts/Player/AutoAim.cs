using UnityEngine;

public class AutoAim : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR_Player;
    WeaponManager weaponManager;
    ConfigurableJoint armLeft, armRight;
    ConfigurableJoint armLeftLow, armRightLow;
    SphereCollider trigger;
    public ObservableList<Enemy> nearEnemies = new ObservableList<Enemy>();

    void Awake()
    {
        APR_Player = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        armLeft = APR_Player.UpperLeftArm.GetComponent<ConfigurableJoint>();
        armRight = APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>();
        armLeftLow = APR_Player.LowerLeftArm.GetComponent<ConfigurableJoint>();
        armRightLow = APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>();
        weaponManager = APR_Player.COMP.GetComponent<WeaponManager>();
        trigger = this.GetComponent<SphereCollider>();
        nearEnemies.CountChanged += OnEnemyCountChanged;
    }

    void FixedUpdate()
    {
        if (APR_Player.useControls)
        {
            if (weaponManager.weaponLeft is Gun)
            {
                nearEnemies.Sort(SortByDistanceToArmLeft);

                Debug.DrawLine(nearEnemies[0].transform.position, armLeft.transform.position, Color.red);

                Vector3 angles = (Quaternion.LookRotation(nearEnemies[0].transform.position - armLeft.transform.position) * Quaternion.Inverse(APR_Player.Root.transform.rotation)).eulerAngles;
                Quaternion newRot = Quaternion.Euler(angles.x - 35, angles.y - 270, angles.z);

                armLeft.targetRotation = newRot;
            }
            if (weaponManager.weaponRight is Gun)
            {
                nearEnemies.Sort(SortByDistanceToArmRight);

                Debug.DrawLine(nearEnemies[0].transform.position, armRight.transform.position, Color.yellow);

                Vector3 angles = (Quaternion.LookRotation(nearEnemies[0].transform.position - armRight.transform.position) * Quaternion.Inverse(APR_Player.Root.transform.rotation)).eulerAngles;
                Quaternion newRot = Quaternion.Euler(angles.x - 35, angles.y - 90, angles.z);

                armRight.targetRotation = newRot;
            }
        }
    }

    void OnEnemyCountChanged(object sender, System.EventArgs e)
    {
        this.enabled = nearEnemies.Count > 0;
    }

    void OnEnable()
    {
        if (weaponManager.weaponLeft is Gun)
        {
            if (!APR_Player.punchingLeft)
            {
                armLeft.angularXDrive = APR_Player.ReachStiffness;
                armLeft.angularYZDrive = APR_Player.ReachStiffness;
                armLeftLow.angularXDrive = APR_Player.ReachStiffness;
                armLeftLow.angularYZDrive = APR_Player.ReachStiffness;
                armLeftLow.targetRotation = Quaternion.identity;

                (weaponManager.weaponLeft as Gun).canShoot = true;
            }
        }
        if (weaponManager.weaponRight is Gun)
        {
            if (!APR_Player.punchingRight)
            {
                armRight.angularXDrive = APR_Player.ReachStiffness;
                armRight.angularYZDrive = APR_Player.ReachStiffness;
                armRightLow.angularXDrive = APR_Player.ReachStiffness;
                armRightLow.angularYZDrive = APR_Player.ReachStiffness;
                armRightLow.targetRotation = Quaternion.identity;

                (weaponManager.weaponRight as Gun).canShoot = true;
            }
        }
    }

    void OnDisable()
    {
        if (weaponManager.weaponLeft is Gun)
        {
            armLeft.angularXDrive = APR_Player.PoseOn;
            armLeft.angularYZDrive = APR_Player.PoseOn;
            armLeftLow.angularXDrive = APR_Player.PoseOn;
            armLeftLow.angularYZDrive = APR_Player.PoseOn;
            armLeft.targetRotation = APR_Player.UpperLeftArmTarget;
            armLeftLow.targetRotation = APR_Player.LowerLeftArmTarget;

            (weaponManager.weaponLeft as Gun).canShoot = false;
        }
        if (weaponManager.weaponRight is Gun)
        {
            armRight.angularXDrive = APR_Player.PoseOn;
            armRight.angularYZDrive = APR_Player.PoseOn;
            armRightLow.angularXDrive = APR_Player.PoseOn;
            armRightLow.angularYZDrive = APR_Player.PoseOn;
            armRight.targetRotation = APR_Player.UpperRightArmTarget;
            armRightLow.targetRotation = APR_Player.LowerRightArmTarget;

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
                    nearEnemies.Remove(enemy);
                }
            }
        }
    }

    int SortByDistanceToArmLeft(Enemy a, Enemy b)
    {
        if (Vector3.Distance(a.transform.position, armLeft.transform.position) < Vector3.Distance(b.transform.position, armLeft.transform.position))
        {
            return -1;
        }
        else if (Vector3.Distance(a.transform.position, armLeft.transform.position) > Vector3.Distance(b.transform.position, armLeft.transform.position))
        {
            return 1;
        }
        return 0;
    }

    int SortByDistanceToArmRight(Enemy a, Enemy b)
    {
        if (Vector3.Distance(a.transform.position, armRight.transform.position) < Vector3.Distance(b.transform.position, armRight.transform.position))
        {
            return -1;
        }
        else if (Vector3.Distance(a.transform.position, armRight.transform.position) > Vector3.Distance(b.transform.position, armRight.transform.position))
        {
            return 1;
        }
        return 0;
    }
}
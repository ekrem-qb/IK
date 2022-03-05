using UnityEngine;

public class AutoAim : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR_Player;
    GunManager gunManager;
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
        gunManager = APR_Player.COMP.GetComponent<GunManager>();
        trigger = this.GetComponent<SphereCollider>();
        nearEnemies.CountChanged += OnEnemyCountChanged;
    }

    void FixedUpdate()
    {
        if (APR_Player.useControls)
        {
            if (gunManager.gunLeft)
            {
                nearEnemies.Sort(SortByDistanceToArmLeft);

                Debug.DrawLine(nearEnemies[0].transform.position, armLeft.transform.position, Color.red);

                Vector3 angles = (Quaternion.LookRotation(nearEnemies[0].transform.position - armLeft.transform.position) * Quaternion.Inverse(APR_Player.Root.transform.rotation)).eulerAngles;
                Quaternion newRot = Quaternion.Euler(angles.x - 35, angles.y - 270, angles.z);

                armLeft.targetRotation = newRot;
            }
            if (gunManager.gunRight)
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
        if (gunManager.gunLeft)
        {
            if (!APR_Player.punchingLeft)
            {
                armLeft.angularXDrive = APR_Player.ReachStiffness;
                armLeft.angularYZDrive = APR_Player.ReachStiffness;
                armLeftLow.angularXDrive = APR_Player.ReachStiffness;
                armLeftLow.angularYZDrive = APR_Player.ReachStiffness;
                armLeftLow.targetRotation = Quaternion.identity;

                gunManager.gunLeft.canShoot = true;
            }
        }
        if (gunManager.gunRight)
        {
            if (!APR_Player.punchingRight)
            {
                armRight.angularXDrive = APR_Player.ReachStiffness;
                armRight.angularYZDrive = APR_Player.ReachStiffness;
                armRightLow.angularXDrive = APR_Player.ReachStiffness;
                armRightLow.angularYZDrive = APR_Player.ReachStiffness;
                armRightLow.targetRotation = Quaternion.identity;

                gunManager.gunRight.canShoot = true;
            }
        }
    }

    void OnDisable()
    {
        if (gunManager.gunLeft)
        {
            armLeft.angularXDrive = APR_Player.PoseOn;
            armLeft.angularYZDrive = APR_Player.PoseOn;
            armLeftLow.angularXDrive = APR_Player.PoseOn;
            armLeftLow.angularYZDrive = APR_Player.PoseOn;
            armLeft.targetRotation = APR_Player.UpperLeftArmTarget;
            armLeftLow.targetRotation = APR_Player.LowerLeftArmTarget;

            gunManager.gunLeft.canShoot = false;
        }
        if (gunManager.gunRight)
        {
            armRight.angularXDrive = APR_Player.PoseOn;
            armRight.angularYZDrive = APR_Player.PoseOn;
            armRightLow.angularXDrive = APR_Player.PoseOn;
            armRightLow.angularYZDrive = APR_Player.PoseOn;
            armRight.targetRotation = APR_Player.UpperRightArmTarget;
            armRightLow.targetRotation = APR_Player.LowerRightArmTarget;

            gunManager.gunRight.canShoot = false;
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
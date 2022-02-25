using System;
using UnityEngine;

public class AutoAim : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR_Player;
    ConfigurableJoint armLeft;
    ConfigurableJoint armRight;
    ConfigurableJoint armLeftLow;
    ConfigurableJoint armRightLow;
    Gun gunLeft;
    Gun gunRight;
    public ObservableList<Enemy> enemyList = new ObservableList<Enemy>();

    void Awake()
    {
        APR_Player = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        armLeft = APR_Player.UpperLeftArm.GetComponent<ConfigurableJoint>();
        armRight = APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>();
        armLeftLow = APR_Player.LowerLeftArm.GetComponent<ConfigurableJoint>();
        armRightLow = APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>();
        gunLeft = armLeft.GetComponentInChildren<Gun>();
        gunRight = armRight.GetComponentInChildren<Gun>();
        enemyList.CountChanged += OnEnemyListChanged;
    }

    void FixedUpdate()
    {
        if (APR_Player.useControls)
        {
            if (enemyList.Count > 0)
            {
                OnEnable();
                if (!APR_Player.punchingLeft)
                {
                    enemyList.Sort(SortByDistanceToArmLeft);

                    Debug.DrawLine(enemyList[0].transform.position, armLeft.transform.position, Color.red);

                    Vector3 angles = (Quaternion.LookRotation(enemyList[0].transform.position - armLeft.transform.position) * Quaternion.Inverse(APR_Player.Root.transform.rotation)).eulerAngles;
                    Quaternion newRot = Quaternion.Euler(angles.x - 35, angles.y - 270, angles.z);

                    armLeft.targetRotation = newRot;
                }

                if (!APR_Player.punchingRight)
                {
                    enemyList.Sort(SortByDistanceToArmRight);

                    Debug.DrawLine(enemyList[0].transform.position, armRight.transform.position, Color.yellow);

                    Vector3 angles = (Quaternion.LookRotation(enemyList[0].transform.position - armRight.transform.position) * Quaternion.Inverse(APR_Player.Root.transform.rotation)).eulerAngles;
                    Quaternion newRot = Quaternion.Euler(angles.x - 35, angles.y - 90, angles.z);

                    armRight.targetRotation = newRot;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            if (!enemyList.Contains(enemy))
            {
                enemyList.Add(enemy);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemyList.Remove(enemy);
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

    void OnEnemyListChanged(object sender, EventArgs e)
    {
        this.enabled = enemyList.Count != 0;
    }

    void OnDisable()
    {
        APR_Player.ResetPose = true;

        armLeft.angularXDrive = APR_Player.PoseOn;
        armLeft.angularYZDrive = APR_Player.PoseOn;

        armRight.angularXDrive = APR_Player.PoseOn;
        armRight.angularYZDrive = APR_Player.PoseOn;

        armLeftLow.angularXDrive = APR_Player.PoseOn;
        armLeftLow.angularYZDrive = APR_Player.PoseOn;

        armRightLow.angularXDrive = APR_Player.PoseOn;
        armRightLow.angularYZDrive = APR_Player.PoseOn;

        if (gunLeft != null)
            gunLeft.enabled = false;
        if (gunRight != null)
            gunRight.enabled = false;
    }

    void OnEnable()
    {
        APR_Player.ResetPose = false;

        armLeft.angularXDrive = APR_Player.ReachStiffness;
        armLeft.angularYZDrive = APR_Player.ReachStiffness;

        armRight.angularXDrive = APR_Player.ReachStiffness;
        armRight.angularYZDrive = APR_Player.ReachStiffness;

        armLeftLow.angularXDrive = APR_Player.ReachStiffness;
        armLeftLow.angularYZDrive = APR_Player.ReachStiffness;

        armRightLow.angularXDrive = APR_Player.ReachStiffness;
        armRightLow.angularYZDrive = APR_Player.ReachStiffness;

        armLeftLow.targetRotation = Quaternion.identity;
        armRightLow.targetRotation = Quaternion.identity;

        if (gunLeft != null)
            gunLeft.enabled = true;
        if (gunRight != null)
            gunRight.enabled = true;
    }
}
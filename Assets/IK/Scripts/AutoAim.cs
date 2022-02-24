using System.Collections.Generic;
using UnityEngine;

public class AutoAim : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR_Player;
    ConfigurableJoint armLeft;
    ConfigurableJoint armRight;
    public List<Enemy> enemyList = new List<Enemy>();

    void Awake()
    {
        APR_Player = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        armLeft = APR_Player.UpperLeftArm.GetComponent<ConfigurableJoint>();
        armRight = APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>();
    }

    void FixedUpdate()
    {
        if (APR_Player.useControls)
        {
            if (enemyList.Count > 0)
            {
                if (!APR_Player.punchingLeft)
                {
                    enemyList.Sort(SortByDistanceToArmLeft);

                    Debug.DrawLine(enemyList[0].transform.position, armLeft.transform.position, Color.red);

                    Vector3 angles = (Quaternion.LookRotation(enemyList[0].transform.position - armLeft.transform.position) * APR_Player.Body.transform.rotation).eulerAngles;
                    Quaternion newRot = Quaternion.Euler(angles.x - 50, angles.y - 270, angles.z);

                    armLeft.targetRotation = newRot;
                    armLeft.angularXDrive = APR_Player.ReachStiffness;
                    armLeft.angularYZDrive = APR_Player.ReachStiffness;

                    APR_Player.LowerLeftArm.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.identity;
                }

                if (!APR_Player.punchingRight)
                {
                    enemyList.Sort(SortByDistanceToArmRight);

                    Debug.DrawLine(enemyList[0].transform.position, armRight.transform.position, Color.yellow);

                    Vector3 angles = (Quaternion.LookRotation(enemyList[0].transform.position - armRight.transform.position) * APR_Player.Body.transform.rotation).eulerAngles;
                    Quaternion newRot = Quaternion.Euler(angles.x - 50, angles.y - 90, angles.z);

                    armRight.targetRotation = newRot;
                    armRight.angularXDrive = APR_Player.ReachStiffness;
                    armRight.angularYZDrive = APR_Player.ReachStiffness;

                    APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>().targetRotation = Quaternion.identity;
                }
            }
            else
            {
                APR_Player.ResetPose = true;

                armLeft.angularXDrive = APR_Player.PoseOn;
                armLeft.angularYZDrive = APR_Player.PoseOn;

                armRight.angularXDrive = APR_Player.PoseOn;
                armRight.angularYZDrive = APR_Player.PoseOn;
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
}
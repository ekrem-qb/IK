using System.Collections.Generic;
using UnityEngine;

public class AutoAim : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR;
    GunManager gunManager;
    ConfigurableJoint armLeft, armRight;
    ConfigurableJoint armLeftLow, armRightLow;
    public Vector3 radarCenter = Vector3.zero;
    public float radarRadius = 15;
    public LayerMask enemiesLayerMask;
    public List<Collider> enemyList = new List<Collider>();

    void Awake()
    {
        APR = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        armLeft = APR.UpperLeftArm.GetComponent<ConfigurableJoint>();
        armRight = APR.UpperRightArm.GetComponent<ConfigurableJoint>();
        armLeftLow = APR.LowerLeftArm.GetComponent<ConfigurableJoint>();
        armRightLow = APR.LowerRightArm.GetComponent<ConfigurableJoint>();
        gunManager = this.GetComponent<GunManager>();
    }

    void FixedUpdate()
    {
        if (APR.useControls)
        {
            if (gunManager.gunLeft || gunManager.gunRight)
            {
                enemyList = new List<Collider>(Physics.OverlapSphere(radarCenter + this.transform.position, radarRadius, enemiesLayerMask));

                if (enemyList.Count > 0)
                {
                    if (gunManager.gunLeft)
                    {
                        if (!APR.punchingLeft)
                        {
                            armLeft.angularXDrive = APR.ReachStiffness;
                            armLeft.angularYZDrive = APR.ReachStiffness;
                            armLeftLow.angularXDrive = APR.ReachStiffness;
                            armLeftLow.angularYZDrive = APR.ReachStiffness;
                            armLeftLow.targetRotation = Quaternion.identity;

                            gunManager.gunLeft.canShoot = true;

                            enemyList.Sort(SortByDistanceToArmLeft);

                            Debug.DrawLine(enemyList[0].transform.position, armLeft.transform.position, Color.red);

                            Vector3 angles = (Quaternion.LookRotation(enemyList[0].transform.position - armLeft.transform.position) * Quaternion.Inverse(APR.Root.transform.rotation)).eulerAngles;
                            Quaternion newRot = Quaternion.Euler(angles.x - 35, angles.y - 270, angles.z);

                            armLeft.targetRotation = newRot;
                        }
                    }

                    if (gunManager.gunRight)
                    {
                        if (!APR.punchingRight)
                        {
                            armRight.angularXDrive = APR.ReachStiffness;
                            armRight.angularYZDrive = APR.ReachStiffness;
                            armRightLow.angularXDrive = APR.ReachStiffness;
                            armRightLow.angularYZDrive = APR.ReachStiffness;
                            armRightLow.targetRotation = Quaternion.identity;

                            gunManager.gunRight.canShoot = true;

                            enemyList.Sort(SortByDistanceToArmRight);

                            Debug.DrawLine(enemyList[0].transform.position, armRight.transform.position, Color.yellow);

                            Vector3 angles = (Quaternion.LookRotation(enemyList[0].transform.position - armRight.transform.position) * Quaternion.Inverse(APR.Root.transform.rotation)).eulerAngles;
                            Quaternion newRot = Quaternion.Euler(angles.x - 35, angles.y - 90, angles.z);

                            armRight.targetRotation = newRot;
                        }
                    }
                }
                else
                {
                    if (gunManager.gunLeft)
                    {
                        armLeft.angularXDrive = APR.PoseOn;
                        armLeft.angularYZDrive = APR.PoseOn;
                        armLeftLow.angularXDrive = APR.PoseOn;
                        armLeftLow.angularYZDrive = APR.PoseOn;
                        armLeft.targetRotation = APR.UpperLeftArmTarget;
                        armLeftLow.targetRotation = APR.LowerLeftArmTarget;

                        gunManager.gunLeft.canShoot = false;
                    }
                    if (gunManager.gunRight)
                    {
                        armRight.angularXDrive = APR.PoseOn;
                        armRight.angularYZDrive = APR.PoseOn;
                        armRightLow.angularXDrive = APR.PoseOn;
                        armRightLow.angularYZDrive = APR.PoseOn;
                        armRight.targetRotation = APR.UpperRightArmTarget;
                        armRightLow.targetRotation = APR.LowerRightArmTarget;

                        gunManager.gunRight.canShoot = false;
                    }
                }
            }
        }
    }

    int SortByDistanceToArmLeft(Collider a, Collider b)
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

    int SortByDistanceToArmRight(Collider a, Collider b)
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(radarCenter + this.transform.position, radarRadius);
    }
}
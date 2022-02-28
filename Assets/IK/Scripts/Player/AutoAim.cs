using System.Collections.Generic;
using UnityEngine;

public class AutoAim : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR_Player;
    GunManager gunManager;
    ConfigurableJoint armLeft, armRight;
    ConfigurableJoint armLeftLow, armRightLow;
    public Vector3 radarCenter = Vector3.zero;
    public float radarRadius = 15;
    public LayerMask enemiesLayerMask;
    public List<Collider> enemyList = new List<Collider>();

    void Awake()
    {
        APR_Player = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        armLeft = APR_Player.UpperLeftArm.GetComponent<ConfigurableJoint>();
        armRight = APR_Player.UpperRightArm.GetComponent<ConfigurableJoint>();
        armLeftLow = APR_Player.LowerLeftArm.GetComponent<ConfigurableJoint>();
        armRightLow = APR_Player.LowerRightArm.GetComponent<ConfigurableJoint>();
        gunManager = this.GetComponent<GunManager>();
    }

    void FixedUpdate()
    {
        if (APR_Player.useControls)
        {
            if (gunManager.gunLeft != null || gunManager.gunRight != null)
            {
                enemyList = new List<Collider>(Physics.OverlapSphere(radarCenter + this.transform.position, radarRadius, enemiesLayerMask));

                if (enemyList.Count > 0)
                {
                    if (gunManager.gunLeft != null)
                    {
                        if (!APR_Player.punchingLeft)
                        {
                            armLeft.angularXDrive = APR_Player.ReachStiffness;
                            armLeft.angularYZDrive = APR_Player.ReachStiffness;
                            armLeftLow.angularXDrive = APR_Player.ReachStiffness;
                            armLeftLow.angularYZDrive = APR_Player.ReachStiffness;
                            armLeftLow.targetRotation = Quaternion.identity;

                            gunManager.gunLeft.canShoot = true;

                            enemyList.Sort(SortByDistanceToArmLeft);

                            Debug.DrawLine(enemyList[0].transform.position, armLeft.transform.position, Color.red);

                            Vector3 angles = (Quaternion.LookRotation(enemyList[0].transform.position - armLeft.transform.position) * Quaternion.Inverse(APR_Player.Root.transform.rotation)).eulerAngles;
                            Quaternion newRot = Quaternion.Euler(angles.x - 35, angles.y - 270, angles.z);

                            armLeft.targetRotation = newRot;
                        }
                    }

                    if (gunManager.gunRight != null)
                    {
                        if (!APR_Player.punchingRight)
                        {
                            armRight.angularXDrive = APR_Player.ReachStiffness;
                            armRight.angularYZDrive = APR_Player.ReachStiffness;
                            armRightLow.angularXDrive = APR_Player.ReachStiffness;
                            armRightLow.angularYZDrive = APR_Player.ReachStiffness;
                            armRightLow.targetRotation = Quaternion.identity;

                            gunManager.gunRight.canShoot = true;

                            enemyList.Sort(SortByDistanceToArmRight);

                            Debug.DrawLine(enemyList[0].transform.position, armRight.transform.position, Color.yellow);

                            Vector3 angles = (Quaternion.LookRotation(enemyList[0].transform.position - armRight.transform.position) * Quaternion.Inverse(APR_Player.Root.transform.rotation)).eulerAngles;
                            Quaternion newRot = Quaternion.Euler(angles.x - 35, angles.y - 90, angles.z);

                            armRight.targetRotation = newRot;
                        }
                    }
                }
                else
                {
                    if (gunManager.gunLeft != null)
                    {
                        armLeft.angularXDrive = APR_Player.PoseOn;
                        armLeft.angularYZDrive = APR_Player.PoseOn;
                        armLeftLow.angularXDrive = APR_Player.PoseOn;
                        armLeftLow.angularYZDrive = APR_Player.PoseOn;
                        armLeft.targetRotation = APR_Player.UpperLeftArmTarget;
                        armLeftLow.targetRotation = APR_Player.LowerLeftArmTarget;

                        gunManager.gunLeft.canShoot = false;
                    }
                    if (gunManager.gunRight != null)
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
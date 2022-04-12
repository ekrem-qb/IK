using UnityEngine;

public class Player : MonoBehaviour
{
    private ARP.APR.Scripts.APRController _aprController;
    private ConfigurableJoint _armLeft, _armRight;
    private ConfigurableJoint _armLeftLow, _armRightLow;
    private SphereCollider _trigger;
    private WeaponManager _weaponManager;
    public ObservableList<Enemy> nearEnemies = new ObservableList<Enemy>();

    private void Awake()
    {
        _aprController = this.transform.root.GetComponent<ARP.APR.Scripts.APRController>();
        _armLeft = _aprController.UpperLeftArm.GetComponent<ConfigurableJoint>();
        _armRight = _aprController.UpperRightArm.GetComponent<ConfigurableJoint>();
        _armLeftLow = _aprController.LowerLeftArm.GetComponent<ConfigurableJoint>();
        _armRightLow = _aprController.LowerRightArm.GetComponent<ConfigurableJoint>();
        _weaponManager = _aprController.COMP.GetComponent<WeaponManager>();
        _trigger = this.GetComponent<SphereCollider>();
        nearEnemies.CountChanged += count => this.enabled = count > 0;
    }

    private void FixedUpdate()
    {
        if (_aprController.useControls)
        {
            if (_weaponManager.weaponLeft is Gun || _weaponManager.weaponRight is Gun)
            {
                Vector3 bodyBendingFactor = new Vector3(_aprController.Body.transform.eulerAngles.x, _aprController.Root.transform.localEulerAngles.y, 0);
                Enemy nearestToArmLeft = null;

                if (_weaponManager.weaponLeft is Gun)
                {
                    nearEnemies.SortByDistanceTo(_armLeft.transform.position);
                    nearestToArmLeft = nearEnemies[0];

                    Debug.DrawLine(nearestToArmLeft.target.position, _armLeft.transform.position, Color.red);

                    Vector3 angles = Quaternion.LookRotation(nearestToArmLeft.target.position - _armLeft.transform.position).eulerAngles;
                    angles -= bodyBendingFactor;
                    Quaternion newRot = Quaternion.Euler(angles.x, angles.y - 270, angles.z);

                    _armLeft.targetRotation = newRot;
                }

                if (_weaponManager.weaponRight is Gun)
                {
                    nearEnemies.SortByDistanceTo(_armRight.transform.position);
                    Enemy nearestToArmRight = nearEnemies[0];
                    if (nearEnemies.Count > 1)
                    {
                        if (nearestToArmRight == nearestToArmLeft)
                        {
                            nearestToArmRight = nearEnemies[1];
                        }
                    }

                    Debug.DrawLine(nearestToArmRight.target.position, _armRight.transform.position, Color.yellow);

                    Vector3 angles = Quaternion.LookRotation(nearestToArmRight.target.position - _armRight.transform.position).eulerAngles;
                    angles -= bodyBendingFactor;
                    Quaternion newRot = Quaternion.Euler(angles.x, angles.y - 90, angles.z);

                    _armRight.targetRotation = newRot;
                }
            }
        }
    }

    private void OnEnable()
    {
        if (_weaponManager.weaponLeft is Gun)
        {
            if (!_aprController.punchingLeft)
            {
                _armLeft.angularXDrive = _aprController.ReachStiffness;
                _armLeft.angularYZDrive = _aprController.ReachStiffness;
                _armLeftLow.angularXDrive = _aprController.ReachStiffness;
                _armLeftLow.angularYZDrive = _aprController.ReachStiffness;
                _armLeftLow.targetRotation = Quaternion.identity;
            }
        }

        if (_weaponManager.weaponRight is Gun)
        {
            if (!_aprController.punchingRight)
            {
                _armRight.angularXDrive = _aprController.ReachStiffness;
                _armRight.angularYZDrive = _aprController.ReachStiffness;
                _armRightLow.angularXDrive = _aprController.ReachStiffness;
                _armRightLow.angularYZDrive = _aprController.ReachStiffness;
                _armRightLow.targetRotation = Quaternion.identity;
            }
        }
    }

    private void OnDisable()
    {
        if (_weaponManager.weaponLeft is Gun)
        {
            _armLeft.angularXDrive = _aprController.DriveOff;
            _armLeft.angularYZDrive = _aprController.DriveOff;
            _armLeftLow.angularXDrive = _aprController.DriveOff;
            _armLeftLow.angularYZDrive = _aprController.DriveOff;
        }

        if (_weaponManager.weaponRight is Gun)
        {
            _armRight.angularXDrive = _aprController.DriveOff;
            _armRight.angularYZDrive = _aprController.DriveOff;
            _armRightLow.angularXDrive = _aprController.DriveOff;
            _armRightLow.angularYZDrive = _aprController.DriveOff;
        }
    }

    private void OnDestroy()
    {
        nearEnemies.ForEach(enemy => enemy.player = null);
    }

    private void OnTriggerEnter(Collider other)
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

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger)
        {
            if (_trigger.radius <= Vector3.Distance(this.transform.position, other.transform.position))
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
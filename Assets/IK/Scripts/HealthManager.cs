using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    ARP.APR.Scripts.APRController APR_Controller;
    AutoAim player;
    Enemy enemy;
    WeaponManager weaponManager;
    [SerializeField]
    private float _health = 100;
    public float health
    {
        get => _health;
        set
        {
            if (value >= 0)
            {
                _health = value;
                if (textHealth)
                {
                    textHealth.text = _health.ToString();
                }
                if (value == 0)
                {
                    Death();
                }
            }
        }
    }
    public Text textHealth;

    void Awake()
    {
        if (textHealth)
        {
            textHealth.text = _health.ToString();
        }
        APR_Controller = this.GetComponent<ARP.APR.Scripts.APRController>();
        player = APR_Controller.Root.GetComponent<AutoAim>();
        enemy = APR_Controller.Root.GetComponent<Enemy>();
        weaponManager = APR_Controller.COMP.GetComponent<WeaponManager>();
    }

    void Death()
    {
        APR_Controller.ActivateRagdoll();
        APR_Controller.autoGetUpWhenPossible = false;
        APR_Controller.useControls = false;
        APR_Controller.useStepPrediction = false;
        if (player)
        {
            Destroy(player);
        }
        if (enemy)
        {
            Destroy(enemy);
        }
        if (weaponManager)
        {
            Destroy(weaponManager);
        }
    }
}
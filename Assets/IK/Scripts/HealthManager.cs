using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    ARP.APR.Scripts.APRController aprController;
    Player player;
    Enemy enemy;
    WeaponManager weaponManager;
    [Range(0, 100)] [SerializeField] float _health = 100;

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

        aprController = this.GetComponent<ARP.APR.Scripts.APRController>();
        player = aprController.Root.GetComponent<Player>();
        enemy = aprController.Root.GetComponent<Enemy>();
        weaponManager = aprController.COMP.GetComponent<WeaponManager>();
    }

    void Death()
    {
        aprController.ActivateRagdoll();
        aprController.autoGetUpWhenPossible = false;
        aprController.useControls = false;
        aprController.useStepPrediction = false;
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
            weaponManager.DropAllWeapons();
            Destroy(weaponManager);
        }
    }
}
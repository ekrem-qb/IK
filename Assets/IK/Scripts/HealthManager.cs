using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [SerializeField]
    private float _health = 100;
    public float health
    {
        get => _health;
        set
        {
            if (value > 0)
            {
                _health = value;
                if (textHealth)
                {
                    textHealth.text = _health.ToString();
                }
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
    }
    public Text textHealth;
    [HideInInspector]

    void Awake()
    {
        if (textHealth)
        {
            textHealth.text = _health.ToString();
        }
    }
}
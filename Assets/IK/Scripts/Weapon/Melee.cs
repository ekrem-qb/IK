using UnityEngine;

public class Melee : Weapon
{
    void OnTriggerEnter(Collider other)
    {
        if (this.enabled)
        {
            if (!other.isTrigger)
            {
                if (this.transform.root != other.transform.root)
                {
                    HealthManager enemy = other.transform.root.GetComponent<HealthManager>();
                    if (enemy)
                    {
                        // enemy.health -= 10;
                    }
                }
            }
        }
    }
}

using System.Collections;

public class Target : Enemy
{
    protected override void Awake()
    {
        target = this.transform;
    }

    protected override void FixedUpdate()
    {
    }

    protected override IEnumerator Attack()
    {
        yield return null;
    }

    protected override void OnDrawGizmosSelected()
    {
    }

    protected override void OnPlayerChanged(Player newPlayer)
    {
    }

    protected override void OnDisable()
    {
    }
}
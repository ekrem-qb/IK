using System.Collections;

public class Target : Enemy
{
	protected override void Awake()
	{
		selfTarget = this.transform;
	}

	protected override void FixedUpdate()
	{
	}

	protected override void OnDisable()
	{
	}

	protected override void OnDrawGizmosSelected()
	{
	}

	protected override IEnumerator Attack()
	{
		yield return null;
	}

	protected override void OnPlayerChanged(Player newPlayer)
	{
	}
}
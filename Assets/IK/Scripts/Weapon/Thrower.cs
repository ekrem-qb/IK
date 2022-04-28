using System.Collections;
using UnityEngine;

public class Thrower : Melee
{
	[Header("Thrower")] public GameObject projectilePrefab;

	[HideInInspector] public MeshRenderer meshRenderer;

	protected override void Awake()
	{
		base.Awake();
		meshRenderer = this.GetComponent<MeshRenderer>();
	}

	public override void Drop()
	{
		meshRenderer.enabled = false;
	}

	public void Attack(Vector3 target)
	{
		if (!isInHook)
		{
			StartCoroutine(Hook());
			StartCoroutine(Throw(target));
		}
	}

	private IEnumerator Throw(Vector3 target)
	{
		yield return new WaitUntil(() => isAttacking);
		Knife knife = Instantiate(projectilePrefab, this.transform.position, Quaternion.identity).GetComponent<Knife>();
		knife.target = target;
		knife.owner = this.transform.root;
	}
}
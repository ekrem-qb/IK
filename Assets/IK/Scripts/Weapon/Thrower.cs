using System.Collections;
using UnityEngine;

public class Thrower : Melee
{
    public GameObject projectilePrefab;
    [HideInInspector] public MeshRenderer meshRenderer;

    protected override void Awake()
    {
        base.Awake();
        meshRenderer = this.GetComponent<MeshRenderer>();
    }

    public override void Attack()
    {
        Attack(Vector3.zero);
    }

    public void Attack(Vector3 target)
    {
        if (!isInHook)
        {
            StartCoroutine(Hook());
            StartCoroutine(Throw(target));
        }
    }

    IEnumerator Throw(Vector3 target)
    {
        yield return new WaitUntil(() => isAttacking);
        Projectile projectile = Instantiate(projectilePrefab, this.transform.position, Quaternion.identity).GetComponent<Projectile>();
        projectile.target = target;
        projectile.owner = this.transform.root;
    }
}
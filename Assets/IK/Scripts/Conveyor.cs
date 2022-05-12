using UnityEngine;

public class Conveyor : MonoBehaviour
{
	public float speed = 20;
	public Vector3 direction = Vector3.forward;

	[Tooltip("Force directed back to the moving direction of rigidbody on conveyor")] [Range(0, 1)]
	public float antiInertia = 0.5f;

	public bool scrollTexture;
	private readonly Gizmos _gizmos = new Gizmos();
	private Material _material;
	private Renderer _meshRenderer;

	private void Awake()
	{
		if (scrollTexture)
		{
			_meshRenderer = this.GetComponent<Renderer>();
			if (_meshRenderer)
			{
				_material = _meshRenderer.material;
			}
		}
	}

	private void Update()
	{
		if (scrollTexture)
		{
			if (_material)
			{
				Vector2 offset = _material.mainTextureOffset;

				if (offset.x >= 1 || offset.x <= -1)
				{
					offset.x = 0;
				}

				if (offset.y >= 1 || offset.y <= -1)
				{
					offset.y = 0;
				}

				offset.x += direction.x * speed * 0.1f * Time.deltaTime;
				offset.y += direction.z * speed * 0.1f * Time.deltaTime;

				_material.mainTextureOffset = offset;
			}
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.transform.root != this.transform.root)
		{
			if (collision.rigidbody)
			{
				collision.rigidbody.velocity = Vector3.Slerp(collision.rigidbody.velocity, this.transform.rotation * direction * collision.rigidbody.velocity.magnitude, antiInertia);
				collision.rigidbody.AddForce(this.transform.rotation * direction * speed, ForceMode.Acceleration);
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (this.transform.rotation * direction != Vector3.zero)
		{
			Gizmos.color = Color.magenta;
			_gizmos.DrawArrow(this.transform.position, this.transform.rotation * direction * speed / 2);
		}
	}
}
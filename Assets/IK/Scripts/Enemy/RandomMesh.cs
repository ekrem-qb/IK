using System.Collections.Generic;
using UnityEngine;

public class RandomMesh : MonoBehaviour
{
	public List<Renderer> meshes = new List<Renderer>();

	private void Awake()
	{
		if (meshes.Count == 0)
		{
			meshes.AddRange(this.transform.GetComponentsInChildren<Renderer>());
		}

		if (meshes.Count > 0)
		{
			int randomMeshIndex = Random.Range(0, meshes.Count);
			for (int i = 0; i < meshes.Count; i++)
			{
				if (i != randomMeshIndex)
				{
					Destroy(meshes[i].gameObject);
				}
			}
		}
	}
}
using System.Collections.Generic;
using UnityEngine;

public class RandomSkin : MonoBehaviour
{
	public int materialIndexInRenderer = 1;
	public List<Texture> textures = new List<Texture>();
	private Renderer _meshRenderer;

	private void Awake()
	{
		if (textures.Count > 0)
		{
			_meshRenderer = this.GetComponent<Renderer>();
			int randomTextureIndex = Random.Range(0, textures.Count);
			Material material = _meshRenderer.materials[materialIndexInRenderer];
			material.mainTexture = textures[randomTextureIndex];
		}
	}
}
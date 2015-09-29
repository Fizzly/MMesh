using UnityEngine;
using System.Collections;

public class MeshCreator
{
	public static Mesh CreatePlane()
	{
		return CreatePlane (1f, 1f, 0f, 1f);
	}

	public static Mesh CreatePlane(float width, float height, float uvMin, float uvMax)
	{
		Mesh m = new Mesh();
		m.name = "Mesh";
		m.vertices = new Vector3[] {
			new Vector3(-width, -height, 0.01f),
			new Vector3(width, -height, 0.01f),
			new Vector3(width, height, 0.01f),
			new Vector3(-width, height, 0.01f)
		};
		m.uv = new Vector2[] {
			new Vector2 (uvMin, uvMin),
			new Vector2 (uvMin, uvMax),
			new Vector2 (uvMax, uvMax),
			new Vector2 (uvMax, uvMin)
		};
		m.triangles = new int[] { 0, 1, 2, 0, 2, 3};
		m.RecalculateNormals();
		
		return m;
	}
}

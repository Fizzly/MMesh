﻿using UnityEngine;
using System.Collections;

public class Rasterizer : MonoBehaviour
{
	private Mesh referencePlaneMesh;
	private Mesh rasterizedPlaneMesh;
	private GameObject referenceGameObject;
	private GameObject rasterizedGameObject;

	public Mesh customMesh;

	// Use this for initialization
	void Start ()
	{
		referencePlaneMesh = MeshCreator.CreatePlane ();
		rasterizedPlaneMesh = MeshCreator.CreatePlane (1f, 1f, 0.3343f, 0.6345f);

		referenceGameObject = CreateGameobjectWithMesh ("ReferenceObject", referencePlaneMesh);
		rasterizedGameObject = CreateGameobjectWithMesh ("RasterizedObject", customMesh);

		referenceGameObject.renderer.material.mainTexture = RasterizeMesh (customMesh, 128);
		rasterizedGameObject.renderer.material.mainTexture = referenceGameObject.renderer.material.mainTexture;
	}


	private Texture2D RasterizeMesh ( Mesh mesh, int resolution)
	{

		Texture2D tex = new Texture2D (resolution, resolution);
		Vector2 vt1;
		Vector2 vt2;
		Vector2 vt3;
				   

		Vector2 vs1;
		Vector2 vs2;
		Vector2 q;


		float step = (1f / resolution);
		float doubleStep = (step * resolution) +8f;

		doubleStep = 0f;
		Vector2[] meshUVs = mesh.uv;
		Vector3[] meshNormals = mesh.normals;
		int[] meshTriangles = mesh.triangles;



		Vector3[] verts = mesh.vertices;
		Vector2[] uvs = mesh.uv;
		Vector3[] normals = mesh.normals;
		Vector4[] tangents = mesh.tangents;
		int[] tris = mesh.triangles;


		for (int triangle = 0; triangle < meshTriangles.Length; triangle +=3) {


			vt1 = meshUVs[meshTriangles[triangle]];
			vt2 = meshUVs[meshTriangles[triangle+1]];
			vt3 = meshUVs[meshTriangles[triangle+2]];

			/* get the bounding box of the triangle */
			float minX = Mathf.Min (vt1.x, Mathf.Min (vt2.x, vt3.x));
			float maxY = Mathf.Max (vt1.y, Mathf.Max (vt2.y, vt3.y));
			float minY = Mathf.Min (vt1.y, Mathf.Min (vt2.y, vt3.y));
			float maxX = Mathf.Max (vt1.x, Mathf.Max (vt2.x, vt3.x));


			//Debug.Log (minX+ " , " +maxX + " , " +minY + " , " + maxY);


			/* spanning vectors of edge (v1,v2) and (v1,v3) */
			vs1 = new Vector2(vt2.x - vt1.x, vt2.y - vt1.y);
			vs2 = new Vector2(vt3.x - vt1.x, vt3.y - vt1.y);


			// Crop rect to size of texture and iterate through every pixel in that rect
			int pixMinX = (int)Mathf.Max(Mathf.Floor(minX*resolution)-1, 0); int pixMaxX = (int)Mathf.Min(Mathf.Ceil(maxX*resolution)+1, resolution); 
			int pixMinY = (int)Mathf.Max(Mathf.Floor(minY*resolution)-1, 0); int pixMaxY = (int)Mathf.Min(Mathf.Ceil(maxY*resolution)+1, resolution);

			Vector3 P1 = meshNormals[meshTriangles[triangle]];
			Vector3 P2 = meshNormals[meshTriangles[triangle+1]];
			Vector3 P3 = meshNormals[meshTriangles[triangle+2]];

			//Prepare variables for this triangle
			Vector3 v0 = verts[tris[triangle  ]];	Vector3 n0 = normals[tris[triangle  ]];  Vector4 tt0 = tangents[tris[triangle  ]];
			Vector3 v1 = verts[tris[triangle+1]];	Vector3 n1 = normals[tris[triangle+1]];  Vector4 tt1 = tangents[tris[triangle+1]];
			Vector3 v2 = verts[tris[triangle+2]];	Vector3 n2 = normals[tris[triangle+2]];  Vector4 tt2 = tangents[tris[triangle+2]];

			//HeronsForumula finds the area of a triangle based on its side lengths (The vertex positions are used as inputs here)
			float area = HeronsForumula(vt1, vt2, vt3);

			for(int x = pixMinX; x < pixMaxX; x++) {
				for(int y = pixMinY; y < pixMaxY; y++) {



					Vector2 pixel = new Vector2(((float)x)*step, ((float)y)*step);
					//Early rejection of pixel if outside of triangle or outside of texture.
					
					if(Vector2.Angle(vt1-pixel, vt2-pixel) + Vector2.Angle(vt2-pixel, vt3-pixel) + Vector2.Angle(vt3-pixel, vt1-pixel) > 359.9)
					{
				
						//For constructing the barycentric coordinate of this pixel within this triangle, we use the area of the main triangle, and the areas of the
						// triangles that result when we subdivide the triangle by drawing lines from each of its vertices to the pixel.
						
						//Find the areas of the triangles formed by this subdivision, indexed such that the triangle is across from, not next to, its vertex
						float a0 = HeronsForumula(pixel, vt2, vt3);
						float a1 = HeronsForumula(vt1, pixel, vt3);
						float a2 = HeronsForumula(vt1, vt2, pixel);
						
						// the position of the pixel is composed of the position of each vertex. the percentage or "wieght" of each vertex in this position
						// is obtained by taking the ratio of the opposing triangle's area to the total area
						Vector3 bary = new Vector3(a0/area, a1/area, a2/area);
						
						// prepare Tangent Space Transformation Matrix
						Vector3 pixelZ = (bary.x*n0+bary.y*n1+bary.z*n2).normalized; // normal
						Vector4 tangent = bary.x*tt0+bary.y*tt1+bary.z*tt2;
						Vector3 pixelX = new Vector3(tangent.x, tangent.y, tangent.z).normalized; // tangent
						Vector3 pixelY = Vector3.Cross(pixelZ, pixelX);	 // binormal


						Vector3 tangentSpaceNormal = new Vector3(0.5f, 0.5f, 0f);
						tangentSpaceNormal.z = Mathf.Sqrt(1f - tangentSpaceNormal.x*tangentSpaceNormal.x - tangentSpaceNormal.y*tangentSpaceNormal.y);
					
					
						// Simply multiply to get world space normal!
					
						Vector3 worldNormal = tangentSpaceNormal.x*pixelX + tangentSpaceNormal.y*pixelY + tangentSpaceNormal.z*pixelZ;
					
	//					worldSpaceNormal.SetPixel(x, y, new Color(worldNormal.x*0.5f + 0.5f, worldNormal.y*0.5f + 0.5f, worldNormal.z*0.5f + 0.5f));



						tex.SetPixel (x,y,  new Color(tangentSpaceNormal.x, tangentSpaceNormal.y, tangentSpaceNormal.z));

					}

						              continue;


					q = new Vector2(x - vt1.x, y - vt1.y);
					
					float t = (cross(vs1, q) / cross(vs1, vs2));
					float s = (cross(q, vs2) / cross(vs1, vs2));

					//Debug.Log (s + " , " + t);



					if ( (s >= 0) && (t >= 0) && (s + t <= 1))
					{

						Vector3 faceNormal=Vector3.Normalize((P1 + P2 + P3) / 3);


						Color color;
						if((triangle % 2) == 1)
						{
							color = Color.red;

						}
						else
							color = Color.red;

						//color = new Color( faceNormal.x * 0.5f + 0.5f, faceNormal.y* 0.5f + 0.5f,  faceNormal.z* 0.5f + 0.5f);

						tex.SetPixel (x,y,  color);
					}
				
				}
			}

		}

		tex.Apply ();

		tex.filterMode = FilterMode.Point;

		return tex;
	}
	

	float HeronsForumula (Vector2 a, Vector2 b, Vector2 c) {
		float ab = (a-b).magnitude;
		float bc = (b-c).magnitude;
		float ca = (c-a).magnitude;
		float s = 0.5f * (ab + bc + ca);
		return Mathf.Sqrt(s*(s-ab)*(s-bc)*(s-ca));				
	}

	float cross(Vector2 a, Vector2 b)
	{
		// just calculate the z-component
		return a.x * b.y - a.y * b.x;
	}

	private GameObject CreateGameobjectWithMesh(string name, Mesh mesh)
	{
		GameObject plane = new GameObject (name);
		MeshFilter meshFilter = (MeshFilter)plane.AddComponent (typeof(MeshFilter));
		meshFilter.mesh = mesh;
		MeshRenderer renderer = plane.AddComponent (typeof(MeshRenderer)) as MeshRenderer;

		Texture2D tex = new Texture2D (1, 1);
		tex.SetPixel (0, 0, Color.green);
		tex.Apply ();

		renderer.material.shader =  Shader.Find ("Unlit/Texture");
		//renderer.material.mainTexture = tex;

		return plane;
	}
}
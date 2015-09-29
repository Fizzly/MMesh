using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MeshUtility;

public class DebugMeshUtility : MonoBehaviour
{
    int id = 0;
    MMesh mesh;

	public Texture2D texture;

	void Start ()
    {
        //mesh = new MMesh(gameObject.GetComponent<MeshFilter>().mesh);

        //List<MVertex> triangle1= new List<MVertex>();
        //triangle1.Add(new MVertex(new Vector3(0f, 0f, 0f), Vector3.down));
        //triangle1.Add(new MVertex(new Vector3(1f, 0f, 0f), Vector3.down));
        //triangle1.Add(new MVertex(new Vector3(1f, 0f, 1f), Vector3.down));

        //List<MVertex> triangle2 = new List<MVertex>();
        //triangle2.Add(new MVertex(new Vector3(1f, 1f, 0f), Vector3.right));
        //triangle2.Add(new MVertex(new Vector3(1f, 0f, 0f), Vector3.right));
        //triangle2.Add(new MVertex(new Vector3(1f, 0f, 1f), Vector3.right));

        //MTriangle ta = new MTriangle(mesh,triangle1);
        //MTriangle tb = new MTriangle(mesh, triangle2);


		mesh = new MMesh(gameObject.GetComponent<MeshFilter>().mesh);
		//texture = new Texture2D(64,64);
		//RenderUtility.RenderToTexture(mesh,Color.black,texture);

		//this.renderer.material.mainTexture = texture;

        

       // Debug.Log(mesh.Vertices.Count);

	}

    void Update()
    {
        if(Input.GetKey(KeyCode.C))
			mesh.ShowTriangleConnections(id);

        if (Input.GetKeyUp(KeyCode.UpArrow))
            id += 1;
        if (Input.GetKeyUp(KeyCode.DownArrow))
            id -= 1;

        if(Input.GetKey(KeyCode.D))
            mesh.Debug();
    }

}

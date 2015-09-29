using UnityEngine;
using System.Collections;
using MeshUtility;

public class Main : MonoBehaviour
{

    private Mesh referencePlaneMesh;
    
    public Mesh rasterizedPlaneMesh;

    private GameObject referenceGameObject;
    private GameObject rasterizedGameObject;


    private MMesh currentMesh;

    private Rasterizer rasterizer;

    int id = 0;

	void Start ()
    {

        rasterizer = new Rasterizer();

        referencePlaneMesh = MeshCreator.CreatePlane();
       // rasterizedPlaneMesh = MeshCreator.CreatePlane(1f, 1f, 0.3343f, 0.6345f);

        currentMesh = new MMesh(rasterizedPlaneMesh);

        referenceGameObject = CreateGameobjectWithMesh("ReferenceObject", referencePlaneMesh);
        rasterizedGameObject = CreateGameobjectWithMesh("RasterizedObject", rasterizedPlaneMesh);

        currentMesh.ApplyPadding((1f / 512) * 8f);

        referenceGameObject.renderer.material.mainTexture = rasterizer.RasterizeMesh(currentMesh, 512);
        rasterizedGameObject.renderer.material.mainTexture = referenceGameObject.renderer.material.mainTexture;
	}


    void Update()
    {
        if (Input.GetKey(KeyCode.C))
            currentMesh.ShowTriangleConnections(id);

       // if (Input.GetKey(KeyCode.C))
        //    referenceGameObject.renderer.material.mainTexture = rasterizer.RasterizeMesh(currentMesh, 512, id);

        if (Input.GetKeyUp(KeyCode.UpArrow))
            id += 1;
        if (Input.GetKeyUp(KeyCode.DownArrow))
            id -= 1;

        if (Input.GetKey(KeyCode.D))
            currentMesh.Debug();
    }

    private GameObject CreateGameobjectWithMesh(string name, Mesh mesh)
    {
        GameObject plane = new GameObject(name);
        MeshFilter meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = mesh;
        MeshRenderer renderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.green);
        tex.Apply();

        renderer.material.shader = Shader.Find("Unlit/Texture");
        //renderer.material.mainTexture = tex;

        return plane;
    }
}

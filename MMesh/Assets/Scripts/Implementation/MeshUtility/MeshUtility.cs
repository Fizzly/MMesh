using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core;

namespace MeshUtility
{
    public class MVertex 
    {

		public override bool Equals (object other)
		{
			if (!(other is MVertex))
			{
				return false;
			}
			MVertex vertex = (MVertex)other;
			
			//if(!normal.Equals(vertex.normal))
				//return false;
			if(!position.Equals(vertex.position))
				return false;

			return true;
		}
		public override int GetHashCode ()
		{
			return normal.GetHashCode() + position.GetHashCode ();
		}

        //Properties
        private Vector3 position;
        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        private Vector3 normal;
        public Vector3 Normal
        {
            get { return normal; }
            set { normal = value; }
        }

        private MUVVertex uv;

		public MUVVertex Uv {
			get {
				return uv;
			}
		}

        private Color color;

        public MVertex(Vector3 position, Vector3 normal)
        {
            this.position = position;
            this.normal = normal;
            this.parents = new List<MTriangle>();
        }

		public MVertex(Vector3 position, Vector3 normal, Vector3 uv)
        {
            this.position = position;
            this.normal = normal;
			this.uv = new MUVVertex(new Vector2(uv.x,uv.y));
            this.parents = new List<MTriangle>();
        }

        //Relation
        private List<MTriangle> parents;
        public List<MTriangle> Parents
        {
            get { return parents; }
            set { parents = value; }
        }

        public void Debug()
        {
			UnityEngine.Debug.DrawRay(position, normal * 0.1f, Color.magenta);
        }
    }

	public class MTriangle
	{

		public override bool Equals (object other)
		{
			if (!(other is MTriangle))
			{
				return false;
			}
			MTriangle triangle = (MTriangle)other;

			if(!normal.Equals(triangle.normal))
				return false;
			return true;
		}

		public override int GetHashCode ()
		{
			return normal.GetHashCode () + center.GetHashCode () ;
		}


        //Properties
        private List<MVertex> vertices;

		public List<MVertex> Vertices {
			get {return vertices;}
		}

        private Vector3 normal;

        //Relation
        private List<MTriangle> connections;
        private MMesh parent;

        private Bounds bounds;
        public Bounds Bounds
        {
            get 
            {
                RecalculateBounds();
                return bounds;
            }
        }

        private Vector3 center;
        public Vector3 Center
        {
            get { return center; }
        }

        public MTriangle(MMesh parent,List<MVertex> vertices)
        {
            this.parent = parent;

            this.vertices = new List<MVertex>();
            this.connections = new List<MTriangle>();

            for (int i = 0; i < 3;i++)
            {
                MVertex vertex = vertices[i];
                normal += vertex.Normal;
                center += vertex.Position;

                if (parent.Vertices.Contains(vertex))
                {
                    int index = parent.Vertices.IndexOf(vertex);
                    if (parent.Vertices[index].Parents.Contains(this) == false) parent.Vertices[index].Parents.Add(this);
                    MVertex tVertex = parent.Vertices[index];
                    tVertex.Parents.Add(this);
                    this.vertices.Add(tVertex);
                }
                else
                {
                    vertex.Parents.Add(this);
                    this.vertices.Add(vertex);
                    parent.Vertices.Add(vertex);
                }
            }

            center = center / 3;

            parent.Triangles.Add(this);
            normal = (normal / 3).normalized;
            FindConnections();
        }

        public void Debug()
        {
			Debug(Color.white);
		}
		public void Debug(Color color)
		{
			UnityEngine.Debug.DrawLine(vertices[0].Position, vertices[1].Position, color);
			UnityEngine.Debug.DrawLine(vertices[1].Position, vertices[2].Position, color);
			UnityEngine.Debug.DrawLine(vertices[2].Position, vertices[0].Position, color);

            UnityEngine.Debug.DrawRay(center, normal * 0.1f,Color.green);
        }

        public void FindConnections()
        {
            foreach(MTriangle triangle in parent.Triangles)
            {
                if(triangle!=this)
                {
                    if (SharedConnections(triangle) == 2)
                    {
                        if (triangle.connections.Contains(this) == true && connections.Contains(triangle) == true)
                            continue;
                        else if (triangle.connections.Contains(this) == true && connections.Contains(triangle) == false)
                            connections.Add(triangle);
                        else if (triangle.connections.Contains(this) == false && connections.Contains(triangle) == false)
                        {
                            connections.Add(triangle);
                            triangle.connections.Add(this);
                        }
                    }
                }
            }
        }

        private int SharedConnections(MTriangle checkTriangle)
        {
            int sharedConnections = 0;
            foreach(MVertex vertex in vertices)
            {
                if (checkTriangle.vertices.Contains(vertex))
                    sharedConnections += 1;
            }
            return sharedConnections;
        }

        public void DebugConnections()
        {
            foreach(MTriangle triangle in connections)
				triangle.Debug(Color.yellow);

			Debug(Color.red);
		}

        private void RecalculateBounds()
        {
            foreach (MVertex vertex in vertices)
                bounds.Encapsulate(vertex.Position);
        }
    }

	public class MUVVertex
	{
		//Properties
		private Vector2 uv;
		public Vector2 UV
		{
			get { return uv; }
			set { UV = value; }
		}

		public MUVVertex( Vector2 uv)
		{
			this.uv = uv;
		}

		public RenderUtility.Point2D ToPoint2D()
		{
			return new RenderUtility.Point2D(uv);
		}
	}

    public class MMesh
    {
        //Properties
        private Mesh mesh;
        private List<MTriangle> triangles;
        public List<MTriangle> Triangles
        {
            get { return triangles; }
            set { triangles = value; }
        }
        private List<MVertex> vertices;
        public List<MVertex> Vertices
        {
            get { return vertices; }
            set { vertices = value; }
        }
        private Bounds bounds;

        public MMesh()
        {
            mesh = new Mesh();
            triangles = new List<MTriangle>();
            vertices = new List<MVertex>();
            bounds = new Bounds(Vector3.zero, Vector3.zero);
        }

        public MMesh(Mesh mesh)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            this.mesh = mesh;
            triangles = new List<MTriangle>();
            vertices = new List<MVertex>();
            bounds = new Bounds(Vector3.zero, Vector3.zero);

			List<MVertex> vertexList = new List<MVertex>();

			// Reference data
			int[] meshTriangles = mesh.triangles;
			Vector3[] meshVertices = mesh.vertices;
			Vector3[] meshNormals = mesh.normals;
			Vector2[] meshUV = mesh.uv;

			for (int triangle = 0; triangle < meshTriangles.Length; triangle += 3)
            {
                vertexList.Clear();

				vertexList.Add(new MVertex(meshVertices[meshTriangles[triangle    ]], meshNormals[meshTriangles[triangle    ]], meshUV[meshTriangles[triangle    ]]));
				vertexList.Add(new MVertex(meshVertices[meshTriangles[triangle + 1]], meshNormals[meshTriangles[triangle + 1]], meshUV[meshTriangles[triangle + 1]]));
				vertexList.Add(new MVertex(meshVertices[meshTriangles[triangle + 2]], meshNormals[meshTriangles[triangle + 2]], meshUV[meshTriangles[triangle + 2]]));

                MTriangle newTriangle = new MTriangle(this, vertexList);
            }

            stopwatch.Stop();
            UnityEngine.Debug.Log("Time taken: " + (stopwatch.Elapsed));
            stopwatch.Reset();

        }
                 
        public void Debug()
        {
            foreach (MTriangle triangle in triangles)
                triangle.Debug();

            foreach (MVertex vertex in vertices)
                vertex.Debug();
        }

        public void ShowTriangleConnections(int id)
        {
            if (id < 0 || id > triangles.Count)
                return;
            triangles[id].DebugConnections();
        }

        public void CalculateBounds()
        {
            foreach (MTriangle triangle in triangles)
                bounds.Encapsulate(triangle.Bounds);
        }
    }
}
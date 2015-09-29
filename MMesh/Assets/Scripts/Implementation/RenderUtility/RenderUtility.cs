using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MeshUtility;

public class RenderUtility
{
    public struct Point2D
    {
        public int x, y;

		public Point2D(Vector2 point)
		{
			x = (int)point.x;
			y = (int)point.y;
		}

		public Point2D(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public int Max()
		{
			if(x> y)
				return x;
			else
				return y;
		}

		public static Point2D operator+(Point2D left, Point2D right)
		{
			Point2D ret;
			ret.x = left.x + right.x;
			ret.y = left.y + right.y;
			return ret;
		}

		public static Point2D operator-(Point2D left, Point2D right)
		{
			Point2D ret;
			ret.x = left.x - right.x;
			ret.y = left.y - right.y;
			return ret;
		}

		public static Point2D operator*(Point2D left, Point2D right)
		{
			Point2D ret;
			ret.x = left.x * right.x;
			ret.y = left.y * right.y;
			return ret;
		}
		public static Point2D operator*(int left, Point2D right)
		{
			Point2D ret;
			ret.x = left * right.x;
			ret.y = left * right.y;
			return ret;
		}
		public static Point2D operator*(Point2D left, int right)
		{
			Point2D ret;
			ret.x = right * left.x;
			ret.y = right * left.y;
			return ret;
		}
    }

    int orient2d(Point2D a, Point2D b, Point2D c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

	public static void RenderToTexture(MMesh mesh, Color color, Texture2D texture)
    {
		foreach(MTriangle triangle in mesh.Triangles)
			RasterizeTriangle(triangle,texture);
    }

	public static void DebugTriangle(MTriangle triangle, Texture2D texture)
	{
		List<Point2D> points = new List<Point2D>();
		foreach(MVertex vertex in triangle.Vertices)
		{
			points.Add (vertex.Uv.ToPoint2D());
		}

		int textureSize = texture.width;

		DrawLine(points[0] * textureSize,points[1]* textureSize,texture);
		DrawLine(points[1] * textureSize,points[2]* textureSize,texture);
		DrawLine(points[2] * textureSize,points[0]* textureSize,texture);

	}

	public static void RasterizeTriangle( MTriangle triangle, Texture2D texture)
	{

		MUVVertex v1 = triangle.Vertices[0].Uv;
		MUVVertex v2 = triangle.Vertices[1].Uv;
		MUVVertex v3 = triangle.Vertices[2].Uv;

		fillBottomFlatTriangle(v1,v2,v3, texture);
		fillTopFlatTriangle(v1,v2,v3, texture);

		texture.Apply();
	}
	



	// http://www.sunshine2k.de/coding/java/TriangleRasterization/TriangleRasterization.html#sunbresenhamarticle
	private static void fillBottomFlatTriangle(MUVVertex v1, MUVVertex v2, MUVVertex v3, Texture2D texture)
	{

		Debug.Log ("fillBottomFlatTriangle " + v1.ToString()+ v2.ToString()+v3.ToString());

		float width = (float)texture.width;

		float invslope1 = (v2.UV.x - v1.UV.x) / (v2.UV.y - v1.UV.y);
		float invslope2 = (v3.UV.x - v1.UV.x) / (v3.UV.y - v1.UV.y);
		
		float curx1 = v1.UV.x;
		float curx2 = v1.UV.x;
		
		for (float scanlineY = v1.UV.y; scanlineY <= v2.UV.y; scanlineY++)
		{
			DrawScanline(new Vector2(curx1 * width, scanlineY* width), new Vector2( curx2* width, scanlineY* width), texture);
			curx1 += invslope1;
			curx2 += invslope2;
		}
	}

	// http://www.sunshine2k.de/coding/java/TriangleRasterization/TriangleRasterization.html#sunbresenhamarticle
	private static void fillTopFlatTriangle(MUVVertex v1, MUVVertex v2, MUVVertex v3, Texture2D texture)
	{
		float width = (float)texture.width;

		float invslope1 = (v3.UV.x - v1.UV.x) / (v3.UV.y - v1.UV.y);
		float invslope2 = (v3.UV.x - v2.UV.x) / (v3.UV.y - v2.UV.y);
		
		float curx1 = v3.UV.x;
		float curx2 = v3.UV.x;
		
		for (int scanlineY = (int)v3.UV.y; scanlineY > (int)v1.UV.y; scanlineY--)
		{
			curx1 -= invslope1;
			curx2 -= invslope2;
			DrawScanline(new Vector2(curx1* width, scanlineY* width), new Vector2( curx2* width, scanlineY* width), texture);
		}
	}

	private static void DrawScanline( Vector2 v1, Vector2 v2, Texture2D texture)
	{
		Debug.Log ("DrawScanline: " + "(" + v1.x + "," + v1.y + ")  -  (" + v2.x + "," + v2.y + ")");
		DrawLine(new Point2D(v1), new Point2D(v2),texture);
	}

	public static void DrawLine(Vector2 from, Vector2 to, Texture2D texture)
	{
		Point2D tFrom = new Point2D(from);
		Point2D tTo = new Point2D(to);


		DrawLine(tFrom,tTo,texture);
	}

	public static void DrawLine(Point2D tFrom, Point2D tTo, Texture2D texture)
	{
		//Debug.Log ("Drawing from: " + tFrom.x + "," + tFrom.y + "  to  " + tTo.x + "," + tTo.y);

		int deltaX = tFrom.x - tTo.x;
		int deltaY = tFrom.y - tTo.y;

		int d = 2*deltaY - deltaX;

		texture.SetPixel(tFrom.x,tFrom.y,Color.black);

		int y = tFrom.y;

		for(int x = tFrom.x + 1; x< tTo.x; x+=1)
		{
			if(d > 0)
			{
				y+=1;
				//Debug.Log (x + " " + y);
				texture.SetPixel(x,y,Color.black);
				d+=(2*deltaY)-(2*deltaX);
			}
			else
			{
				//Debug.Log (x + " " + y);
				texture.SetPixel(x,y,Color.black);
				d+=(2*deltaY);
			}
		}
		texture.Apply();
	}
}
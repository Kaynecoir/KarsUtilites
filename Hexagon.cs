using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kars
{
	public class Hexagon<T>
	{
		// Fields for work with HexGrid
		public HexGrid<T> hexGrid;
		public int positionX { get; private set; }
		public int positionY { get; private set; }
		//public Dictionary<string, Hexagon< HexObject>> neigbourHex;
		public List<Hexagon<T>> neigbourHex { get; private set; }
		public T Value { get; private set; }


		public Vector3 worldPosition { get; private set; }
		public Vector3[] corner { get; private set; }
		public float radius { get; private set; }
		public float littleRadius { get; protected set; }
		public float height { get; private set; }   // for 3D
		public bool isVertical { get; private set; }

		public Hexagon(float radius, Vector3 worldPosition = default(Vector3), T value = default(T), bool isVertical = false)
		{
			this.radius = radius;
			this.littleRadius = radius * Mathf.Sin(Mathf.PI / 3);
			this.worldPosition = worldPosition;
			this.isVertical = isVertical;
			this.neigbourHex = new List<Hexagon<T>>();
			this.corner = new Vector3[6];

			SetCorner();
		}
		public Hexagon(float radius, Vector3 worldPosition, Func<Hexagon<T>, T> valueFunc, bool isVertical = false)
		{
			this.radius = radius;
			this.littleRadius = radius * Mathf.Sin(Mathf.PI / 3);
			this.worldPosition = worldPosition;
			this.isVertical = isVertical;
			this.neigbourHex = new List<Hexagon<T>>();
			this.Value = valueFunc(this);
			this.corner = new Vector3[6];

			SetCorner();
		}
		/// <summary>
		/// Set position of corner points foundation on hexagon positiob=n
		/// </summary>
		public void SetCorner()
		{
			for (int i = 0; i < 6; i++)
			{
				float angel_del = 60 * i + (isVertical ? 90 : 0);
				float angel_rad = angel_del / 180 * Mathf.PI;
				corner[i] = new Vector3(Mathf.Cos(angel_rad), Mathf.Sin(angel_rad)) * radius + worldPosition;
			}
		}
		public void SetGrid(int x, int y, HexGrid<T> hexGrid)
		{
			this.positionX = x; this.positionY = y;
			this.hexGrid = hexGrid;
		}
		public void SetValue(T val)
		{
			Value = val;
		}
		public Vector3 GetPosition()
		{
			return worldPosition;
		}
		public void AddNeigbourHex(Hexagon<T> hex)
		{
			this.neigbourHex.Add(hex);
			hex.neigbourHex.Add(this);
		}
		public bool inTrianArea(Vector3 mouseWorldPosition)
		{
			return inTrianArea(mouseWorldPosition.x, mouseWorldPosition.y);
		}
		public bool inTrianArea(float x, float y)
		{
			if (y < 0) return false;
			if (x < radius / 2 && x >= 0) return Mathf.Tan(Mathf.PI / 3) >= y / x;
			if (x >= radius / 2 && x <= radius) return Mathf.Tan(Mathf.PI / 3) >= y / (radius - x);
			return false;
		}
		public bool inHexArea(Vector3 mouseWorldPosition, Vector3 ZeroCoordinate = default(Vector3))
		{
			return inHexArea(mouseWorldPosition.x, mouseWorldPosition.y, ZeroCoordinate);
		}
		public bool inHexArea(float x, float y, Vector3 zeroCoordinate = default(Vector3))
		{
			x -= zeroCoordinate.x;
			y -= zeroCoordinate.y;
			if (x * x + y * y > radius * radius) return false;
			if (x * x + y * y < littleRadius * littleRadius) return true;
			float angel_del = isVertical ? 90 : 0;

			for (int i = 0; i < 6; i++)
			{
				if (inTrianArea(x, y)) return true;
				angel_del = 60;
			}
			return false;
		}

		public Mesh CreateHexMesh()
		{
			Mesh mesh = new Mesh();
			mesh.name = "Hex";
			Vector3[] vertices = new Vector3[7];
			Vector3[] normals = new Vector3[7];
			Vector2[] uv = new Vector2[7];
			for (int i = 0; i < 6; i++)
			{
				vertices[i] = corner[i];
				normals[i] = Vector3.back;
				uv[i] = corner[i] / radius / 2 + new Vector3(0.5f, 0.5f);
			}
			vertices[6] = worldPosition;
			normals[6] = Vector3.back;
			uv[6] = new Vector2(0.5f, 0.5f);

			int[] triangles = new int[18];
			for (int i = 0; i < 6; i++)
			{
				triangles[i * 3 + 0] = 6;
				triangles[i * 3 + 1] = i == 5 ? 0 : i + 1;
				triangles[i * 3 + 2] = i;
			}

			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uv;
			mesh.triangles = triangles;

			return mesh;
		}
	}
}

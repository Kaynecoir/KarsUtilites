using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kars
{
	public class HexGrid<T>
	{
		public int Height { get; protected set; }
		public int Width { get; protected set; }
		public float Radius { get; protected set; }
		public float littleRadius { get; protected set; }
		public bool isVertical { get; private set; }
		public Vector3 zeroCoord { get; protected set; }
		public Vector3 PositionToCenter { get; protected set; }
		public Mesh gridMesh;
		protected TextMesh[,] textArray;
		public Hexagon<T>[,] gridArray { get; protected set; }

		public delegate void VoidFunc();
		public event VoidFunc ChangeValue;
		public HexGrid(int height, int width, float radius, Vector3 worldPosition, bool isVertical = false, bool isDebuging = false)
		{
			Height = height;
			Width = width;
			Radius = radius;
			littleRadius = radius * Mathf.Sin(Mathf.PI / 3);
			zeroCoord = worldPosition;
			PositionToCenter = new Vector3(radius * (isVertical ? Mathf.Sin(Mathf.PI / 3) : 1), radius * (!isVertical ? Mathf.Sin(Mathf.PI / 3) : 1));
			this.isVertical = isVertical;
			gridArray = new Hexagon<T>[Height, Width];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{

					Hexagon<T> h = new Hexagon<T>(Radius, GetPositionFromCenter(x, y) + zeroCoord, isVertical: this.isVertical);
					h.SetCorner();
					h.SetGrid(x, y, this);
					if (x - 1 >= 0) h.AddNeigbourHex(gridArray[y, x - 1]);
					if (y - 1 >= 0) h.AddNeigbourHex(gridArray[y - 1, x]);
					if (isVertical)
					{
						if (y % 2 == 0 && x - 1 >= 0 && y - 1 >= 0) h.AddNeigbourHex(gridArray[y - 1, x - 1]);
						else if (y % 2 == 1 && x + 1 < Width) h.AddNeigbourHex(gridArray[y - 1, x + 1]);
					}
					else
					{
						if (x % 2 == 0 && x - 1 >= 0 && y - 1 >= 0) h.AddNeigbourHex(gridArray[y - 1, x - 1]);
						if (x % 2 == 0 && x + 1 < Width && y - 1 >= 0) h.AddNeigbourHex(gridArray[y - 1, x + 1]);
					}
					gridArray[y, x] = h;

				}
			}
		}
		public HexGrid(int height, int width, float radius, Vector3 worldPosition, Func<Hexagon<T>, T> createHexObject, bool isVertical = false, bool isDebuging = false)
		{
			Height = height;
			Width = width;
			Radius = radius;
			littleRadius = radius * Mathf.Sin(Mathf.PI / 3);
			zeroCoord = worldPosition;
			PositionToCenter = new Vector3(radius * (isVertical ? Mathf.Sin(Mathf.PI / 3) : 1), radius * (!isVertical ? Mathf.Sin(Mathf.PI / 3) : 1));
			this.isVertical = isVertical;
			gridArray = new Hexagon<T>[Height, Width];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{

					Hexagon<T> h = new Hexagon<T>(Radius, GetPositionFromCenter(x, y) + zeroCoord, createHexObject, this.isVertical);

					h.SetCorner();
					h.SetGrid(x, y, this);
					if (x - 1 >= 0) h.AddNeigbourHex(gridArray[y, x - 1]);
					if (y - 1 >= 0) h.AddNeigbourHex(gridArray[y - 1, x]);
					if (isVertical)
					{
						if (y % 2 == 0 && x - 1 >= 0 && y - 1 >= 0) h.AddNeigbourHex(gridArray[y - 1, x - 1]);
						else if (y % 2 == 1 && x + 1 < Width) h.AddNeigbourHex(gridArray[y - 1, x + 1]);
					}
					else
					{
						if (x % 2 == 0 && x - 1 >= 0 && y - 1 >= 0) h.AddNeigbourHex(gridArray[y - 1, x - 1]);
						if (x % 2 == 0 && x + 1 < Width && y - 1 >= 0) h.AddNeigbourHex(gridArray[y - 1, x + 1]);
					}
					gridArray[y, x] = h;

				}
			}
		}
		public Hexagon<T> this[int y, int x]
		{
			get
			{
				return gridArray[y, x];
			}
			private set
			{
				gridArray[y, x] = value;
			}
		}
		public Vector3 GetPositionFromCenter(int x, int y)
		{
			Vector3 pos = zeroCoord;
			if (isVertical)
			{
				pos = new Vector3((2 * x + (y % 2)) * MathF.Sqrt(3) / 2, y * 1.5f) * Radius + PositionToCenter;
			}
			else
			{
				pos = new Vector3(x * 1.5f, (2 * y + (x % 2)) * MathF.Sqrt(3) / 2) * Radius + PositionToCenter;
			}
			return pos;
		}
		public Vector3 GetPositionFromWorld(int x, int y)
		{
			Vector3 pos = zeroCoord;
			pos = gridArray[y, x].worldPosition;
			return pos;
		}
		public bool inHexArea(int x, int y, Vector3 cursorPosition)
		{
			return gridArray[y, x].inHexArea(cursorPosition - gridArray[y, x].worldPosition);
		}
		public Vector3Int GetXY(Vector3 worldPosition)
		{
			return GetXY(worldPosition, out int x, out int y);
		}
		public Vector3Int GetXY(Vector3 CursorPosition, out int x, out int y)
		{
			x = 0; y = 0;
			Vector3 CursorToCenter = CursorPosition - zeroCoord;
			if (isVertical)
			{
				// Находим x и y по положению курсора относительно нулевого координата
				y = Mathf.RoundToInt(CursorToCenter.y * 2 / (3 * Radius) - 0.5f);
				x = Mathf.RoundToInt((CursorToCenter.x / littleRadius - (y % 2) - 1.0f) / 2);

				if (x >= 0 && x < Width && y >= 0 && y < Height)
				{
					if (y % 2 == 0)
					{
						// Проверяем находиться ли курсор в гексагоне по положению курсора относительно мира, так как гексагон проверяет по мировым координатам
						if (inHexArea(x, y, CursorPosition))
						{
							return new Vector3Int(x, y);
						}
						// Проверяем находиться ли курсор в гексагоне по положению курсора относительно мира, так как гексагон проверяет по мировым координатам
						if (y - 1 >= 0 && x - 1 >= 0 && inHexArea(x - 1, y - 1, CursorPosition))
						{
							return new Vector3Int(--x, --y);
						}
						// Проверяем находиться ли курсор в гексагоне по положению курсора относительно мира, так как гексагон проверяет по мировым координатам
						if (y - 1 >= 0 && inHexArea(x, y - 1, CursorPosition))
						{
							return new Vector3Int(x, --y);
						}
					}
					else
					{
						// Проверяем находиться ли курсор в гексагоне по положению курсора относительно мира, так как гексагон проверяет по мировым координатам
						if (inHexArea(x, y, CursorPosition))
						{
							return new Vector3Int(x, y);
						}
						// Проверяем находиться ли курсор в гексагоне по положению курсора относительно мира, так как гексагон проверяет по мировым координатам
						if (y - 1 >= 0 && x + 1 < Width && inHexArea(x + 1, y - 1, CursorPosition))
						{
							return new Vector3Int(++x, --y);
						}
						// Проверяем находиться ли курсор в гексагоне по положению курсора относительно мира, так как гексагон проверяет по мировым координатам
						if (y - 1 >= 0 && inHexArea(x, y - 1, CursorPosition))
						{
							return new Vector3Int(x, --y);
						}
					}
				}
			}
			else
			{
				x = Mathf.RoundToInt(CursorToCenter.x * 2 / (3 * Radius) - 0.0f);
				y = Mathf.RoundToInt((CursorToCenter.y / littleRadius - (x % 2) - 0.0f) / 2);

				UnityEngine.Debug.Log(x + " " + y);

				if (x >= 0 && x < Width && y >= 0 && y < Height)
				{

					if (x % 2 == 0)
					{
						if (inHexArea(x, y, CursorPosition))
						{
							return new Vector3Int(x, y);        //UnityEngine.Debug.Log($"{x}, {y}: {gridArray[y, x].worldPosition} <- {CursorPosition} = {(gridArray[y, x].worldPosition - CursorPosition).magnitude} -> {inHexArea(x, y, CursorPosition)}");
						}
						if (x - 1 >= 0 && y - 1 >= 0 && inHexArea(x - 1, y - 1, CursorPosition))
						{
							return new Vector3Int(--x, --y);  //UnityEngine.Debug.Log($"{x - 1}, {y - 1}: {gridArray[y - 1, x - 1].worldPosition} <- {CursorPosition} = {(gridArray[y - 1, x - 1].worldPosition - CursorPosition).magnitude} -> {inHexArea(x - 1, y - 1, CursorPosition)}");
						}
						if (x - 1 >= 0 && inHexArea(x - 1, y, CursorPosition))
						{
							return new Vector3Int(x, --y);  //UnityEngine.Debug.Log($"{x}, {y - 1}: {gridArray[y-1, x].worldPosition} <- {CursorPosition} = {(gridArray[y-1, x].worldPosition - CursorPosition).magnitude} -> {inHexArea(x, y-1, CursorPosition)}");
						}
					}
					else
					{
						if (inHexArea(x, y, CursorPosition))
						{
							return new Vector3Int(x, y);        //UnityEngine.Debug.Log($"{x}, {y}: {gridArray[y, x].worldPosition} <- {CursorPosition} = {(gridArray[y, x].worldPosition - CursorPosition).magnitude} -> {inHexArea(x, y, CursorPosition)}");
						}
						if (x - 1 >= 0 && y + 1 < Height && inHexArea(x - 1, y + 1, CursorPosition))
						{
							return new Vector3Int(--x, ++y);  //UnityEngine.Debug.Log($"{x - 1}, {y - 1}: {gridArray[y - 1, x - 1].worldPosition} <- {CursorPosition} = {(gridArray[y - 1, x - 1].worldPosition - CursorPosition).magnitude} -> {inHexArea(x - 1, y - 1, CursorPosition)}");
						}
						if (x - 1 >= 0 && inHexArea(x - 1, y, CursorPosition))
						{
							return new Vector3Int(x, --y);  //UnityEngine.Debug.Log($"{x}, {y - 1}: {gridArray[y-1, x].worldPosition} <- {CursorPosition} = {(gridArray[y-1, x].worldPosition - CursorPosition).magnitude} -> {inHexArea(x, y-1, CursorPosition)}");
						}
					}
				}
			}
			x = 0; y = 0;
			return new Vector3Int(x, y);
		}
		public void SetValue(Vector3 worldPosition, T val)
		{
			int x, y;
			GetXY(worldPosition, out x, out y);
			SetValue(x, y, val);
		}
		public void SetValue(int x, int y, T val)
		{
			if (x >= 0 && x < Width && y >= 0 && y < Height)
			{
				gridArray[y, x].SetValue(val);
				ChangeValue?.Invoke();
			}
		}
		public Hexagon<T> GetValue(Vector3 worldPosition)
		{
			GetXY(worldPosition, out int x, out int y);
			return GetValue(x, y);
		}
		public Hexagon<T> GetValue(int x, int y)
		{
			if (x >= 0 && x < Width && y >= 0 && y < Height)
			{
				return gridArray[y, x];
			}
			else return default(Hexagon<T>);
		}
		public Mesh CreateMeshArray()
		{
			gridMesh = new Mesh();
			gridMesh.name = "HexGrid";
			Vector3[] vertices = new Vector3[(Width * Height) * 7];
			Vector3[] normals = new Vector3[(Width * Height) * 7];
			Vector2[] uv = new Vector2[(Width * Height) * 7];
			int[] triangles = new int[(Width * Height) * 18];

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					int index = y * Width + x;
					AddHexMeshToArray(GetValue(x, y), vertices, normals, uv, triangles, index);
				}
			}
			gridMesh.vertices = vertices;
			gridMesh.normals = normals;
			gridMesh.uv = uv;
			gridMesh.triangles = triangles;

			return gridMesh;
		}
		public Mesh CreateMeshArray(out Vector3[] vertices, out Vector3[] normals, out Vector2[] uv, out int[] triangles)
		{
			gridMesh = new Mesh();
			gridMesh.name = "HexGrid";
			vertices = new Vector3[(Width * Height) * 7];
			normals = new Vector3[(Width * Height) * 7];
			uv = new Vector2[(Width * Height) * 7];
			triangles = new int[(Width * Height) * 18];

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					int index = y * Width + x;
					AddHexMeshToArray(GetValue(x, y), vertices, normals, uv, triangles, index);
				}
			}
			gridMesh.vertices = vertices;
			gridMesh.normals = normals;
			gridMesh.uv = uv;
			gridMesh.triangles = triangles;

			return gridMesh;
		}
		public Mesh CreateMeshArray(Vector2 uvPoint)
		{
			gridMesh = new Mesh();
			gridMesh.name = "HexGrid";
			Vector3[] vertices = new Vector3[(Width * Height) * 7];
			Vector3[] normals = new Vector3[(Width * Height) * 7];
			Vector2[] uv = new Vector2[(Width * Height) * 7];
			int[] triangles = new int[(Width * Height) * 18];

			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					int index = y * Width + x;
					AddHexMeshToArray(GetValue(x, y), vertices, normals, uv, triangles, index, uvPoint);
				}
			}
			gridMesh.vertices = vertices;
			gridMesh.normals = normals;
			gridMesh.uv = uv;
			gridMesh.triangles = triangles;

			return gridMesh;
		}

		public void AddHexMeshToArray(Hexagon<T> hex, Vector3[] vertices, Vector3[] normals, Vector2[] uv, int[] triangles, int index)
		{
			for (int i = 0; i < 6; i++)
			{
				vertices[index * 7 + i] = hex.corner[i] - zeroCoord;
				//UnityEngine.Debug.Log(vertices[index * 7 + i]);
				normals[index * 7 + i] = Vector3.back;
				uv[index * 7 + i] = (hex.corner[i] - hex.worldPosition) / hex.radius / 2 + new Vector3(0.5f, 0.5f);
			}
			vertices[index * 7 + 6] = hex.worldPosition - zeroCoord;
			normals[index * 7 + 6] = Vector3.back;
			uv[index * 7 + 6] = new Vector2(0.5f, 0.5f);

			for (int i = 0; i < 6; i++)
			{
				triangles[index * 18 + i * 3 + 0] = (index * 7 + 6);
				triangles[index * 18 + i * 3 + 1] = ((i == 5) ? (index * 7 + 0) : (index * 7 + i + 1));
				triangles[index * 18 + i * 3 + 2] = (index * 7 + i);
			}
		}
		public void AddHexMeshToArray(Hexagon<T> hex, Vector3[] vertices, Vector3[] normals, Vector2[] uv, int[] triangles, int index, Vector2 uvPoint)
		{
			for (int i = 0; i < 6; i++)
			{
				vertices[index * 7 + i] = hex.corner[i] - zeroCoord;
				normals[index * 7 + i] = Vector3.back;
				uv[index * 7 + i] = uvPoint;
			}
			vertices[index * 7 + 6] = hex.worldPosition - zeroCoord;
			normals[index * 7 + 6] = Vector3.back;
			uv[index * 7 + 6] = uvPoint;

			for (int i = 0; i < 6; i++)
			{
				triangles[index * 18 + i * 3 + 0] = (index * 7 + 6);
				triangles[index * 18 + i * 3 + 1] = ((i == 5) ? (index * 7 + 0) : (index * 7 + i + 1));
				triangles[index * 18 + i * 3 + 2] = (index * 7 + i);
			}
		}
		public void ClearHexMeshArray()
		{
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					int index = y * Width + x;
					for (int i = 0; i < 6; i++)
					{
						gridMesh.uv[index * 7 + i] = (GetValue(x, y).corner[i] - GetValue(x, y).worldPosition) / Radius / 2 + new Vector3(0.5f, 0.5f);
					}
					gridMesh.uv[index * 7 + 6] = new Vector2(0.5f, 0.5f);
				}
			}
		}
	}
}
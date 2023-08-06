using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Kars
{
	public class Grid<T>
	{
		public int Height { get; protected set; }
		public int Width { get; protected set; }
		public float Size { get; protected set; }
		public Vector3 PositionToWorld { get; protected set; }
		protected TextMesh[,] textArray;
		public GridObject<T>[,] gridArray { get; protected set; }

		public delegate void VoidFunc();
		public event VoidFunc ChangeValue;

		public Grid()
		{

		}
		public Grid(int height, int width, float size, Vector3 positionToWorld, Func<GridObject<T>> createGridObject, bool isDebuging = false)
		{
			Height = height;
			Width = width;
			Size = size;
			PositionToWorld = positionToWorld;
			gridArray = new GridObject<T>[Height, Width];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					GridObject<T> t = createGridObject();
					gridArray[y, x] = t;
				}
			}
		}
		public Grid(int height, int width, float size, Vector3 positionToWorld, Func<Grid<T>, int, int, GridObject<T>> createGridObject, bool isDebuging = false)
		{
			Height = height;
			Width = width;
			Size = size;
			PositionToWorld = positionToWorld;
			gridArray = new GridObject<T>[Height, Width];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					GridObject<T> t = createGridObject(this, y, x);
					gridArray[y, x] = t;

				}
			}
		}

		public GridObject<T> this[int y, int x]
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
		public Vector3 GetWorldPosition(int x, int y) => new Vector3(x, y) * Size + PositionToWorld;
		public Vector3Int GetXY(Vector3 worldPosition)
		{
			int x, y;

			return GetXY(worldPosition, out x, out y);
		}
		public Vector3Int GetXY(Vector3 worldPosition, out int x, out int y)
		{
			x = Mathf.RoundToInt((worldPosition - PositionToWorld).x / Size - 0.5f);
			y = Mathf.RoundToInt((worldPosition - PositionToWorld).y / Size - 0.5f);
			x = x < Width && x >= 0 ? x : 0;
			y = y < Height && y >= 0 ? y : 0;

			return new Vector3Int(x, y);
		}
		public void SetValue(Vector3 worldPosition, GridObject<T> gObj)
		{
			int x, y;
			GetXY(worldPosition, out x, out y);
			SetValue(x, y, gObj.Value);
		}
		public void SetValue(int x, int y, T value)
		{
			if (x >= 0 && x < Width && y >= 0 && y < Height)
			{
				gridArray[y, x].Value = value;
				//textMeshArray[pos.y, pos.x].text = gridArray[pos.y, pos.x].ToString();
				ChangeValue?.Invoke();
			}
		}
		public T GetValue(Vector3 worldPosition)
		{
			int x, y;
			GetXY(worldPosition, out x, out y);
			return GetValue(x, y);
		}
		public T GetValue(int x, int y)
		{
			if (x >= 0 && x < Width && y >= 0 && y < Height)
			{
				return gridArray[y, x].Value;
			}
			else return default(T);
		}
	}
}

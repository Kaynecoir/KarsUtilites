using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Kars
{
	public class GridObject<T>
	{
		// Fields for work with HexGrid
		public Grid<T> grid;
		public int positionX { get; set; }
		public int positionY { get; set; }
		//public Dictionary<string, Hexagon< HexObject>> neigbourHex;
		public List<GridObject<T>> neigbourObj { get; set; }
		public T Value { get; set; }


		public Vector3 worldPosition { get; private set; }
		public Vector3[] corner { get; private set; }
		public float Size { get; private set; }
	}
}

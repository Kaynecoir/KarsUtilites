using System;
using System.Collections.Generic;
using System.Text;

namespace Kars.Pathfinding
{
	public class HexPathNode
	{
		public Hexagon<HexPathNode> HexParant { get; set; }

		public HexGrid<HexPathNode> Grid { get; }

		public int Y { get; set; }
		public int X { get; set; }
		public int GCost { get; set; }
		public int HCost { get; set; }
		public int FCost { get; set; }
		public bool IsWalking { get; set; }
		public HexPathNode CameFromNode { get; set; }

		public HexPathNode()
		{

		}
		public HexPathNode(Hexagon<HexPathNode> hex)
		{
			this.HexParant = hex;
			this.Grid = hex.hexGrid;
			this.Y = hex.positionY;
			this.X = hex.positionX;
			IsWalking = true;
		}
		public void SetHexParant(Hexagon<HexPathNode> hex)
		{
			HexParant = hex;
			Y = hex.positionY;
			X = hex.positionX;
		}

		public override string ToString()
		{
			return X + ", " + Y + "\nh = " + HCost + " g = " + GCost + " F = " + FCost;
		}

		public void CalculateFCost()
		{
			FCost = HCost + GCost;
		}
	}
}

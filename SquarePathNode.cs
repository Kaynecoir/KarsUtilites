using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kars.Pathfinding
{
	public class SquarePathNode
	{
		public Grid<SquarePathNode> Grid { get; }
		public int Y { get; set; }
		public int X { get; set; }
		public int GCost { get; set; }
		public int HCost { get; set; }
		public int FCost { get; set; }
		public bool IsWalking { get; set; }
		public SquarePathNode CameFromNode { get; set; }
		public Hexagon<SquarePathNode> HexParant { get; set; }

		public SquarePathNode()
		{

		}
		public SquarePathNode(Grid<SquarePathNode> grid, int y, int x)
		{
			this.Grid = grid;
			this.Y = y;
			this.X = x;
			IsWalking = true;
		}

		public void CalculateFCost()
		{
			FCost = HCost + GCost;
		}
		public override string ToString()
		{
			return X + ", " + Y + "\nh = " + HCost + " g = " + GCost + " F = " + FCost;
		}
	}
}

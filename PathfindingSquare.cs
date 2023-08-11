using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Kars.Pathfinding
{
	public class PathfindingSquare
	{
		private const int MOVE_STRAIGHT_COST = 10;
		private const int MOVE_DIAGONAL_COST = 14;

		private Grid<SquarePathNode> grid;
		private SquarePathNode startNode;
		private SquarePathNode endNode;
		private List<SquarePathNode> openList;
		private List<SquarePathNode> closedList;

		public delegate void GridFunc(SquarePathNode node);
		public event GridFunc AddToOpenList, AddToClosedList, FindWay, ChangeWalking;
		public PathfindingSquare(int height, int width, float size, Vector3 positionToWorld, bool isDebugging = false)
		{
			grid = new Grid<SquarePathNode>(height, width, size, positionToWorld);
			for (int y = 0; y < grid.Height; y++)
			{
				for (int x = 0; x < grid.Width; x++)
				{
					grid[y, x].SetValue(new SquarePathNode(grid, y, x));
				}
			}
			SetStartNode(0, 0);
		}

		public Grid<SquarePathNode> GetGrid()
		{
			return grid;
		}
		public void SetWalking(Vector3 pos, bool value)
		{
			grid.GetXY(pos, out int x, out int y);
			SquarePathNode node = grid[y, x].Value;
			node.IsWalking = value;
			ChangeWalking?.Invoke(node);
		}
		public void SetStartNode(SquarePathNode node)
		{
			startNode = node;
		}
		public void SetStartNode(int x, int y)
		{
			startNode = grid[y, x].Value;
		}
		public List<SquarePathNode> FindPath(int endX, int endY)
		{
			return FindPath(startNode.X, startNode.Y, endX, endY);
		}
		public List<SquarePathNode> FindPath(int startX, int startY, int endX, int endY)
		{
			startNode = grid.GetValue(startX, startY).Value;
			endNode = grid.GetValue(endX, endY).Value;
			openList = new List<SquarePathNode> { startNode };
			AddToOpenList?.Invoke(startNode);
			closedList = new List<SquarePathNode>();

			for (int y = 0; y < grid.Height; y++)
			{
				for (int x = 0; x < grid.Width; x++)
				{
					SquarePathNode pathNode = grid.GetValue(x, y).Value;
					pathNode.GCost = int.MaxValue;
					pathNode.CalculateFCost();
					pathNode.CameFromNode = null;
				}
			}
			startNode.GCost = 0;
			startNode.HCost = CalculateDistance(startNode, endNode);
			startNode.CalculateFCost();

			while (openList.Count > 0)
			{
				SquarePathNode currentNode = GetLowerFCostNode(openList);
				if (currentNode == endNode)
				{
					// Final
					List<SquarePathNode> way = CalculatePath(endNode);
					FindWay?.Invoke(endNode);
					return way;
				}

				openList.Remove(currentNode);
				closedList.Add(currentNode);
				AddToClosedList?.Invoke(currentNode);

				foreach (SquarePathNode neighbourNode in GetNeighbourList(currentNode))
				{
					if (closedList.Contains(neighbourNode)) continue;

					int tentativeGCost = currentNode.GCost + CalculateDistance(currentNode, neighbourNode);

					if (tentativeGCost < neighbourNode.GCost)
					{
						neighbourNode.CameFromNode = currentNode;
						neighbourNode.GCost = tentativeGCost;
						neighbourNode.HCost = CalculateDistance(neighbourNode, endNode);
						neighbourNode.CalculateFCost();

						if (!openList.Contains(neighbourNode))
						{
							openList.Add(neighbourNode);
							AddToOpenList?.Invoke(neighbourNode);
						}
					}
				}
			}
			// way not found
			return null;
		}
		private List<SquarePathNode> GetNeighbourList(SquarePathNode currentNode)
		{
			List<SquarePathNode> neighbourList = new List<SquarePathNode>();
			int x = currentNode.X, y = currentNode.Y;
			if (x + 1 < grid.Width)
			{
				if (y + 1 < grid.Height && grid[y + 1, x + 1].Value.IsWalking) neighbourList.Add(grid[y + 1, x + 1].Value); // right-up
				if (grid[y + 0, x + 1].Value.IsWalking) neighbourList.Add(grid[y + 0, x + 1].Value); // right-middle
				if (y - 1 >= 0 && grid[y - 1, x + 1].Value.IsWalking) neighbourList.Add(grid[y - 1, x + 1].Value); // right-dowm
			}

			if (y + 1 < grid.Height && grid[y + 1, x + 0].Value.IsWalking) neighbourList.Add(grid[y + 1, x + 0].Value); // center-up
			if (y - 1 >= 0 && grid[y - 1, x + 0].Value.IsWalking) neighbourList.Add(grid[y - 1, x + 0].Value); // center-down

			if (x - 1 >= 0)
			{
				if (y + 1 < grid.Height && grid[y + 1, x - 1].Value.IsWalking) neighbourList.Add(grid[y + 1, x - 1].Value); // left-up
				if (grid[y + 0, x - 1].Value.IsWalking) neighbourList.Add(grid[y + 0, x - 1].Value);    // left-middle
				if (y - 1 >= 0 && grid[y - 1, x - 1].Value.IsWalking) neighbourList.Add(grid[y - 1, x - 1].Value);  // left-down
			}

			return neighbourList;
		}
		private List<SquarePathNode> CalculatePath(SquarePathNode endNode)
		{
			List<SquarePathNode> pathNodes = new List<SquarePathNode> { endNode };
			SquarePathNode nextPathStep = endNode.CameFromNode;
			while (nextPathStep != null)
			{
				pathNodes.Add(nextPathStep);
				nextPathStep = nextPathStep.CameFromNode;
			}
			pathNodes.Reverse();
			return pathNodes;
		}
		private int CalculateDistance(SquarePathNode a, SquarePathNode b)
		{
			int distanceX = Mathf.Abs(a.X - b.X);
			int distanceY = Mathf.Abs(a.Y - b.Y);
			int remaining = Mathf.Abs(distanceY - distanceX);
			return remaining * MOVE_STRAIGHT_COST + Mathf.Min(distanceX, distanceY) * MOVE_DIAGONAL_COST;
		}
		private SquarePathNode GetLowerFCostNode(List<SquarePathNode> pathNodeList)
		{
			SquarePathNode lowerFCostNode = pathNodeList[0];
			for (int i = 1; i < pathNodeList.Count; i++)
			{
				if (lowerFCostNode.FCost > pathNodeList[i].FCost)
				{
					lowerFCostNode = pathNodeList[i];
				}
			}
			return lowerFCostNode;
		}
	}
}

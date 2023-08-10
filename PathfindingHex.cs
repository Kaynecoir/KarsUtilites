using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kars.Pathfinding
{
	public class PathfindingHex
	{
		private const int MOVE_STRAIGHT_COST = 10;

		private static PathfindingHex instance;
		public static PathfindingHex Instance
		{
			get
			{
				if (instance == null)
					instance = new PathfindingHex();
				return instance;
			}
		}
		private HexGrid<HexPathNode> hexGrid;
		private HexPathNode startNode;
		private HexPathNode endNode;
		private List<HexPathNode> openList;
		private List<HexPathNode> closedList;

		public delegate void GridFunc(HexPathNode node);
		public event GridFunc AddToOpenList, AddToClosedList, FindWay, ChangeWalking;

		public PathfindingHex(int height = 1, int width = 1, float radius = 1.0f, Vector3 positionToWorld = default(Vector3), bool isVertical = false, bool isDebugging = false)
		{
			hexGrid = new HexGrid<HexPathNode>(height, width, radius, positionToWorld, isVertical);
			for (int y = 0; y < hexGrid.Height; y++)
			{
				for (int x = 0; x < hexGrid.Width; x++)
				{
					hexGrid[y, x].SetValue(new HexPathNode(hexGrid[y, x]));
				}
			}
			SetStartNode(0, 0);

			//if (isDebugging) hexGrid.SeeDebug();
		}
		public PathfindingHex(int height, int width, float radius, Vector3 positionToWorld, Func<Hexagon<HexPathNode>, HexPathNode> createHexObject, bool isVertical = false, bool isDebugging = false)
		{
			hexGrid = new HexGrid<HexPathNode>(height, width, radius, positionToWorld, createHexObject, isVertical);
			for (int y = 0; y < hexGrid.Height; y++)
			{
				for (int x = 0; x < hexGrid.Width; x++)
				{
					hexGrid[y, x].SetValue(new HexPathNode(hexGrid[y, x]));
				}
			}

			SetStartNode(0, 0);

			//if (isDebugging) hexGrid.SeeDebug();
		}

		public HexGrid<HexPathNode> GetGrid()
		{
			return hexGrid;
		}
		public void SetGrid(HexGrid<HexPathNode> hexGrid)
		{
			SetGrid(hexGrid.Height, hexGrid.Width, hexGrid.Radius, default(Vector3), hexGrid.isVertical);
		}
		public void SetGrid(int height, int width, float radius, Vector3 positionToWorld, bool isVertical)
		{
			hexGrid = new HexGrid<HexPathNode>(height, width, radius, positionToWorld, isVertical);
			for (int y = 0; y < hexGrid.Height; y++)
			{
				for (int x = 0; x < hexGrid.Width; x++)
				{
					hexGrid[y, x].SetValue(new HexPathNode(hexGrid[y, x]));
				}
			}
			SetStartNode(0, 0);
		}
		public void SetWalking(Vector3 pos, bool value)
		{
			hexGrid.GetXY(pos, out int x, out int y);
			HexPathNode node = hexGrid[y, x].Value;
			node.IsWalking = value;
			ChangeWalking?.Invoke(node);
		}
		public void SetStartNode(HexPathNode node)
		{
			startNode = node;
		}
		public void SetStartNode(int x, int y)
		{
			startNode = hexGrid[y, x].Value;
		}
		public List<HexPathNode> FindPath(int endX, int endY)
		{
			return FindPath(startNode.X, startNode.Y, endX, endY);
		}
		public List<HexPathNode> FindPath(int startX, int startY, int endX, int endY)
		{

			startNode = hexGrid.GetValue(startX, startY).Value;
			endNode = hexGrid.GetValue(endX, endY).Value;

			if (!endNode.IsWalking) return null;

			openList = new List<HexPathNode> { startNode };
			AddToOpenList?.Invoke(startNode);
			closedList = new List<HexPathNode>();

			for (int y = 0; y < hexGrid.Height; y++)
			{
				for (int x = 0; x < hexGrid.Width; x++)
				{
					HexPathNode pathNode = hexGrid.GetValue(x, y).Value;
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
				HexPathNode currentNode = GetLowerFCostNode(openList);
				if (currentNode == endNode)
				{
					// Final
					List<HexPathNode> way = CalculatePath(endNode);
					FindWay?.Invoke(endNode);
					return way;
				}

				openList.Remove(currentNode);
				closedList.Add(currentNode);
				AddToClosedList?.Invoke(currentNode);

				foreach (HexPathNode neighbourNode in GetNeighbourList(currentNode))
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

		private List<HexPathNode> GetNeighbourList(HexPathNode currentNode)
		{
			List<HexPathNode> neighbourList = new List<HexPathNode>();

			foreach (Hexagon<HexPathNode> hex in currentNode.HexParant.neigbourHex)
			{
				if (hexGrid[hex.positionY, hex.positionX].Value.IsWalking)
					neighbourList.Add(hexGrid[hex.positionY, hex.positionX].Value);
			}

			return neighbourList;
		}
		private List<HexPathNode> CalculatePath(HexPathNode endNode)
		{
			List<HexPathNode> pathNodes = new List<HexPathNode> { endNode };
			HexPathNode nextPathStep = endNode.CameFromNode;
			while (nextPathStep != null)
			{
				pathNodes.Add(nextPathStep);
				nextPathStep = nextPathStep.CameFromNode;
			}
			pathNodes.Reverse();
			return pathNodes;
		}
		private int CalculateDistance(HexPathNode a, HexPathNode b)
		{
			float distanceX;
			float distanceY;
			int distance;
			float minim;

			if (hexGrid.isVertical)
			{
				distanceX = Mathf.Abs(a.HexParant.worldPosition.x - b.HexParant.worldPosition.x) / hexGrid.littleRadius;
				distanceY = Mathf.Abs(a.HexParant.worldPosition.y - b.HexParant.worldPosition.y) * 2 / Mathf.Sqrt(3) / hexGrid.littleRadius;
				minim = Mathf.Min(distanceX, distanceY / Mathf.Sqrt(3));
				distance = Mathf.RoundToInt(distanceX + 2 / Mathf.Sqrt(3) * distanceY - minim);
			}
			else
			{
				UnityEngine.Debug.Log(a.HexParant.worldPosition + " " + hexGrid);
				distanceY = Mathf.Abs(a.HexParant.worldPosition.y - b.HexParant.worldPosition.y) / hexGrid.littleRadius;
				distanceX = Mathf.Abs(a.HexParant.worldPosition.x - b.HexParant.worldPosition.x) * 2 / Mathf.Sqrt(3) / hexGrid.littleRadius;
				minim = Mathf.Min(distanceY, distanceX / Mathf.Sqrt(3));
				distance = Mathf.RoundToInt(distanceY + 2 / Mathf.Sqrt(3) * distanceX - minim);
			}

			return distance * MOVE_STRAIGHT_COST;
		}
		private HexPathNode GetLowerFCostNode(List<HexPathNode> pathNodeList)
		{
			HexPathNode lowerFCostNode = pathNodeList[0];
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

	class FindHexMapVisual
	{
		private HexGrid<HexPathNode> grid;
		MeshFilter meshFilter;
		Transform transform;

		Vector3[] vertices;
		Vector3[] normals;
		Vector2[] uv;
		int[] triangles;

		public FindHexMapVisual(PathfindingHex pathfinding, MeshFilter meshFilter, Transform transform)
		{
			this.grid = pathfinding.GetGrid();
			this.grid.ChangeValue += UpdateFindMapVisual;
			this.meshFilter = meshFilter;
			this.transform = transform;
			//pathfinding.AddToOpenList += (HexPathNode node) => { UpdateNodeOfMapVisual(node, 0.2f);/*UnityEngine.Debug.Log(node);*/ };
			//pathfinding.AddToClosedList += (HexPathNode node) => { UpdateNodeOfMapVisual(node, 0.6f); };
			//pathfinding.FindWay += (HexPathNode node) =>
			//{
			//	HexPathNode current = node;
			//	while (current != null)
			//	{
			//		UpdateNodeOfMapVisual(current, 0.99f);
			//		current = current.CameFromNode;
			//	}
			//};
			pathfinding.ChangeWalking += (HexPathNode node) => { UpdateNodeOfMapVisual(node, 0.1f); };

			grid.CreateMeshArray(out vertices, out normals, out uv, out triangles);


			UpdateFindMapVisual();
		}

		public void ClearMap()
		{
			UpdateFindMapVisual();
		}
		public void UpdateFindMapVisual()
		{
			for (int y = 0; y < grid.Height; y++)
			{
				for (int x = 0; x < grid.Width; x++)
				{
					int index = y * grid.Width + x;
					Vector2 uvPoint = (!grid[y, x].Value.IsWalking) ? new Vector2(0.8f, 0.0f) : new Vector3(0.1f, 0.0f);
					grid.AddHexMeshToArray(grid[y, x], vertices, normals, uv, triangles, index, uvPoint);
				}
			}
			Mesh mesh = new Mesh();
			mesh.name = "GridMesh";
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uv;
			mesh.triangles = triangles;

			meshFilter.mesh = mesh;
		}

		public void UpdateNodeOfMapVisual(HexPathNode node, float step)
		{

			int x = node.X, y = node.Y;
			int index = y * grid.Width + x;
			grid.AddHexMeshToArray(grid[y, x], vertices, normals, uv, triangles, index, new Vector2(step, 0.0f));

			Mesh mesh = new Mesh();
			mesh.name = "GridMesh";
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uv;
			mesh.triangles = triangles;

			meshFilter.mesh = mesh;
		}
	}
}


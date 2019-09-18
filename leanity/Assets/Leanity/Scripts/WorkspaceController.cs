using System.Collections.Generic;
using UnityEngine;

namespace Leanity
{
	public class WorkspaceController
	{
		private Vector3 _size;
		private List<Vector3> _gridLines;
		private readonly Vector3[] _cubeVertices =
			{
			new Vector3( 1f,  1f,  1f), // 0
			new Vector3( 1f,  1f, -1f), // 1
			new Vector3( 1f, -1f,  1f), // 2
			new Vector3( 1f, -1f, -1f), // 3
			new Vector3(-1f,  1f,  1f), // 4
			new Vector3(-1f,  1f, -1f), // 5
			new Vector3(-1f, -1f,  1f), // 6
			new Vector3(-1f, -1f, -1f) // 7
		};

		public WorkspaceController(Vector3 size)
		{
			_size = size;
			_gridLines = new List<Vector3>();
		}

		private void GenerateMesh()
		{
			Mesh mesh = new Mesh();
			Vector3[] vertices = new Vector3[6];
		}


		public List<Vector3> GetWorkspaceGridLines()
		{
			_gridLines.Clear();

			// Top
			PaintGrid(0, 1, 5, 4);

			// Bottom
			PaintGrid(2, 3, 7, 6);

			// Front
			PaintGrid(0, 2, 6, 4);

			// Left
			PaintGrid(4, 5, 7, 6);

			// Right
			PaintGrid(0, 1, 3, 2);

			return _gridLines;
		}

		// Clockwise vertices
		private void PaintGrid(uint i0, uint i1, uint i2, uint i3)
		{
			var v0 = GetCubeCoord(i0);
			var v1 = GetCubeCoord(i1);
			var v2 = GetCubeCoord(i2);
			var v3 = GetCubeCoord(i3);

			int div = Options.NumGridLines + 1;
			for (int i = 0; i <= div; i++)
			{
				float f = i / (float)div;
				var vert0 = Vector3.Lerp(v0, v1, f);
				var vert1 = Vector3.Lerp(v3, v2, f);
				_gridLines.Add(vert0);
				_gridLines.Add(vert1);

				vert0 = Vector3.Lerp(v0, v3, f);
				vert1 = Vector3.Lerp(v1, v2, f);
				_gridLines.Add(vert0);
				_gridLines.Add(vert1);
			}
		}

		private Vector3 GetCubeCoord(uint id)
		{
			if (id < 8)
			{
				var localPosition = Vector3.Scale(_size, _cubeVertices[id]) * 0.5f;
				return HandTracking.ToWorldCoordinates(localPosition);
			}
			return Vector3.zero;
		}
	}
}

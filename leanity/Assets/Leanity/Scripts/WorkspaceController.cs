using System.Collections.Generic;
using UnityEngine;

namespace Leanity
{
	public enum WorkspaceState { Hide, Idle, Grab, Pinch };

	public class WorkspaceController
	{
		private WorkspaceState _state;
		public WorkspaceState State
		{
			get { return _state; }
			set { _state = value; }
		}

		private static readonly string _shaderStr = "UI/Unlit/Transparent";
		private Vector3 _size;
		private List<Vector3> _gridLines;
		private Vector3[] _cubeVertices =
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
		private Mesh _mesh;
		private Material _mat;

		public WorkspaceController(Vector3 size)
		{
			_size = size;
			_gridLines = new List<Vector3>();

			for (int i = 0; i < _cubeVertices.Length; i++)
			{
				_cubeVertices[i] = Vector3.Scale(_cubeVertices[i], _size) * 0.5f;
			}

			//TODO: choose shader in options?
			Shader shader = Shader.Find(_shaderStr);
			if (!shader)
			{
				Debug.LogError("Leanity: Unable to load shader " + _shaderStr);
			}
			else
			{
				_mat = new Material(shader);
				_mat.hideFlags = HideFlags.HideAndDontSave;
			}

			GenerateDrawingStuff();

			Options.OnOptionsChange += GenerateDrawingStuff;
		}

		private void GenerateDrawingStuff()
		{
			GenerateMesh();
			GenerateWorkspaceGridLines();
		}

		public void Draw(float alpha, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			if (_mat != null)
			{
				Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);

				if (Options.ShowWorkspace)
				{
					DrawMesh(alpha, matrix);
				}

				if (Options.ShowGrid)
				{
					Color color = _state == WorkspaceState.Idle ? Options.GridColor : Options.GrabGridColor;
					color.a = alpha;
					DrawLines(color, matrix);
				}
			}
		}

		private void DrawMesh(float alpha, Matrix4x4 matrix)
		{
			if (_mesh != null)
			{
				_mat.color = new Color(0, 0, 0, alpha);
				_mat.SetPass(0);
				Graphics.DrawMeshNow(_mesh, matrix);
			}
			else
			{
				Debug.LogWarning("Leanity: Workspace mesh is null");
			}
		}

		private void GenerateMesh()
		{
			_mesh = new Mesh();

			int[] triangles = {
				0, 5, 1, 0, 4, 5,
				2, 3, 7, 2, 7, 6,
				0, 2, 6, 0, 6, 4,
				4, 7, 5, 4, 6, 7,
				0, 1, 3, 0, 3, 2
			};

			_mesh.vertices = _cubeVertices;
			_mesh.triangles = triangles;
		}

		public void GenerateWorkspaceGridLines()
		{
			_gridLines.Clear();

			GenerateGridOnQuad(0, 1, 5, 4); // Top
			GenerateGridOnQuad(2, 3, 7, 6); // Bottom
			GenerateGridOnQuad(0, 2, 6, 4); // Front
			GenerateGridOnQuad(4, 5, 7, 6); // Left
			GenerateGridOnQuad(0, 1, 3, 2); // Right
		}

		// Clockwise vertices
		private void GenerateGridOnQuad(uint i0, uint i1, uint i2, uint i3)
		{
			var v0 = _cubeVertices[i0];
			var v1 = _cubeVertices[i1];
			var v2 = _cubeVertices[i2];
			var v3 = _cubeVertices[i3];

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

		private void DrawLines(Color color, Matrix4x4 matrix)
		{
			_mat.color = color;
			_mat.SetPass(0);

			GL.PushMatrix();
			GL.MultMatrix(matrix);

			GL.Begin(GL.LINES);
			foreach(var vertex in _gridLines)
			{
				GL.Vertex(vertex);
			}
			GL.End();

			GL.PopMatrix();
		}
	}
}

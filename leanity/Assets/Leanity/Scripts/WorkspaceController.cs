using System.Collections.Generic;
using UnityEngine;

namespace Leanity
{
	public enum WorkspaceState { Hide = 0, Idle = 1, Grab = 2, Pinch = 3};

	public class WorkspaceController
	{
		private WorkspaceState _state;
		public WorkspaceState State
		{
			get { return _state; }
			set { _state = value; }
		}

		private static readonly string _workspaceShaderStr = "UI/Unlit/Transparent";
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
		private Mesh _workspaceMesh;
		private Material _workspaceMat;
		private Material _handMat;
		private Mesh[] _handMeshes;
		private MotionController _motionController;

		public WorkspaceController(Vector3 size, MotionController motionController)
		{
			_motionController = motionController;
			ScaleWorkspace(size);
			CreateMaterials();
			GenerateDrawingStuff();
			LoadMeshes();

			Options.OnOptionsChange += GenerateDrawingStuff;
		}

		private void CreateMaterials()
		{
			_workspaceMat = CreateMaterial(_workspaceShaderStr);
			_handMat = Resources.Load<Material>("HandMaterial");
		}

		private Material CreateMaterial (string shaderStr)
		{
			Material mat = null;
			Shader shader = Shader.Find(shaderStr);
			if (!shader)
			{
				Debug.LogError("Leanity: Unable to load shader " + shaderStr);
			}
			else
			{
				mat = new Material(shader);
				mat.hideFlags = HideFlags.HideAndDontSave;
			}
			return mat;
		}

		private void ScaleWorkspace(Vector3 size)
		{
			for (int i = 0; i < _cubeVertices.Length; i++)
			{
				_cubeVertices[i] = Vector3.Scale(_cubeVertices[i], size) * 0.5f;
			}
		}

		private void GenerateDrawingStuff()
		{
			GenerateMesh();
			GenerateWorkspaceGridLines();
		}

		public void Draw(float alpha, Vector3 position, Quaternion rotation, Vector3 scale)
		{
			if (_workspaceMat != null)
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

				DrawHand(HandTracking.LeftHandData);
				DrawHand(HandTracking.RightHandData);
			}
		}

		private void DrawMesh(float alpha, Matrix4x4 matrix)
		{
			if (_workspaceMesh != null)
			{
				_workspaceMat.color = new Color(0, 0, 0, alpha);
				_workspaceMat.SetPass(0);
				Graphics.DrawMeshNow(_workspaceMesh, matrix);
			}
			else
			{
				GenerateMesh();
			}
		}

		private void GenerateMesh()
		{
			_workspaceMesh = new Mesh();


			int[] triangles = {
				0, 5, 1, 0, 4, 5,
				2, 3, 7, 2, 7, 6,
				0, 2, 6, 0, 6, 4,
				4, 7, 5, 4, 6, 7,
				0, 1, 3, 0, 3, 2
			};

			_workspaceMesh.vertices = _cubeVertices;
			_workspaceMesh.triangles = triangles;

			if (!_workspaceMesh) Debug.Log("Unable to create workspace mesh");
		}

		public void GenerateWorkspaceGridLines()
		{
			_gridLines = new List<Vector3>();

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
			_workspaceMat.color = color;
			_workspaceMat.SetPass(0);

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


		private void LoadMeshes()
		{
			_handMeshes = new Mesh[8];

			_handMeshes[0] = null;
			_handMeshes[1] = Resources.Load<Mesh>("extended_hand_left");
			_handMeshes[2] = Resources.Load<Mesh>("grab_hand_left");
			_handMeshes[3] = Resources.Load<Mesh>("pinch_hand_left");

			_handMeshes[4] = null;
			_handMeshes[5] = Resources.Load<Mesh>("extended_hand_right");
			_handMeshes[6] = Resources.Load<Mesh>("grab_hand_right");
			_handMeshes[7] = Resources.Load<Mesh>("pinch_hand_right");
		}

		private void DrawHand(HandData hand)
		{
			if (hand.Detected)
			{
				var handState = GetHandState(hand);

				Mesh mesh = _handMeshes[(int)handState + (hand.IsRight ? 4 : 0)];
				if (mesh != null)
				{
					_handMat.SetPass(0);
					var handPos = HandTracking.ToWorldCoordinates(hand.Position);
					var handRot = HandTracking.ToWorldCoordinates(hand.Rotation) * Quaternion.Euler(180f, 0f, 0f);
					Vector3 offset = new Vector3(0f, 0f, 0.025f) * Options.HandScale * Options.PosScale;
					offset = handRot * offset;

					Matrix4x4 matrix = Matrix4x4.TRS(handPos + offset, handRot, Vector3.one * Options.PosScale * Options.HandScale);
					Graphics.DrawMeshNow(mesh, matrix);
				}
			}
		}

		private WorkspaceState GetHandState (HandData hand)
		{
			WorkspaceState handState = WorkspaceState.Idle;
			if (hand.IsRight)
			{
				if (_motionController.RightGrab.IsHolding)
				{
					handState = WorkspaceState.Grab;
				}
				else if (_motionController.RightPinch.IsHolding)
				{
					handState = WorkspaceState.Pinch;
				}
			}
			else
			{
				if (_motionController.LeftGrab.IsHolding)
				{
					handState = WorkspaceState.Grab;
				}
				else if (_motionController.LeftPinch.IsHolding)
				{
					handState = WorkspaceState.Pinch;
				}
			}

			return handState;
		}
	}
}

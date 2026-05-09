using UnityEngine;

namespace Lemonity.Core
{
	public class LemonityComponent : MonoBehaviour
	{
		[Header("Operation Mode")]
		public WorkingMode _mode = WorkingMode.FlyOneHand;

		[Header("Fly Options")]
		public float _flySpeed = 1.2f;
		public float _flyYRotationSpeed = 1f;
		public float _flyXRotationSpeed = 1f;
		public bool _flyHover = false;

		[Header("Orbit Options")]
		public Transform _orbitObject;
		public bool _orbitExponentialZoom = false;

		[Header("Common Options")]
		public bool _inertia = true;

		private MotionController _motion;

		void Start()
		{
			_motion = new MotionController(_mode);
			_motion.MotionRuntime.SelectionCenter = OrbitCenter;
			OnValidate();
		}

		void Update()
		{
			if (Application.isFocused)
			{
				_motion.Update(transform.position, transform.rotation, 1f);

				transform.SetPositionAndRotation(_motion.Position, _motion.Rotation);
			}
		}

		private void OnValidate()
		{
			if (_motion != null)
			{
				_motion.CurrentMode = _mode;
				Options.FlyMode.Hover = _flyHover;
				Options.FlyMode.PosScale = _flySpeed;
				Options.FlyMode.YawScale = _flyYRotationSpeed;
				Options.FlyMode.PitchScale = _flyXRotationSpeed;

				Options.OrbitMode.ExponentialZoom = _orbitExponentialZoom;

				Options.Inertia.EnableInertia = _inertia;
			}
		}

		private Vector3 OrbitCenter()
		{
			if (_orbitObject != null)
			{
				return _orbitObject.position;
			}

			return Vector3.zero;
		}
	}
}

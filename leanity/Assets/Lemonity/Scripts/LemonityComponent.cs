using UnityEngine;

namespace Lemonity
{

	public class LemonityComponent : MonoBehaviour
	{
		[Header("Operation Mode")]
		public WorkingMode _mode = WorkingMode.FlyOneHand;
		public bool _hover = false;

		public Transform _orbitObject;

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
				Options.FlyHover = _hover;
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
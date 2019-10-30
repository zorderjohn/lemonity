using UnityEngine;
using System.Collections;
using Leap;

namespace Lemonity
{

	public class LemonityComponent : MonoBehaviour
	{
		[Header("Operation Mode")]

		public bool _isCamera = false;
		public bool _invertAxis = false;

		[Range(0f, 90f)]
		public float PitchLimit = 75f;

		public bool absoluteMovement = true;
		public bool twoHands = false;
		private MotionController motion;

		void Start()
		{
			_invertAxis = Options.InvertAxis;

			motion = new MotionController();
			OnValidate();
		}

		// Update is called once per frame
		void Update()
		{
			motion.Update(transform.position, transform.rotation, 1f);

			transform.position = motion.Position;
			transform.rotation = motion.Rotation;
		}


		private void OnValidate()
		{
			Options.InvertAxis = _invertAxis;

		}
	}
}
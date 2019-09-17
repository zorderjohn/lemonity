using UnityEngine;
using System.Collections;
using Leap;

namespace Leanity
{

	public class LeapTestCalc : MonoBehaviour
	{
		[Header("Operation Mode")]

		public bool isCamera = false;
		public bool invertAxis = false;
		[Range(0f, 90f)]
		public float PitchLimit = 75f;

		public bool absoluteMovement = true;
		public bool twoHands = false;
		private MotionController motion;

		void Start()
		{
			motion = new MotionController();
			OnValidate();
		}

		// Update is called once per frame
		void Update()
		{
			motion.Update(transform.position, transform.rotation);

			transform.position = motion.Position;
			transform.rotation = motion.Rotation;
		}


		private void OnValidate()
		{


		}
	}
}
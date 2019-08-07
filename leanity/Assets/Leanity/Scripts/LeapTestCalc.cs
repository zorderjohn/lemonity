using UnityEngine;
using System.Collections;
using Leap;

namespace Leanity
{

	public class LeapTestCalc : MonoBehaviour
	{
		HandData mainHand;
		HandData auxHand;

		public KeyCode key = KeyCode.A;
		Controller c;
		bool isHolding = false;

		Quaternion startupObjectRot;
		Vector3 startupObjectPos;

		Quaternion initialObjectRot;
		Vector3 initialObjectPos;

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

			startupObjectPos = transform.position;
			startupObjectRot = transform.rotation;

			OnValidate();
		}

		// Update is called once per frame
		void Update()
		{
			motion.Update(transform.position, transform.rotation);

			transform.position = motion.ObjectPosition;
			transform.rotation = motion.ObjectRotation;
		}


		private void OnValidate()
		{


		}
	}
}
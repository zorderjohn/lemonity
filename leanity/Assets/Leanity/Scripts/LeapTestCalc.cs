using UnityEngine;
using System.Collections;
using Leap;


public class LeapTestCalc : MonoBehaviour {
	class HandData
	{
		public Vector3 InitialPosition { get; private set; }
		public Quaternion InitialRotation { get; private set; }

		private Vector3 _position;
		public Vector3 Position
		{
			get { return _position; }
			set
			{
				_position = _positionFilter.Filter(value);
			}
		}

		public Quaternion _rotation;
		public Quaternion Rotation
		{
			get { return _rotation;  }
			set
			{
				_rotation = _rotationFilter.Filter(value);
			}
		}

		public Vector3 DeltaPosition
		{
			get { return Position - InitialPosition; }
		}

		public Quaternion DeltaRotation
		{
			get { return Quaternion.Inverse(InitialRotation) * Rotation; }
		}


		public float grabValue;
		public bool detected;

		private OneEuroFilter<Quaternion> _rotationFilter;
		private OneEuroFilter<Vector3> _positionFilter;

		public HandData(float filterFrequency)
		{
			_rotationFilter = new OneEuroFilter<Quaternion>(filterFrequency);
			_positionFilter = new OneEuroFilter<Vector3>(filterFrequency);
		}

		public void CaptureInitialPose()
		{
			InitialPosition = Position;
			InitialRotation = Rotation;
		}

		public void SetRotationFilterParams (float frequency, float minCutOff, float beta, float dCutOff)
		{
			_rotationFilter.UpdateParams(frequency, minCutOff, beta, dCutOff);
		}

		public void SetPositionFilterParams (float frequency, float minCutOff, float beta, float dCutOff)
		{
			_positionFilter.UpdateParams(frequency, minCutOff, beta, dCutOff);
		}
	}

	HandData mainHand;
	HandData auxHand;

	public KeyCode key = KeyCode.A;
	Controller c;
	bool isHolding = false;

	Quaternion initialObjectRot;
	Vector3 initialObjectPos;

	[Header("Sensitivity")]

	[Range(0f, 1f)]
	public float posScale = 1f;

	[Range(0f, 1f)]
	public float rotScale = 1f;

	public Vector3 perAxisRotationScale = Vector3.one;

	[Header("Operation Mode")]

	public bool isCamera = false;
	public bool invertAxis = false;

	[Range(0f, 1f)]
	public float grabThreshold = .5f;
	public bool grabEnabled = false;

	public bool absoluteMovement = true;
	public bool twoHands = false;

	[Header("Filter")]
	public bool stabilizePosition = true;
	public bool stabilizeRotation = true;

	public float filterFrequency = 120f;

	[Header("RotationFilter")]
	public float rotFilterMinCutoff = 1.0f;
	public float rotFilterBeta = 0.0f;
	public float rotFilterDcutoff = 1.0f;

	[Header("PositionFilter")]
	public float posFilterMinCutoff = 1.0f;
	public float posFilterBeta = 0.0f;
	public float posFilterDcutoff = 1.0f;


	void Start () {
		// Initialize tracking
		c = new Controller();

		//Create Hands
		mainHand = new HandData(filterFrequency);
		auxHand = new HandData(filterFrequency);

		OnValidate();
	}


	void UpdateHandData(ref Hand leapHand, ref HandData customHand)
	{
		customHand.Rotation = lpToUnityRot(leapHand.Rotation);
		customHand.Position = lpToUnityVec(leapHand.PalmPosition);
		customHand.grabValue = leapHand.GrabStrength;
		customHand.detected = true;
	}

	void UpdateTracking()
	{
		Frame frame = c.Frame();
		// Being pesimistic to avoid some conditionals
		mainHand.detected = false;
		auxHand.detected = false;

		// TODO: Check coherence between left/right and hand ids
		if (frame.Hands.Count > 0)
		{
			Hand h = frame.Hands[0];
			UpdateHandData(ref h, ref mainHand);
		}

		if (frame.Hands.Count > 1)
		{
			Hand h = frame.Hands[1];
			UpdateHandData(ref h, ref auxHand);
		}
	}

	// Update is called once per frame
	void Update () {
		UpdateTracking();
		KeyController();
	}

	Vector3 lpToUnityVec(Vector lv) {
		float invertValue = invertAxis ? -1f : 1f;
		float scaleFactorX = 0.001f * invertValue;
		float scaleFactorY = 0.001f * invertValue;
		float scaleFactorZ = -0.001f * invertValue;
		return new Vector3(lv.x * scaleFactorX, lv.y * scaleFactorY, lv.z * scaleFactorZ);
	}
	Quaternion lpToUnityRot(LeapQuaternion lq) {
		float invertValue = invertAxis ? -1f : 1f;
		return new Quaternion(-lq.x * invertValue, -lq.y * invertValue, lq.z * invertValue, lq.w);
	}

	void StartMoving() {
		mainHand.CaptureInitialPose();
		auxHand.CaptureInitialPose();
	}


	private Vector3 NormalizedEulerAngles (Quaternion q)
	{
		Vector3 euler = q.eulerAngles;
		euler.x = NormalizeAngle(euler.x);
		euler.y = NormalizeAngle(euler.y);
		euler.z = NormalizeAngle(euler.z);

		return euler;
	}

	private float NormalizeAngle(float angle)
	{
		if (angle > 180f)
		{
			return angle - 360f;
		}
		return angle;
	}

	private void OnValidate()
	{
		mainHand?.SetRotationFilterParams(filterFrequency, rotFilterMinCutoff, rotFilterBeta, rotFilterDcutoff);
		mainHand?.SetPositionFilterParams(filterFrequency, posFilterMinCutoff, posFilterBeta, posFilterDcutoff);

		auxHand?.SetRotationFilterParams(filterFrequency, rotFilterMinCutoff, rotFilterBeta, rotFilterDcutoff);
		auxHand?.SetPositionFilterParams(filterFrequency, posFilterMinCutoff, posFilterBeta, posFilterDcutoff);
	}

	void OneHandMove() {
		Vector3 deltaMovement = mainHand.DeltaPosition * posScale;

		if (isCamera)
		{
			deltaMovement = transform.rotation * deltaMovement;
		}

		transform.position = absoluteMovement ? initialObjectPos + deltaMovement : transform.position + deltaMovement;


		Quaternion deltaRot = mainHand.DeltaRotation;

		Vector3 eulerDeltaRot = NormalizedEulerAngles(deltaRot);
		eulerDeltaRot.Scale(perAxisRotationScale);
		eulerDeltaRot *= rotScale;

		deltaRot = Quaternion.Euler(eulerDeltaRot);


		if(isCamera) {
			Quaternion targetRotation = absoluteMovement? initialObjectRot * deltaRot : transform.rotation * deltaRot;

			// Disable Z rotation
			Vector3 vRots = targetRotation.eulerAngles;
			vRots.z = 0f;
			transform.rotation = Quaternion.Euler(vRots);

		} else {
			transform.rotation = deltaRot * initialObjectRot;
		}
	}

	void StopMoving() {
	}

	void KeyController() {
		bool isGrabbing = false;
		if (twoHands)
		{
			isGrabbing = mainHand.detected && mainHand.grabValue >= grabThreshold &&
			             auxHand.detected && auxHand.grabValue >= grabThreshold;
		} else
		{
			isGrabbing = mainHand.detected && mainHand.grabValue >= grabThreshold;
		}


		bool triggerOn = grabEnabled && isGrabbing || Input.GetKeyDown(key);
		bool triggerOff = !triggerOn;

		if(!isHolding && triggerOn ) {
			isHolding = true;
			StartMoving();
		} else if(isHolding && triggerOff) {
			isHolding = false;
			StopMoving();
		}

		if(isHolding) {
			OneHandMove();
		}
	}
}

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

	Quaternion startupObjectRot;
	Vector3 startupObjectPos;

	Quaternion initialObjectRot;
	Vector3 initialObjectPos;

	[Header("Sensitivity")]

	[Range(1, 10)]
	public int posScaleMultiplier = 1;

	[Range(0f, 1f)]
	public float posScale = 1f;

	[Range(1, 10)]
	public int rotScaleMultiplier = 1;

	[Range(0f, 1f)]
	public float rotScale = 1f;

	public Vector3 perAxisRotScaleAdjustment = Vector3.one;

	[Header("Operation Mode")]

	public bool isCamera = false;
	public bool invertAxis = false;

	[Range(0f, 1f)]
	public float grabThreshold = .5f;
	public bool grabEnabled = false;

	public bool absoluteMovement = true;
	public bool twoHands = false;

	[Header("Inertia")]
	public bool enableInertia = false;
	public float angularDrag = 1f;
	public float linearDrag = 1f;
	public int velocityFrames = 5;
	public int discardFrames = 5;

	[Header("Filter")]
	public float filterFrequency = 120f;

	[Header("RotationFilter")]
	public float rotFilterMinCutoff = 1.0f;
	public float rotFilterBeta = 0.0f;
	public float rotFilterDcutoff = 1.0f;

	[Header("PositionFilter")]
	public float posFilterMinCutoff = 1.0f;
	public float posFilterBeta = 0.0f;
	public float posFilterDcutoff = 1.0f;

	InertialObject _inertialData;

	void Start () {

		startupObjectPos = transform.position;
		startupObjectRot = transform.rotation;

		// Initialize tracking
		c = new Controller();

		//Create Hands
		mainHand = new HandData(filterFrequency);
		auxHand = new HandData(filterFrequency);

		_inertialData = new InertialObject(velocityFrames);

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
		EventController();
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
		Vector3 deltaMovement = mainHand.DeltaPosition * posScale * posScaleMultiplier;

		if (isCamera)
		{
			deltaMovement = transform.rotation * deltaMovement;
		}

		transform.position = absoluteMovement ? initialObjectPos + deltaMovement : transform.position + deltaMovement;


		Quaternion deltaRot = mainHand.DeltaRotation;

		Vector3 eulerDeltaRot = NormalizedEulerAngles(deltaRot);
		eulerDeltaRot.Scale(perAxisRotScaleAdjustment);
		eulerDeltaRot *= rotScale * rotScaleMultiplier;

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

		// Capture inertial data
		float t = Time.time;
		_inertialData.SetPosition(transform.position, t);
		_inertialData.SetRotation(transform.rotation, t);
	}

	void TwoHandsMove()
	{

	}

	void InertialMove()
	{
		float deltaTime = Time.deltaTime;


		var linearVelocity = _inertialData.GetLinearVelocity();
		linearVelocity *= linearDrag;
		transform.position += linearVelocity * deltaTime;
		_inertialData.SetPosition(transform.position, Time.time);

		Vector3 eulerVelocity = _inertialData.AngularVelocityEuler * angularDrag;

		// Up vector always pointing up
		eulerVelocity.z = 0;
		_inertialData.AngularVelocityEuler = eulerVelocity;

		Quaternion deltaRotation = Quaternion.Euler( eulerVelocity * deltaTime );

		Quaternion orientation = deltaRotation * transform.rotation;

		// Up vector always pointing up
		Vector3 eulerOrientation = orientation.eulerAngles;
		eulerOrientation.z = 0;
		orientation.eulerAngles = eulerOrientation;

		//TODO: Limit roll angle (avoid looking directly up or down)

		transform.rotation = orientation;
		//_inertialData.SetRotation(orientation, Time.time);
	}

	void StartMoving() {
		mainHand.CaptureInitialPose();
		auxHand.CaptureInitialPose();

		initialObjectPos = transform.position;
		initialObjectRot = transform.rotation;

		_inertialData.Clear();
	}


	void StopMoving()
	{
		//TODO: calculate angular velocity every frame to detect discontinuities
		//_inertialData.DiscardFrames(discardFrames);
		//_inertialData.CalculateAngularVelocity();
	}

	void EventController() {

		if (Input.GetKeyDown(KeyCode.R))
		{
			transform.position = startupObjectPos;
			transform.rotation = startupObjectRot;
			_inertialData.Clear();
		}

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
			if (twoHands)
			{
				TwoHandsMove();
			}
			else
			{
				OneHandMove();
			}
			_inertialData.CalculateAngularVelocity();
		}
		else if (enableInertia)
		{
			InertialMove();
		}

		GraphDbg.Log("vel", _inertialData.GetLinearVelocity().magnitude);

		GraphDbg.Log("angularVel", _inertialData.AngularVelocityEuler.magnitude, 1000);
	}
}

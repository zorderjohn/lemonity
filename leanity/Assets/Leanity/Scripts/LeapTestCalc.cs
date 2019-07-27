using UnityEngine;
using System.Collections;
using Leap;

public class LeapTestCalc : MonoBehaviour {
	public KeyCode key = KeyCode.A;
	Controller c;
	bool isHolding = false;
	Quaternion initialObjectRot;
	Quaternion initialHandRot;
	Vector3 initialObjectPos;
	Vector3 initialHandPos;

	Quaternion curHandRotation;
	Vector3 curHandPosition;
	public float posScale = 1f;
	public int rotScale = 2;
	public bool isCamera = false;

	public bool invertAxis = false;
	[Range(0f, 1f)]
	public float grabThreshold = .5f;
	public bool grabEnabled = false;

	[Header("Filter")]
	public bool stabilizePosition = true;
	[Range(0f, 1f)]
	public float positionFilterValue = 0.5f;
	[Range(0f, 1f)]
	public float rotationFilterValue = 0.5f;

	private float _grabValue = 0f;


	void Start () {
		c = new Controller();
	}

	// Update is called once per frame
	void Update () {
		Frame frame = c.Frame();
		Hand h;
		if(frame.Hands.Count > 0) {
			h = frame.Hands[0];

			curHandRotation = lpToUnityRot(h.Rotation);
			curHandPosition = lpToUnityVec(h.PalmPosition);
			_grabValue = h.GrabStrength;

		}

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
		//Debug.Log("holding!");
		initialHandPos = curHandPosition;
		initialObjectPos = transform.position;

		initialHandRot = curHandRotation;
		initialObjectRot = transform.rotation;
	}

	void DoMove() {
		Vector3 deltaMovement = (curHandPosition - initialHandPos) * posScale;
		if(isCamera) {
			deltaMovement = transform.rotation * deltaMovement;
		}
		Vector3 targetPosition = initialObjectPos + deltaMovement;
		transform.position = stabilizePosition ? Vector3.Lerp(transform.position, targetPosition, positionFilterValue) : targetPosition;
		Quaternion deltaRot = Quaternion.Inverse( initialHandRot ) * curHandRotation;
		for(int i=1; i<rotScale; i++) {
			deltaRot *= deltaRot;
		}



		if(isCamera) {
			Quaternion targetRotation = initialObjectRot * deltaRot;
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 0.5f);
			Vector3 vRots = transform.rotation.eulerAngles;
			vRots.z = 0f;
			transform.rotation = Quaternion.Euler(vRots);
		} else {
			transform.rotation = deltaRot * initialObjectRot;
		}


	}

	void StopMoving() {
		//Debug.Log("unholding");
	}

	void KeyController() {
		bool triggerOn = grabEnabled ? _grabValue >= grabThreshold : Input.GetKeyDown(key);
		bool triggerOff = grabEnabled ? _grabValue < grabThreshold : Input.GetKeyUp(key);

		if(!isHolding && triggerOn ) {
			isHolding = true;
			StartMoving();
		} else if(isHolding && triggerOff) {
			isHolding = false;
			StopMoving();
		}

		if(isHolding) {
			DoMove();
		}
	}
}

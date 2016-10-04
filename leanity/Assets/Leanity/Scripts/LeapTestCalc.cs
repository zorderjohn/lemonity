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
		}

		KeyController();
	}


	Vector3 lpToUnityVec(Vector lv) {
		const float scaleFactorX = 0.001f;
		const float scaleFactorY = 0.001f;
		const float scaleFactorZ = -0.001f;
		return new Vector3(lv.x * scaleFactorX, lv.y * scaleFactorY, lv.z * scaleFactorZ);

	}
	Quaternion lpToUnityRot(LeapQuaternion lq) {
		return new Quaternion(-lq.x, -lq.y, lq.z, lq.w);
	}

	void StartMoving() {
		Debug.Log("holding!");
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
		transform.position = initialObjectPos + deltaMovement;
		Quaternion deltaRot = Quaternion.Inverse( initialHandRot ) * curHandRotation; 
		for(int i=1; i<rotScale; i++) {
			deltaRot *= deltaRot;
		}



		if(isCamera) {
			transform.rotation = initialObjectRot * deltaRot;
			Vector3 vRots = transform.rotation.eulerAngles;
			vRots.z = 0f;
			transform.rotation = Quaternion.Euler(vRots);
		} else {
			transform.rotation = deltaRot * initialObjectRot;
		}


	}

	void StopMoving() {
		Debug.Log("unholding");
	}

	void KeyController() {
		if(!isHolding && Input.GetKeyDown(key) ) {
			isHolding = true;
			StartMoving();
		} else if(isHolding && Input.GetKeyUp(key)) {
			isHolding = false;
			StopMoving();
		}

		if(isHolding) {
			DoMove();
		}
	}
}

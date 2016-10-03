using UnityEngine;
using System.Collections;
using Leap;

public class LeapTestCalc : MonoBehaviour {

	Controller c;
	bool isHolding = false;
	Quaternion initialObjectRot;
	Quaternion initialHandRot;
	Vector3 initialObjectPos;
	Vector3 initialHandPos;

	Quaternion curHandRotation;
	Vector3 curHandPosition;

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
		//transform.position = initialObjectPos + (curHandPosition - initialHandPos);
		Quaternion deltaRot = Quaternion.Inverse( initialHandRot ) * curHandRotation; 
		transform.rotation = deltaRot * initialObjectRot;
	}

	void StopMoving() {
		Debug.Log("unholding");
	}

	void KeyController() {
		if(!isHolding && Input.GetKeyDown(KeyCode.A) ) {
			isHolding = true;
			StartMoving();
		} else if(isHolding && Input.GetKeyUp(KeyCode.A)) {
			isHolding = false;
			StopMoving();
		}

		if(isHolding) {
			DoMove();
		}
	}
}

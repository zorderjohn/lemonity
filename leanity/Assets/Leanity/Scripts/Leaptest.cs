using UnityEngine;
using System.Collections;
using Leap;

public class Leaptest : MonoBehaviour {

	Controller c;

	// Use this for initialization
	void Start () {
		c = new Controller();


	}
	
	// Update is called once per frame
	void Update () {
		Frame frame = c.Frame();
		Hand h;
		if(frame.Hands.Count > 0) {
			setVisible(true);
			h = frame.Hands[0];
			transform.rotation = lpToUnityRot(h.Rotation);
			transform.localPosition = lpToUnityVec(h.PalmPosition);
		} else {
			setVisible(false);	
		}
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

	void setVisible(bool value) {
		for(int i=0; i<transform.childCount; i++) {
			transform.GetChild(i).gameObject.SetActive(value);
		}
	}
}

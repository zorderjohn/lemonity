using System;
using System.Runtime.InteropServices;
using UnityEngine;
using Leanity;

namespace SpaceNavigatorDriver {

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    struct SVector3
    {
        public float X, Y, Z;
    }
    struct SQuaternion
    {
        public float Angle, X, Y, Z;
    }
	
    class Sensor
    {
        private LeapManager _lm;
        public Sensor()
        {
            _lm = new LeapManager();
        }

        public SVector3 Translation
        {
            get
            {
                SVector3 sv = new SVector3();
                Vector3 v3 = _lm.Position;
                sv.X = v3.x;
                sv.Y = v3.y;
                sv.Z = v3.z;
                return sv;
            }
        }
        public SQuaternion Rotation;
    }
    class Device
    {
        public Sensor Sensor;
        public bool IsConnected;
        public Device()
        {
            IsConnected = true;
            Sensor = new Sensor();
        }

        public void Connect()
        {

        }
        public void Disconnect()
        {

        }

    }

	class SpaceNavigatorLeap : SpaceNavigator {
		private const float TransSensScale = 0.1f, RotSensScale = 0.0008f;

		// Public API
		public override Vector3 GetTranslation() {
			float sensitivity = Application.isPlaying ? Settings.PlayTransSens : Settings.TransSens[Settings.CurrentGear];
			return (SubInstance._sensor == null ?
						Vector3.zero :
						new Vector3(
							Settings.GetLock(DoF.Translation, Axis.X) ? 0 : (float)SubInstance._sensor.Translation.X,
							Settings.GetLock(DoF.Translation, Axis.Y) ? 0 : (float)SubInstance._sensor.Translation.Y,
							Settings.GetLock(DoF.Translation, Axis.Z) ? 0 : -(float)SubInstance._sensor.Translation.Z) *
						sensitivity * TransSensScale);
		}
		public override Quaternion GetRotation() {
			float sensitivity = Application.isPlaying ? Settings.PlayRotSens : Settings.RotSens;
			return (SubInstance._sensor == null ?
						Quaternion.identity :
						Quaternion.AngleAxis(
							(float)SubInstance._sensor.Rotation.Angle * sensitivity * RotSensScale,
							new Vector3(
								Settings.GetLock(DoF.Rotation, Axis.X) ? 0 : -(float)SubInstance._sensor.Rotation.X,
								Settings.GetLock(DoF.Rotation, Axis.Y) ? 0 : -(float)SubInstance._sensor.Rotation.Y,
								Settings.GetLock(DoF.Rotation, Axis.Z) ? 0 : (float)SubInstance._sensor.Rotation.Z)));
		}

		// Device variables
		private Sensor _sensor;
		private Device _device;
        //private Keyboard _keyboard;

        #region - Singleton -
        /// <summary>
        /// Private constructor, prevents a default instance of the <see cref="SpaceNavigatorLeap" /> class from being created.
        /// </summary>
        private SpaceNavigatorLeap() {
			try {
				if (_device == null) {
					_device = new Device();
					_sensor = _device.Sensor;
					//_keyboard = _device.Keyboard;
				}
				if (!_device.IsConnected)
					_device.Connect();
			}
			catch (COMException ex) {
				Debug.LogError(ex.ToString());
			}
		}

		public static SpaceNavigatorLeap SubInstance {
			get { return _subInstance ?? (_subInstance = new SpaceNavigatorLeap()); }
		}
		private static SpaceNavigatorLeap _subInstance;
		#endregion - Singleton -

		#region - IDisposable -
		public override void Dispose() {
			try {
				if (_device != null && _device.IsConnected) {
					_device.Disconnect();
					_subInstance = null;
					GC.Collect();
				}
			}
			catch (COMException ex) {
				Debug.LogError(ex.ToString());
			}
		}
		#endregion - IDisposable -
	}
#endif    // UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
}
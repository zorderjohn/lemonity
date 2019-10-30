using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lemonity
{
	public class ValueFade
	{

		public float MinValue { get; set; }
		public float MaxValue { get; set; }
		public float InTime { get; set; }
		public float OutTime { get; set; }
		public float Value
		{
			get
			{
				float deltaTime = Time.realtimeSinceStartup - _lastTime;
				if (_countDown > 0f)
				{
					_countDown -= deltaTime;
					if (_countDown <= 0f)
					{
						FadeOut(_nextFadeoutTime);
					}
				}

				_value = Mathf.Clamp(_value + _velocity * deltaTime, MinValue, MaxValue);
				_lastTime += deltaTime;
				return _value;
			}
			set
			{
				_lastTime = Time.realtimeSinceStartup;
				_value = value;
				_velocity = 0f;
			}
		}
		private float _value = 0f;
		private float _velocity = 0f;
		private float _lastTime = 0f;
		private float _countDown = 0f;
		private float _nextFadeoutTime = 1f;

		public ValueFade()
		{
			MinValue = 0f;
			MaxValue = 1f;
			InTime = .5f;
			OutTime = 1f;
			_lastTime = Time.realtimeSinceStartup;
		}

		public void FadeIn(float inTime)
		{
			_velocity = (MaxValue - MinValue) / inTime;
			_lastTime = Time.realtimeSinceStartup;
			_countDown = 0f;
		}

		public void FadeIn()
		{
			FadeIn(InTime);
		}

		public void FadeOut(float outTime)
		{
			_velocity = (MinValue - MaxValue) / outTime;
			_lastTime = Time.realtimeSinceStartup;
			_countDown = 0f;
		}

		public void FadeOut()
		{
			FadeOut(OutTime);
		}

		public void FadeOutAfterTime(float outTime, float countDown)
		{
			_countDown = countDown;
			_nextFadeoutTime = outTime;
		}
	}
}
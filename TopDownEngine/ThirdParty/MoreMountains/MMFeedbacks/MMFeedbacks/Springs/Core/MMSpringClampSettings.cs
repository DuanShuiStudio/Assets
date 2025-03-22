using System;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
	[Serializable]
	public class MMSpringClampSettings
	{
		[Header("Min最小值")]
		/// whether or not to clamp the min value of this spring, preventing it from going below a certain value
		[Tooltip("是否限制这个弹簧的最小值，以防止其低于某个特定的值。 ")]
		public bool ClampMin = false;
		/// the value below which this spring can't go
		[Tooltip("这个弹簧不能低于的数值。 ")]
		[MMCondition("ClampMin", true)]
		public float ClampMinValue = 0f;
		/// if ClampMin is true, whether or not to use the initial value as the min value
		[Tooltip("如果“限制最小值（ClampMin）”为真，是否将初始值用作最小值。 ")]
		[MMCondition("ClampMin", true)]
		public bool ClampMinInitial = false;
		/// whether or not the spring should bounce off the min value or not
		[Tooltip("这个弹簧是否应该在达到最小值时反弹。 ")]
		[MMCondition("ClampMin", true)]
		public bool ClampMinBounce = false;
		
		[Header("Max最大值")]
		/// whether or not to clamp the max value of this spring, preventing it from going above a certain value
		[Tooltip("是否限制这个弹簧的最大值，防止它超过某个特定的值。 ")]
		public bool ClampMax = false;
		/// the value above which this spring can't go
		[Tooltip("这个弹簧不能超过的数值。")]
		[MMCondition("ClampMax", true)]
		public float ClampMaxValue = 10f;
		/// if ClampMax is true, whether or not to use the initial value as the max value
		[Tooltip("如果“限制最大值（ClampMax）”为真，是否将初始值用作最大值。 ")]
		[MMCondition("ClampMax", true)]
		public bool ClampMaxInitial = false;
		/// whether or not the spring should bounce off the max value or not
		[Tooltip("这个弹簧是否应该在达到最大值时反弹。 ")]
		[MMCondition("ClampMax", true)]
		public bool ClampMaxBounce = false;

		public bool ClampNeeded => ClampMin || ClampMax || ClampMinBounce || ClampMaxBounce;

		public virtual float GetTargetValue(float value, float initialValue)
		{
			float targetValue = value;
			float clampMinValue = ClampMinInitial ? initialValue : ClampMinValue;
			if (ClampMin && value < clampMinValue)
			{
				targetValue = clampMinValue;
			}
			float clampMaxValue = ClampMaxInitial ? initialValue : ClampMaxValue;
			if (ClampMax && value > clampMaxValue)
			{
				targetValue = clampMaxValue;
			}
			return targetValue;
		}
	}
}


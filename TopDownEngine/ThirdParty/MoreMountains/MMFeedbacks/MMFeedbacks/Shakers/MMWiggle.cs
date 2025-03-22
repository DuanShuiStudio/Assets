using UnityEngine;
using System.Collections;
using System;
using MoreMountains.Tools;

namespace MoreMountains.Feedbacks
{
    /// 可能的摆动类型
    public enum WiggleTypes { None, Random, PingPong, Noise, Curve }

    /// <summary>
    /// 一个用于存储公共摆动属性的类。
    /// </summary>
    [Serializable]
	public class WiggleProperties
	{
		[Header("Status状态")]
		public bool WigglePermitted = true;

		[Header("Type类型")]
		/// the position mode : none, random or ping pong - none won't do anything, random will randomize min and max bounds, ping pong will oscillate between min and max bounds
		[Tooltip("位置模式：无、随机或往返模式 - “无”模式不会产生任何效果，“随机”模式会将最小值和最大值边界随机化，“往返”模式会在最小值和最大值边界之间来回振荡。 ")]
		public WiggleTypes WiggleType = WiggleTypes.Random;
		/// if this is true, unscaled delta time, otherwise regular delta time
		[Tooltip("如果这为真，则使用不受时间缩放影响的时间增量（unscaled delta time），否则使用常规的时间增量（regular delta time）。 ")]
		public bool UseUnscaledTime = false;
		/// a multiplier to apply to all time related operations, allowing you to speed up or slow down the wiggle
		[Tooltip("一个应用于所有与时间相关操作的乘数，可让你加快或减慢摆动的速度。 ")]
		public float TimeMultiplier = 1f;
		
		/// whether or not this object should start wiggling automatically on Start()
		[Tooltip("该对象在 `Start()` 方法调用时是否应自动开始摆动。 ")]
		public bool StartWigglingAutomatically = true;
		/// if this is true, position will be ping ponged with an ease in/out curve
		[Tooltip("如果这为真，位置将以缓入/缓出曲线的方式进行往返摆动。 ")]
		public bool SmoothPingPong = true;

		[Header("Speed速度")]
		/// Whether or not the position's speed curve will be used
		[Tooltip("是否将使用位置的速度曲线。 ")]
		public bool UseSpeedCurve = false;
		/// an animation curve to define the speed over time from one position to the other (x), and the actual position (y), allowing for overshoot
		[Tooltip("一条动画曲线，用于定义从一个位置到另一个位置（x轴）随时间变化的速度，以及实际位置（y轴），并允许出现超调情况。 ")]
		public AnimationCurve SpeedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[Header("Frequency频率")]
		/// the minimum time (in seconds) between two position changes
		[Tooltip("两次位置变化之间的最短时间（以秒为单位）。")]
		public float FrequencyMin = 0f;
		/// the maximum time (in seconds) between two position changes
		[Tooltip("两次位置变化之间的最长时间（以秒为单位）。")]
		public float FrequencyMax = 1f;

		[Header("Amplitude振幅")]
		/// the minimum position the object can have
		[Tooltip("该物体所能达到的最小位置。")]
		public Vector3 AmplitudeMin = Vector3.zero;
		/// the maximum position the object can have
		[Tooltip("该物体所能达到的最大位置。")]
		public Vector3 AmplitudeMax = Vector3.one;
		/// if this is true, amplitude will be relative, otherwise world space
		[Tooltip("如果这为真，振幅将是相对的，否则就是基于世界空间的。 ")]
		public bool RelativeAmplitude = true;
		/// if this is true, all amplitude values will match the x amplitude value
		[Tooltip("如果这为真，所有的振幅值都将与x轴方向的振幅值相匹配。 ")]
		public bool UniformValues = false;
		/// if this is true, when randomizing amplitude, the resulting vector's length will be forced to match ForcedVectorLength
		[Tooltip("如果这为真，在对振幅进行随机化处理时，生成的向量的长度将被强制与“强制向量长度（ForcedVectorLength）”相匹配。 ")]
		public bool ForceVectorLength = false;
		/// the length of the randomized amplitude if ForceVectorLength is true
		[Tooltip("如果“强制向量长度（ForceVectorLength）”为真时，随机化振幅的长度。 ")]
		[MMCondition("ForceVectorLength", true)]
		public float ForcedVectorLength = 1f;

		[Header("Curve曲线")]
		/// a curve to animate this property on
		[Tooltip("用于对该属性进行动画处理的一条曲线。 ")]
		public AnimationCurve Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
		/// the minimum value to randomize the curve's zero remap to
		[Tooltip("将曲线的0值重新映射时进行随机化处理的最小值。 ")]
		public Vector3 RemapCurveZeroMin = Vector3.zero;
		/// the maximum value to randomize the curve's zero remap to
		[Tooltip("将曲线的0值重新映射时进行随机化处理的最大值。 ")]
		public Vector3 RemapCurveZeroMax = Vector3.zero;
		/// the minimum value to randomize the curve's one remap to
		[Tooltip("将曲线的1值重新映射时进行随机化处理的最小值。")]
		public Vector3 RemapCurveOneMin = Vector3.one;
		/// the maximum value to randomize the curve's one remap to
		[Tooltip("将曲线的1值重新映射时进行随机化处理的最大值。")]
		public Vector3 RemapCurveOneMax = Vector3.one;
		/// whether or not to add the initial value of this property to the curve's outcome
		[Tooltip("是否将此属性的初始值添加到曲线的计算结果中。 ")]
		public bool RelativeCurveAmplitude = true;
		/// whether or not the curve should be read from left to right, then right to left
		[Tooltip("这条曲线是否应该先从左向右读取，然后再从右向左读取。 ")]
		public bool CurvePingPong = false;

		[Header("Pause暂停")]
		/// the minimum time to spend between two random positions
		[Tooltip("在两个随机位置之间所花费的最短时间 ")]
		public float PauseMin = 0f;
		/// the maximum time to spend between two random positions
		[Tooltip("在两个随机位置之间所花费的最长时间 ")]
		public float PauseMax = 0f;

		[Header("Limited Time限时")]
		/// if this is true, this property will only animate for the specified time
		[Tooltip("如果这为真，此属性将仅在指定的时间内进行动画效果展示。 ")]
		public bool LimitedTime = false;
		/// the maximum time left
		[Tooltip("the maximum time left")]
		public float LimitedTimeTotal;
		/// the animation curve to use to decrease the effect of the wiggle as time goes
		[Tooltip("随着时间推移，用于减弱摆动效果的动画曲线。 ")]
		public AnimationCurve LimitedTimeFalloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);
		/// if this is true, original position will be restored when time left reaches zero
		[Tooltip("如果这为真，当剩余时间达到零时，将会恢复到初始位置。 ")]
		public bool LimitedTimeResetValue = true;
		/// the actual time left
		[Tooltip("实际剩余时间")]
		[MMFReadOnly]
		public float LimitedTimeLeft;        

		[Header("噪点频率")]
		/// the minimum time between two changes of noise frequency
		[Tooltip("两次噪点频率变化之间的最短时间 ")]
		public Vector3 NoiseFrequencyMin = Vector3.zero;
		/// the maximum time between two changes of noise frequency
		[Tooltip("两次噪点频率变化之间的最长时间 ")]
		public Vector3 NoiseFrequencyMax = Vector3.one;

		[Header("噪点偏移")]
		/// how much the noise should be shifted at minimum
		[Tooltip("噪点至少应该偏移多少 ")]
		public Vector3 NoiseShiftMin = Vector3.zero;
		/// how much the noise should be shifted at maximum
		[Tooltip("噪点最多应该偏移多少")]
		public Vector3 NoiseShiftMax = Vector3.zero;


        /// <summary>
        /// 返回时间增量，可以是常规的时间增量，也可以是不受缩放影响的时间增量。 
        /// </summary>
        /// <returns></returns>
        public float GetDeltaTime()
		{
			float deltaTime = UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
			deltaTime *= TimeMultiplier;
			return deltaTime;
		}

        /// <summary>
        /// 返回时间，可以是常规时间，也可以是未缩放的时间。 
        /// </summary>
        /// <returns></returns>
        public float GetTime()
		{
			float time = UseUnscaledTime ? Time.unscaledTime : Time.time;
			time *= TimeMultiplier;
			return time;
		}
	}

    /// <summary>
    /// 一个用于存储内部摆动属性的结构体。 
    /// </summary>
    public struct InternalWiggleProperties
	{
		public Vector3 returnVector;
		public Vector3 newValue;
		public Vector3 initialValue;
		public Vector3 startValue;
		public float timeSinceLastChange ;
		public float randomFrequency;
		public Vector3 randomNoiseFrequency;
		public Vector3 randomAmplitude;
		public Vector3 randomNoiseShift;
		public float timeSinceLastPause;
		public float pauseDuration;
		public float noiseElapsedTime;
		public Vector3 limitedTimeValueSave;
		public Vector3 remapZero;
		public Vector3 remapOne;
		public float curveDirection;
		public bool ping;
	}

    /// <summary>
    /// 将这个类添加到一个游戏对象上，以便能够单独且周期性地控制其位置、旋转或缩放，使其能够 “摆动”（或者只是按照你想要的方式周期性地移动）。 
    /// </summary>
    [AddComponentMenu("More Mountains/Feedbacks/Shakers/Various/MM Wiggle")]
	public class MMWiggle : MonoBehaviour 
	{
        /// 可能的更新模式
        public enum UpdateModes { Update, FixedUpdate, LateUpdate }

		/// the selected update mode
		[Tooltip("所选的更新模式")]
		public UpdateModes UpdateMode = UpdateModes.Update;
		/// whether or not position wiggle is active
		[Tooltip("位置摆动是否处于激活状态")]
		public bool PositionActive = false;
		/// whether or not rotation wiggle is active
		[Tooltip("旋转摆动是否处于激活状态")]
		public bool RotationActive = false;
		/// whether or not scale wiggle is active
		[Tooltip("缩放摆动是否处于激活状态")]
		public bool ScaleActive = false;
		/// all public info related to position wiggling
		[Tooltip("所有与位置摆动相关的公开信息")]
		public WiggleProperties PositionWiggleProperties;
		/// all public info related to rotation wiggling
		[Tooltip("所有与旋转摆动相关的公开信息")]
		public WiggleProperties RotationWiggleProperties;
		/// all public info related to scale wiggling
		[Tooltip("所有与缩放摆动相关的公开信息")]
		public WiggleProperties ScaleWiggleProperties;
		/// a debug duration used in conjunction with the debug buttons
		[Tooltip("一个与调试按钮结合使用的调试时长。 ")]
		public float DebugWiggleDuration = 2f;

		protected InternalWiggleProperties _positionInternalProperties;
		protected InternalWiggleProperties _rotationInternalProperties;
		protected InternalWiggleProperties _scaleInternalProperties;

		public virtual void WigglePosition(float duration)
		{
			WiggleValue(ref PositionWiggleProperties, ref _positionInternalProperties, duration);
		}

		public virtual void WiggleRotation(float duration)
		{
			WiggleValue(ref RotationWiggleProperties, ref _rotationInternalProperties, duration);
		}

		public virtual void WiggleScale(float duration)
		{
			WiggleValue(ref ScaleWiggleProperties, ref _scaleInternalProperties, duration);
		}

		protected virtual void WiggleValue(ref WiggleProperties property, ref InternalWiggleProperties internalProperties, float duration)
		{
			InitializeRandomValues(ref property, ref internalProperties);
			internalProperties.limitedTimeValueSave = internalProperties.initialValue;
			property.LimitedTime = true;
			property.LimitedTimeLeft = duration;
			property.LimitedTimeTotal = duration;
			property.WigglePermitted = true;
		}

        /// <summary>
        /// 在 `Start()` 方法中，我们触发初始化操作。 
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 在初始化时，我们获取起始值，并为每个属性触发我们的协程。 
        /// </summary>
        public virtual void Initialization()
		{
			_positionInternalProperties.initialValue = transform.localPosition;
			_positionInternalProperties.startValue = this.transform.localPosition;

			_rotationInternalProperties.initialValue = transform.localEulerAngles;
			_rotationInternalProperties.startValue = this.transform.localEulerAngles;

			_scaleInternalProperties.initialValue = transform.localScale;
			_scaleInternalProperties.startValue = this.transform.localScale;

			InitializeRandomValues(ref PositionWiggleProperties, ref _positionInternalProperties);
			InitializeRandomValues(ref RotationWiggleProperties, ref _rotationInternalProperties);
			InitializeRandomValues(ref ScaleWiggleProperties, ref _scaleInternalProperties);
		}

        /// <summary>
        /// 初始化指定摆动值的内部属性。
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="internalProperties"></param>
        protected virtual void InitializeRandomValues(ref WiggleProperties properties, ref InternalWiggleProperties internalProperties)
		{
			internalProperties.newValue = internalProperties.initialValue;
			internalProperties.timeSinceLastChange = 0;
			internalProperties.returnVector = Vector3.zero;
			internalProperties.randomFrequency = UnityEngine.Random.Range(properties.FrequencyMin, properties.FrequencyMax);
			internalProperties.randomNoiseFrequency = Vector3.zero;
			internalProperties.randomAmplitude = Vector3.zero;
			internalProperties.timeSinceLastPause = 0;
			internalProperties.pauseDuration = 0;
			internalProperties.noiseElapsedTime = 0;
			internalProperties.curveDirection = 1f;
			properties.LimitedTimeLeft = properties.LimitedTimeTotal;

			RandomizeVector3(ref internalProperties.randomAmplitude, properties.AmplitudeMin, properties.AmplitudeMax);
			RandomizeVector3(ref internalProperties.randomNoiseFrequency, properties.NoiseFrequencyMin, properties.NoiseFrequencyMax);
			RandomizeVector3(ref internalProperties.randomNoiseShift, properties.NoiseShiftMin, properties.NoiseShiftMax);
			RandomizeVector3(ref internalProperties.remapZero, properties.RemapCurveZeroMin, properties.RemapCurveZeroMax);
			RandomizeVector3(ref internalProperties.remapOne, properties.RemapCurveOneMin, properties.RemapCurveOneMax);

			if (properties.ForceVectorLength)
			{
				internalProperties.randomAmplitude = internalProperties.randomAmplitude.normalized * properties.ForcedVectorLength; 
			}

			internalProperties.newValue = DetermineNewValue(properties, internalProperties.newValue, internalProperties.initialValue, ref internalProperties.startValue, 
				ref internalProperties.randomAmplitude, ref internalProperties.randomFrequency, ref internalProperties.pauseDuration, true);
		}

        /// <summary>
        /// 每一帧我们都会更新对象的位置、旋转角度和缩放比例。 
        /// </summary>
        protected virtual void Update()
		{
			if (UpdateMode == UpdateModes.Update)
			{
				ProcessUpdate();
			}
		}

        /// <summary>
        /// 每一帧我们都会更新对象的位置、旋转角度和缩放比例。 
        /// </summary>
        protected virtual void LateUpdate()
		{
			if (UpdateMode == UpdateModes.LateUpdate)
			{
				ProcessUpdate();
			}
		}

        /// <summary>
        /// 每一帧我们都会更新对象的位置、旋转角度和缩放比例。 
        /// </summary>
        protected virtual void FixedUpdate()
		{
			if (UpdateMode == UpdateModes.FixedUpdate)
			{
				ProcessUpdate();
			}
		}

        /// <summary>
        /// 旨在以所选的更新模式执行。 
        /// </summary>
        protected virtual void ProcessUpdate()
		{
			_positionInternalProperties.returnVector = transform.localPosition;
			if (UpdateValue(PositionActive, PositionWiggleProperties, ref _positionInternalProperties))
			{
				transform.localPosition = _positionInternalProperties.returnVector;
			}

			_rotationInternalProperties.returnVector = transform.localEulerAngles;
			if (UpdateValue(RotationActive, RotationWiggleProperties, ref _rotationInternalProperties))
			{
				transform.localEulerAngles = _rotationInternalProperties.returnVector;
			}

			_scaleInternalProperties.returnVector = transform.localScale;
			if (UpdateValue(ScaleActive, ScaleWiggleProperties, ref _scaleInternalProperties))
			{
				transform.localScale = _scaleInternalProperties.returnVector;
			}
		}

        /// <summary>
        /// 计算指定属性的下一个 Vector3 值。 
        /// </summary>
        /// <param name="valueActive"></param>
        /// <param name="properties"></param>
        /// <param name="internalProperties"></param>
        /// <returns></returns>
        protected virtual bool UpdateValue(bool valueActive, WiggleProperties properties, ref InternalWiggleProperties internalProperties)
		{
			if (!valueActive) { return false; }
			if (!properties.WigglePermitted) { return false;  }

			// handle limited time
			if ((properties.LimitedTime) && (properties.LimitedTimeTotal > 0f))
			{
				float timeSave = properties.LimitedTimeLeft;
				properties.LimitedTimeLeft -= properties.GetDeltaTime();
				if (properties.LimitedTimeLeft <= 0)
				{
					if (timeSave > 0f)
					{
						if (properties.LimitedTimeResetValue)
						{
							internalProperties.returnVector = internalProperties.limitedTimeValueSave;
							properties.LimitedTimeLeft = 0;
							properties.WigglePermitted = false;
							return true;
						}
					}                    
					return false;
				}
			}

			switch (properties.WiggleType)
			{
				case WiggleTypes.PingPong:
					return MoveVector3TowardsTarget(ref internalProperties.returnVector, properties, ref internalProperties.startValue, internalProperties.initialValue, 
						ref internalProperties.newValue, ref internalProperties.timeSinceLastPause, 
						ref internalProperties.timeSinceLastChange, ref internalProperties.randomAmplitude, 
						ref internalProperties.randomFrequency, 
						ref internalProperties.pauseDuration, internalProperties.randomFrequency);
                    

				case WiggleTypes.Random:
					return MoveVector3TowardsTarget(ref internalProperties.returnVector, properties, ref internalProperties.startValue, internalProperties.initialValue, 
						ref internalProperties.newValue, ref internalProperties.timeSinceLastPause, 
						ref internalProperties.timeSinceLastChange, ref internalProperties.randomAmplitude, 
						ref internalProperties.randomFrequency, 
						ref internalProperties.pauseDuration, internalProperties.randomFrequency);

				case WiggleTypes.Noise:
					internalProperties.returnVector = AnimateNoiseValue(ref internalProperties, properties);                    
					return true;

				case WiggleTypes.Curve:
					internalProperties.returnVector = AnimateCurveValue(ref internalProperties, properties);
					return true;
			}
			return false;
		}

        /// <summary>
        /// 根据已花费的时间以及一个衰减动画曲线，对计算出的值应用衰减效果。 
        /// </summary>
        /// <param name="newValue"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected float ApplyFalloff(WiggleProperties properties)
		{
			float newValue = 1f;
			if ((properties.LimitedTime) && (properties.LimitedTimeTotal > 0f))
			{
				float curveProgress = (properties.LimitedTimeTotal - properties.LimitedTimeLeft) / properties.LimitedTimeTotal;
				newValue = properties.LimitedTimeFalloff.Evaluate(curveProgress);
			}
			return newValue;
		}

        /// <summary>
        /// 沿着柏林噪声（Perlin噪声）对一个三维向量（Vector3）值进行动画处理。 
        /// </summary>
        /// <param name="internalProperties"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected virtual Vector3 AnimateNoiseValue(ref InternalWiggleProperties internalProperties, WiggleProperties properties)
		{
			internalProperties.noiseElapsedTime += properties.GetDeltaTime();

			internalProperties.newValue.x = (Mathf.PerlinNoise(internalProperties.randomNoiseFrequency.x * internalProperties.noiseElapsedTime, internalProperties.randomNoiseShift.x) * 2.0f - 1.0f) * internalProperties.randomAmplitude.x;
			internalProperties.newValue.y = (Mathf.PerlinNoise(internalProperties.randomNoiseFrequency.y * internalProperties.noiseElapsedTime, internalProperties.randomNoiseShift.y) * 2.0f - 1.0f) * internalProperties.randomAmplitude.y;
			internalProperties.newValue.z = (Mathf.PerlinNoise(internalProperties.randomNoiseFrequency.z * internalProperties.noiseElapsedTime, internalProperties.randomNoiseShift.z) * 2.0f - 1.0f) * internalProperties.randomAmplitude.z;

			internalProperties.newValue *= ApplyFalloff(properties);
            
			if (properties.RelativeAmplitude)
			{
				internalProperties.newValue += internalProperties.initialValue;
			}

			if (properties.UniformValues)
			{
				internalProperties.newValue.y = internalProperties.newValue.x;
				internalProperties.newValue.z = internalProperties.newValue.x;
			}

			return internalProperties.newValue;
		}

        /// <summary>
        /// 沿着指定的曲线对一个三维向量（Vector3）值进行动画处理。 
        /// </summary>
        /// <param name="internalProperties"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        protected virtual Vector3 AnimateCurveValue(ref InternalWiggleProperties internalProperties, WiggleProperties properties)
		{
			internalProperties.timeSinceLastPause += properties.GetDeltaTime();
			internalProperties.timeSinceLastChange += properties.GetDeltaTime();

            // 处理暂停
            if (internalProperties.timeSinceLastPause < internalProperties.pauseDuration)
			{
				float curveProgress = (internalProperties.curveDirection == 1f) ? 1f : 0f;

				EvaluateCurve(properties.Curve, curveProgress, internalProperties.remapZero, internalProperties.remapOne, ref internalProperties.newValue, properties);
				if (properties.RelativeCurveAmplitude)
				{
					internalProperties.newValue += internalProperties.initialValue;
				}
			}

            // 如果我们刚刚结束暂停状态。 
            if (internalProperties.timeSinceLastPause == internalProperties.timeSinceLastChange)
			{
				internalProperties.timeSinceLastChange = 0f;
			}

            // 如果我们已经到达了终点。
            if (internalProperties.randomFrequency > 0)
			{
				float curveProgress = (internalProperties.timeSinceLastChange) / internalProperties.randomFrequency;
				if (internalProperties.curveDirection < 0f)
				{
					curveProgress = 1 - curveProgress;
				}

				EvaluateCurve(properties.Curve, curveProgress, internalProperties.remapZero, internalProperties.remapOne, ref internalProperties.newValue, properties);
                
				if (internalProperties.timeSinceLastChange > internalProperties.randomFrequency)
				{
					internalProperties.timeSinceLastChange = 0f;
					internalProperties.timeSinceLastPause = 0f;
					if (properties.CurvePingPong)
					{
						internalProperties.curveDirection = -internalProperties.curveDirection;
					}                    

					RandomizeFloat(ref internalProperties.randomFrequency, properties.FrequencyMin, properties.FrequencyMax);
				}
			}
            
			if (properties.RelativeCurveAmplitude)
			{
				internalProperties.newValue = internalProperties.initialValue + internalProperties.newValue;
			}
			
			return internalProperties.newValue;
		}

		protected virtual void EvaluateCurve(AnimationCurve curve, float percent, Vector3 remapMin, Vector3 remapMax, ref Vector3 returnValue, WiggleProperties properties)
		{
			returnValue.x = MMFeedbacksHelpers.Remap(curve.Evaluate(percent), 0f, 1f, remapMin.x, remapMax.x);
			returnValue.y = MMFeedbacksHelpers.Remap(curve.Evaluate(percent), 0f, 1f, remapMin.y, remapMax.y);
			returnValue.z = MMFeedbacksHelpers.Remap(curve.Evaluate(percent), 0f, 1f, remapMin.z, remapMax.z);
			returnValue *= ApplyFalloff(properties);
		}

        /// <summary>
        /// 将一个三维向量（Vector3）的值朝着一个目标移动。 
        /// </summary>
        /// <param name="movedValue"></param>
        /// <param name="properties"></param>
        /// <param name="startValue"></param>
        /// <param name="initialValue"></param>
        /// <param name="destinationValue"></param>
        /// <param name="timeSinceLastPause"></param>
        /// <param name="timeSinceLastValueChange"></param>
        /// <param name="randomAmplitude"></param>
        /// <param name="randomFrequency"></param>
        /// <param name="pauseDuration"></param>
        /// <param name="frequency"></param>
        /// <returns></returns>
        protected virtual bool MoveVector3TowardsTarget(ref Vector3 movedValue, WiggleProperties properties, ref Vector3 startValue, Vector3 initialValue, 
			ref Vector3 destinationValue, ref float timeSinceLastPause, ref float timeSinceLastValueChange, 
			ref Vector3 randomAmplitude, ref float randomFrequency,
			ref float pauseDuration, float frequency)
		{
			timeSinceLastPause += properties.GetDeltaTime();
			timeSinceLastValueChange += properties.GetDeltaTime();

            // 处理暂停
            if (timeSinceLastPause < pauseDuration)
			{
				return false;
			}

            // 如果我们刚刚结束暂停状态。
            if (timeSinceLastPause == timeSinceLastValueChange)
			{
				timeSinceLastValueChange = 0f;
			}

            // 如果我们已经到达了终点。
            if (frequency > 0)
			{
				float curveProgress = (timeSinceLastValueChange) / frequency;

				if (!properties.UseSpeedCurve)
				{
					movedValue = Vector3.Lerp(startValue, destinationValue, curveProgress);
				}
				else
				{
					float curvePercent = properties.SpeedCurve.Evaluate(curveProgress);
					movedValue = Vector3.LerpUnclamped(startValue, destinationValue, curvePercent);
				}

				if (timeSinceLastValueChange > frequency)
				{
					timeSinceLastValueChange = 0f;
					timeSinceLastPause = 0f;
					movedValue = destinationValue;
					destinationValue = DetermineNewValue(properties, movedValue, initialValue, ref startValue, 
						ref randomAmplitude, ref randomFrequency, ref pauseDuration);
				}
			}
			return true;
		}

        /// <summary>
        /// 选取一个新的目标值
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="newValue"></param>
        /// <param name="initialValue"></param>
        /// <param name="startValue"></param>
        /// <param name="randomAmplitude"></param>
        /// <param name="randomFrequency"></param>
        /// <param name="pauseDuration"></param>
        /// <returns></returns>
        protected virtual Vector3 DetermineNewValue(WiggleProperties properties, Vector3 newValue, Vector3 initialValue, ref Vector3 startValue, 
			ref Vector3 randomAmplitude, ref float randomFrequency, ref float pauseDuration, bool firstPlay = false)
		{
			switch (properties.WiggleType)
			{
				case WiggleTypes.PingPong:
					if (properties.RelativeAmplitude)
					{
						if (firstPlay)
						{
							startValue = properties.AmplitudeMin * ApplyFalloff(properties) + initialValue;
							newValue = properties.AmplitudeMax * ApplyFalloff(properties) + initialValue;
						}
						else
						{
							if (newValue == properties.AmplitudeMin + initialValue)
							{
								startValue = newValue;
								newValue = properties.AmplitudeMax * ApplyFalloff(properties) + initialValue;
							}
							else
							{
								startValue = newValue;
								newValue = properties.AmplitudeMin  * ApplyFalloff(properties) + initialValue;
							}
						}
					}
					else
					{
						if (firstPlay)
						{
							startValue = properties.AmplitudeMin * ApplyFalloff(properties);
							newValue = properties.AmplitudeMax * ApplyFalloff(properties);
						}
						else
						{
							startValue = newValue;
							newValue = (newValue == properties.AmplitudeMin) ? properties.AmplitudeMax * ApplyFalloff(properties) : properties.AmplitudeMin;	
						}
					}                    
					RandomizeFloat(ref randomFrequency, properties.FrequencyMin, properties.FrequencyMax);
					RandomizeFloat(ref pauseDuration, properties.PauseMin, properties.PauseMax);

					if (properties.UniformValues)
					{
						newValue.y = newValue.x;
						newValue.z = newValue.x;
					}
					
					return newValue;

				case WiggleTypes.Random:
					startValue = newValue;
					RandomizeFloat(ref randomFrequency, properties.FrequencyMin, properties.FrequencyMax);
					RandomizeVector3(ref randomAmplitude, properties.AmplitudeMin, properties.AmplitudeMax);
					RandomizeFloat(ref pauseDuration, properties.PauseMin, properties.PauseMax);
					newValue = randomAmplitude;
                    
					if (properties.UniformValues)
					{
						newValue.y = newValue.x;
						newValue.z = newValue.x;
					}
                    
					newValue *= ApplyFalloff(properties);
					if (properties.RelativeAmplitude)
					{
						newValue += initialValue;
					}
                    
					return newValue;
			}
			return Vector3.zero;            
		}

        /// <summary>
        /// 在边界范围内随机生成一个浮点数 
        /// </summary>
        /// <param name="randomizedFloat"></param>
        /// <param name="floatMin"></param>
        /// <param name="floatMax"></param>
        /// <returns></returns>
        protected virtual float RandomizeFloat(ref float randomizedFloat, float floatMin, float floatMax)
		{
			randomizedFloat = UnityEngine.Random.Range(floatMin, floatMax);
			return randomizedFloat;
		}

        /// <summary>
        /// 在边界范围内随机生成一个三维向量（Vector3）。 
        /// </summary>
        /// <param name="randomizedVector"></param>
        /// <param name="vectorMin"></param>
        /// <param name="vectorMax"></param>
        /// <returns></returns>
        protected virtual Vector3 RandomizeVector3(ref Vector3 randomizedVector, Vector3 vectorMin, Vector3 vectorMax)
		{
			randomizedVector.x = UnityEngine.Random.Range(vectorMin.x, vectorMax.x);
			randomizedVector.y = UnityEngine.Random.Range(vectorMin.y, vectorMax.y);
			randomizedVector.z = UnityEngine.Random.Range(vectorMin.z, vectorMax.z);
			return randomizedVector;
		}
		
		public virtual void RestoreInitialValues()
		{
			transform.localPosition = _positionInternalProperties.initialValue;
			transform.localEulerAngles = _rotationInternalProperties.initialValue;
			transform.localScale = _scaleInternalProperties.initialValue;
		}
	}
}
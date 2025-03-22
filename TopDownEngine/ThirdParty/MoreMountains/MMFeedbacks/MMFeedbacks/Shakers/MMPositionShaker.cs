using System;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 这个抖动器能让你移动一个变换组件的位置，既可以是移动一次，也可以是持续移动，使其在指定的持续时间内，在指定的范围内抖动位置。 
    /// 你可以沿着某个方向应用那种抖动效果，无论是否随机，还可以选择添加噪点以及设置衰减效果。 
    /// </summary>
    public class MMPositionShaker : MMShaker
	{
		public enum Modes { Transform, RectTransform }

		[MMInspectorGroup("Target", true, 41)]
		/// whether this shaker should target Transforms or RectTransforms
		[Tooltip("这个抖动器是应该以变换组件（Transforms）还是矩形变换组件（RectTransforms）为目标。 ")]
		public Modes Mode = Modes.Transform;
		/// the transform to shake the position of. If left blank, this component will target the transform it's put on.
		[Tooltip("要抖动其位置的变换组件。如果留空，此组件将以它所附加的变换组件为目标。 ")]
		[MMEnumCondition("Mode", (int)Modes.Transform)]
		public Transform TargetTransform;
		/// the rect transform to shake the position of. If left blank, this component will target the transform it's put on.
		[Tooltip("要抖动其位置的矩形变换组件。如果留空，此组件将以它所附加的变换组件为目标。 ")]
		[MMEnumCondition("Mode", (int)Modes.RectTransform)]
		public RectTransform TargetRectTransform;

		[MMInspectorGroup("Shake Settings", true, 42)]
		/// the speed at which the transform should shake
		[Tooltip("该变换组件进行抖动时的速度。 ")]
		public float ShakeSpeed = 20f;
		/// the maximum distance from its initial position the transform will move to during the shake
		[Tooltip("在抖动过程中，变换组件从其初始位置移动到的最远距离。 ")]
		public float ShakeRange = 0.5f;
		/// an offset to apply to the oscillation
		[Tooltip("一个用于应用到振荡上的偏移量。 ")]
		public float OscillationOffset = 0f;
        
		[MMInspectorGroup("Direction", true, 43)]
		/// the direction along which to shake the transform's position
		[Tooltip("用于使变换组件的位置产生抖动的方向。 ")]
		public Vector3 ShakeMainDirection = Vector3.up;
		/// if this is true, instead of using ShakeMainDirection as the direction of the shake, a random vector3 will be generated, randomized between ShakeMainDirection and ShakeAltDirection
		[Tooltip("如果这为真，那么将不会使用“抖动主方向（ShakeMainDirection）”作为抖动方向，而是会生成一个随机的三维向量，该向量在“抖动主方向（ShakeMainDirection）”和“抖动备用方向（ShakeAltDirection）”之间随机取值。  ")]
		public bool RandomizeDirection = false;
		/// when in RandomizeDirection mode, a vector against which to randomize the main direction
		[Tooltip("当处于“随机化方向”模式时，用于对主方向进行随机化处理的一个向量。 ")]
		[MMCondition("RandomizeDirection", true)]
		public Vector3 ShakeAltDirection = Vector3.up;
		/// if this is true, a new direction will be randomized every time a shake happens
		[Tooltip("如果这为真，那么每次发生抖动时都会随机生成一个新的方向。 ")]
		public bool RandomizeDirectionOnPlay = false;

		/// whether or not to randomize the x value of the main direction
		[Tooltip("是否对主方向的 x 值进行随机化处理。 ")]
		public bool RandomizeDirectionX = true;
		/// whether or not to randomize the y value of the main direction
		[Tooltip("是否对主方向的 y 值进行随机化处理。 ")]
		public bool RandomizeDirectionY = true;
		/// whether or not to randomize the z value of the main direction
		[Tooltip("是否对主方向的 z 值进行随机化处理。 ")]
		public bool RandomizeDirectionZ= true;

		[MMInspectorGroup("Directional Noise", true, 47)]
		/// whether or not to add noise to the main direction
		[Tooltip("是否向主方向添加噪点。")]
		public bool AddDirectionalNoise = true;
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMax
		[Tooltip("当添加方向噪点时，噪点强度将在此值与“方向噪点强度最大值（DirectionalNoiseStrengthMax）”之间随机取值。 ")]
		[MMCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMin = new Vector3(0.25f, 0.25f, 0.25f);
		/// when adding directional noise, noise strength will be randomized between this value and DirectionalNoiseStrengthMin
		[Tooltip("当添加方向噪点时，噪点强度将在此值与“方向噪点强度最小值（DirectionalNoiseStrengthMin）”之间随机取值。 ")]
		[MMCondition("AddDirectionalNoise", true)]
		public Vector3 DirectionalNoiseStrengthMax = new Vector3(0.25f, 0.25f, 0.25f);
        
		[MMInspectorGroup("Randomness", true, 44)]
		/// a unique seed you can use to get different outcomes when shaking more than one transform at once
		[Tooltip("一个唯一的种子值，当同时抖动多个变换组件时，你可以使用它来获得不同的结果。 ")]
		public Vector3 RandomnessSeed;
		/// whether or not to generate a unique seed automatically on every shake
		[Tooltip("是否在每次抖动时自动生成一个唯一的种子值。 ")]
		public bool RandomizeSeedOnShake = true;

		[MMInspectorGroup("One Time", true, 45)]
		/// whether or not to use attenuation, which will impact the amplitude of the shake, along the defined curve
		[Tooltip("是否使用衰减效果，该衰减效果将沿着定义的曲线对抖动的幅度产生影响。 ")]
		public bool UseAttenuation = true;
		/// the animation curve used to define attenuation, impacting the amplitude of the shake
		[Tooltip("用于定义衰减的动画曲线，它会对抖动的幅度产生影响。 ")]
		[MMCondition("UseAttenuation", true)]
		public AnimationCurve AttenuationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));

		[MMInspectorGroup("Test", true, 46)]
		[MMInspectorButton("StartShaking")] 
		public bool StartShakingButton;

		public virtual float Randomness => RandomnessSeed.x + RandomnessSeed.y + RandomnessSeed.z;

		protected float _attenuation = 1f;
		protected float _oscillation;
		protected Vector3 _initialPosition;
		protected Vector3 _workDirection;
		protected Vector3 _noiseVector;
		protected Vector3 _newPosition;
		protected Vector3 _randomNoiseStrength;
		protected Vector3 _noNoise = Vector3.zero;
		protected Vector3 _randomizedDirection;

        /// <summary>
        /// 在初始化时，我们初始化我们的值
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			if (TargetTransform == null) { TargetTransform = this.transform; }
			if (TargetRectTransform == null) { TargetRectTransform = this.GetComponent<RectTransform>(); }

			GrabInitialPosition();
		}

		public virtual void GrabInitialPosition()
		{
			switch (Mode)
			{
				case Modes.Transform:
					_initialPosition = TargetTransform.localPosition;
					break;
				case Modes.RectTransform:
					_initialPosition = TargetRectTransform.anchoredPosition;
					break;
			}
		}

        /// <summary>
        /// 当那个抖动器被添加时，我们会初始化它的抖动持续时间。 
        /// </summary>
        protected virtual void Reset()
		{
			ShakeDuration = 0.5f;
		}

		protected override void ShakeStarts()
		{
			GrabInitialPosition();
	        
			if (RandomizeSeedOnShake)
			{
				RandomnessSeed = Random.insideUnitSphere;
			}

			if (RandomizeDirectionOnPlay)
			{
				ShakeMainDirection.x = RandomizeDirectionX ? Random.insideUnitSphere.x : ShakeMainDirection.x;
				ShakeMainDirection.y = RandomizeDirectionY ? Random.insideUnitSphere.y : ShakeMainDirection.y;
				ShakeMainDirection.z = RandomizeDirectionZ ? Random.insideUnitSphere.z : ShakeMainDirection.z;
				ShakeAltDirection.x = RandomizeDirectionX ? Random.insideUnitSphere.x : ShakeAltDirection.x;
				ShakeAltDirection.y = RandomizeDirectionY ? Random.insideUnitSphere.y : ShakeAltDirection.y;
				ShakeAltDirection.z = RandomizeDirectionZ ? Random.insideUnitSphere.z : ShakeAltDirection.z;
			}
           
			_randomizedDirection = RandomizeDirection ? MMMaths.RandomVector3(ShakeMainDirection, ShakeAltDirection) : ShakeMainDirection;
			if (!RandomizeDirectionX) { _randomizedDirection.x = ShakeMainDirection.x; }
			if (!RandomizeDirectionY) { _randomizedDirection.y = ShakeMainDirection.y; }
			if (!RandomizeDirectionZ) { _randomizedDirection.z = ShakeMainDirection.z; }
		}
        
		protected override void Shake()
		{
			_oscillation = OscillationOffset + Mathf.Sin(ShakeSpeed * (Randomness + _journey));
			float remappedTime = MMFeedbacksHelpers.Remap(_journey, 0f, ShakeDuration, 0f, 1f);
           
			_attenuation = ComputeAttenuation(remappedTime);
			_workDirection = ShakeMainDirection + ComputeNoise(_journey);
			_workDirection.Normalize();
			_newPosition = ComputeNewPosition();
			ApplyNewPosition(_newPosition);
		}
        
		protected override void ShakeComplete()
		{
			base.ShakeComplete();
			_attenuation = 0f;
			_newPosition = ComputeNewPosition();
			if (TargetTransform != null)
			{
				ApplyNewPosition(_newPosition);	
			}
		}

		protected virtual void ApplyNewPosition(Vector3 newPosition)
		{
			switch (Mode)
			{
				case Modes.Transform:
					TargetTransform.localPosition = newPosition;
					break;
				case Modes.RectTransform:
					TargetRectTransform.anchoredPosition = newPosition;
					break;
			}
		}

		protected virtual Vector3 ComputeNewPosition()
		{
			return _initialPosition + _workDirection * _oscillation * ShakeRange * _attenuation;
		}

		protected virtual float ComputeAttenuation(float remappedTime)
		{
			return (UseAttenuation && !PermanentShake) ? AttenuationCurve.Evaluate(remappedTime) : 1f;
		}
        
		protected virtual Vector3 ComputeNoise(float time)
		{
			if (!AddDirectionalNoise)
			{
				return _noNoise;
			}

			_randomNoiseStrength = MMMaths.RandomVector3(DirectionalNoiseStrengthMin, DirectionalNoiseStrengthMax); 
	        
			_noiseVector.x = _randomNoiseStrength.x * (Mathf.PerlinNoise(RandomnessSeed.x, time) - 0.5f) ;
			_noiseVector.y = _randomNoiseStrength.y * (Mathf.PerlinNoise(RandomnessSeed.y, time) - 0.5f);
			_noiseVector.z = _randomNoiseStrength.z * (Mathf.PerlinNoise(RandomnessSeed.z, time) - 0.5f);
	        
			return _noiseVector;
		}
        
		protected float _originalDuration;
		protected float _originalShakeSpeed;
		protected float _originalShakeRange;
		protected Vector3 _originalShakeMainDirection;
		protected bool _originalRandomizeDirection;
		protected Vector3 _originalShakeAltDirection;
		protected bool _originalRandomizeDirectionOnPlay;
		protected bool _originalRandomizeDirectionX;
		protected bool _originalRandomizeDirectionY;
		protected bool _originalRandomizeDirectionZ;
		protected bool _originalAddDirectionalNoise;
		protected Vector3 _originalDirectionalNoiseStrengthMin;
		protected Vector3 _originalDirectionalNoiseStrengthMax;
		protected Vector3 _originalRandomnessSeed;
		protected bool _originalRandomizeSeedOnShake;
		protected bool _originalUseAttenuation;
		protected AnimationCurve _originalAttenuationCurve;

		public virtual void OnMMPositionShakeEvent(float duration, float shakeSpeed, float shakeRange, Vector3 shakeMainDirection, bool randomizeDirection, Vector3 shakeAltDirection, bool randomizeDirectionOnPlay, 
			bool randomizeDirectionX, bool randomizeDirectionY, bool randomizeDirectionZ, bool addDirectionalNoise, 
			Vector3 directionalNoiseStrengthMin, Vector3 directionalNoiseStrengthMax, Vector3 randomnessSeed, bool randomizeSeedOnShake, bool useAttenuation, AnimationCurve attenuationCurve,
			bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			if (!CheckEventAllowed(channelData, useRange, rangeDistance, rangePosition) || (!Interruptible && Shaking))
			{
				return;
			}
            
			if (stop)
			{
				Stop();
				return;
			}
            
			if (restore)
			{
				ResetTargetValues();
				return;
			}
            
			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalDuration = ShakeDuration;
				_originalShakeSpeed = ShakeSpeed;
				_originalShakeRange = ShakeRange;
				_originalShakeMainDirection = ShakeMainDirection;
				_originalRandomizeDirection = RandomizeDirection;
				_originalShakeAltDirection = ShakeAltDirection;
				_originalRandomizeDirectionOnPlay = RandomizeDirectionOnPlay;
				_originalRandomizeDirectionX = RandomizeDirectionX;
				_originalRandomizeDirectionY = RandomizeDirectionY;
				_originalRandomizeDirectionZ = RandomizeDirectionZ;
				_originalAddDirectionalNoise = AddDirectionalNoise;
				_originalDirectionalNoiseStrengthMin = DirectionalNoiseStrengthMin;
				_originalDirectionalNoiseStrengthMax = DirectionalNoiseStrengthMax;
				_originalRandomnessSeed = RandomnessSeed;
				_originalRandomizeSeedOnShake = RandomizeSeedOnShake;
				_originalUseAttenuation = UseAttenuation;
				_originalAttenuationCurve = AttenuationCurve;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				ShakeSpeed = shakeSpeed;
				ShakeRange = shakeRange * feedbacksIntensity * ComputeRangeIntensity(useRange, rangeDistance, useRangeFalloff, rangeFalloff, remapRangeFalloff, rangePosition);
				ShakeMainDirection = shakeMainDirection;
				RandomizeDirection = randomizeDirection;
				ShakeAltDirection = shakeAltDirection;
				RandomizeDirectionOnPlay = randomizeDirectionOnPlay;
				RandomizeDirectionX = randomizeDirectionX;
				RandomizeDirectionY = randomizeDirectionY;
				RandomizeDirectionZ = randomizeDirectionZ;
				AddDirectionalNoise = addDirectionalNoise;
				DirectionalNoiseStrengthMin = directionalNoiseStrengthMin;
				DirectionalNoiseStrengthMax = directionalNoiseStrengthMax;
				RandomnessSeed = randomnessSeed;
				RandomizeSeedOnShake = randomizeSeedOnShake;
				UseAttenuation = useAttenuation;
				AttenuationCurve = attenuationCurve;
				ForwardDirection = forwardDirection;	
			}

			Play();
		}

        /// <summary>
        /// 重置目标的数值。
        /// </summary>
        protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			switch (Mode)
			{
				case Modes.Transform:
					TargetTransform.localPosition = _initialPosition;
					break;
				case Modes.RectTransform:
					TargetRectTransform.anchoredPosition = _initialPosition;
					break;
			}
		}

        /// <summary>
        /// 重置抖动器的数值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalDuration;
			ShakeSpeed = _originalShakeSpeed;
			ShakeRange = _originalShakeRange;
			ShakeMainDirection = _originalShakeMainDirection;
			RandomizeDirection = _originalRandomizeDirection;
			ShakeAltDirection = _originalShakeAltDirection;
			RandomizeDirectionOnPlay = _originalRandomizeDirectionOnPlay;
			RandomizeDirectionX = _originalRandomizeDirectionX;
			RandomizeDirectionY = _originalRandomizeDirectionY;
			RandomizeDirectionZ = _originalRandomizeDirectionZ;
			AddDirectionalNoise = _originalAddDirectionalNoise;
			DirectionalNoiseStrengthMin = _originalDirectionalNoiseStrengthMin;
			DirectionalNoiseStrengthMax = _originalDirectionalNoiseStrengthMax;
			RandomnessSeed = _originalRandomnessSeed;
			RandomizeSeedOnShake = _originalRandomizeSeedOnShake;
			UseAttenuation = _originalUseAttenuation;
			AttenuationCurve = _originalAttenuationCurve;
		}

        /// <summary>
        /// 开始监听事件。
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMPositionShakeEvent.Register(OnMMPositionShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMPositionShakeEvent.Unregister(OnMMPositionShakeEvent);
		}
	}
	
	public struct MMPositionShakeEvent
	{
		static private event Delegate OnEvent;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
		static public void Register(Delegate callback) { OnEvent += callback; }
		static public void Unregister(Delegate callback) { OnEvent -= callback; }

		public delegate void Delegate(float duration, float shakeSpeed, float shakeRange, Vector3 shakeMainDirection, bool randomizeDirection, Vector3 shakeAltDirection, bool randomizeDirectionOnPlay, 
			bool randomizeDirectionX, bool randomizeDirectionY, bool randomizeDirectionZ, bool addDirectionalNoise, 
			Vector3 directionalNoiseStrengthMin, Vector3 directionalNoiseStrengthMax, Vector3 randomnessSeed, bool randomizeSeedOnShake, bool useAttenuation, AnimationCurve attenuationCurve,
			bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false);

		static public void Trigger(float duration, float shakeSpeed, float shakeRange, Vector3 shakeMainDirection, bool randomizeDirection, Vector3 shakeAltDirection, bool randomizeDirectionOnPlay, 
			bool randomizeDirectionX, bool randomizeDirectionY, bool randomizeDirectionZ, bool addDirectionalNoise, 
			Vector3 directionalNoiseStrengthMin, Vector3 directionalNoiseStrengthMax, Vector3 randomnessSeed, bool randomizeSeedOnShake, bool useAttenuation, AnimationCurve attenuationCurve,
			bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false, bool restore = false)
		{
			OnEvent?.Invoke( duration, shakeSpeed,  shakeRange,  shakeMainDirection,  randomizeDirection,  shakeAltDirection,  randomizeDirectionOnPlay,  
				randomizeDirectionX, randomizeDirectionY, randomizeDirectionZ, addDirectionalNoise, 
				directionalNoiseStrengthMin,  directionalNoiseStrengthMax,  randomnessSeed,  randomizeSeedOnShake,  useAttenuation,  attenuationCurve,
				useRange, rangeDistance, useRangeFalloff, rangeFalloff, remapRangeFalloff, rangePosition,
				feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop, restore);
		}
	}
}
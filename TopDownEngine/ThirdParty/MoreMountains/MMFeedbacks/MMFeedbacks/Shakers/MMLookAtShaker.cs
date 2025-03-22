using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 将这个添加到一个对象上，它就能监听MMFLookAtShakeEvents（可能是某种特定的“看向抖动事件”），并且当接收到这样一个事件时，它会相应地旋转其关联的变换组件。  
    /// </summary>
    public class MMLookAtShaker : MMShaker
	{
		[MMInspectorGroup("Look at settings", true, 37)]
		/// the duration of this shake, in seconds
		[Tooltip("这次抖动的持续时间，以秒为单位。 ")]
		public float Duration = 1f;
		/// the curve over which to animate the look at transition
		[Tooltip("用于对“看向”过渡效果进行动画处理的曲线。 ")]
		public MMTweenType LookAtTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// whether or not to lock rotation on the x axis
		[Tooltip("是否锁定X轴上的旋转。")]
		public bool LockXAxis = false;
		/// whether or not to lock rotation on the y axis
		[Tooltip("是否锁定Y轴上的旋转。")]
		public bool LockYAxis = false;
		/// whether or not to lock rotation on the z axis
		[Tooltip("是否锁定Z轴上的旋转。")]
		public bool LockZAxis = false;

		[MMInspectorGroup("What we want to rotate", true, 37)]
		/// in Direct mode, the transform to rotate to have it look at our target - if left empty, will be the transform this shaker is on
		[Tooltip("在直接模式下，用于旋转以使其看向我们目标的变换——如果留空，将是这个抖动器所在的变换。 ")]
		public Transform TransformToRotate;
        /// 代表我们想要旋转并看向目标的物体上的向上方向的矢量。 
        public MMF_LookAt.UpwardVectors UpwardVector = MMF_LookAt.UpwardVectors.Up;

		[MMInspectorGroup("What we want to look at", true, 37)]
		/// the different target modes : either a specific transform to look at, the coordinates of a world position, or a direction vector
		[Tooltip("不同的目标模式：可以是要看向的特定变换，也可以是世界位置的坐标，或者是一个方向矢量。 ")]
		public MMF_LookAt.LookAtTargetModes LookAtTargetMode = MMF_LookAt.LookAtTargetModes.Transform;
		/// the transform we want to look at 
		[Tooltip("我们想要看向的那个变换组件")]
		[MMFEnumCondition("LookAtTargetMode", (int)MMF_LookAt.LookAtTargetModes.Transform)]
		public Transform LookAtTarget;
		/// the coordinates of a point the world that we want to look at
		[Tooltip("我们想要看向的世界中的一个点的坐标。 ")]
		[MMFEnumCondition("LookAtTargetMode", (int)MMF_LookAt.LookAtTargetModes.TargetWorldPosition)]
		public Vector3 LookAtTargetWorldPosition = Vector3.forward;
		/// a direction (from our rotating object) that we want to look at
		[Tooltip("（从我们正在旋转的物体出发的）我们想要看向的一个方向。 ")]
		[MMFEnumCondition("LookAtTargetMode", (int)MMF_LookAt.LookAtTargetModes.Direction)]
		public Vector3 LookAtDirection = Vector3.forward;
		
		[MMInspectorGroup("Test", true, 46)]
		[MMInspectorButton("StartShaking")] 
		public bool StartShakingButton;

        /// <summary>
        /// 一种用于触发“看向抖动”效果的事件。 
        /// </summary>
        public struct MMLookAtShakeEvent
		{
			static private event Delegate OnEvent;
			[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void RuntimeInitialization() { OnEvent = null; }
			static public void Register(Delegate callback) { OnEvent += callback; }
			static public void Unregister(Delegate callback) { OnEvent -= callback; }

			public delegate void Delegate(float duration, 
				bool lockXAxis, bool lockYAxis, bool lockZAxis, MMF_LookAt.UpwardVectors upwardVector, MMF_LookAt.LookAtTargetModes lookAtTargetMode,Transform lookAtTarget, Vector3 lookAtTargetWorldPosition, Vector3 lookAtDirection, Transform transformToRotate, MMTweenType lookAtTween,
				bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
				float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
				bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false);

			static public void Trigger(float duration, 
				bool lockXAxis, bool lockYAxis, bool lockZAxis, MMF_LookAt.UpwardVectors upwardVector, MMF_LookAt.LookAtTargetModes lookAtTargetMode,Transform lookAtTarget, Vector3 lookAtTargetWorldPosition, Vector3 lookAtDirection, Transform transformToRotate, MMTweenType lookAtTween,
				bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
				float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
				bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
			{
				OnEvent?.Invoke( duration, lockXAxis, lockYAxis, lockZAxis, upwardVector, lookAtTargetMode, lookAtTarget, lookAtTargetWorldPosition, lookAtDirection, transformToRotate, lookAtTween,
					useRange, rangeDistance, useRangeFalloff, rangeFalloff, remapRangeFalloff, rangePosition,
					feedbacksIntensity, channelData, resetShakerValuesAfterShake, resetTargetValuesAfterShake, forwardDirection, timescaleMode, stop);
			}
		}
		
		protected Quaternion _newRotation;
		protected Vector3 _lookAtPosition;
		protected Vector3 _upwards;
		protected Vector3 _direction;
		protected Quaternion _initialRotation;
		protected float _originalDuration = 1f;
		protected MMTweenType _originalLookAtTween;
		protected bool _originalLockXAxis;
		protected bool _originalLockYAxis;
		protected bool _originalLockZAxis;
		protected MMF_LookAt.UpwardVectors _originalUpwardVector;
		protected MMF_LookAt.LookAtTargetModes _originalLookAtTargetMode;
		protected Transform _originalLookAtTarget;
		protected Vector3 _originalLookAtTargetWorldPosition;
		protected Vector3 _originalLookAtDirection;

        /// <summary>
        /// 在初始化时，我们存储我们的初始旋转角度。 
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			if (TransformToRotate == null)
			{
				TransformToRotate = this.transform;
			}
			_initialRotation = TransformToRotate.rotation;
		}

        /// <summary>
        /// 当那个抖动器被添加时，我们对它的抖动持续时间进行初始化设置。  
        /// </summary>
        protected virtual void Reset()
		{
			ShakeDuration = 0.5f;
		}

        /// <summary>
        /// 在抖动发生时，我们对目标变换（组件）应用旋转操作。 
        /// </summary>
        protected override void Shake()
		{
			ApplyRotation(_journey);
		}

        /// <summary>
        /// 当抖动完成时，我们应用最终的旋转设置。 
        /// </summary>
        protected override void ShakeComplete()
		{
			ApplyRotation(1f);
			base.ShakeComplete();
		}

        /// <summary>
        /// 旋转相关联的变换组件，使其看向我们的目标。 
        /// </summary>
        /// <param name="journey"></param>
        protected virtual void ApplyRotation(float journey)
		{
			float percent = Mathf.Clamp01(journey / ShakeDuration);
			percent = LookAtTween.Evaluate(percent);
			
			switch (LookAtTargetMode)
			{
				case MMF_LookAt.LookAtTargetModes.Transform:
					_lookAtPosition = LookAtTarget.position;
					break;
				case MMF_LookAt.LookAtTargetModes.TargetWorldPosition:
					_lookAtPosition = LookAtTargetWorldPosition;
					break;
				case MMF_LookAt.LookAtTargetModes.Direction:
					_lookAtPosition = TransformToRotate.position + LookAtDirection;
					break;
			}
			
			if (LockXAxis) { _lookAtPosition.x = TransformToRotate.position.x; }
			if (LockYAxis) { _lookAtPosition.y = TransformToRotate.position.y; }
			if (LockZAxis) { _lookAtPosition.z = TransformToRotate.position.z; }
	            
			_direction = _lookAtPosition - TransformToRotate.position;
			_newRotation = Quaternion.LookRotation(_direction, _upwards);
			
			TransformToRotate.transform.rotation = Quaternion.SlerpUnclamped(_initialRotation, _newRotation, percent);
		}

        /// <summary>
        /// 当接收到一个新的“看向”事件时，我们让我们的变换组件看向指定的目标。 
        /// </summary>
        public virtual void OnMMLookAtShakeEvent(float duration, 
			bool lockXAxis, bool lockYAxis, bool lockZAxis, MMF_LookAt.UpwardVectors upwardVector, MMF_LookAt.LookAtTargetModes lookAtTargetMode,Transform lookAtTarget, Vector3 lookAtTargetWorldPosition, Vector3 lookAtDirection, Transform transformToRotate, MMTweenType lookAtTween,
			bool useRange = false, float rangeDistance = 0f, bool useRangeFalloff = false, AnimationCurve rangeFalloff = null, Vector2 remapRangeFalloff = default(Vector2), Vector3 rangePosition = default(Vector3),
			float feedbacksIntensity = 1.0f, MMChannelData channelData = null, bool resetShakerValuesAfterShake = true, 
			bool resetTargetValuesAfterShake = true, bool forwardDirection = true, TimescaleModes timescaleMode = TimescaleModes.Scaled, bool stop = false)
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
            
			_resetShakerValuesAfterShake = resetShakerValuesAfterShake;
			_resetTargetValuesAfterShake = resetTargetValuesAfterShake;

			if (resetShakerValuesAfterShake)
			{
				_originalDuration = ShakeDuration;
				_originalLookAtTween = LookAtTween;
				_originalLockXAxis = LockXAxis;
				_originalLockYAxis = LockYAxis;
				_originalLockZAxis = LockZAxis;
				_originalUpwardVector = UpwardVector;
				_originalLookAtTargetMode = LookAtTargetMode;
				_originalLookAtTarget = LookAtTarget;
				_originalLookAtTargetWorldPosition = LookAtTargetWorldPosition;
				_originalLookAtDirection = LookAtDirection;
			}

			if (!OnlyUseShakerValues)
			{
				TimescaleMode = timescaleMode;
				ShakeDuration = duration;
				LookAtTween = lookAtTween;
				LockXAxis = lockXAxis;
				LockYAxis = lockYAxis;
				LockZAxis = lockZAxis;
				UpwardVector = upwardVector;
				LookAtTargetMode = lookAtTargetMode;
				LookAtTarget = lookAtTarget;
				LookAtTargetWorldPosition = lookAtTargetWorldPosition;
				LookAtDirection = lookAtDirection;
				ForwardDirection = forwardDirection;
			}

			Play();
		}

        /// <summary>
        /// 在执行“重置目标值（ResetTargetValue）”操作时，我们会重置目标变换组件的旋转角度。 
        /// </summary>
        protected override void ResetTargetValues()
		{
			base.ResetTargetValues();
			TransformToRotate.rotation = _initialRotation;
		}

        /// <summary>
        /// 重置抖动器的值。
        /// </summary>
        protected override void ResetShakerValues()
		{
			base.ResetShakerValues();
			ShakeDuration = _originalDuration;
			LookAtTween = _originalLookAtTween;
			LockXAxis = _originalLockXAxis;
			LockYAxis = _originalLockYAxis;
			LockZAxis = _originalLockZAxis;
			UpwardVector = _originalUpwardVector;
			LookAtTargetMode = _originalLookAtTargetMode;
			LookAtTarget = _originalLookAtTarget;
			LookAtTargetWorldPosition = _originalLookAtTargetWorldPosition;
			LookAtDirection = _originalLookAtDirection;
		}

        /// <summary>
        /// 开始监听事件
        /// </summary>
        public override void StartListening()
		{
			base.StartListening();
			MMLookAtShakeEvent.Register(OnMMLookAtShakeEvent);
		}

        /// <summary>
        /// 停止监听事件
        /// </summary>
        public override void StopListening()
		{
			base.StopListening();
			MMLookAtShakeEvent.Unregister(OnMMLookAtShakeEvent);
		}
	}
}
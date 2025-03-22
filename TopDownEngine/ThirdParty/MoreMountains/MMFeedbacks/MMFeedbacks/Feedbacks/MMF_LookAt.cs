using System;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you animate the rotation of a transform to look at a target over time.
	/// You can also use it to broadcast a MMLookAtShake event, that MMLookAtShakers on the right channel will be able to listen for and act upon 
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将允许您随时间动画化变换组件的旋转以朝向目标。您还可以使用它广播MMLookAtShake事件，正确通道上的MMLookAtShakers将能够监听并作出反应。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/LookAt")]
	public class MMF_LookAt : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup()
		{
			if (Mode == Modes.Direct)
			{
				return TransformToRotate == null;
			}
			else
			{
				return false;
			}
		}
		public override string RequiredTargetText
		{
			get
			{
				if ((Mode == Modes.Direct) && (TransformToRotate != null))
				{
					return TransformToRotate.name;
				}
				else
				{
					return "";
				}
			}
		}
		public override string RequiresSetupText { get { return "在直接模式下，此反馈需要设置DirectTargetTransform才能正常工作。您可以在下方设置一个"; } }
#endif

        /// 此反馈的持续时间是移动的持续时间（以秒为单位）
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } } 
		public override bool HasChannel => true; 
		public override bool HasRange => true;

        /// 此反馈的可能模式，无论是直接针对一个变换组件，还是广播一个事件
        public enum Modes { Direct, Event }
        /// 是查看特定的变换组件、世界中的位置，还是方向向量
        public enum LookAtTargetModes { Transform, TargetWorldPosition, Direction }
        /// 当查看一个方向时，要视为“up”的向量
        public enum UpwardVectors { Forward, Up, Right }
		
		[MMFInspectorGroup("Look at settings", true, 37, true)]
		/// the duration of this feedback, in seconds
		[Tooltip("此反馈的持续时间（以秒为单位）")]
		public float Duration = 1f;
		/// the curve over which to animate the look at transition
		[Tooltip("要动画化查看过渡的曲线")]
		public MMTweenType LookAtTween = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// whether or not to lock rotation on the x axis
		[Tooltip("是否锁定绕x轴的旋转")]
		public bool LockXAxis = false;
		/// whether or not to lock rotation on the y axis
		[Tooltip("是否锁定绕y轴的旋转")]
		public bool LockYAxis = false;
		/// whether or not to lock rotation on the z axis
		[Tooltip("是否锁定绕z轴的旋转")]
		public bool LockZAxis = false;

		[MMFInspectorGroup("What we want to rotate", true, 37, true)]
		/// whether to make a certain transform look at a target, or to broadcast an event
		[Tooltip("是使某个变换组件朝向目标，还是广播事件")]
		public Modes Mode = Modes.Direct;
		/// in Direct mode, the transform to rotate to have it look at our target
		[Tooltip("在直接模式下，要旋转以使其朝向目标的变换组件")]
		[MMFEnumCondition("Mode", (int)Modes.Direct)]
		public Transform TransformToRotate;
		/// the vector representing the up direction on the object we want to rotate and look at our target
		[Tooltip("表示我们要旋转并注视目标的物体的上方向的向量")]
		[MMFEnumCondition("Mode", (int)Modes.Direct)]
		public UpwardVectors UpwardVector = UpwardVectors.Up;
		/// whether or not to reset shaker values after shake
		[Tooltip("是否在震动之后重置震动器的值")]
		[MMFEnumCondition("Mode", (int)Modes.Event)]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("是否在震动之后重置目标的值。")]
		[MMFEnumCondition("Mode", (int)Modes.Event)]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("What we want to look at", true, 37, true)]
		/// the different target modes : either a specific transform to look at, the coordinates of a world position, or a direction vector
		[Tooltip("不同的目标模式：要么是注视一个特定的变换组件，要么是世界坐标系中一个位置的坐标，要么是一个方向向量")]
		public LookAtTargetModes LookAtTargetMode = LookAtTargetModes.Transform;
		/// the transform we want to look at 
		[Tooltip("我们想要注视的变换组件")]
		[MMFEnumCondition("LookAtTargetMode", (int)LookAtTargetModes.Transform)]
		public Transform LookAtTarget;
		/// the coordinates of a point the world that we want to look at
		[Tooltip("我们想要注视的世界坐标系中一个点的坐标")]
		[MMFEnumCondition("LookAtTargetMode", (int)LookAtTargetModes.TargetWorldPosition)]
		public Vector3 LookAtTargetWorldPosition = Vector3.forward;
		/// a direction (from our rotating object) that we want to look at
		[Tooltip("（从我们正在旋转的物体出发的）我们想要注视的方向")]
		[MMFEnumCondition("LookAtTargetMode", (int)LookAtTargetModes.Direction)]
		public Vector3 LookAtDirection = Vector3.forward;
		
		protected Coroutine _coroutine;
		protected Quaternion _initialDirectTargetTransformRotation;
		protected Quaternion _newRotation;
		protected Vector3 _lookAtPosition;
		protected Vector3 _upwards;
		protected Vector3 _direction;
		protected Quaternion _initialRotation;

        /// <summary>
        /// 在初始化（init）时，我们初始化向上的向量
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			
			switch (UpwardVector)
			{
				case UpwardVectors.Forward:
					_upwards = Vector3.forward;
					break;
				case UpwardVectors.Up:
					_upwards = Vector3.up;
					break;
				case UpwardVectors.Right:
					_upwards = Vector3.right;
					break;
			}
		}

        /// <summary>
        /// 在播放（Play）时，我们开始注视我们的目标
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			if (Active || Owner.AutoPlayOnEnable)
			{
				InitiateLookAt(position);
			}
		}

        /// <summary>
        /// 根据我们选择的模式，通过启动一个协程或广播一个事件来开始注视
        /// </summary>
        /// <param name="position"></param>
        protected virtual void InitiateLookAt(Vector3 position)
		{
			_initialRotation = TransformToRotate.transform.rotation;
			
			switch (Mode)
			{
				case Modes.Direct:
					ClearCoroutine();
					_coroutine = Owner.StartCoroutine(AnimateLookAt());
					break;
				case Modes.Event:
					MMLookAtShaker.MMLookAtShakeEvent.Trigger(Duration, LockXAxis, LockYAxis, LockZAxis, UpwardVector,
						LookAtTargetMode, LookAtTarget, LookAtTargetWorldPosition, LookAtDirection, null,
						LookAtTween,
						UseRange, RangeDistance, UseRangeFalloff, RangeFalloff, RemapRangeFalloff, position,
						1f, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection,
						ComputedTimescaleMode);
					break;
			}
		}

        /// <summary>
        /// 随时间动画化注视方向
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator AnimateLookAt()
		{
			if (TransformToRotate != null)
			{
				_initialDirectTargetTransformRotation = TransformToRotate.transform.rotation;
			}

			float duration = FeedbackDuration;
			float journey = NormalPlayDirection ? 0f : duration;

			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);
				percent = LookAtTween.Evaluate(percent);
				ApplyRotation(percent);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                
				yield return null;
			}

			ApplyRotation(LookAtTween.Evaluate(1f));
			_coroutine = null;
			IsPlaying = false;
		}

        /// <summary>
        /// 在旅程中按指定的时间应用旋转
        /// </summary>
        /// <param name="percent"></param>
        protected virtual void ApplyRotation(float percent)
		{
			switch (LookAtTargetMode)
			{
				case LookAtTargetModes.Transform:
					_lookAtPosition = LookAtTarget.position;
					break;
				case LookAtTargetModes.TargetWorldPosition:
					_lookAtPosition = LookAtTargetWorldPosition;
					break;
				case LookAtTargetModes.Direction:
					_lookAtPosition = TransformToRotate.position + LookAtDirection;
					break;
			}
	            
			_direction = _lookAtPosition - TransformToRotate.position;
			_newRotation = Quaternion.LookRotation(_direction, _upwards);
			
			if (LockXAxis) { _newRotation.x = TransformToRotate.rotation.x; }
			if (LockYAxis) { _newRotation.y = TransformToRotate.rotation.y; }
			if (LockZAxis) { _newRotation.z = TransformToRotate.rotation.z; }
			
			TransformToRotate.transform.rotation = Quaternion.SlerpUnclamped(_initialDirectTargetTransformRotation, _newRotation, percent);
		}

        /// <summary>
        /// 在停止（Stop）时，如果我们正在进行移动（仅在直接模式中），则停止移动
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (_coroutine == null))
			{
				return;
			}
            
			base.CustomStopFeedback(position, feedbacksIntensity);
			IsPlaying = false;
			ClearCoroutine();
		}

        /// <summary>
        /// 清除当前的协程
        /// </summary>
        protected virtual void ClearCoroutine()
		{
			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}

        /// <summary>
        /// 在恢复（restore）时，我们恢复初始状态
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			TransformToRotate.transform.rotation = _initialRotation;
		}
	}
}
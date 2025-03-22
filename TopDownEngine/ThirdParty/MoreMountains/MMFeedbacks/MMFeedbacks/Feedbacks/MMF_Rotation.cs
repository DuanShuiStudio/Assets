using System.Collections;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 此反馈在触发时会为指定对象的旋转添加动画效果。
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈将在指定的持续时间（以秒为单位）内，依据 3 条指定的动画曲线（每个轴对应一条）为目标对象的旋转添加动画效果。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Rotation")]
	public class MMF_Rotation : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 此反馈的可能模式如下：
        //绝对模式（Absolute）：始终从头到尾遵循曲线，即完全按照预设的动画曲线来执行旋转动画，不受对象当前状态的影响，每次都从曲线的起始值开始到结束值结束。
		//累加模式（Additive）：在该反馈触发时，将曲线的值与对象当前已有的值相加，在对象当前状态的基础上进行旋转动画的调整。
		public enum Modes { Absolute, Additive, ToDestination }
        /// 此反馈可以运行的时间尺度模式。
        public enum TimeScales { Scaled, Unscaled }

        /// 为该反馈设置检视器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (AnimateRotationTarget == null); }
		public override string RequiredTargetText { get { return AnimateRotationTarget != null ? AnimateRotationTarget.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a AnimatePositionTarget and a Destination be set to be able to work properly. You can set one below."; } }
		public override bool HasCustomInspectors { get { return true; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => AnimateRotationTarget = FindAutomatedTarget<Transform>();

		[MMFInspectorGroup("Rotation Target", true, 61, true)]
		/// the object whose rotation you want to animate
		[Tooltip("你想要为其旋转添加动画效果的对象")]
		public Transform AnimateRotationTarget;

		[MMFInspectorGroup("Transition", true, 63)]
		/// whether this feedback should animate in absolute values or additive
		[Tooltip("此反馈应使用绝对值进行动画处理还是累加值进行动画处理。")]
		public Modes Mode = Modes.Absolute;
		/// whether this feedback should play on local or world rotation
		[Tooltip("此反馈应基于局部旋转还是世界旋转来播放")]
		public Space RotationSpace = Space.World;
		/// the duration of the transition
		[Tooltip("过渡的持续时间")]
		public float AnimateRotationDuration = 0.2f;
		/// the value to remap the curve's 0 value to
		[Tooltip("将曲线的 0 值重新映射到的值")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public float RemapCurveZero = 0f;
		/// the value to remap the curve's 1 value to
		[Tooltip("将曲线的 1 值重新映射到的值")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public float RemapCurveOne = 360f;
		/// if this is true, should animate the X rotation
		[Tooltip("如果此条件为真，则应播放 X 轴旋转动画")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public bool AnimateX = true;
		
		
		/// how the x part of the rotation should animate over time, in degrees
		[Tooltip("旋转的 X 轴部分应如何随时间以度为单位进行动画处理")]
		[MMFCondition("AnimateX")]
		public MMTweenType AnimateRotationTweenX = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// if this is true, should animate the Y rotation
		[Tooltip("如果此条件为真，则应播放 Y 轴旋转动画")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public bool AnimateY = true;
		/// how the y part of the rotation should animate over time, in degrees
		[Tooltip("旋转的 Y 轴部分应如何随时间以度为单位进行动画处理")]
		[MMFCondition("AnimateY")]
		public MMTweenType AnimateRotationTweenY = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// if this is true, should animate the Z rotation
		[Tooltip("如果此条件为真，则应播放 Z 轴旋转动画")]
		[MMFEnumCondition("Mode", (int)Modes.Absolute, (int)Modes.Additive)]
		public bool AnimateZ = true;
		/// how the z part of the rotation should animate over time, in degrees
		[Tooltip("旋转的 Z 轴部分应如何随时间以度为单位进行动画处理")]
		[MMFCondition("AnimateZ")]
		public MMTweenType AnimateRotationTweenZ = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		
		
		
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果此条件为真，即使该反馈正在进行中，调用它也会触发。如果为假，则在当前播放完成之前，将阻止任何新的播放操作。")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, initial and destination rotations will be recomputed on every play
		[Tooltip("如果此条件为真，每次播放时都会重新计算初始旋转和目标旋转")]
		public bool DetermineRotationOnPlay = false;
        
		[Header("To Destination到目标位置")]
		/// the space in which the ToDestination mode should operate 
		[Tooltip("“To Destination”（到目标状态）模式应在其中运行的空间。")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Space ToDestinationSpace = Space.World;
		/// the angles to match when in ToDestination mode
		[Tooltip("在 “To Destination”（到目标状态）模式下要匹配的角度")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public Vector3 DestinationAngles = new Vector3(0f, 180f, 0f);
		/// how the x part of the rotation should animate over time, in degrees
		[Tooltip("旋转的 X 轴分量应如何随时间以度为单位进行动画化处理。")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public MMTweenType ToDestinationTween = new MMTweenType(MMTween.MMTweenCurve.EaseInQuintic);

        /// 此反馈的持续时间即为旋转的持续时间。
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateRotationDuration); } set { AnimateRotationDuration = value; } }
		public override bool HasRandomness => true;

        /// [DEPRECATED] 旋转的 X 轴部分应如何随时间以度为单位进行动画处理。
        [HideInInspector] public AnimationCurve AnimateRotationX = null;
        /// [DEPRECATED] 旋转的 Y 轴部分应如何随时间以度为单位进行动画处理。
        [HideInInspector] public AnimationCurve AnimateRotationY = null;
        /// [DEPRECATED] 旋转的 Z 轴部分应如何随时间以度为单位进行动画处理。
        [HideInInspector] public AnimationCurve AnimateRotationZ = null;
        /// [DEPRECATED] 在向目标状态进行动画处理时要使用的动画曲线（上述单独的 X、Y、Z 轴曲线将不会被使用）。
        [HideInInspector] public AnimationCurve ToDestinationCurve = null;

		protected Quaternion _initialRotation;
		protected Vector3 _initialToDestinationAngles;
		protected Quaternion _destinationRotation;
		protected Coroutine _coroutine;

        /// <summary>
        /// 在初始化时，我们会存储初始旋转状态。
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (AnimateRotationTarget != null))
			{
				GetInitialRotation();
			}
		}

        /// <summary>
        /// 存储初始旋转状态以供将来使用。
        /// </summary>
        protected virtual void GetInitialRotation()
		{
			_initialRotation = (RotationSpace == Space.World) ? AnimateRotationTarget.rotation : AnimateRotationTarget.localRotation;
			_initialToDestinationAngles = _initialRotation.eulerAngles;
		}

        /// <summary>
        /// 在播放时，我们触发旋转动画。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (AnimateRotationTarget == null))
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			if (Active || Owner.AutoPlayOnEnable)
			{
				if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (DetermineRotationOnPlay && NormalPlayDirection) { GetInitialRotation(); }
					ClearCoroutine();
					_coroutine = Owner.StartCoroutine(AnimateRotation(AnimateRotationTarget, Vector3.zero, FeedbackDuration, AnimateRotationTweenX, AnimateRotationTweenY, AnimateRotationTweenZ, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier));
				}
				else if (Mode == Modes.ToDestination)
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (DetermineRotationOnPlay && NormalPlayDirection) { GetInitialRotation(); }
					ClearCoroutine();
					_coroutine = Owner.StartCoroutine(RotateToDestination());
				}
			}
		}

		protected virtual void ClearCoroutine()
		{
			if (_coroutine != null)
			{
				Owner.StopCoroutine(_coroutine);
				_coroutine = null;
			}
		}

        /// <summary>
        /// 一个用于将目标旋转至其目标旋转状态的协程
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator RotateToDestination()
		{
			if (AnimateRotationTarget == null)
			{
				yield break;
			}

			if ((AnimateRotationTweenX == null) || (AnimateRotationTweenY == null) || (AnimateRotationTweenZ == null))
			{
				yield break;
			}

			if (FeedbackDuration == 0f)
			{
				yield break;
			}

			Vector3 destinationAngles = NormalPlayDirection ? DestinationAngles : _initialToDestinationAngles;
			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			_initialRotation = AnimateRotationTarget.transform.rotation;
			if (ToDestinationSpace == Space.Self)
			{
				AnimateRotationTarget.transform.localRotation = Quaternion.Euler(destinationAngles);
			}
			else
			{
				AnimateRotationTarget.transform.rotation = Quaternion.Euler(destinationAngles);
			}
            
			_destinationRotation = AnimateRotationTarget.transform.rotation;
			AnimateRotationTarget.transform.rotation = _initialRotation;
			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);
				percent = ToDestinationTween.Evaluate(percent);

				Quaternion newRotation = Quaternion.LerpUnclamped(_initialRotation, _destinationRotation, percent);
				AnimateRotationTarget.transform.rotation = newRotation;

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                
				yield return null;
			}

			if (ToDestinationSpace == Space.Self)
			{
				AnimateRotationTarget.transform.localRotation = Quaternion.Euler(destinationAngles);
			}
			else
			{
				AnimateRotationTarget.transform.rotation = Quaternion.Euler(destinationAngles);
			}
			IsPlaying = false;
			_coroutine = null;
			yield break;
		}

        /// <summary>
        /// 一个用于随时间计算旋转情况的协程
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <param name="vector"></param>
        /// <param name="duration"></param>
        /// <param name="curveX"></param>
        /// <param name="curveY"></param>
        /// <param name="curveZ"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        protected virtual IEnumerator AnimateRotation(Transform targetTransform,
			Vector3 vector,
			float duration,
			MMTweenType curveX,
			MMTweenType curveY,
			MMTweenType curveZ,
			float remapZero,
			float remapOne)
		{
			if (targetTransform == null)
			{
				yield break;
			}

			if ((curveX == null) || (curveY == null) || (curveZ == null))
			{
				yield break;
			}

			if (duration == 0f)
			{
				yield break;
			}
            
			float journey = NormalPlayDirection ? 0f : duration;

			if (Mode == Modes.Additive)
			{
				_initialRotation = (RotationSpace == Space.World) ? targetTransform.rotation : targetTransform.localRotation;
			}

			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);
                
				ApplyRotation(targetTransform, remapZero, remapOne, curveX, curveY, curveZ, percent);

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                
				yield return null;
			}

			ApplyRotation(targetTransform, remapZero, remapOne, curveX, curveY, curveZ, FinalNormalizedTime);
			_coroutine = null;
			IsPlaying = false;
            
			yield break;
		}

        /// <summary>
        /// 计算旋转情况并将其应用到对象上
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <param name="multiplier"></param>
        /// <param name="curveX"></param>
        /// <param name="curveY"></param>
        /// <param name="curveZ"></param>
        /// <param name="percent"></param> 
        protected virtual void ApplyRotation(Transform targetTransform, float remapZero, float remapOne, MMTweenType curveX, MMTweenType curveY, MMTweenType curveZ, float percent)
		{
			if (RotationSpace == Space.World)
			{
				targetTransform.transform.rotation = _initialRotation;    
			}
			else
			{
				targetTransform.transform.localRotation = _initialRotation;
			}

			if (AnimateX)
			{
				float x = MMTween.Tween(percent, 0f, 1f, remapZero, remapOne, curveX);
				targetTransform.Rotate(Vector3.right, x, RotationSpace);
			}
			if (AnimateY)
			{
				float y = MMTween.Tween(percent, 0f, 1f, remapZero, remapOne, curveY);
				targetTransform.Rotate(Vector3.up, y, RotationSpace);
			}
			if (AnimateZ)
			{
				float z = MMTween.Tween(percent, 0f, 1f, remapZero, remapOne, curveZ);
				targetTransform.Rotate(Vector3.forward, z, RotationSpace);
			}
		}

        /// <summary>
        /// 在停止操作时，如果移动正在进行，我们会中断该移动。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active && FeedbackTypeAuthorized && (_coroutine != null))
			{
				Owner.StopCoroutine(_coroutine);
				_coroutine = null;
				IsPlaying = false;
			}
		}

        /// <summary>
        /// 在禁用状态下，我们会重置协程。
        /// </summary>
        public override void OnDisable()
		{
			_coroutine = null;
		}

        /// <summary>
        /// 在执行验证时，如有必要，我们会将已弃用的动画曲线转换为我们的缓动类型。
        /// </summary>
        public override void OnValidate()
		{
			base.OnValidate();
			MMFeedbacksHelpers.MigrateCurve(AnimateRotationX, AnimateRotationTweenX, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateRotationY, AnimateRotationTweenY, Owner);
			MMFeedbacksHelpers.MigrateCurve(AnimateRotationZ, AnimateRotationTweenZ, Owner);
			MMFeedbacksHelpers.MigrateCurve(ToDestinationCurve, ToDestinationTween, Owner);
		}

        /// <summary>
        /// 在恢复操作时，我们会恢复到初始状态。
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (RotationSpace == Space.World)
			{
				AnimateRotationTarget.rotation = _initialRotation;
			}
			else
			{
				AnimateRotationTarget.localRotation= _initialRotation;	
			}
		}
	}
}
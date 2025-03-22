using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will animate the target's position (not its rotation), on an arc around the specified rotation center, for the specified duration (in seconds).
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将使目标的位置（而非其旋转）在指定的旋转中心周围沿弧线移动，持续指定的时长（以秒为单位）")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Rotate Position Around")]
	public class MMF_RotatePositionAround : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
        public enum TimeScales { Scaled, Unscaled }

        /// 为这个反馈设置检视器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (AnimateRotationTarget == null); }
		public override string RequiredTargetText { get { return ((AnimateRotationTarget != null) || (AnimateRotationCenter != null)) ? AnimateRotationTarget.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要设置一个 “位置动画目标”（AnimatePositionTarget）和一个 “旋转动画中心”（AnimateRotationCenter）才能正常工作。你可以在下面进行设置。"; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => AnimateRotationTarget = FindAutomatedTarget<Transform>();

		[MMFInspectorGroup("Animation Targets", true, 61, true)]
		/// the object whose rotation you want to animate
		[Tooltip("你想要对其旋转进行动画处理的对象。")]
		public Transform AnimateRotationTarget;
		/// the object around which to rotate AnimateRotationTarget
		[Tooltip("用于让 “旋转动画目标”（AnimateRotationTarget）围绕其进行旋转的对象")]
		public Transform AnimateRotationCenter;
		
		[MMFInspectorGroup("Transition", true, 63)]
		/// the duration of the transition
		[Tooltip("过渡的持续时间。")]
		public float AnimateRotationDuration = 0.2f;
		/// the value to remap the curve's 0 value to
		[Tooltip("将曲线的 0 值重新映射到的值")]
		public float RemapCurveZero = 0f;
		/// the value to remap the curve's 1 value to
		[Tooltip("将曲线的 1 值重新映射到的值")]
		public float RemapCurveOne = 180f;
		/// if this is true, should animate movement on the X axis
		[Tooltip("如果此条件为真，则应在 X 轴上进行移动动画")]
		public bool AnimateX = false;
		/// how the x part of the movement should animate over time, in degrees
		[Tooltip("移动的 X 轴部分应如何随时间以度数为单位进行动画化处理")]
		[MMCondition("AnimateX", true)]
		public AnimationCurve AnimateRotationX = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// if this is true, should animate movement on the Y axis
		[Tooltip("如果此条件为真，则应在 Y 轴上进行移动动画。")]
		public bool AnimateY = true;
		/// how the y part of the rotation should animate over time, in degrees
		[Tooltip("旋转的 Y 轴部分应如何随时间以度数为单位进行动画处理")]
		[MMCondition("AnimateY", true)]
		public AnimationCurve AnimateRotationY = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// if this is true, should animate movement on the Z axis
		[Tooltip("如果此条件为真，则应在 Z 轴上进行移动动画。")]
		public bool AnimateZ = false;
		/// how the z part of the rotation should animate over time, in degrees
		[Tooltip("旋转的 Z 轴部分应如何随时间以度数为单位进行动画处理")]
		[MMCondition("AnimateZ", true)]
		public AnimationCurve AnimateRotationZ = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果此条件为真，调用该反馈将触发它，即使它正在进行中。如果此条件为假，在当前播放完成之前，将阻止任何新的播放操作。")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, initial and destination rotations will be recomputed on every play
		[Tooltip("如果此条件为真，每次运行时都会重新计算初始旋转和目标旋转")]
		public bool DetermineRotationOnPlay = false;

        /// 此反馈的持续时间即为旋转的持续时间。
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateRotationDuration); } set { AnimateRotationDuration = value; } }
		public override bool HasRandomness => true;

		protected Vector3 _initialPosition;
		protected Vector3 _rotationAngles;
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
				GetInitialPosition();
			}
		}

        /// <summary>
        /// 存储初始旋转状态以供后续使用。
        /// </summary>
        protected virtual void GetInitialPosition()
		{
			_initialPosition = AnimateRotationTarget.transform.position;
		}

        /// <summary>
        /// 在开始运行时，我们触发旋转动画。
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
				if (!AllowAdditivePlays && (_coroutine != null))
				{
					return;
				}
				if (DetermineRotationOnPlay && NormalPlayDirection) { GetInitialPosition(); }
				ClearCoroutine();
				_coroutine = Owner.StartCoroutine(AnimateRotation(AnimateRotationTarget, Vector3.zero, FeedbackDuration, AnimateRotationX, AnimateRotationY, AnimateRotationZ, RemapCurveZero * intensityMultiplier, RemapCurveOne * intensityMultiplier));
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
        /// 一个用于随时间计算旋转的协程。
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
			AnimationCurve curveX,
			AnimationCurve curveY,
			AnimationCurve curveZ,
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
        /// 计算旋转并将其应用到对象上。
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <param name="multiplier"></param>
        /// <param name="curveX"></param>
        /// <param name="curveY"></param>
        /// <param name="curveZ"></param>
        /// <param name="percent"></param> 
        protected virtual void ApplyRotation(Transform targetTransform, float remapZero, float remapOne, AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, float percent)
		{
			targetTransform.position = _initialPosition;

			_rotationAngles.x = 0f;
			_rotationAngles.y = 0f;
			_rotationAngles.z= 0f;
			
			if (AnimateX)
			{
				_rotationAngles.x = curveX.Evaluate(percent);
				_rotationAngles.x = MMFeedbacksHelpers.Remap(_rotationAngles.x, 0f, 1f, remapZero, remapOne);
			}
			if (AnimateY)
			{
				_rotationAngles.y = curveY.Evaluate(percent);
				_rotationAngles.y = MMFeedbacksHelpers.Remap(_rotationAngles.y, 0f, 1f, remapZero, remapOne);
			}
			if (AnimateZ)
			{
				_rotationAngles.z = curveZ.Evaluate(percent);
				_rotationAngles.z = MMFeedbacksHelpers.Remap(_rotationAngles.z, 0f, 1f, remapZero, remapOne);
			}

			targetTransform.position = MMMaths.RotatePointAroundPivot(targetTransform.position, AnimateRotationCenter.position, _rotationAngles);
		}

        /// <summary>
        /// 在停止时，如果移动正在进行，我们将中断该移动。
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
        /// 在恢复时，我们会恢复到初始状态
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			AnimateRotationTarget.transform.position = _initialPosition;
		}

        /// <summary>
        /// 在禁用时，我们会重置协程。
        /// </summary>
        public override void OnDisable()
		{
			_coroutine = null;
		}
	}
}
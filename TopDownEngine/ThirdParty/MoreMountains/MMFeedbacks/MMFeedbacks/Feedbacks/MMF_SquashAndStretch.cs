using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you modify the scale of an object on an axis while the other two axis (or only one) get automatically modified to conserve mass
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Squash and Stretch")]
	[FeedbackHelp("这个反馈机制会让你在某一轴向上修改物体的缩放比例，同时另外两个轴向（或者仅一个轴向）会自动进行调整以保持质量守恒。\r\n")]
	public class MMF_SquashAndStretch : MMF_Feedback
	{
        /// 为这个反馈设置检视面板（Inspector）中的颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (SquashAndStretchTarget == null); }
		public override string RequiredTargetText { get { return SquashAndStretchTarget != null ? SquashAndStretchTarget.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a SquashAndStretchTarget be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => SquashAndStretchTarget = FindAutomatedTarget<Transform>();

        /// 一个静态布尔变量，用于一次性禁用所有此类反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 此反馈可运行的可能模式。
        public enum Modes { Absolute, Additive, ToDestination }
        /// 应用挤压和拉伸效果的各个轴向。
        public enum PossibleAxis { XtoYZ, XtoY, XtoZ, YtoXZ, YtoX, YtoZ, ZtoXZ, ZtoX, ZtoY }
        /// 缩放动画可能的时间尺度。
        public enum TimeScales { Scaled, Unscaled }

		[MMFInspectorGroup("Squash & Stretch", true, 54, true)]

		/// the object to animate
		[Tooltip("要进行动画处理的对象。")]
		public Transform SquashAndStretchTarget;
		/// the mode this feedback should operate on
		/// Absolute : follows the curve
		/// Additive : adds to the current scale of the target
		/// ToDestination : sets the scale to the destination target, whatever the current scale is
		[Tooltip("此反馈应采用的运行模式" +
                 "Absolute绝对模式：遵循曲线变化" +
                 "Additive累加模式：在目标当前缩放比例的基础上进行累加" +
                 "ToDestination至目标值模式：无论当前缩放比例是多少，都将缩放比例设置为目标值")]
		public Modes Mode = Modes.Absolute;
		public PossibleAxis Axis = PossibleAxis.YtoXZ;
		/// the duration of the animation
		[Tooltip("the duration of the animation")]
		public float AnimateScaleDuration = 0.2f;
		/// the value to remap the curve's 0 value to
		[Tooltip("将曲线的 0 值重新映射到的值")]
		public float RemapCurveZero = 1f;
		/// the value to remap the curve's 1 value to
		[Tooltip("将曲线的 1 值重新映射到的值")]
		[FormerlySerializedAs("Multiplier")]
		public float RemapCurveOne = 2f;
		/// how much should be added to the curve
		[Tooltip("应该给曲线增加多少数值")]
		public float Offset = 0f;
		/// the curve along which to animate the scale
		[Tooltip("用于对缩放进行动画处理的曲线")]
		public AnimationCurve AnimateCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1.5f), new Keyframe(1, 0));
		/// if this is true, calling that feedback will trigger it, even if it's in progress. If it's false, it'll prevent any new Play until the current one is over
		[Tooltip("如果此选项为真，调用该反馈将触发它，即使它正在进行中。如果为假，在当前播放结束之前，它将阻止任何新的播放操作")] 
		public bool AllowAdditivePlays = false;
		/// if this is true, initial and destination scales will be recomputed on every play
		[Tooltip("如果此选项为真，每次播放时都会重新计算初始和目标缩放比例")]
		public bool DetermineScaleOnPlay = false;
		/// the scale to reach when in ToDestination mode
		[Tooltip("在 ToDestination“至目标值” 模式下要达到的缩放比例")]
		[MMFEnumCondition("Mode", (int)Modes.ToDestination)]
		public float DestinationScale = 2f;

        /// 此反馈的持续时间即为缩放动画的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(AnimateScaleDuration); } set { AnimateScaleDuration = value; } }
		public override bool HasRandomness => true;

		protected Vector3 _initialScale;
		protected float _initialAxisScale;
		protected Vector3 _newScale;
		protected Coroutine _coroutine;

        /// <summary>
        /// 在初始化时，我们会存储初始缩放比例。
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (SquashAndStretchTarget != null))
			{
				GetInitialScale();
			}
		}

        /// <summary>
        /// 存储初始缩放比例以供后续使用。
        /// </summary>
        protected virtual void GetInitialScale()
		{
			_initialScale = SquashAndStretchTarget.localScale;
		}

		protected virtual void GetAxisScale()
		{
			switch (Axis)
			{
				case PossibleAxis.XtoYZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.x;
					break;
				case PossibleAxis.XtoY:
					_initialAxisScale = SquashAndStretchTarget.localScale.x;
					break;
				case PossibleAxis.XtoZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.x;
					break;
				case PossibleAxis.YtoXZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.y;
					break;
				case PossibleAxis.YtoX:
					_initialAxisScale = SquashAndStretchTarget.localScale.y;
					break;
				case PossibleAxis.YtoZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.y;
					break;
				case PossibleAxis.ZtoXZ:
					_initialAxisScale = SquashAndStretchTarget.localScale.z;
					break;
				case PossibleAxis.ZtoX:
					_initialAxisScale = SquashAndStretchTarget.localScale.z;
					break;
				case PossibleAxis.ZtoY:
					_initialAxisScale = SquashAndStretchTarget.localScale.z;
					break;
			}
		}

        /// <summary>
        /// 播放时，触发缩放动画。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (SquashAndStretchTarget == null)) 
			{
				return;
			}
            
			if (DetermineScaleOnPlay && NormalPlayDirection)
			{
				GetInitialScale();
			}
            
			GetAxisScale();
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			if (Active || Owner.AutoPlayOnEnable)
			{
				if ((Mode == Modes.Absolute) || (Mode == Modes.Additive))
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(AnimateScale(SquashAndStretchTarget, FeedbackDuration, AnimateCurve, Axis, RemapCurveZero, RemapCurveOne * intensityMultiplier));
				}
				if (Mode == Modes.ToDestination)
				{
					if (!AllowAdditivePlays && (_coroutine != null))
					{
						return;
					}
					if (_coroutine != null) { Owner.StopCoroutine(_coroutine); }
					_coroutine = Owner.StartCoroutine(ScaleToDestination());
				}                   
			}
		}

        /// <summary>
        /// 一个用于将目标对象缩放到其目标缩放比例的内部协程
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ScaleToDestination()
		{
			if (SquashAndStretchTarget == null)
			{
				yield break;
			}

			if (FeedbackDuration == 0f)
			{
				yield break;
			}

			float journey = NormalPlayDirection ? 0f : FeedbackDuration;

			_initialScale = SquashAndStretchTarget.localScale;
			_newScale = _initialScale;
			GetAxisScale();
			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= FeedbackDuration) && (FeedbackDuration > 0))
			{
				float percent = Mathf.Clamp01(journey / FeedbackDuration);

				float newScale = Mathf.LerpUnclamped(_initialAxisScale, DestinationScale, AnimateCurve.Evaluate(percent) + Offset);
				newScale = MMFeedbacksHelpers.Remap(newScale, 0f, 1f, RemapCurveZero, RemapCurveOne);

				ApplyScale(newScale);
                
				SquashAndStretchTarget.localScale = _newScale;

				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
                
				yield return null;
			}

			ApplyScale(DestinationScale);
			SquashAndStretchTarget.localScale = NormalPlayDirection ? _newScale : _initialScale;
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

        /// <summary>
        /// 一个用于随时间推移对缩放进行动画处理的内部协程
        /// </summary>
        /// <param name="targetTransform"></param>
        /// <param name="duration"></param>
        /// <param name="curveX"></param>
        /// <param name="curveY"></param>
        /// <param name="curveZ"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        protected virtual IEnumerator AnimateScale(Transform targetTransform, float duration, AnimationCurve curve, PossibleAxis axis, float remapCurveZero = 0f, float remapCurveOne = 1f)
		{
			if (targetTransform == null)
			{
				yield break;
			}

			if (duration == 0f)
			{
				yield break;
			}
            
			float journey = NormalPlayDirection ? 0f : duration;
            
			_initialScale = targetTransform.localScale;
			IsPlaying = true;
            
			while ((journey >= 0) && (journey <= duration) && (duration > 0))
			{
				float percent = Mathf.Clamp01(journey / duration);
				ComputeAndApplyScale(percent, curve, remapCurveZero, remapCurveOne, targetTransform);
				journey += NormalPlayDirection ? FeedbackDeltaTime : -FeedbackDeltaTime;
				yield return null;
			}

			ComputeAndApplyScale(1f, curve, remapCurveZero, remapCurveOne, targetTransform);
			_coroutine = null;
			IsPlaying = false;
			yield return null;
		}

        /// <summary>
        /// 根据当前百分比计算新的缩放比例，并将其应用到变换组件上
        /// </summary>
        /// <param name="percent"></param>
        /// <param name="curve"></param>
        /// <param name="remapCurveZero"></param>
        /// <param name="remapCurveOne"></param>
        /// <param name="targetTransform"></param>
        protected virtual void ComputeAndApplyScale(float percent, AnimationCurve curve, float remapCurveZero, float remapCurveOne, Transform targetTransform)
		{
			float newScale = curve.Evaluate(percent) + Offset;
			newScale = MMFeedbacksHelpers.Remap(newScale, 0f, 1f, remapCurveZero, remapCurveOne);
			if (Mode == Modes.Additive)
			{
				newScale += _initialAxisScale;
			}
			newScale = Mathf.Abs(newScale);
			ApplyScale(newScale);
			targetTransform.localScale = _newScale;
		}

        /// <summary>
        /// 在选定的轴上应用新的缩放比例
        /// </summary>
        /// <param name="newScale"></param>
        protected virtual void ApplyScale(float newScale)
		{
			float invertScale = 1 / Mathf.Sqrt(newScale);
			switch (Axis)
			{
				case PossibleAxis.XtoYZ:
					_newScale.x = newScale;
					_newScale.y = invertScale;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.XtoY:
					_newScale.x = newScale;
					_newScale.y = invertScale;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.XtoZ:
					_newScale.x = newScale;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoXZ:
					_newScale.x = invertScale;
					_newScale.y = newScale;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.YtoX:
					_newScale.x = invertScale;
					_newScale.y = newScale;
					_newScale.z = _initialScale.z;
					break;
				case PossibleAxis.YtoZ:
					_newScale.x = newScale;
					_newScale.y = _initialScale.y;
					_newScale.z = invertScale;
					break;
				case PossibleAxis.ZtoXZ:
					_newScale.x = invertScale;
					_newScale.y = invertScale;
					_newScale.z = newScale;
					break;
				case PossibleAxis.ZtoX:
					_newScale.x = invertScale;
					_newScale.y = _initialScale.y;
					_newScale.z = newScale;
					break;
				case PossibleAxis.ZtoY:
					_newScale.x = _initialScale.x;
					_newScale.y = invertScale;
					_newScale.z = newScale;
					break;
			}
		}

        /// <summary>
        /// 停止时，如果移动操作正在进行，我们将中断该移动
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active && (_coroutine != null))
			{
				Owner.StopCoroutine(_coroutine);
				_coroutine = null;
				IsPlaying = false;
			}
		}

        /// <summary>
        /// 禁用时，我们会重置协程
        /// </summary>
        public override void OnDisable()
		{
			_coroutine = null;
		}

        /// <summary>
        /// 恢复时，我们将恢复到初始状态
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			SquashAndStretchTarget.localScale = _initialScale;
		}
	}
}
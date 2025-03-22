using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 当触发该反馈时，它会根据所选设置激活一个 `MMWiggle` 对象的摆动方法，使其位置、旋转、缩放或者这三者同时产生摆动效果。 
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈功能允许你在配备了 MMWiggle 组件的对象上，在指定的持续时间内触发其位置、旋转和/或缩放的摆动效果。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Transform/Wiggle")]
	public class MMF_Wiggle : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TransformColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetWiggle == null); }
		public override string RequiredTargetText { get { return TargetWiggle != null ? TargetWiggle.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈功能要求设置一个目标摆动对象（TargetWiggle）才能正常工作。你可以在下方进行设置。"; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetWiggle = FindAutomatedTarget<MMWiggle>();

		[MMFInspectorGroup("Target", true, 54, true)]
		/// the Wiggle component to target
		[Tooltip("要作为目标的摆动（Wiggle）组件。")]
		public MMWiggle TargetWiggle;
        
		[MMFInspectorGroup("Position", true, 55)]
		/// whether or not to wiggle position
		[Tooltip("是否对位置进行摆动处理。")]
		public bool WigglePosition = true;
		/// the duration (in seconds) of the position wiggle
		[Tooltip("位置摆动的持续时间（以秒为单位）。")]
		public float WigglePositionDuration;

		[MMFInspectorGroup("Rotation", true, 56)]
		/// whether or not to wiggle rotation
		[Tooltip("是否对旋转进行摆动处理。")]
		public bool WiggleRotation;
		/// the duration (in seconds) of the rotation wiggle
		[Tooltip("旋转摆动的持续时间（以秒为单位）。")]
		public float WiggleRotationDuration;

		[MMFInspectorGroup("Scale", true, 57)]
		/// whether or not to wiggle scale
		[Tooltip("是否对缩放进行摆动处理。")]
		public bool WiggleScale;
		/// the duration (in seconds) of the scale wiggle
		[Tooltip("缩放摆动的持续时间（以秒为单位）。")]
		public float WiggleScaleDuration;


        /// 此反馈的持续时间即为正在播放的剪辑片段的时长。
        public override float FeedbackDuration
		{
			get { return Mathf.Max(ApplyTimeMultiplier(WigglePositionDuration), ApplyTimeMultiplier(WiggleRotationDuration), ApplyTimeMultiplier(WiggleScaleDuration)); }
			set { WigglePositionDuration = value;
				WiggleRotationDuration = value;
				WiggleScaleDuration = value;
			} 
		}

        /// <summary>
        /// 播放时，我们会触发所需的摆动效果。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetWiggle == null))
			{
				return;
			}
            
			TargetWiggle.enabled = true;
			if (WigglePosition)
			{
				TargetWiggle.PositionWiggleProperties.UseUnscaledTime = !InScaledTimescaleMode;
				TargetWiggle.WigglePosition(ApplyTimeMultiplier(WigglePositionDuration));
			}
			if (WiggleRotation)
			{
				TargetWiggle.RotationWiggleProperties.UseUnscaledTime = !InScaledTimescaleMode;
				TargetWiggle.WiggleRotation(ApplyTimeMultiplier(WiggleRotationDuration));
			}
			if (WiggleScale)
			{
				TargetWiggle.ScaleWiggleProperties.UseUnscaledTime = !InScaledTimescaleMode;
				TargetWiggle.WiggleScale(ApplyTimeMultiplier(WiggleScaleDuration));
			}
		}

        /// <summary>
        /// 停止时，如有必要，我们会更改对象的状态。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetWiggle == null))
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);

			TargetWiggle.enabled = false;
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

			TargetWiggle.RestoreInitialValues();
		}
	}
}
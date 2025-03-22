using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will trigger a post processing moving filter event, meant to be caught by a MMPostProcessingMovableFilter object
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将触发一个后期处理移动滤镜事件，该事件旨在被一个MM后期处理可移动滤镜对象捕获。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("PostProcess/PPMovingFilter")]
	public class MMF_PPMovingFilter : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
        
		/// 设置此反馈在检查器中的颜色。
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		public override string RequiredTargetText => RequiredChannelText;
		#endif
        
		/// 此反馈的持续时间就是过渡过程的持续时间。 
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(TransitionDuration); } set { TransitionDuration = value;  } }
		public override bool HasChannel => true;

		/// 此反馈的可能模式
		public enum Modes { Toggle, On, Off }

		[MMFInspectorGroup("PostProcessing Profile Moving Filter", true, 54)]
		/// the selected mode for this feedback 
		[Tooltip("此反馈所选择的模式")]
		public Modes Mode = Modes.Toggle;
		/// the duration of the transition
		[Tooltip("过渡的持续时间")]
		public float TransitionDuration = 1f;
		/// the curve to move along to
		[Tooltip("要沿着其移动的曲线")]
		public MMTweenType Curve = new MMTweenType(MMTween.MMTweenCurve.EaseInCubic);

		protected bool _active = false;
		protected bool _toggle = false;

		/// <summary>
		/// 在自定义播放时，我们会使用选定的参数触发一个MM后期处理移动滤镜事件。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			_active = (Mode == Modes.On);
			_toggle = (Mode == Modes.Toggle);

			MMPostProcessingMovingFilterEvent.Trigger(Curve, _active, _toggle, FeedbackDuration, Channel);
		}

		/// <summary>
		/// 在停止时，我们停止过渡效果。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			MMPostProcessingMovingFilterEvent.Trigger(Curve, _active, _toggle, FeedbackDuration, stop:true);
		}

		/// <summary>
		/// 在恢复时，我们将对象放回到其初始位置。 
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			MMPostProcessingMovingFilterEvent.Trigger(Curve, _active, _toggle, FeedbackDuration, restore:true);
		}
	}
}
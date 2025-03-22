using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 此反馈在播放时通过发送“时间缩放（TimeScale）”事件来改变时间尺度。
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈会触发一个 MMTimeScaleEvent 事件。如果你的场景中有一个 MMTimeManager 对象，该事件将被捕获，并根据指定的设置来修改时间尺度。这些设置包括新的时间尺度（例如，0.5 表示比正常速度慢两倍，2 表示比正常速度快两倍等）、时间尺度修改的持续时间，以及可选的在正常时间尺度和更改后的时间尺度之间进行过渡的速度。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Time/Timescale Modifier")]
	public class MMF_TimescaleModifier : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// <summary>
        /// 此反馈的可能模式有：
		/// - shake震动：在一定时长内改变时间尺度
		/// - change改变：将时间尺度设置为一个新值，并永久保持（直到你再次更改它）。
		/// - reset重置：将时间尺度恢复到其之前的值。
		/// </summary>
		public enum Modes { Shake, Change, Reset, Unfreeze }

        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TimeColor; } }
		public override string RequiredTargetText { get { return Mode.ToString() + " x" + TimeScale ;  } }
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
		#endif

		[MMFInspectorGroup("Timescale Modifier", true, 63)]
		/// the selected mode
		[Tooltip("可选择的模式 : shake震动：在一定时长内改变时间尺度" +
                 "- change改变：将时间尺度设置为一个新值，并永久保持（直到你再次更改它）" +
                 "- reset重置：将时间尺度恢复到其之前的值")]
		public Modes Mode = Modes.Shake;

		/// the new timescale to apply
		[Tooltip("要应用的新时间尺度。")]
		public float TimeScale = 0.5f;
		/// the duration of the timescale modification
		[Tooltip("时间尺度修改的持续时间。")]
		[MMFEnumCondition("Mode", (int)Modes.Shake)]
		public float TimeScaleDuration = 1f;
		/// whether to reset the timescale on Stop or not
		[Tooltip("是否在停止时重置时间尺度。")]
		public bool ResetTimescaleOnStop = false;
		
		[MMFInspectorGroup("Interpolation", true, 63)]
		/// whether or not we should lerp the timescale
		[Tooltip("我们是否应该对时间尺度进行线性插值处理。")]
		public bool TimeScaleLerp = false;
		/// whether to lerp over a set duration, or at a certain speed
		[Tooltip("是要在一段设定的时长内进行线性插值，还是以一定的速度进行线性插值 ")]
		public MMTimeScaleLerpModes TimescaleLerpMode = MMTimeScaleLerpModes.Speed;
		/// in Speed mode, the speed at which to lerp the timescale
		[Tooltip("在速度模式下，用于对时间尺度进行线性插值的速度。")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Speed)]
		public float TimeScaleLerpSpeed = 1f;
		/// in Duration mode, the curve to use to lerp the timescale
		[Tooltip("在时长模式下，用于对时间尺度进行线性插值的曲线。")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public MMTweenType TimescaleLerpCurve = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1))); 
		/// in Duration mode, the duration of the timescale interpolation, in unscaled time seconds
		[Tooltip("在时长模式下，时间尺度插值的持续时长，以未缩放的时间秒数为单位。")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public float TimescaleLerpDuration = 1f;
		/// whether or not we should lerp the timescale as it goes back to normal afterwards
		[Tooltip("在时间尺度随后恢复正常的过程中，是否应该对其进行线性插值处理。")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public bool TimeScaleLerpOnReset = false;
		/// in Duration mode, the curve to use to lerp the timescale
		[Tooltip("在时长模式下，用于对时间尺度进行线性插值的曲线。")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public MMTweenType TimescaleLerpCurveOnReset = new MMTweenType( new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// in Duration mode, the duration of the timescale interpolation, in unscaled time seconds
		[Tooltip("在时长模式下，时间尺度插值的持续时长，以未缩放的时间秒数为单位。")]
		[MMFEnumCondition("TimescaleLerpMode", (int)MMTimeScaleLerpModes.Duration)]
		public float TimescaleLerpDurationOnReset = 1f;

        /// 此反馈的持续时长即为时间修改的持续时长。
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(TimeScaleDuration); } set { TimeScaleDuration = value; } }

        /// <summary>
        /// 播放时，触发一个时间尺度事件。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			switch (Mode)
			{
				case Modes.Shake:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, FeedbackDuration, TimeScaleLerp, TimeScaleLerpSpeed, false, TimescaleLerpMode, TimescaleLerpCurve, TimescaleLerpDuration, TimeScaleLerpOnReset, TimescaleLerpCurveOnReset, TimescaleLerpDurationOnReset);
					break;
				case Modes.Change:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, 0f, TimeScaleLerp, TimeScaleLerpSpeed, true, TimescaleLerpMode, TimescaleLerpCurve, TimescaleLerpDuration, TimeScaleLerpOnReset, TimescaleLerpCurveOnReset, TimescaleLerpDurationOnReset);
					break;
				case Modes.Reset:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
					break;
				case Modes.Unfreeze:
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, TimeScale, 0f, false, 0f, true);
					break;
			}     
		}

        /// <summary>
        /// 停止时，若有需要，我们会重置时间尺度。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || !ResetTimescaleOnStop)
			{
				return;
			}
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
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
			MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Reset, TimeScale, 0f, false, 0f, true);
		}

        /// <summary>
        /// 自动将一个 MM 时间管理器添加到场景中。
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			(MMTimeManager timeManager, bool createdNew) = Owner.gameObject.MMFindOrCreateObjectOfType<MMTimeManager>("MMTimeManager", null);
			if (createdNew)
			{
				MMDebug.DebugLogInfo("已将一个 MM 时间管理器添加到场景中。一切准备就绪。");	
			}
		}
	}
}
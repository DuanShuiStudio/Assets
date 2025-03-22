using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个用于绑定 Unity 事件并在播放时触发它们的反馈
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈允许你将任何类型的 Unity 事件绑定到这个反馈的播放、停止、初始化和重置方法上。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Events/Unity Events")]
	public class MMF_Events : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.EventsColor; } }
		#endif

		[MMFInspectorGroup("Events", true, 44)]
		/// the events to trigger when the feedback is played
		[Tooltip("反馈播放时要触发的事件")]
		public UnityEvent PlayEvents;
		/// the events to trigger when the feedback is stopped
		[Tooltip("反馈停止时要触发的事件")]
		public UnityEvent StopEvents;
		/// the events to trigger when the feedback is initialized
		[Tooltip("反馈初始化时要触发的事件")]
		public UnityEvent InitializationEvents;
		/// the events to trigger when the feedback is reset
		[Tooltip("反馈重置时要触发的事件")]
		public UnityEvent ResetEvents;

        /// <summary>
        /// 在初始化时，触发初始化事件
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
			if (Active && (InitializationEvents != null))
			{
				InitializationEvents.Invoke();
			}
		}

        /// <summary>
        /// 在播放时，触发播放事件
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (PlayEvents == null))
			{
				return;
			}
			PlayEvents.Invoke();    
		}

        /// <summary>
        /// 在停止时，触发停止事件
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (StopEvents == null))
			{
				return;
			}
			StopEvents.Invoke();
		}

        /// <summary>
        /// 在重置时，触发重置事件
        /// </summary>
        protected override void CustomReset()
		{
			if (!Active || !FeedbackTypeAuthorized || (ResetEvents == null))
			{
				return;
			}
			base.CustomReset();
			ResetEvents.Invoke();
		}
	}
}
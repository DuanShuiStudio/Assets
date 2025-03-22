using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 此反馈允许您触发 TopDown Engine 事件，然后这些事件可以被其他类捕获
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackPath("Events/TopDown Engine Events")]
	[FeedbackHelp("此反馈允许您触发 TopDown Engine 事件，然后这些事件可以被其他类捕获")]
	public class MMF_TopDownEngineEvent : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈在检查器中的颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.EventsColor; } }
		#endif
		
		[MMFInspectorGroup("TopDown Engine Events", true, 17)]

		/// the type of event to trigger
		[Tooltip("要触发的事件类型")]
		public TopDownEngineEventTypes EventType = TopDownEngineEventTypes.PauseNoMenu;
		/// an optional Character to pass to the event
		[Tooltip("一个可选的 Character 参数，可以传递给事件")]
		public Character TargetCharacter;

        /// <summary>
        /// 在播放时，我们请求生成一个浮动文字
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
		{
			if (Active)
			{
				TopDownEngineEvent.Trigger(EventType, TargetCharacter);
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 此反馈在播放时会触发一个冻结帧事件，使游戏暂停指定的持续时间（通常较短，但不一定）。
    /// </summary>
    [AddComponentMenu("")]
	[FeedbackHelp("此反馈将在指定的持续时间（以秒为单位）内冻结时间刻度。我通常使用0.01秒或0.02秒，但您可以随意调整。它需要场景中有MMTimeManager才能工作。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Time/Freeze Frame")]
	public class MMF_FreezeFrame : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.TimeColor; } }
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
		#endif

		[MMFInspectorGroup("Freeze Frame", true, 63)]
		/// the duration of the freeze frame
		[Tooltip("冻结帧的持续时间")]
		public float FreezeFrameDuration = 0.02f;
		/// the minimum value the timescale should be at for this freeze frame to happen. This can be useful to avoid triggering freeze frames when the timescale is already frozen. 
		[Tooltip("时间缩放应达到的最小值，以便发生此冻结帧。这在时间缩放已经冻结时避免触发冻结帧非常有用。")]
		public float MinimumTimescaleThreshold = 0.1f;

        /// 此反馈的持续时间就是冻结帧的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(FreezeFrameDuration); } set { FreezeFrameDuration = value; } }

        /// <summary>
        /// 在播放时，我们触发一个冻结帧事件
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			if (Time.timeScale < MinimumTimescaleThreshold)
			{
				return;
			}
            
			MMFreezeFrameEvent.Trigger(FeedbackDuration);
		}

        /// <summary>
        /// 自动向场景中添加一个MMTimeManager
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			(MMTimeManager timeManager, bool createdNew) = Owner.gameObject.MMFindOrCreateObjectOfType<MMTimeManager>("MMTimeManager", null);
			if (createdNew)
			{
				MMDebug.DebugLogInfo("已向场景中添加了MMTimeManager。您已全部设置完毕");	
			}
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// this feedback will "hold", or wait, until all previous feedbacks have been executed, and will then pause the execution of your MMFeedbacks sequence, for the specified duration
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将“保持”或等待，直到所有先前的反馈都已执行，然后它将暂停您的MMFeedbacks序列的执行，持续指定的时间")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Pause/Holding Pause")]
	public class MMF_HoldingPause : MMF_Pause
	{
        /// 在检查器中设置此反馈的颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.HoldingPauseColor; } }
		#endif
		public override bool HoldingPause { get { return true; } }

        /// 此反馈的持续时间就是暂停的时长
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }

        /// <summary>
        /// 在自定义播放时，我们只播放暂停。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (Active)
			{
				ProcessNewPauseDuration();
				Owner.StartCoroutine(PlayPause());
			}
		}
	}
}
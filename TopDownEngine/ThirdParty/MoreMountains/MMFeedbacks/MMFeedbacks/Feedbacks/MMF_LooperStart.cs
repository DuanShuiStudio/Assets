using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback can act as a pause but also as a start point for your loops. Add a FeedbackLooper below this (and after a few feedbacks) and your MMFeedbacks will loop between both
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这条反馈可以作为一个暂停点，也可以作为循环的起点。在这个（以及几个反馈之后）下面添加一个 FeedbackLooper，你的 MMFeedbacks 就会在两者之间循环")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Loop/Looper Start")]
	public class MMF_LooperStart : MMF_Pause
	{
        /// 在检查器中设置此反馈的颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.LooperStartColor; } }
		#endif
		public override bool LooperStart { get { return true; } }

        /// 此反馈的持续时间就是暂停的时长
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(PauseDuration); } set { PauseDuration = value; } }

        /// <summary>
        /// 覆盖默认值
        /// </summary>
        protected virtual void Reset()
		{
			PauseDuration = 0;
		}

        /// <summary>
        /// 在播放时，我们运行暂停
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈将强制中断，暂停编辑器。 
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这条反馈信息将会强制暂停，使编辑器进入暂停状态")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Debug/Break")]
	public class MMF_DebugLBreak : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 此反馈的持续时间为0。
		public override float FeedbackDuration { get { return 0f; } }

		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
		#endif
        
		/// <summary>
		/// 在开始播放（游戏等）时，我们进行中断操作。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			Debug.Break();
		}
	}
}
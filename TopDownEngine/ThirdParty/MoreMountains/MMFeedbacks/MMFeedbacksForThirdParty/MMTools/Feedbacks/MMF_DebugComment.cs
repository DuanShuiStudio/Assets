using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback doesn't do anything by default, it's just meant as a comment, you can store text in it for future reference, maybe to remember how you setup a particular MMFeedbacks. Optionally it can also output that comment to the console on Play.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这条反馈默认情况下不会执行任何操作，它仅用作注释。你可以在其中存储文本以备日后参考，或许是为了记住你是如何设置某个特定的MMFeedbacks的。另外，它还可以在播放时将该注释输出到控制台。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Debug/Comment")]
	public class MMF_DebugComment : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
		#endif
     
		[MMFInspectorGroup("Comment", true, 61)]
		/// the comment / note associated to this feedback 
		[Tooltip("与此反馈相关联的评论/注释")]
		[TextArea(10,30)] 
		public string Comment;

		/// if this is true, the comment will be output to the console on Play 
		[Tooltip("如果这为真，那么在播放时这条注释将被输出到控制台。 ")]
		public bool LogComment = false;
		/// the color of the message when in DebugLogTime mode
		[Tooltip("处于“DebugLogTime（调试日志时间）”模式时消息的颜色 ")]
		[MMCondition("LogComment", true)]
		public Color DebugColor = Color.gray;
        
		/// <summary>
		/// 在开始播放时，如果有需要，我们会将消息输出到控制台。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || !LogComment)
			{
				return;
			}
            
			MMDebug.DebugLogInfo(Comment);
		}
	}
}
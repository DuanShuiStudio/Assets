using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you output a message to the console, using a custom MM debug method, or Log, Assertion, Error or Warning logs.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将使你能够使用自定义的MM调试方法，或者通过日志（Log）、断言（Assertion）、错误（Error）或警告（Warning）日志，将一条消息输出到控制台。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Debug/Log")]
	public class MMF_DebugLog : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 此反馈的持续时间为0。
		public override float FeedbackDuration { get { return 0f; } }

		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.DebugColor; } }
		#endif
        
		/// 可能的调试模式
		public enum DebugLogModes { DebugLogTime, Log, Assertion, Error, Warning }

		[MMFInspectorGroup("Debug", true, 17)]
		/// the selected debug mode
		[Tooltip("所选的调试模式")]
		public DebugLogModes DebugLogMode = DebugLogModes.DebugLogTime;

		/// the message to display 
		[Tooltip("要显示的消息")]
		[TextArea] 
		public string DebugMessage = "YOUR DEBUG MESSAGE GOES HERE";
		/// the color of the message when in DebugLogTime mode
		[Tooltip("处于调试日志时间模式时消息的颜色")]
		[MMFEnumCondition("DebugLogMode", (int) DebugLogModes.DebugLogTime)]
		public Color DebugColor = Color.cyan;
		/// whether or not to display the frame count when in DebugLogTime mode
		[Tooltip("在调试日志时间模式下是否显示帧数 ")]
		[MMFEnumCondition("DebugLogMode", (int) DebugLogModes.DebugLogTime)]
		public bool DisplayFrameCount = true;

		/// <summary>
		/// 在播放时，我们会使用所选模式将消息输出到控制台。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			switch (DebugLogMode)
			{
				case DebugLogModes.Assertion:
					Debug.LogAssertion(DebugMessage);
					break;
				case DebugLogModes.Log:
					Debug.Log(DebugMessage);
					break;
				case DebugLogModes.Error:
					Debug.LogError(DebugMessage);
					break;
				case DebugLogModes.Warning:
					Debug.LogWarning(DebugMessage);
					break;
				case DebugLogModes.DebugLogTime:
					string color = "#" + ColorUtility.ToHtmlStringRGB(DebugColor);
					MMDebug.DebugLogTime(DebugMessage, color, 3, DisplayFrameCount);
					break;
			}
		}
	}
}
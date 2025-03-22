using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will trigger a MMGameEvent of the specified name when played
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈在播放时将触发一个指定名称的MM游戏事件。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Events/MMGameEvent")]
	public class MMF_MMGameEvent : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.EventsColor; } }
		public override bool EvaluateRequiresSetup() { return (MMGameEventName == ""); }
		public override string RequiredTargetText { get { return MMGameEventName;  } }
		public override string RequiresSetupText { get { return "此反馈要求你在下方指定一个MM游戏事件名称。"; } }
		#endif

		[MMFInspectorGroup("MMGameEvent", true, 57, true)]
		public string MMGameEventName;
		
		[MMFInspectorGroup("Optional Payload", true, 58, true)]
		public int IntParameter;
		public Vector2 Vector2Parameter;
		public Vector3 Vector3Parameter;
		public bool BoolParameter;
		public string StringParameter;

		/// <summary>
		/// 在播放时，我们会更改雾效的相关数值。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMGameEvent.Trigger(MMGameEventName, IntParameter, Vector2Parameter, Vector3Parameter, BoolParameter, StringParameter);
		}
	}
}
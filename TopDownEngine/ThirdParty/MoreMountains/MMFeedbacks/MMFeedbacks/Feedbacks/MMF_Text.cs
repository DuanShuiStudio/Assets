using UnityEngine;
using System.Collections;
#if MM_UI
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the contents of a target Text over time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这种反馈机制使你能够随着时间的推移控制目标文本的内容")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("UI/Text")]
	public class MMF_Text : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
		public enum ColorModes { Instant, Gradient, Interpolate }

        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetText == null); }
		public override string RequiredTargetText { get { return TargetText != null ? TargetText.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈功能需要设置一个目标文本才能正常工作。你可以在下方设置一个目标文本。"; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetText = FindAutomatedTarget<Text>();

		[MMFInspectorGroup("Text", true, 76, true)]
		/// the Text component to control
		[Tooltip(" 要控制的文本组件。")]
		public Text TargetText;
		/// the new text to replace the old one with
		[Tooltip("用于替换旧文本的新文本。")]
		[TextArea]
		public string NewText = "Hello World";

		protected string _initialText;

        /// <summary>
        /// 在播放时，我们会更改目标文本网格（TextMeshPro，简称 TMP）文本组件的文本内容。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetText == null)
			{
				return;
			}

			_initialText = TargetText.text;
			TargetText.text = NewText;
		}

        /// <summary>
        /// 在恢复时，我们将对象放回其初始位置。
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			TargetText.text = _initialText;
		}
	}
}
#endif
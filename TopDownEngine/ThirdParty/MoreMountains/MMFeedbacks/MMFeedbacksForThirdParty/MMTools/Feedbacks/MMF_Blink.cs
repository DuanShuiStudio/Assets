using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈将触发一个MMBlink对象，使你能够让某些东西闪烁。 
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈可让你触发一个MMBlink对象进行闪烁。 ")]
	[FeedbackPath("Renderer/MMBlink")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	public class MMF_Blink : MMF_Feedback
	{
		/// 设置此反馈的检查器颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get => MMFeedbacksInspectorColors.RendererColor; }
		public override bool HasCustomInspectors { get { return true; } }
		public override bool EvaluateRequiresSetup() => (TargetBlink == null);
		public override string RequiredTargetText => TargetBlink != null ? TargetBlink.name : "";
		public override string RequiresSetupText => "此反馈要求必须设置一个“目标闪烁（TargetBlink）”才能正常工作。你可以在下方设置一个。 ";
		#endif
		public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetBlink = FindAutomatedTarget<MMBlink>();
        
		/// 一个静态布尔值，用于一次性禁用所有此类反馈。 
		public static bool FeedbackTypeAuthorized = true;

		/// 此反馈的可能模式，这些模式对应于 MMBlink 的公共方法。 
		public enum BlinkModes { Toggle, Start, Stop }
        
		[MMFInspectorGroup("Blink", true, 61, true)]
		/// the target object to blink
		[Tooltip("要闪烁的目标对象")]
		public MMBlink TargetBlink;
		/// an optional list of extra target objects to blink
		[Tooltip("一个可选的额外要闪烁的目标对象列表")]
		public List<MMBlink> ExtraTargetBlinks;
		/// the selected mode for this feedback
		[Tooltip("此反馈所选择的模式")]
		public BlinkModes BlinkMode = BlinkModes.Toggle;
		/// the duration of the blink. You can set it manually, or you can press the GrabDurationFromBlink button to automatically compute it. For performance reasons, this isn't updated unless you press the button, make sure you do so if you change the blink's duration.
		[Tooltip("眨眼的持续时间。你可以手动设置它，也可以按下“从眨眼获取持续时间（GrabDurationFromBlink）”按钮来自动计算它。出于性能方面的考虑，除非你按下该按钮，否则此设置不会更新。如果你更改了眨眼的持续时间，请务必按下该按钮。 ")]
		public float Duration;
		public MMF_Button GrabDurationFromBlinkButton;

		/// <summary>
		/// 初始化我们的持续时间按钮。
		/// </summary>
		public override void InitializeCustomAttributes()
		{
			GrabDurationFromBlinkButton = new MMF_Button("Grab Duration From Blink Component", GrabDurationFromBlink);
		}

		/// <summary>
		/// 在自定义播放模式下，我们会触发我们的MMBlink对象。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized || (TargetBlink == null))
			{
				return;
			}
			HandleBlink(TargetBlink);
			foreach (MMBlink blink in ExtraTargetBlinks)
			{
				HandleBlink(blink);
			}
		}

		/// <summary>
		/// 切换、启动或停止目标上的闪烁效果。 
		/// </summary>
		/// <param name="target"></param>
		protected virtual void HandleBlink(MMBlink target)
		{
			target.TimescaleMode = ComputedTimescaleMode;
			switch (BlinkMode)
			{
				case BlinkModes.Toggle:
					target.ToggleBlinking();
					break;
				case BlinkModes.Start:
					target.StartBlinking();
					break;
				case BlinkModes.Stop:
					target.StopBlinking();
					break;
			}
		}
		
		/// <summary>
		/// 在恢复操作时，我们恢复到初始状态。 
		/// </summary>
		protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			TargetBlink.StopBlinking();
			foreach (MMBlink blink in ExtraTargetBlinks)
			{
				blink.StopBlinking();
			}
		}
		
		/// <summary>
		/// 如果已设置目标闪烁组件，则获取并存储该组件的持续时间。 
		/// </summary>
		public virtual void GrabDurationFromBlink()
		{
			if (TargetBlink != null)
			{
				Duration = TargetBlink.Duration;	
			}
		}
	}
}
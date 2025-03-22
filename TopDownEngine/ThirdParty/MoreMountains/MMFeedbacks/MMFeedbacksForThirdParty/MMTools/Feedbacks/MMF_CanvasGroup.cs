using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// 此反馈可让你随着时间推移控制画布组的不透明度。 
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这个反馈功能可让你随时间推移控制一个画布组的透明度。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("UI/CanvasGroup")]
	public class MMF_CanvasGroup : MMF_FeedbackBase
	{
		/// 为此反馈设置检查器颜色。
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetCanvasGroup == null); }
		public override string RequiredTargetText { get { return TargetCanvasGroup != null ? TargetCanvasGroup.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈要求设置一个目标画布组，以便能够正常工作。你可以在下方设置一个。 "; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		public override bool CanForceInitialValue => true;
		protected override void AutomateTargetAcquisition() => TargetCanvasGroup = FindAutomatedTarget<CanvasGroup>();

		[MMFInspectorGroup("Canvas Group", true, 12, true)]
		/// the receiver to write the level to
		[Tooltip("用于写入层的接收方")]
		public CanvasGroup TargetCanvasGroup;
        
		/// the curve to tween the opacity on
		[Tooltip("用于对不透明度进行补间（动画过渡）的曲线 ")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public MMTweenType AlphaCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.3f, 1f), new Keyframe(1, 0)));
		/// the value to remap the opacity curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapZero = 0f;
		/// the value to remap the opacity curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.OverTime)]
		public float RemapOne = 1f;
		/// the value to move the opacity to in instant mode
		[Tooltip("在即时模式下将不透明度调整到的值")]
		[MMFEnumCondition("Mode", (int)MMFeedbackBase.Modes.Instant)]
		public float InstantAlpha;

		public override void OnAddFeedback()
		{
			base.OnAddFeedback();
			RelativeValues = false;
		}
        
		protected override void FillTargets()
		{
			if (TargetCanvasGroup == null)
			{
				return;
			}

			MMF_FeedbackBaseTarget target = new MMF_FeedbackBaseTarget();
			MMPropertyReceiver receiver = new MMPropertyReceiver();
			receiver.TargetObject = TargetCanvasGroup.gameObject;
			receiver.TargetComponent = TargetCanvasGroup;
			receiver.TargetPropertyName = "alpha";
			receiver.RelativeValue = RelativeValues;
			target.Target = receiver;
			target.LevelCurve = AlphaCurve;
			target.RemapLevelZero = RemapZero;
			target.RemapLevelOne = RemapOne;
			target.InstantLevel = InstantAlpha;

			_targets.Add(target);
		}

	}
}
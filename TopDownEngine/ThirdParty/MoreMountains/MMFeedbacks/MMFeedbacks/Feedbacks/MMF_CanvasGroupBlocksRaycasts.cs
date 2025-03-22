using System.Collections;
using System.Collections.Generic;
using UnityEngine;using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you turn the BlocksRaycast parameter of a target CanvasGroup on or off on play
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将让你在播放时打开或关闭目标 CanvasGroup 的 BlocksRaycast 参数")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("UI/CanvasGroup BlocksRaycasts")]
	public class MMF_CanvasGroupBlocksRaycasts : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetCanvasGroup == null); }
		public override string RequiredTargetText { get { return TargetCanvasGroup != null ? TargetCanvasGroup.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要设置一个 TargetCanvasGroup 才能正常工作。你可以在下面设置一个"; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetCanvasGroup = FindAutomatedTarget<CanvasGroup>();
        
		[MMFInspectorGroup("Block Raycasts", true, 54, true)]
		/// the target canvas group we want to control the BlocksRaycasts parameter on 
		[Tooltip("我们想要控制其 BlocksRaycasts 参数的目标画布组")]
		public CanvasGroup TargetCanvasGroup;
		/// if this is true, on play, the target canvas group will block raycasts, if false it won't
		[Tooltip("如果此值为真，在播放时，目标画布组将阻挡光线投射；如果为假，则不会")]
		public bool ShouldBlockRaycasts = true;

		protected bool _initialState;

        /// <summary>
        /// 在播放时打开或关闭光线投射阻挡
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetCanvasGroup == null)
			{
				return;
			}

			_initialState = TargetCanvasGroup.blocksRaycasts;
			TargetCanvasGroup.blocksRaycasts = ShouldBlockRaycasts;
		}

        /// <summary>
        /// 在恢复时，我们恢复初始状态
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			TargetCanvasGroup.blocksRaycasts = _initialState;
		}
	}
}
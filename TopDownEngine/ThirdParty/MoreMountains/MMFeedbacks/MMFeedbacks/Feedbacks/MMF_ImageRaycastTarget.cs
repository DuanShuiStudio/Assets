using UnityEngine;
#if MM_UI
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you control the RaycastTarget parameter of a target image, turning it on or off on play
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将让您控制目标图像的“光线投射目标”参数，在播放时开启或关闭它")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("UI/Image RaycastTarget")]
	public class MMF_ImageRaycastTarget : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类型的反馈。
        public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetImage == null); }
		public override string RequiredTargetText { get { return TargetImage != null ? TargetImage.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈要求设置一个TargetImage才能正常工作。您可以在下面设置一个"; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetImage = FindAutomatedTarget<Image>();
        
		[MMFInspectorGroup("Image", true, 12, true)]
		/// the target Image we want to control the RaycastTarget parameter on
		[Tooltip("我们想要控制其“光线投射目标”参数的目标图像")]
		public Image TargetImage;
		/// if this is true, when played, the target image will become a raycast target
		[Tooltip("如果为真，当播放时，目标图像将成为一个光线投射目标")]
		public bool ShouldBeRaycastTarget = true;

		protected bool _initialState;

        /// <summary>
        /// 在播放时，我们开启或关闭光线投射目标
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetImage == null)
			{
				return;
			}

			_initialState = TargetImage.raycastTarget;
			TargetImage.raycastTarget = NormalPlayDirection ? ShouldBeRaycastTarget : !ShouldBeRaycastTarget;
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
			TargetImage.raycastTarget = _initialState;
		}
	}
}
#endif
#if MM_UI
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the material on a target UI Image
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将让您更改目标UI图像的材质")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Renderer/Image Material")]
	public class MMF_ImageMaterial : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类型的反馈
        public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetImage == null); }
		public override string RequiredTargetText { get { return TargetImage != null ? TargetImage.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetImage be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetImage = FindAutomatedTarget<Image>();
        
		[MMFInspectorGroup("Image", true, 12, true)]
		/// the target Image we want to change the material on
		[Tooltip("我们想要更改其材质的目标图像")]
		public Image TargetImage;
		
		/// the new material to apply to the target image
		[Tooltip("要应用于目标图像的新材质")]
		public Material NewMaterial;

		protected Material _initialMaterial;

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

			_initialMaterial = TargetImage.material;
			TargetImage.material = NewMaterial;
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
			TargetImage.material = _initialMaterial;
		}
	}
}
#endif
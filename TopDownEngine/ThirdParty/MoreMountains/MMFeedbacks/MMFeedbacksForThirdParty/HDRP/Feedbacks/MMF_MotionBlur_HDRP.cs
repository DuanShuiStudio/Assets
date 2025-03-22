using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Scripting.APIUpdating;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control HDRP motion blur intensity over time.
	/// It requires you have in your scene an object with a Volume 
	/// with MotionBlur active, and a MMMotionBlurShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	#if MM_HDRP
	[FeedbackPath("PostProcess/Motion Blur HDRP")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.HDRP")]
	[FeedbackHelp("这种反馈能让你随着时间的推移控制运动模糊的强度。  " +
                  "它要求你的场景中存在一个带有体积（Volume）组件的对象" +
                  "在运动模糊处于启用状态，并且有一个 MMMotionBlurShaker_HDRP 组件的情况下。 ")]
	public class MMF_MotionBlur_HDRP : MMF_Feedback
	{
        /// 一个用于一次性禁用所有此类反馈的静态布尔值。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈在检查器中的颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
#endif

        /// 这种反馈的持续时间就是抖动的持续时间。
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Motion Blur", true, 24)]
		/// the duration of the shake, in seconds
		[Tooltip("抖动的持续时间，以秒为单位。")]
		public float Duration = 0.2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("抖动之后是否要重置抖动器的值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("抖动后是否要重置目标的值")]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("Intensity", true, 25)]
		/// the curve to animate the intensity on
		[Tooltip("用于对强度进行动画设置的曲线。 ")]
		public AnimationCurve Intensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to which to remap the curve's zero to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值 ")]
		public float RemapIntensityZero = 0f;
		/// the value to which to remap the curve's one to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值 ")]
		public float RemapIntensityOne = 1000f;
		/// whether or not to add to the initial intensity
		[Tooltip("是否要在初始强度的基础上增加数值 ")]
		public bool RelativeIntensity = false;

        /// <summary>
        ///	触发运动模糊抖动效果。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			MMMotionBlurShakeEvent_HDRP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, intensityMultiplier,
				ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
		}

        /// <summary>
        /// 当停止时，我们停止我们的过渡操作。 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			base.CustomStopFeedback(position, feedbacksIntensity);
			MMMotionBlurShakeEvent_HDRP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne,
				RelativeIntensity, channelData:ChannelData, stop: true);
		}

        /// <summary>
        /// 在恢复时，我们将物体放回其初始位置。 
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			MMMotionBlurShakeEvent_HDRP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne,
				RelativeIntensity, channelData:ChannelData, restore: true);
		}

        /// <summary>
        /// 自动设置后期处理配置文件和抖动效果器。 
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			#if MM_HDRP && UNITY_EDITOR
			MMHDRPHelpers.GetOrCreateVolume<MotionBlur, MMMotionBlurShaker_HDRP>(Owner, "MotionBlur");
			#endif
		}
	}
}
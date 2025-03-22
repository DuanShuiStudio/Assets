using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Scripting.APIUpdating;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control bloom intensity and threshold over time. It requires you have in your scene an object with a Volume with Bloom active, and a MMBloomShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈机制能让你随时间控制泛光（bloom）的强度和阈值。" +
                  "它要求你的场景中存在一个带有已启用泛光效果的 Volume 对象，以及一个 MMBloomShaker_HDRP 组件。")]
	#if MM_HDRP
	[FeedbackPath("PostProcess/Bloom HDRP")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.HDRP")]
	public class MMF_Bloom_HDRP : MMF_Feedback
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
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); }  set { ShakeDuration = value;  } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Bloom", true, 3)]
		/// the duration of the feedback, in seconds
		[Tooltip("反馈的持续时间，以秒为单位。 ")]
		public float ShakeDuration = 0.2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("是否在抖动之后重置抖动器的值 ")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("抖动之后是否重置目标的值。")]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to add to the initial intensity
		[Tooltip("是否要在初始强度的基础上进行增加。 ")]
		public bool RelativeValues = true;

		[MMFInspectorGroup("Intensity", true, 4)]
		/// the curve to animate the intensity on
		[Tooltip("用于对强度进行动画处理的曲线。 ")]
		public AnimationCurve ShakeIntensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		public float RemapIntensityZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		public float RemapIntensityOne = 1f;

		[MMFInspectorGroup("Threshold", true, 5)]
		/// the curve to animate the threshold on
		[Tooltip("用于对强度值进行动画处理的曲线")]
		public AnimationCurve ShakeThreshold = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		public float RemapThresholdZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		public float RemapThresholdOne = 0f;

        /// <summary>
        /// 触发一次泛光抖动。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="attenuation"></param>
        protected override void CustomPlayFeedback(Vector3 position, float attenuation = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			attenuation = ComputeIntensity(attenuation, position);
            
			MMBloomShakeEvent_HDRP.Trigger(ShakeIntensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, ShakeThreshold, RemapThresholdZero, RemapThresholdOne,
				RelativeValues, attenuation, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
            
		}

        /// <summary>
        /// 在停止时，我们停止我们的过渡效果。 
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
            
			MMBloomShakeEvent_HDRP.Trigger(ShakeIntensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, ShakeThreshold, RemapThresholdZero, RemapThresholdOne,
				RelativeValues, channelData:ChannelData, stop:true);
		}

        /// <summary>
        /// 在恢复时，我们将我们的物体放回其初始位置。 
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			MMBloomShakeEvent_HDRP.Trigger(ShakeIntensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, ShakeThreshold, RemapThresholdZero, RemapThresholdOne,
				RelativeValues, channelData:ChannelData, restore:true);
		}

        /// <summary>
        /// 自动设置后期处理配置文件和抖动器。 
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			#if MM_HDRP && UNITY_EDITOR
			MMHDRPHelpers.GetOrCreateVolume<Bloom, MMBloomShaker_HDRP>(Owner, "Bloom");
			#endif
		}
	}
}
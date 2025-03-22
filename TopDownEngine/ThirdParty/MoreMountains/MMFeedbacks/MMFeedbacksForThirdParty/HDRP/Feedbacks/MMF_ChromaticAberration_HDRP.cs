using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Scripting.APIUpdating;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control chromatic aberration intensity over time. It requires you have in your scene an object with a Volume 
	/// with HDRP Chromatic Aberration active, and a MMChromaticAberrationShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	#if MM_HDRP
	[FeedbackPath("PostProcess/Chromatic Aberration HDRP")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.HDRP")]
	[FeedbackHelp("这种反馈能让你随着时间推移控制色差强度。它要求你的场景中有一个带有“体积（Volume）”的对象" +
                  "已启用色差功能，并且带有一个 MM 色差抖动器 HDRP 组件。 ")]
	public class MMF_ChromaticAberration_HDRP : MMF_Feedback
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
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value;  } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Chromatic Aberration", true, 10)]
		/// the duration of the shake, in seconds
		[Tooltip("抖动的持续时间，以秒为单位。")]
		public float Duration = 0.2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("抖动之后是否重置抖动器的值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("抖动之后是否重置目标的数值。")]
		public bool ResetTargetValuesAfterShake = true;
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(0f, 1f)]
		public float RemapIntensityZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(0f, 1f)]
		public float RemapIntensityOne = 1f;

		[MMFInspectorGroup("Intensity", true, 11)]
		/// the curve to animate the intensity on
		[Tooltip("用于对强度进行动画处理的曲线。 ")]
		public AnimationCurve Intensity = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the multiplier to apply to the intensity curve
		[Tooltip("应用于强度曲线的乘数。")]
		[Range(0f, 1f)]
		public float Amplitude = 1.0f;
		/// whether or not to add to the initial intensity
		[Tooltip("是否要在初始强度的基础上增加（强度） ")]
		public bool RelativeIntensity = false;

        /// <summary>
        /// 触发一次色差抖动。
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
			MMChromaticAberrationShakeEvent_HDRP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, intensityMultiplier,
				ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
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
            
			MMChromaticAberrationShakeEvent_HDRP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, channelData:ChannelData, stop:true);
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
			
			MMChromaticAberrationShakeEvent_HDRP.Trigger(Intensity, FeedbackDuration, RemapIntensityZero, RemapIntensityOne, RelativeIntensity, channelData:ChannelData, restore:true);
		}

        /// <summary>
        /// 自动设置后期处理配置文件和抖动效果器。 
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			#if MM_HDRP && UNITY_EDITOR
			MMHDRPHelpers.GetOrCreateVolume<ChromaticAberration, MMChromaticAberrationShaker_HDRP>(Owner, "ChromaticAberration");
			#endif
		}
	}
}
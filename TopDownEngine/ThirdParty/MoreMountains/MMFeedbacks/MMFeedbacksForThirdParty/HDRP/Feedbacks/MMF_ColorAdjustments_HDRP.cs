using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Scripting.APIUpdating;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control color adjustments' post exposure, hue shift, saturation and contrast over time.
	/// It requires you have in your scene an object with a Volume 
	/// with Color Adjustments active, and a MMColorAdjustmentsShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	#if MM_HDRP
	[FeedbackPath("PostProcess/Color Adjustments HDRP")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.HDRP")]
	[FeedbackHelp("这种反馈使您能够随着时间的推移控制颜色调整的后期曝光、色调偏移、饱和度和对比度。  " +
                  "它要求你的场景中要有一个带有体积（Volume）的对象。  " +
                  "已激活颜色调整功能，并且带有一个 MM 颜色调整抖动器 HDRP 组件。 ")]
	public class MMF_ColorAdjustments_HDRP : MMF_Feedback
	{
        /// 一个用于一次性禁用所有此类反馈的静态布尔值。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈在检查器中的颜色     
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.PostProcessColor; } }
		public override bool HasCustomInspectors => true;
		public override bool HasAutomaticShakerSetup => true;
#endif

        /// 这种反馈的持续时间就是抖动的持续时间。 
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); } set { ShakeDuration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Color Grading", true, 16)]
		/// the duration of the shake, in seconds
		[Tooltip("抖动的持续时间，以秒为单位。")]
		public float ShakeDuration = 1f;
		/// whether or not to add to the initial intensity
		[Tooltip("是否要在初始强度上进行增加 ")]
		public bool RelativeIntensity = true;
		/// whether or not to reset shaker values after shake
		[Tooltip("抖动后是否重置抖动器的值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("抖动后是否重置目标的值")]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("Post Exposure", true, 15)]
		/// the curve used to animate the focus distance value on
		[Tooltip("用于对焦点距离值进行动画处理的曲线。 ")]
		public AnimationCurve ShakePostExposure = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		public float RemapPostExposureZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		public float RemapPostExposureOne = 1f;

		[MMFInspectorGroup("Hue Shift", true, 14)]
		/// the curve used to animate the aperture value on
		[Tooltip("用于对光圈值进行动画处理的曲线。 ")]
		public AnimationCurve ShakeHueShift = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-180f, 180f)]
		public float RemapHueShiftZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-180f, 180f)]
		public float RemapHueShiftOne = 180f;

		[MMFInspectorGroup("Saturation", true, 13)]
		/// the curve used to animate the focal length value on
		[Tooltip("用于对焦距值进行动画处理的曲线。 ")]
		public AnimationCurve ShakeSaturation = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapSaturationZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapSaturationOne = 100f;

		[MMFInspectorGroup("Contrast", true, 12)]
		/// the curve used to animate the focal length value on
		[Tooltip("用于对焦距值进行动画处理的曲线。 ")]
		public AnimationCurve ShakeContrast = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapContrastZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapContrastOne = 100f;
        
		[Header("Color Filter颜色滤镜")] 
		/// the selected color filter mode :
		/// None : nothing will happen,
		/// gradient : evaluates the color over time on that gradient, from left to right,
		/// interpolate : lerps from the current color to the destination one 
		[Tooltip("所选的颜色滤镜模式：" +
                 "None无：不会发生任何情况," +
                 "gradient渐变：从左到右随着时间推移在该渐变上评估颜色。 ," +
                 "interpolate插值：从当前颜色线性插值到目标颜色 ")]
		public MMColorAdjustmentsShaker_HDRP.ColorFilterModes ColorFilterMode = MMColorAdjustmentsShaker_HDRP.ColorFilterModes.None;
		/// the gradient to use to animate the color filter over time
		[Tooltip("用于随着时间推移对颜色滤镜进行动画处理的渐变效果（所使用的渐变） ")]
		[MMFEnumCondition("ColorFilterMode", (int)MMColorAdjustmentsShaker_HDRP.ColorFilterModes.Gradient)]
		[GradientUsage(true)]
		public Gradient ColorFilterGradient;
		/// the destination color when in interpolate mode
		[Tooltip("处于插值模式时的目标颜色")]
		[MMFEnumCondition("ColorFilterMode", (int) MMColorAdjustmentsShaker_HDRP.ColorFilterModes.Interpolate)]
		public Color ColorFilterDestination = Color.yellow;
		/// the curve to use when interpolating towards the destination color
		[Tooltip("在向目标颜色进行插值时所使用的曲线 ")]
		[MMFEnumCondition("ColorFilterMode", (int) MMColorAdjustmentsShaker_HDRP.ColorFilterModes.Interpolate)]
		public AnimationCurve ColorFilterCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));


        /// <summary>
        /// 触发颜色调整抖动效果。
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
			MMColorAdjustmentsShakeEvent_HDRP.Trigger(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne,
				ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne,
				ShakeSaturation, RemapSaturationZero, RemapSaturationOne,
				ShakeContrast, RemapContrastZero, RemapContrastOne,
				ColorFilterMode, ColorFilterGradient, ColorFilterDestination, ColorFilterCurve,
				FeedbackDuration,
				RelativeIntensity, intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
            
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
            
			MMColorAdjustmentsShakeEvent_HDRP.Trigger(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne,
				ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne,
				ShakeSaturation, RemapSaturationZero, RemapSaturationOne,
				ShakeContrast, RemapContrastZero, RemapContrastOne,
				ColorFilterMode, ColorFilterGradient, ColorFilterDestination, ColorFilterCurve,
				FeedbackDuration,
				RelativeIntensity, channelData:ChannelData, stop:true);
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
			
			MMColorAdjustmentsShakeEvent_HDRP.Trigger(ShakePostExposure, RemapPostExposureZero, RemapPostExposureOne,
				ShakeHueShift, RemapHueShiftZero, RemapHueShiftOne,
				ShakeSaturation, RemapSaturationZero, RemapSaturationOne,
				ShakeContrast, RemapContrastZero, RemapContrastOne,
				ColorFilterMode, ColorFilterGradient, ColorFilterDestination, ColorFilterCurve,
				FeedbackDuration,
				RelativeIntensity, channelData:ChannelData, restore:true);
		}

        /// <summary>
        /// 自动设置后期处理配置文件和抖动器。 
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			#if MM_HDRP && UNITY_EDITOR
			MMHDRPHelpers.GetOrCreateVolume<ColorAdjustments, MMColorAdjustmentsShaker_HDRP>(Owner, "ColorAdjustments");
			#endif
		}
	}
}
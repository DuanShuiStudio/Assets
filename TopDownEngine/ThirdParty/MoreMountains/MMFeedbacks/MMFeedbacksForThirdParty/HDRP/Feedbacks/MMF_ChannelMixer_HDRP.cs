using UnityEngine;	
using MoreMountains.Feedbacks;	
using UnityEngine.Scripting.APIUpdating;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty	
{	
	/// <summary>	
	/// This feedback allows you to control channel mixer's red, green and blue over time.	
	/// It requires you have in your scene an object with a Volume 	
	/// with Channel Mixer active, and a MMChannelMixerShaker_HDRP component.	
	/// </summary>	
	[AddComponentMenu("")]	
	#if MM_HDRP
	[FeedbackPath("PostProcess/Channel Mixer HDRP")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.HDRP")]
	[FeedbackHelp("这种反馈能让你随着时间推移控制通道混合器的红色、绿色和蓝色参数。 " +
                  "它要求你的场景中有一个带有“体积（Volume）”的对象。 " +
                  "该对象需启用了通道混合器功能，并且带有一个 MM 通道混合器 HDRP 组件。 ")]
	public class MMF_ChannelMixer_HDRP : MMF_Feedback	
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
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); } set { ShakeDuration = value;  } }	
		public override bool HasChannel => true;
		public override bool HasRandomness => true;
        
		[MMFInspectorGroup("Color Grading", true, 10)]
		/// the duration of the shake, in seconds
		[Tooltip("抖动的持续时间，以秒为单位。")]
		public float ShakeDuration = 1f;
		/// whether or not to add to the initial intensity
		[Tooltip("是否要在初始强度上进行累加")]
		public bool RelativeIntensity = true;
		/// whether or not to reset shaker values after shake
		[Tooltip("抖动后是否重置抖动器的值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("抖动后是否重置目标的值")]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("Red", true, 13)]
		/// the curve used to animate the red value on
		[Tooltip("用于对红色值进行动画处理的曲线。 ")]
		public AnimationCurve ShakeRed = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapRedZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapRedOne = 200f;

		[MMFInspectorGroup("Green", true, 12)]
		/// the curve used to animate the green value on
		[Tooltip("用于对绿色值进行动画处理的曲线。")]
		public AnimationCurve ShakeGreen = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapGreenZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapGreenOne = 200f;

		[MMFInspectorGroup("Blue", true, 11)]
		/// the curve used to animate the blue value on
		[Tooltip("用于对蓝色值进行动画处理的曲线。")]
		public AnimationCurve ShakeBlue = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapBlueZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-200f, 200f)]
		public float RemapBlueOne = 200f;

        /// <summary>
        /// 触发一次颜色调整抖动。
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
			MMChannelMixerShakeEvent_HDRP.Trigger(ShakeRed, RemapRedZero, RemapRedOne,
				ShakeGreen, RemapGreenZero, RemapGreenOne,
				ShakeBlue, RemapBlueZero, RemapBlueOne,
				FeedbackDuration,
				RelativeIntensity, intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
		}

        /// <summary>
        /// 当停止时，我们停止我们的过渡过程。 
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
            
			MMChannelMixerShakeEvent_HDRP.Trigger(ShakeRed, RemapRedZero, RemapRedOne,
				ShakeGreen, RemapGreenZero, RemapGreenOne,
				ShakeBlue, RemapBlueZero, RemapBlueOne,
				FeedbackDuration,
				RelativeIntensity, channelData:ChannelData, stop:true);
		}

        /// <summary>
        /// 在恢复时，我们将我们的对象放回其初始位置。 
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			MMChannelMixerShakeEvent_HDRP.Trigger(ShakeRed, RemapRedZero, RemapRedOne,
				ShakeGreen, RemapGreenZero, RemapGreenOne,
				ShakeBlue, RemapBlueZero, RemapBlueOne,
				FeedbackDuration,
				RelativeIntensity, channelData:ChannelData, restore:true);
		}

        /// <summary>
        /// 自动设置后期处理配置文件和抖动器。 
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			#if MM_HDRP && UNITY_EDITOR
			MMHDRPHelpers.GetOrCreateVolume<ChannelMixer, MMChannelMixerShaker_HDRP>(Owner, "Channel Mixer");
			#endif
		}
	}	
}
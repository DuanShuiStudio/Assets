using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Scripting.APIUpdating;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control white balance temperature and tint over time. 
	/// It requires you have in your scene an object with a Volume with White Balance active, and a MMWhiteBalanceShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这种反馈能让你随着时间推移控制白平衡的色温与色调。  " +
                  "它要求你的场景中存在一个带有体积（Volume）组件的对象" +
                  "在白平衡处于激活状态，并且有一个 MMWhiteBalanceShaker_HDRP 组件的情况下。 ")]
	#if MM_HDRP
	[FeedbackPath("PostProcess/White Balance HDRP")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.HDRP")]
	public class MMF_WhiteBalance_HDRP : MMF_Feedback
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
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(ShakeDuration); } set { ShakeDuration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("White Balance", true, 32)]
		/// the duration of the shake, in seconds
		[Tooltip("抖动的持续时间，以秒为单位。")]
		public float ShakeDuration = 1f;
		/// whether or not to add to the initial value
		[Tooltip("是否要在初始值的基础上增加数值 ")]
		public bool RelativeValues = true;
		/// whether or not to reset shaker values after shake
		[Tooltip("抖动之后是否重置抖动器的值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("抖动后是否要重置目标的值")]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("Temperature", true, 33)]
		/// the curve used to animate the temperature value on
		[Tooltip("用于对色温值进行动画处理的曲线 ")]
		public AnimationCurve ShakeTemperature = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapTemperatureZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapTemperatureOne = 100f;

		[MMFInspectorGroup("Tint", true, 34)]
		/// the curve used to animate the tint value on
		[Tooltip("用于对色调值进行动画处理的曲线")]
		public AnimationCurve ShakeTint = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapTintZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(-100f, 100f)]
		public float RemapTintOne = 100f;

        /// <summary>
        /// 触发一次白平衡抖动效果。
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
			MMWhiteBalanceShakeEvent_HDRP.Trigger(ShakeTemperature, FeedbackDuration, RemapTemperatureZero, RemapTemperatureOne,
				ShakeTint, RemapTintZero, RemapTintOne, RelativeValues, intensityMultiplier,
				ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
		}

        /// <summary>
        /// 当停止时，我们就停止我们的过渡（过程）。 
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
			MMWhiteBalanceShakeEvent_HDRP.Trigger(ShakeTemperature, FeedbackDuration, RemapTemperatureZero, RemapTemperatureOne,
				ShakeTint, RemapTintZero, RemapTintOne, RelativeValues, channelData:ChannelData, stop:true);
		}

        /// <summary>
        /// 在恢复时，我们将我们的对象恢复到其初始位置。 
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			MMWhiteBalanceShakeEvent_HDRP.Trigger(ShakeTemperature, FeedbackDuration, RemapTemperatureZero, RemapTemperatureOne,
				ShakeTint, RemapTintZero, RemapTintOne, RelativeValues, channelData:ChannelData, restore:true);
		}

        /// <summary>
        /// 自动设置后期处理配置文件和抖动效果器。 
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			#if MM_HDRP && UNITY_EDITOR
			MMHDRPHelpers.GetOrCreateVolume<WhiteBalance, MMWhiteBalanceShaker_HDRP>(Owner, "WhiteBalance");
			#endif
		}
	}
}
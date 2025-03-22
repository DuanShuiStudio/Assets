using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Scripting.APIUpdating;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control Panini Projection distance and crop to fit over time. 
	/// It requires you have in your scene an object with a Volume with Bloom active, and a MMPaniniProjectionShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("这种反馈可让你随着时间推移控制帕尼尼投影（Panini Projection）的距离，并进行裁剪以适配。  " +
                  "它要求你的场景中存在一个带有“体积”（Volume）组件的对象" +
                  "在帕尼尼投影处于启用状态，并且有一个 MMPaniniProjectionShaker_HDRP 组件的情况下。 ")]
	#if MM_HDRP
	[FeedbackPath("PostProcess/Panini Projection HDRP")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.HDRP")]
	public class MMF_PaniniProjection_HDRP : MMF_Feedback
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

		[MMFInspectorGroup("Panini Projection", true, 26)]
		/// the duration of the shake, in seconds
		[Tooltip("抖动的持续时间，以秒为单位。")]
		public float Duration = 0.2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("抖动后是否重置抖动器的值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("抖动后是否要重置目标的值")]
		public bool ResetTargetValuesAfterShake = true;

		[MMFInspectorGroup("Distance", true, 27)]
		/// whether or not to add to the initial value
		[Tooltip("是否要在初始值的基础上增加数值 ")]
		public bool RelativeDistance = false;
		/// the curve used to animate the distance value on
		[Tooltip("用于对距离值进行动画设置的曲线 ")]
		public AnimationCurve ShakeDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[Range(0f, 1f)]
		public float RemapDistanceZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[Range(0f, 1f)]
		public float RemapDistanceOne = 1f;

        /// <summary>
        /// 触发一次泛光抖动效果。 
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
			MMPaniniProjectionShakeEvent_HDRP.Trigger(ShakeDistance, FeedbackDuration, RemapDistanceZero, RemapDistanceOne, RelativeDistance, intensityMultiplier, ChannelData, 
				ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
		}

        /// <summary>
        /// 停止时，我们就停止过渡效果。 
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
			MMPaniniProjectionShakeEvent_HDRP.Trigger(ShakeDistance, FeedbackDuration, RemapDistanceZero, RemapDistanceOne, RelativeDistance, channelData:ChannelData, stop:true);
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
			
			MMPaniniProjectionShakeEvent_HDRP.Trigger(ShakeDistance, FeedbackDuration, RemapDistanceZero, RemapDistanceOne, RelativeDistance, channelData:ChannelData, restore:true);
		}

        /// <summary>
        /// 自动设置后期处理配置文件和抖动器。 
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			#if MM_HDRP && UNITY_EDITOR
			MMHDRPHelpers.GetOrCreateVolume<PaniniProjection, MMPaniniProjectionShaker_HDRP>(Owner, "PaniniProjection");
			#endif
		}
	}
}
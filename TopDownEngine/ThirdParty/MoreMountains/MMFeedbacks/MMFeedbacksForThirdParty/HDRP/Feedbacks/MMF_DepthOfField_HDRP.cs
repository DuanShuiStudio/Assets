using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.Scripting.APIUpdating;
#if MM_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif

namespace MoreMountains.FeedbacksForThirdParty
{
	/// <summary>
	/// This feedback allows you to control HDRP Depth of Field focus distance or near/far ranges over time.
	/// It requires you have in your scene an object with a Volume 
	/// with Depth of Field active, and a MMDepthOfFieldShaker_HDRP component.
	/// </summary>
	[AddComponentMenu("")]
	#if MM_HDRP
	[FeedbackPath("PostProcess/Depth of Field HDRP")]
	#endif
	[MovedFrom(false, null, "MoreMountains.Feedbacks.HDRP")]
	[FeedbackHelp("这种反馈使你能够随着时间的推移控制高动态范围渲染管线（HDRP）的景深对焦距离或近/远范围。 " +
                  "这要求你的场景中有一个带有“体积”（Volume）组件的对象" +
                  "在景深处于启用状态的情况下，并且要有一个“MMDepthOfFieldShaker_HDRP”组件。 ")]
	public class MMF_DepthOfField_HDRP : MMF_Feedback
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

		[MMFInspectorGroup("Depth of Field", true, 28)]
		/// the duration of the shake, in seconds
		[Tooltip("抖动的持续时间，以秒为单位。")]
		public float Duration = 0.2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("抖动之后是否重置抖动器的值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("抖动后是否重置目标的值")]
		public bool ResetTargetValuesAfterShake = true;
		
		[MMFInspectorGroup("Focus Distance", true, 53)]
		/// whether or not to animate the focus distance
		[Tooltip("是否对对焦距离进行动画处理")]
		public bool AnimateFocusDistance = true;
		/// the curve used to animate the focus distance value on
		[Tooltip("用于对对焦距离值进行动画处理的曲线。 ")]
		[MMFCondition("AnimateFocusDistance", true)]
		public AnimationCurve ShakeFocusDistance = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMFCondition("AnimateFocusDistance", true)]
		public float RemapFocusDistanceZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMFCondition("AnimateFocusDistance", true)]
		public float RemapFocusDistanceOne = 3f;
		
		
		[MMFInspectorGroup("Near Range", true, 52)]
		
		[Header("Near Range Start近景范围起始值")]
		/// whether or not to animate the near range start
		[Tooltip("是否对近景范围起始值进行动画处理")]
		public bool AnimateNearRangeStart = false;
		/// the curve used to animate the near range start on
		[Tooltip("用于对近景范围起始值进行动画处理的曲线。 ")]
		[MMFCondition("AnimateNearRangeStart", true)]
		public AnimationCurve ShakeNearRangeStart = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMFCondition("AnimateNearRangeStart", true)]
		public float RemapNearRangeStartZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMFCondition("AnimateNearRangeStart", true)]
		public float RemapNearRangeStartOne = 3f;
		
		[Header("Near Range End近景范围结束值")]
		/// whether or not to animate the near range end
		[Tooltip("是否对近景范围结束值进行动画处理")]
		public bool AnimateNearRangeEnd = false;
		/// the curve used to animate the near range end on
		[Tooltip("用于对近景范围结束值进行动画处理的曲线。 ")]
		[MMFCondition("AnimateNearRangeEnd", true)]
		public AnimationCurve ShakeNearRangeEnd = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMFCondition("AnimateNearRangeEnd", true)]
		public float RemapNearRangeEndZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMFCondition("AnimateNearRangeEnd", true)]
		public float RemapNearRangeEndOne = 3f;
		
		[MMFInspectorGroup("Far Range", true, 51)]
		
		[Header("Far Range Start远景范围起始值")]
		/// whether or not to animate the far range start
		[Tooltip("是否对远景范围起始值进行动画处理")]
		public bool AnimateFarRangeStart = false;
		/// the curve used to animate the far range start on
		[Tooltip("用于对远景范围起始值进行动画处理的曲线。 ")]
		[MMFCondition("AnimateFarRangeStart", true)]
		public AnimationCurve ShakeFarRangeStart = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMFCondition("AnimateFarRangeStart", true)]
		public float RemapFarRangeStartZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMFCondition("AnimateFarRangeStart", true)]
		public float RemapFarRangeStartOne = 3f;
		
		[Header("Far Range End远景范围结束值")]
		/// whether or not to animate the far range end
		[Tooltip("是否对远景范围结束值进行动画处理")]
		public bool AnimateFarRangeEnd = false;
		/// the curve used to animate the far range end on
		[Tooltip("用于对远景范围结束值进行动画处理的曲线。 ")]
		[MMFCondition("AnimateFarRangeEnd", true)]
		public AnimationCurve ShakeFarRangeEnd = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("用于将曲线的 0 值重新映射到的那个值")]
		[MMFCondition("AnimateFarRangeEnd", true)]
		public float RemapFarRangeEndZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("用于将曲线的 1 值重新映射到的那个值")]
		[MMFCondition("AnimateFarRangeEnd", true)]
		public float RemapFarRangeEndOne = 3f;

        /// <summary>
        /// 触发晕影抖动效果 
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
			MMDepthOfFieldShakeEvent_HDRP.Trigger(Duration, intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, 
				ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode, false, false, 
				AnimateFocusDistance, ShakeFocusDistance, RemapFocusDistanceZero, RemapFocusDistanceOne,
				AnimateNearRangeStart, ShakeNearRangeStart, RemapNearRangeStartZero, RemapNearRangeStartOne,
				AnimateNearRangeEnd, ShakeNearRangeEnd, RemapNearRangeEndZero, RemapNearRangeEndOne,
				AnimateFarRangeStart, ShakeFarRangeStart, RemapFarRangeStartZero, RemapFarRangeStartOne,
				AnimateFarRangeEnd, ShakeFarRangeEnd,RemapFarRangeEndZero,RemapFarRangeEndOne);
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
			MMDepthOfFieldShakeEvent_HDRP.Trigger(Duration, channelData: ChannelData, stop:true);
		}

        /// <summary>
        /// 在恢复时，我们将对象恢复到其初始位置。 
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			
			MMDepthOfFieldShakeEvent_HDRP.Trigger(Duration, channelData: ChannelData, restore:true);
		}

        /// <summary>
        /// 自动设置后期处理配置文件和抖动器。 
        /// </summary>
        public override void AutomaticShakerSetup()
		{
			#if MM_HDRP && UNITY_EDITOR
			MMHDRPHelpers.GetOrCreateVolume<DepthOfField, MMDepthOfFieldShaker_HDRP>(Owner, "DepthOfField");
			#endif
		}
	}
}
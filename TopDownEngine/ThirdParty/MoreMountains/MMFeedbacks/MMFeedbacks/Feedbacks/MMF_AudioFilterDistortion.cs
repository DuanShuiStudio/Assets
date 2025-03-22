using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 此反馈允许你控制失真滤波器的失真水平。你需要在过滤器上使用MMAudioFilterDistortionShaker
    /// </summary>
    [AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Audio/Audio Filter Distortion")]
	[FeedbackHelp("此反馈允许你随时间控制失真音频滤波器。你需要在过滤器上使用MMAudioFilterDistortionShaker")]
	public class MMF_AudioFilterDistortion : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类型的所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈在检查器中的颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText => RequiredChannelText;
#endif
        /// 返回反馈的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Distortion Filter", true, 28)]
		/// the duration of the shake, in seconds
		[Tooltip("摇动的持续时间（以秒为单位）")]
		public float Duration = 2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("是否在摇动后重置摇动器的值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("是否在摇动后重置目标的值")]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to add to the initial value
		[Tooltip("是否添加到初始值")]
		public bool RelativeDistortion = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于动画化强度值的曲线")]
		public AnimationCurve ShakeDistortion = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
		/// the value to remap the curve's 0 to
		[Tooltip("将曲线的0值重新映射到的值")]
		[Range(0f, 1f)]
		public float RemapDistortionZero = 0f;
		/// the value to remap the curve's 1 to
		[Tooltip("将曲线的1值重新映射到的值")]
		[Range(0f, 1f)]
		public float RemapDistortionOne = 1f;

        /// <summary>
        /// 触发相应的协程
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			float intensityMultiplier = ComputeIntensity(feedbacksIntensity, position);
			float remapZero = 0f;
			float remapOne = 0f;
            
			if (!Timing.ConstantIntensity)
			{
				remapZero = RemapDistortionZero * intensityMultiplier;
				remapOne = RemapDistortionOne * intensityMultiplier;
			}
            
			MMAudioFilterDistortionShakeEvent.Trigger(ShakeDistortion, FeedbackDuration, remapZero, remapOne, RelativeDistortion,
				intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
		}

        /// <summary>
        /// 在停止时，我们停止过渡
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1)
		{
			base.CustomStopFeedback(position, feedbacksIntensity);
            
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMAudioFilterDistortionShakeEvent.Trigger(ShakeDistortion, FeedbackDuration, 0f,0f, stop:true);
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
			MMAudioFilterDistortionShakeEvent.Trigger(ShakeDistortion, FeedbackDuration, 0f,0f, restore:true);
		}
	}
}
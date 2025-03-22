using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 这个反馈允许你控制混响滤波器的混响水平。你需要在滤波器上添加一个MMAudioFilterReverbShaker
    /// </summary>
    [AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Audio/Audio Filter Reverb")]
	[FeedbackHelp(
        "这个反馈允许你控制混响滤波器的混响水平。你需要在滤波器上添加一个MMAudioFilterReverbShaker")]
	public class MMF_AudioFilterReverb : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText => RequiredChannelText;
#endif
        /// 返回反馈的持续时间
        public override float FeedbackDuration { get { return ApplyTimeMultiplier(Duration); } set { Duration = value; } }
		public override bool HasChannel => true;
		public override bool HasRandomness => true;

		[MMFInspectorGroup("Reverb Filter", true, 28)]
		/// the duration of the shake, in seconds
		[Tooltip("震动的持续时间（以秒为单位）")]
		public float Duration = 2f;
		/// whether or not to reset shaker values after shake
		[Tooltip("是否在震动后重置震动器值")]
		public bool ResetShakerValuesAfterShake = true;
		/// whether or not to reset the target's values after shake
		[Tooltip("是否在震动后重置目标的值")]
		public bool ResetTargetValuesAfterShake = true;
		/// whether or not to add to the initial value
		[Tooltip("是否添加到初始值")]
		public bool RelativeReverb = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于动画化强度值的曲线")]
		public AnimationCurve ShakeReverb = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.5f, 1f), new Keyframe(1, 0f));
		/// the value to remap the curve's 0 to
		[Range(-10000f, 2000f)]
		[Tooltip("将曲线的0值重新映射到的值")]
		public float RemapReverbZero = -10000f;
		/// the value to remap the curve's 1 to
		[Range(-10000f, 2000f)]
		[Tooltip("将曲线的1值重新映射到的值")]
		public float RemapReverbOne = 2000f;

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
			MMAudioFilterReverbShakeEvent.Trigger(ShakeReverb, FeedbackDuration, RemapReverbZero, RemapReverbOne, RelativeReverb,
				intensityMultiplier, ChannelData, ResetShakerValuesAfterShake, ResetTargetValuesAfterShake, NormalPlayDirection, ComputedTimescaleMode);
		}

        /// <summary>
        /// 在停止时，我们停止过渡
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
            
			MMAudioFilterReverbShakeEvent.Trigger(ShakeReverb, FeedbackDuration, RemapReverbZero, RemapReverbOne, stop:true);
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
			MMAudioFilterReverbShakeEvent.Trigger(ShakeReverb, FeedbackDuration, RemapReverbZero, RemapReverbOne, restore:true);
		}
	}
}
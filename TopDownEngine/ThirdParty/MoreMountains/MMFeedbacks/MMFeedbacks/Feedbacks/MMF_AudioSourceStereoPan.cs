using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control the stereo pan of a target AudioSource over time.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Audio/AudioSource Stereo Pan")]
	[FeedbackHelp("这个反馈允许你随时间控制目标音频源的立体声声像")]
	public class MMF_AudioSourceStereoPan : MMF_Feedback
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

		[MMFInspectorGroup("AudioSource Stereo Pan", true, 48)]
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
		public bool RelativeStereoPan = false;
		/// the curve used to animate the intensity value on
		[Tooltip("用于动画化强度值的曲线")]
		public AnimationCurve ShakeStereoPan = new AnimationCurve(new Keyframe(0, 0f), new Keyframe(0.3f, 1f), new Keyframe(0.6f, -1f), new Keyframe(1, 0f));
		/// the value to remap the curve's 0 to
		[Range(-1f, 1f)]
		[Tooltip("将曲线的0值重新映射到的值")]
		public float RemapStereoPanZero = 0f;
		/// the value to remap the curve's 1 to
		[Range(-1f, 1f)]
		[Tooltip("将曲线的1值重新映射到的值")]
		public float RemapStereoPanOne = 1f;


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
			MMAudioSourceStereoPanShakeEvent.Trigger(ShakeStereoPan, FeedbackDuration, RemapStereoPanZero, RemapStereoPanOne, RelativeStereoPan,
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
			MMAudioSourceStereoPanShakeEvent.Trigger(ShakeStereoPan, FeedbackDuration, RemapStereoPanZero, RemapStereoPanOne, stop:true);
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
			MMAudioSourceStereoPanShakeEvent.Trigger(ShakeStereoPan, FeedbackDuration, RemapStereoPanZero, RemapStereoPanOne, restore:true);
		}
	}
}
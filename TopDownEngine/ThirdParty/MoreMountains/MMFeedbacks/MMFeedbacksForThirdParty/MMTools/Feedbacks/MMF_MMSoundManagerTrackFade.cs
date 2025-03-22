using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using MoreMountains.Tools;
using UnityEngine.Audio;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you fade all the sounds on a specific track at once. You will need a MMSoundManager in your scene for this to work.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Audio/MMSoundManager Track Fade")]
	[FeedbackHelp("此反馈可让你一次性使特定音轨上的所有声音逐渐淡入或淡出。要实现此功能，你的场景中需要有一个MM声音管理器。 ")]
	public class MMF_MMSoundManagerTrackFade : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get { return Track.ToString();  } }
		#endif

		/// 此反馈的时长就是淡入淡出的时长。 
		public override float FeedbackDuration { get { return FadeDuration; } }
        
		[MMFInspectorGroup("MMSoundManager Track Fade", true, 30)]
		/// the track to fade the volume on
		[Tooltip("要对其音量进行淡入淡出处理的音轨。 ")]
		public MMSoundManager.MMSoundManagerTracks Track;
		/// the duration of the fade, in seconds
		[Tooltip("淡入淡出的持续时间，单位为秒。")]
		public float FadeDuration = 1f;
		/// the volume to reach at the end of the fade
		[Tooltip("淡入淡出结束时要达到的音量")]
		[Range(MMSoundManagerSettings._minimalVolume,MMSoundManagerSettings._maxVolume)]
		public float FinalVolume = MMSoundManagerSettings._minimalVolume;
		/// the tween to operate the fade on
		[Tooltip("用于操作淡入淡出效果的补间（动画） ")]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
        
		/// <summary>
		/// 在播放时，会触发一个淡入淡出事件，该事件旨在被MM声音管理器捕获。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			MMSoundManagerTrackFadeEvent.Trigger(MMSoundManagerTrackFadeEvent.Modes.PlayFade, Track, FadeDuration, FinalVolume, FadeTween);
		}
        
		/// <summary>
		/// 在停止时，我们通过一个淡入淡出事件来停止淡入淡出效果。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			MMSoundManagerTrackFadeEvent.Trigger(MMSoundManagerTrackFadeEvent.Modes.StopFade, Track, FadeDuration, FinalVolume, FadeTween);
		}
	}
}
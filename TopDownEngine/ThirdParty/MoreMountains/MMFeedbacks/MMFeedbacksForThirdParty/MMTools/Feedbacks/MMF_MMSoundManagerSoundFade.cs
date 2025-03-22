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
	/// This feedback lets you trigger fades on a specific sound via the MMSoundManager. You will need a MMSoundManager in your scene for this to work.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Audio/MMSoundManager Sound Fade")]
	[FeedbackHelp("此反馈可让你通过MM声音管理器在特定声音上触发淡入淡出效果。要使此功能生效，你的场景中需要有一个MM声音管理器。 ")]
	public class MMF_MMSoundManagerSoundFade : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get { return "ID "+SoundID;  } }
		#endif

		[MMFInspectorGroup("MMSoundManager Sound Fade", true, 30)]
		/// the ID of the sound you want to fade. Has to match the ID you specified when playing the sound initially
		[Tooltip("你想要进行淡入淡出处理的声音的ID。该ID必须与你最初播放该声音时指定的ID相匹配。 ")]
		public int SoundID = 0;
		/// the duration of the fade, in seconds
		[Tooltip("淡入淡出的持续时间，单位为秒。")]
		public float FadeDuration = 1f;
		/// the volume towards which to fade
		[Tooltip("要淡入/淡出到的音量 ")]
		[Range(MMSoundManagerSettings._minimalVolume,MMSoundManagerSettings._maxVolume)]
		public float FinalVolume = MMSoundManagerSettings._minimalVolume;
		/// the tween to apply over the fade
		[Tooltip("在淡入淡出过程中应用的补间（动画效果） ")]
		public MMTweenType FadeTween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutQuartic);
        
		protected AudioSource _targetAudioSource;
        
		/// <summary>
		/// 在播放时，我们通过一个淡入淡出事件来开始进行淡入淡出处理。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			MMSoundManagerSoundFadeEvent.Trigger(MMSoundManagerSoundFadeEvent.Modes.PlayFade, SoundID, FadeDuration, FinalVolume, FadeTween);
		}
        
		/// <summary>
		/// 在停止播放时，我们通过一个淡入淡出事件来停止淡入淡出处理。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomStopFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			MMSoundManagerSoundFadeEvent.Trigger(MMSoundManagerSoundFadeEvent.Modes.StopFade, SoundID, FadeDuration, FinalVolume, FadeTween);
		}
	}
}
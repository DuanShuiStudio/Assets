using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Audio/AudioSource")]
	[FeedbackHelp("此反馈允许您播放目标音频源，并带有一些随机元素")]
	public class MMF_AudioSource : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有此类反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetAudioSource == null); }
		public override string RequiredTargetText { get { return TargetAudioSource != null ? TargetAudioSource.name : "";  } }
		public override string RequiresSetupText { get { return "此反馈需要一个TargetAudioSource才能正常工作。你可以在下面设置一个"; } }
		#endif
		public override bool HasRandomness => true;
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetAudioSource = FindAutomatedTarget<AudioSource>();

        /// 与音频源交互的可能方式
        public enum Modes { Play, Pause, UnPause, Stop }

		[MMFInspectorGroup("Audiosource", true, 28, true)]
		/// the target audio source to play
		[Tooltip("要播放的目标音频源")]
		public AudioSource TargetAudioSource;
		/// whether we should play the audio source or stop it or pause it
		[Tooltip("我们是应该播放音频源、停止它还是暂停它")]
		public Modes Mode = Modes.Play;
        
		[Header("Random Sound随机声音")]
		/// an array to pick a random sfx from
		[Tooltip("一个用于从中挑选随机音效的数组")]
		public AudioClip[] RandomSfx;

		[MMFInspectorGroup("Audio Settings", true, 29)]
        
		[Header("Volume音量")]
		/// the minimum volume to play the sound at
		[Tooltip("播放声音时的最小音量")]
		public float MinVolume = 1f;
		/// the maximum volume to play the sound at
		[Tooltip("播放声音时的最大音量")]
		public float MaxVolume = 1f;

		[Header("Pitch音调")]
		/// the minimum pitch to play the sound at
		[Tooltip("播放声音时的最小音调")]
		public float MinPitch = 1f;
		/// the maximum pitch to play the sound at
		[Tooltip("播放声音时的最大音调")]
		public float MaxPitch = 1f;

		[Header("Mixer混音器")]
		/// the audiomixer to play the sound with (optional)
		[Tooltip("用于播放声音的混音器（可选）")]
		public AudioMixerGroup SfxAudioMixerGroup;

        /// 此反馈的持续时间是所播放剪辑的持续时间
        public override float FeedbackDuration { get { return _duration; } set { _duration = value; } }

		protected AudioClip _randomClip;
		protected float _duration;

        /// <summary>
        /// 播放随机声音或指定的声音效果
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
			switch(Mode)
			{
				case Modes.Play:
					if (RandomSfx.Length > 0)
					{
						_randomClip = RandomSfx[Random.Range(0, RandomSfx.Length)];
						TargetAudioSource.clip = _randomClip;
					}
					float volume = Random.Range(MinVolume, MaxVolume) * intensityMultiplier;
					float pitch = Random.Range(MinPitch, MaxPitch);
					_duration = TargetAudioSource.clip.length;
					PlayAudioSource(TargetAudioSource, volume, pitch);
					break;

				case Modes.Pause:
					_duration = 0.1f;
					TargetAudioSource.Pause();
					break;

				case Modes.UnPause:
					_duration = 0.1f;
					TargetAudioSource.UnPause();
					break;

				case Modes.Stop:
					_duration = 0.1f;
					TargetAudioSource.Stop();
					break;
			}
		}

        /// <summary>
        /// 以选定的音量和音调播放音频源
        /// </summary>
        /// <param name="audioSource"></param>
        /// <param name="volume"></param>
        /// <param name="pitch"></param>
        protected virtual void PlayAudioSource(AudioSource audioSource, float volume, float pitch)
		{
            // 我们将音频源的音量设置为参数中的音量
            audioSource.volume = volume;
			audioSource.pitch = pitch;
			audioSource.timeSamples = 0;

			if (!NormalPlayDirection)
			{
				audioSource.pitch = -1;
				audioSource.timeSamples = audioSource.clip.samples - 1;
			}

            // 我们开始播放声音
            audioSource.Play();
		}

        /// <summary>
        /// 停止音频源的播放
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        public override void Stop(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			base.Stop(position, feedbacksIntensity);
			if (TargetAudioSource != null)
			{
				TargetAudioSource?.Stop();
			}            
		}
	}
}
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you control video players in all sorts of ways (Play, Pause, Toggle, Stop, Prepare, StepForward, StepBackward, SetPlaybackSpeed, SetDirectAudioVolume, SetDirectAudioMute, GoToFrame, ToggleLoop)
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈功能允许你以各种方式控制视频播放器（播放、暂停、切换播放状态、停止、准备、快进一帧、快退一帧、设置播放速度、设置直接音频音量、设置直接音频静音、跳转到指定帧、切换循环模式）。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("UI/Video Player")]
	public class MMF_VideoPlayer : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
		public enum VideoActions { Play, Pause, Toggle, Stop, Prepare, StepForward, StepBackward, SetPlaybackSpeed, SetDirectAudioVolume, SetDirectAudioMute, GoToFrame, ToggleLoop  }

        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetVideoPlayer == null); }
		public override string RequiredTargetText { get { return TargetVideoPlayer != null ? TargetVideoPlayer.name + " " + VideoAction.ToString() : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetVideoPlayer be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetVideoPlayer = FindAutomatedTarget<VideoPlayer>();

		[MMFInspectorGroup("Video Player", true, 58, true)]
		/// the Video Player to control with this feedback
		[Tooltip("用于通过此反馈进行控制的视频播放器。")]
		public VideoPlayer TargetVideoPlayer;
		/// the Video Player to control with this feedback
		[Tooltip("用于通过此反馈进行控制的视频播放器。")]
		public VideoActions VideoAction = VideoActions.Pause;
		/// the frame at which to jump when in GoToFrame mode
		[Tooltip("在“跳转到指定帧（GoToFrame）”模式下要跳转至的帧。")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.GoToFrame)]
		public long TargetFrame = 10;
		/// the new playback speed (between 0 and 10)
		[Tooltip("新的播放速度（范围在 0 到 10 之间）。")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetPlaybackSpeed)]
		public float PlaybackSpeed = 2f;
		/// the track index on which to control volume
		[Tooltip("要控制音量的音轨索引。")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetDirectAudioMute, (int)VideoActions.SetDirectAudioVolume)]
		public int TrackIndex = 0;
		/// the new volume for the specified track, between 0 and 1
		[Tooltip("指定音轨的新音量，取值范围在 0 到 1 之间。")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetDirectAudioVolume)]
		public float Volume = 1f;
		/// whether to mute the track or not when that feedback plays
		[Tooltip("当该反馈触发时，是否将音轨静音。")]
		[MMFEnumCondition("VideoAction", (int)VideoActions.SetDirectAudioMute)]
		public bool Mute = true;

        /// <summary>
        /// 播放时，我们会将所选命令应用到目标视频播放器上。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetVideoPlayer == null)
			{
				return;
			}

			switch (VideoAction)
			{
				case VideoActions.Play:
					TargetVideoPlayer.Play();
					break;
				case VideoActions.Pause:
					TargetVideoPlayer.Pause();
					break;
				case VideoActions.Toggle:
					if (TargetVideoPlayer.isPlaying)
					{
						TargetVideoPlayer.Pause();
					}
					else
					{
						TargetVideoPlayer.Play();
					}
					break;
				case VideoActions.Stop:
					TargetVideoPlayer.Stop();
					break;
				case VideoActions.Prepare:
					TargetVideoPlayer.Prepare();
					break;
				case VideoActions.StepForward:
					TargetVideoPlayer.StepForward();
					break;
				case VideoActions.StepBackward:
					TargetVideoPlayer.Pause();
					TargetVideoPlayer.frame = TargetVideoPlayer.frame - 1;
					break;
				case VideoActions.SetPlaybackSpeed:
					TargetVideoPlayer.playbackSpeed = PlaybackSpeed;
					break;
				case VideoActions.SetDirectAudioVolume:
					TargetVideoPlayer.SetDirectAudioVolume((ushort)TrackIndex, Volume);
					break;
				case VideoActions.SetDirectAudioMute:
					TargetVideoPlayer.SetDirectAudioMute((ushort)TrackIndex, Mute);
					break;
				case VideoActions.GoToFrame:
					TargetVideoPlayer.frame = TargetFrame;
					break;
				case VideoActions.ToggleLoop:
					TargetVideoPlayer.isLooping = !TargetVideoPlayer.isLooping;
					break;
			}

		}
	}
}
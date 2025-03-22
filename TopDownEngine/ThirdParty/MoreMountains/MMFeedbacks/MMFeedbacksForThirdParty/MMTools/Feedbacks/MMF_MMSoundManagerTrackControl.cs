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
	/// This feedback will let you control all sounds playing on a specific track (master, UI, music, sfx), and play, pause, mute, unmute, resume, stop, free them all at once. You will need a MMSoundManager in your scene for this to work.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Audio/MMSoundManager Track Control")]
	[FeedbackHelp("此反馈可让你控制在特定音轨（主音轨、用户界面音轨、音乐音轨、音效音轨）上正在播放的所有声音，并且可以一次性对它们进行播放、暂停、静音、取消静音、恢复播放、停止以及释放操作。要使此功能生效，你的场景中需要有一个MM声音管理器。 ")]
	public class MMF_MMSoundManagerTrackControl : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get { return Track.ToString() + " " + ControlMode.ToString();  } }
		#endif
        
		/// 你可以用来与该音轨进行交互的可能模式。“释放（Free）”模式将停止所有声音并将它们返回到资源池中。 
		public enum ControlModes { Mute, UnMute, SetVolume, Pause, Play, Stop, Free }
        
		[MMFInspectorGroup("MMSoundManager Track Control", true, 30)]
		/// the track to mute/unmute/pause/play/stop/free/etc
		[Tooltip("要进行静音/取消静音/暂停/播放/停止/释放等操作的音轨 ")]
		public MMSoundManager.MMSoundManagerTracks Track;
		/// the selected control mode to interact with the track. Free will stop all sounds and return them to the pool
		[Tooltip("用于与该音轨进行交互的所选控制模式。“释放”模式会停止所有声音并将它们返回到资源池中。 ")]
		public ControlModes ControlMode = ControlModes.Pause;
		/// if setting the volume, the volume to assign to the track 
		[Tooltip("如果要设置音量，就是要分配给该音轨的音量。 ")]
		[MMEnumCondition("ControlMode", (int) ControlModes.SetVolume)]
		public float Volume = 0.5f;

		/// <summary>
		/// 在播放时，通过MM声音管理器事件命令整个音轨执行特定的指令。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
            
			switch (ControlMode)
			{
				case ControlModes.Mute:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.MuteTrack, Track);
					break;
				case ControlModes.UnMute:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.UnmuteTrack, Track);
					break;
				case ControlModes.SetVolume:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.SetVolumeTrack, Track, Volume);
					break;
				case ControlModes.Pause:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.PauseTrack, Track);
					break;
				case ControlModes.Play:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.PlayTrack, Track);
					break;
				case ControlModes.Stop:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.StopTrack, Track);
					break;
				case ControlModes.Free:
					MMSoundManagerTrackEvent.Trigger(MMSoundManagerTrackEventTypes.FreeTrack, Track);
					break;
			}
		}
	}
}
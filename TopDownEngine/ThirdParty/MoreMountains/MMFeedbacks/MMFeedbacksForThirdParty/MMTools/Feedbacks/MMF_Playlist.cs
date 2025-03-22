using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you pilot a MMPlaylist
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将使你能够操控一个MM播放列表。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Audio/MMPlaylist")]
	public class MMF_Playlist : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get => Mode.ToString(); }
		public override bool HasChannel => true;
		#endif
		
		public enum Modes { Play, PlayNext, PlayPrevious, Stop, Pause, PlaySongAt, SetVolumeMultiplier, ChangePlaylist }
 
		[MMFInspectorGroup("MMPlaylist", true, 13)]
		/// the action to call on the playlist
		[Tooltip("要在播放列表上调用的操作")]
		public Modes Mode = Modes.PlayNext;
		/// the index of the song to play
		[Tooltip("要播放的歌曲的索引")]
		[MMEnumCondition("Mode", (int)Modes.PlaySongAt)]
		public int SongIndex = 0;
		/// the volume multiplier to apply
		[Tooltip("要应用的音量倍数")]
		[MMEnumCondition("Mode", (int)Modes.SetVolumeMultiplier)]
		public float VolumeMultiplier = 1f;
		/// whether to apply the volume multiplier instantly (true) or only when the next song starts playing (false)
		[Tooltip("是立即应用音量倍数（值为“真”），还是仅在当下一首歌曲开始播放时才应用（值为“假”） ")]
		[MMEnumCondition("Mode", (int)Modes.SetVolumeMultiplier)]
		public bool ApplyVolumeMultiplierInstantly = false;
		/// in change playlist mode, the playlist to which to switch to. Only works with MMSMPlaylistManager
		[Tooltip("在切换播放列表模式下，要切换到的播放列表。此功能仅适用于MM（声音管理系统）的播放列表管理器。  ")]
		[MMEnumCondition("Mode", (int)Modes.ChangePlaylist)]
		public MMSMPlaylist NewPlaylist;
		/// in change playlist mode, whether or not to play the new playlist after the switch. Only works with MMSMPlaylistManager
		[Tooltip("在切换播放列表模式下，切换后是否播放新的播放列表。此功能仅适用于MM（声音管理系统）的播放列表管理器。 ")]
		[MMEnumCondition("Mode", (int)Modes.ChangePlaylist)]
		public bool ChangePlaylistAndPlay = true;
        
		protected Coroutine _coroutine;

		/// <summary>
		/// 在播放时，我们会更改雾效的各项数值。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			switch (Mode)
			{
				case Modes.Play:
					MMPlaylistPlayEvent.Trigger(Channel);
					break;
				case Modes.PlayNext:
					MMPlaylistPlayNextEvent.Trigger(Channel);
					break;
				case Modes.PlayPrevious:
					MMPlaylistPlayPreviousEvent.Trigger(Channel);
					break;
				case Modes.Stop:
					MMPlaylistStopEvent.Trigger(Channel);
					break;
				case Modes.Pause:
					MMPlaylistPauseEvent.Trigger(Channel);
					break;
				case Modes.PlaySongAt:
					MMPlaylistPlayIndexEvent.Trigger(Channel, SongIndex);
					break;
				case Modes.SetVolumeMultiplier:
					MMPlaylistVolumeMultiplierEvent.Trigger(Channel, VolumeMultiplier, ApplyVolumeMultiplierInstantly);
					break;
				case Modes.ChangePlaylist:
					MMPlaylistChangeEvent.Trigger(Channel, NewPlaylist, ChangePlaylistAndPlay);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
            
		}
	}
}
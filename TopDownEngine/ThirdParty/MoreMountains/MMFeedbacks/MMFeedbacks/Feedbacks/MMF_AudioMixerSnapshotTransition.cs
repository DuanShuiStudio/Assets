using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you transition to a target AudioMixer Snapshot over a specified time
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将让你在指定的时间内过渡到目标AudioMixer快照")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Audio/AudioMixer Snapshot Transition")]
	public class MMF_AudioMixerSnapshotTransition : MMF_Feedback
	{
        ///一个静态布尔值，用于一次性禁用所有此类反馈
        public static bool FeedbackTypeAuthorized = true;
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.UIColor; } }
		public override bool EvaluateRequiresSetup() { return ((TargetSnapshot == null) || (OriginalSnapshot == null)); }
		public override string RequiredTargetText { get { return ((TargetSnapshot != null) && (OriginalSnapshot != null)) ? TargetSnapshot.name + " to "+ OriginalSnapshot.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that both a OriginalSnapshot and TargetSnapshot be set to be able to work properly. You can set these below."; } }
		#endif
        
		[MMFInspectorGroup("AudioMixer Snapshot", true, 44)]
		/// the target audio mixer snapshot we want to transition to 
		[Tooltip("我们想要过渡到的目标音频混合器快照")]
		public AudioMixerSnapshot TargetSnapshot;
		/// the audio mixer snapshot we want to transition from, optional, only needed if you plan to play this feedback in reverse 
		[Tooltip("我们想要从中过渡的音频混合器快照（可选），只有当你计划反向播放此反馈时才需要")]
		public AudioMixerSnapshot OriginalSnapshot;
		/// the duration, in seconds, over which to transition to the selected snapshot
		[Tooltip("过渡到所选快照的持续时间（以秒为单位）")]
		public float TransitionDuration = 1f;

        /// <summary>
        /// 在播放时，我们过渡到所选快照
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (TargetSnapshot == null)
			{
				return;
			}

			if (!NormalPlayDirection)
			{
				if (OriginalSnapshot != null)
				{
					OriginalSnapshot.TransitionTo(TransitionDuration);     
				}
				else
				{
					TargetSnapshot.TransitionTo(TransitionDuration);
				}
			}
			else
			{
				TargetSnapshot.TransitionTo(TransitionDuration);     
			}
		}
	}
}
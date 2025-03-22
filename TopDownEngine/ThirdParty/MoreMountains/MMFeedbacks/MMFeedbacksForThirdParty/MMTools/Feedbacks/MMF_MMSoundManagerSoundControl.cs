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
	/// This feedback will let you control a specific sound (or sounds), targeted by SoundID, which has to match the SoundID of the sound you intially played. You will need a MMSoundManager in your scene for this to work.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Audio/MMSoundManager Sound Control")]
	[FeedbackHelp("此反馈将使你能够控制特定的一个或多个声音，这些声音以“声音ID”为目标，该“声音ID”必须与你最初播放的声音的“声音ID”相匹配。为使此功能生效，你的场景中需要有一个多媒体声音管理器（MMSoundManager）。 ")]
	public class MMF_MMSoundManagerSoundControl : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。 
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get { return ControlMode.ToString();  } }
		#endif

		[MMFInspectorGroup("MMSoundManager Sound Control", true, 30)]
		/// the action to trigger on the specified sound
		[Tooltip("要在指定声音上触发的操作")]
		public MMSoundManagerSoundControlEventTypes ControlMode = MMSoundManagerSoundControlEventTypes.Pause;
		/// the ID of the sound, has to match the one you specified when playing it
		[Tooltip("声音的ID，必须与你播放该声音时所指定的ID相匹配。 ")]
		public int SoundID = 0;

		protected AudioSource _targetAudioSource;
        
		/// <summary>
		/// 在播放时，会触发一个事件，该事件旨在被多媒体声音管理器（MMSoundManager）捕获并据此采取行动。 
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			MMSoundManagerSoundControlEvent.Trigger(ControlMode, SoundID);
		}
	}
}
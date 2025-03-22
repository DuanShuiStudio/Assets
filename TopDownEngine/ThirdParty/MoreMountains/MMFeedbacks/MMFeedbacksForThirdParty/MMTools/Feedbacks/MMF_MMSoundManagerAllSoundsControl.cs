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
	/// A feedback used to control all sounds playing on the MMSoundManager at once. It'll let you pause, play, stop and free (stop and returns the audiosource to the pool) sounds.  You will need a MMSoundManager in your scene for this to work.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Audio/MMSoundManager All Sounds Control")]
	[FeedbackHelp("一种用于一次性控制在MM声音管理器上播放的所有声音的反馈。它能让你暂停、播放、停止以及释放（停止并将音频源返回到资源池）声音。要使此功能生效，你的场景中需要有一个MM声音管理器。 ")]
	public class MMF_MMSoundManagerAllSoundsControl : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get { return ControlMode.ToString();  } }
		#endif
        
		[MMFInspectorGroup("MMSoundManager All Sounds Control", true, 30)]
		/// The selected control mode. 
		[Tooltip("所选的控制模式")]
		public MMSoundManagerAllSoundsControlEventTypes ControlMode = MMSoundManagerAllSoundsControlEventTypes.Pause;

		/// <summary>
		/// 在播放时，我们会调用指定的事件，该事件将由MM声音管理器捕获。 
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
				case MMSoundManagerAllSoundsControlEventTypes.Pause:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Pause);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Play:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Play);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Stop:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Stop);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.Free:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.Free);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.FreeAllButPersistent:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.FreeAllButPersistent);
					break;
				case MMSoundManagerAllSoundsControlEventTypes.FreeAllLooping:
					MMSoundManagerAllSoundsControlEvent.Trigger(MMSoundManagerAllSoundsControlEventTypes.FreeAllLooping);
					break;
			}
		}
	}
}
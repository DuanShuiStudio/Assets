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
	/// This feedback will let you trigger save, load, and reset on MMSoundManager settings. You will need a MMSoundManager in your scene for this to work.
	/// </summary>
	[AddComponentMenu("")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Audio/MMSoundManager Save and Load")]
	[FeedbackHelp("此反馈可让你在MM声音管理器的设置上触发保存、加载和重置操作。要使此功能生效，你的场景中需要有一个MM声音管理器。 ")]
	public class MMF_MMSoundManagerSaveLoad : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SoundsColor; } }
		public override string RequiredTargetText { get { return Mode.ToString();  } }
		#endif

		/// 你可以用来与保存设置进行交互的可能模式。 
		public enum Modes { Save, Load, Reset }

		[MMFInspectorGroup("MMSoundManager Save and Load", true, 30)]
		/// the selected mode to interact with save settings on the MMSoundManager
		[Tooltip("与MM声音管理器上的保存设置进行交互的所选模式")]
		public Modes Mode = Modes.Save;
        
		/// <summary>
		/// 在播放时，保存、加载或重置设置。 
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
				case Modes.Save:
					MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.SaveSettings);
					break;
				case Modes.Load:
					MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.LoadSettings);
					break;
				case Modes.Reset:
					MMSoundManagerEvent.Trigger(MMSoundManagerEventTypes.ResetSettings);
					break;
			}
		}
	}
}
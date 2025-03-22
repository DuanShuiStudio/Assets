using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback allows you to control one or more target MMF Players
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈允许你控制一个或多个目标MMF播放器")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Feedbacks/MMF Player Control")]
	public class MMF_PlayerControl : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
		public override string RequiredTargetText => Mode.ToString();
		#endif
		
		public override bool HasChannel => false;

		public override float FeedbackDuration
		{
			get
			{
				if (TargetPlayers == null)
				{
					return 0f;
				}

				if (!WaitForTargetPlayersToFinish)
				{
					return 0f;
				}

				if ((Mode == Modes.PlayFeedbacks) && (TargetPlayers.Count > 0))
				{
					float totalDuration = 0f;
					foreach (MMF_Player player in TargetPlayers)
					{
						if ((player != null) && (totalDuration < player.TotalDuration))
						{
							totalDuration = player.TotalDuration;	
						}
					}

					return totalDuration;
				}

				return 0f;
			}
		}

		public override bool IsPlaying
		{
			get
			{
				if (WaitForTargetPlayersToFinish)
				{
					foreach (MMF_Player player in TargetPlayers)
					{
						if (player.IsPlaying)
						{
							return true;
						}
					}	
				}
				
				return false;
			}
		}

		public enum Modes
		{
			PlayFeedbacks,
			StopFeedbacks,
			PauseFeedbacks,
			ResumeFeedbacks,
			Initialization,
			PlayFeedbacksInReverse,
			PlayFeedbacksOnlyIfReversed,
			PlayFeedbacksOnlyIfNormalDirection,
			ResetFeedbacks,
			Revert,
			SetDirectionTopToBottom,
			SetDirectionBottomToTop,
			RestoreInitialValues,
			SkipToTheEnd,
			RefreshCache
		}
	
        
		[MMFInspectorGroup("MMF Player", true, 79)]

        /// 要播放的目标MMF_Players列表
        [Tooltip("特定的MMFeedbacks / MMF_Player播放")]
		public List<MMF_Player> TargetPlayers;
		/// if this is true, this feedback will be considered as Playing while any of the target players are still Playing
		[Tooltip("如果为真，只要任一目标播放器仍在播放，此反馈将被视为正在播放")]
		public bool WaitForTargetPlayersToFinish = true;

		public Modes Mode = Modes.PlayFeedbacks;

        /// <summary>
        /// 在初始化时，如果需要则关闭灯光
        /// </summary>
        /// <param name="owner"></param>
        protected override void CustomInitialization(MMF_Player owner)
		{
			base.CustomInitialization(owner);
		}

        /// <summary>
        /// 在播放时，我们触发目标播放器上的选定方法
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (TargetPlayers.Count == 0)
			{
				return;
			}
			
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			switch (Mode)
			{
				case Modes.PlayFeedbacks:
					foreach (MMF_Player player in TargetPlayers) { player.PlayFeedbacks(position, feedbacksIntensity); }
					break;
				case Modes.StopFeedbacks:
					foreach (MMF_Player player in TargetPlayers) { player.StopFeedbacks(); }
					break;
				case Modes.PauseFeedbacks:
					foreach (MMF_Player player in TargetPlayers) { player.PauseFeedbacks(); }
					break;
				case Modes.ResumeFeedbacks:
					foreach (MMF_Player player in TargetPlayers) { player.ResumeFeedbacks(); }
					break;
				case Modes.Initialization:
					foreach (MMF_Player player in TargetPlayers) { player.Initialization(); }
					break;
				case Modes.PlayFeedbacksInReverse:
					foreach (MMF_Player player in TargetPlayers) { player.PlayFeedbacksInReverse(position, feedbacksIntensity); }
					break;
				case Modes.PlayFeedbacksOnlyIfReversed:
					foreach (MMF_Player player in TargetPlayers) { player.PlayFeedbacksOnlyIfReversed(position, feedbacksIntensity); }
					break;
				case Modes.PlayFeedbacksOnlyIfNormalDirection:
					foreach (MMF_Player player in TargetPlayers) { player.PlayFeedbacksOnlyIfNormalDirection(position, feedbacksIntensity); }
					break;
				case Modes.ResetFeedbacks:
					foreach (MMF_Player player in TargetPlayers) { player.ResetFeedbacks(); }
					break;
				case Modes.Revert:
					foreach (MMF_Player player in TargetPlayers) { player.Revert(); }
					break;
				case Modes.SetDirectionTopToBottom:
					foreach (MMF_Player player in TargetPlayers) { player.SetDirectionTopToBottom(); }
					break;
				case Modes.SetDirectionBottomToTop:
					foreach (MMF_Player player in TargetPlayers) { player.SetDirectionBottomToTop(); }
					break;
				case Modes.RestoreInitialValues:
					foreach (MMF_Player player in TargetPlayers) { player.RestoreInitialValues(); }
					break;
				case Modes.SkipToTheEnd:
					foreach (MMF_Player player in TargetPlayers) { player.SkipToTheEnd(); }
					break;
				case Modes.RefreshCache:
					foreach (MMF_Player player in TargetPlayers) { player.RefreshCache(); }
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback allows you to chain any number of target MMF Players and play them in sequence, with optional delays before and after
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈允许你链接任意数量的目标MMF播放器，并按顺序播放它们，同时在之前和之后可选择延迟")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Feedbacks/MMF Player Chain")]
	public class MMF_PlayerChain : MMF_Feedback
	{
        /// <summary>
        /// 一个用于存储和定义MMF播放器链中项目的类
        /// </summary>
        [Serializable]
		public class PlayerChainItem
		{
			/// the target MMF Player 
			[Tooltip("目标MMF播放器")]
			public MMF_Player TargetPlayer;
			/// a delay in seconds to wait for before playing this MMF Player (x) and after (y)
			[Tooltip("在播放此MMF播放器（x）之前和之后等待的秒数延迟（y）")]
			[MMVector("Before", "After")]
			public Vector2 Delay;
			/// whether this player is active in the list or not. Inactive players will be skipped when playing the chain of players
			[Tooltip("此播放器在列表中是否为活动状态。非活动状态的播放器在播放播放器链时将被跳过")]
			public bool Inactive = false;
			/// if this is true, the sequence will be blocked until this player has completed playing
			[Tooltip("如果为真，则序列将被阻止，直到此播放器完成播放")]
			public bool WaitUntilComplete = true;
		}

        /// 一个静态布尔值，用于一次性禁用所有这种类型的反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.FeedbacksColor; } }
#endif
        /// 此反馈的持续时间是链的持续时间
        public override float FeedbackDuration 
		{
			get
			{
				if ((Players == null) || (Players.Count == 0))
				{
					return 0f;
				}
				
				float totalDuration = 0f;
				foreach (PlayerChainItem item in Players)
				{
					if ((item == null) || (item.TargetPlayer == null) || item.Inactive)
					{
						continue;
					}

					totalDuration += item.Delay.x;
					totalDuration += item.TargetPlayer.TotalDuration;
					totalDuration += item.Delay.y; 
				}
				return totalDuration;
			} 
		}

		[MMFInspectorGroup("Feedbacks", true, 79)]
		/// the list of MMF Player that make up the chain. The chain's items will be played from index 0 to the last in the list
		[Tooltip("组成链的MMF播放器列表。链的项目将从索引0播放到列表中的最后一个")]
		public List<PlayerChainItem> Players;

        /// <summary>
        /// 在播放时，我们开始我们的链
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if ((Players == null) || (Players.Count == 0))
			{
				return;
			}
			
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			Owner.StartCoroutine(PlayChain());
		}

        /// <summary>
        /// 按顺序播放链中的所有播放器
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator PlayChain()
		{
			IsPlaying = true;
			foreach (PlayerChainItem item in Players)
			{
				if ((item == null) || (item.TargetPlayer == null) || item.Inactive)
				{
					continue;
				}

				if (item.Delay.x > 0) { yield return WaitFor(item.Delay.x); }
				
				if (item.WaitUntilComplete) 
				{
					item.TargetPlayer.PlayFeedbacks();
					yield return WaitFor(item.TargetPlayer.TotalDuration);
				} 
				else 
				{
					item.TargetPlayer.PlayFeedbacks();
				}
				
				if (item.Delay.y > 0) { yield return WaitFor(item.Delay.y); }
			}
			while (FeedbacksStillPlaying())
			{
				yield return null;
			}
			IsPlaying = false;
		}
	
		protected virtual bool FeedbacksStillPlaying()
		{
			bool feedbacksStillPlaying = false;
			foreach (PlayerChainItem item in Players)
			{
				if (item.TargetPlayer.IsPlaying)
				{
					feedbacksStillPlaying = true;
				}
			}
			return feedbacksStillPlaying;
		}

        /// <summary>
        /// 在跳到结尾时，我们跳过链中的所有播放器
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomSkipToTheEnd(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			foreach (PlayerChainItem item in Players)
			{
				if ((item == null) || (item.TargetPlayer == null) || item.Inactive)
				{
					continue;
				}

				item.TargetPlayer.PlayFeedbacks();
				item.TargetPlayer.SkipToTheEnd();
			}
		}

        /// <summary>
        /// 在恢复时，我们将对象放回其初始位置
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			foreach (PlayerChainItem item in Players)
			{
				item.TargetPlayer.RestoreInitialValues();
			}
		}
	}
}
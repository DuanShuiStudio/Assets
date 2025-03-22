using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 此操作用于播放mmfeedback
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action MMFeedbacks")]
	public class AIActionMMFeedbacks : AIAction
	{
		/// The MMFeedbacks to play when this action gets performed by the AIBrain
		[Tooltip("当AIBrain执行此操作时要播放的MMFeedbacks")]
		public MMFeedbacks TargetFeedbacks;
		/// If this is false, the feedback will be played every PerformAction (by default every frame while in this state), otherwise it'll only play once, when entering the state
		[Tooltip("如果这个值为假，那么反馈将会在每一帧（默认情况下）执行动作时播放；否则，它只会在进入状态时播放一次")]
		public bool OnlyPlayWhenEnteringState = true;
		/// If this is true, the target game object the TargetFeedbacks is on will be set active when performing this action
		[Tooltip("如果这个值为真，那么在执行此操作时，带有TargetFeedbacks的目标游戏对象将被设置为激活状态")]
		public bool SetTargetGameObjectActive = false;

		protected bool _played = false;

        /// <summary>
        /// 在PerformAction上，我们播放mmfeedback
        /// </summary>
        public override void PerformAction()
		{
			PlayFeedbacks();
		}

        /// <summary>
        /// 播放目标mmfeedback
        /// </summary>
        protected virtual void PlayFeedbacks()
		{
			if (OnlyPlayWhenEnteringState && _played)
			{
				return;
			}

			if (TargetFeedbacks != null)
			{
				if (SetTargetGameObjectActive)
				{
					TargetFeedbacks.gameObject.SetActive(true);
				}
				TargetFeedbacks.PlayFeedbacks();
				_played = true;
			}
		}

        /// <summary>
        /// 在进入状态时，我们初始化playbool
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			_played = false;
		}
	}
}
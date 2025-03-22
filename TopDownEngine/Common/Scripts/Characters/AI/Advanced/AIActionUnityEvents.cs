using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个动作被用来触发一个UnityEvent
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Unity Events")]
	public class AIActionUnityEvents : AIAction
	{
		/// The UnityEvent to trigger when this action gets performed by the AIBrain
		[Tooltip("当AIBrain执行此操作时要触发的UnityEvent")]
		public UnityEvent TargetEvent;
		/// If this is false, the Unity Event will be triggered every PerformAction (by default every frame while in this state), otherwise it'll only play once, when entering the state
		[Tooltip("如果这个值为假，Unity事件将在每一帧（默认情况下）执行动作时触发；否则，它只会在进入状态时播放一次")]
		public bool OnlyPlayWhenEnteringState = true;

		protected bool _played = false;

        /// <summary>
        /// 在PerformAction上触发事件
        /// </summary>
        public override void PerformAction()
		{
			TriggerEvent();
		}

        /// <summary>
        /// 触发目标事件
        /// </summary>
        protected virtual void TriggerEvent()
		{
			if (OnlyPlayWhenEnteringState && _played)
			{
				return;
			}

			if (TargetEvent != null)
			{
				TargetEvent.Invoke();
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
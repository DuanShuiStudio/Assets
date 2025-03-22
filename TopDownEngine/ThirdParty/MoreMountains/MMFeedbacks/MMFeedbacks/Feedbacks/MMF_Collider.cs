using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you enable/disable/toggle a target collider, or change its trigger status
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将让你启用/禁用/切换目标碰撞器，或更改其触发状态")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("GameObject/Collider")]
	public class MMF_Collider : MMF_Feedback
	{
        /// 一个静态布尔值用于一次性禁用此类型的所有反馈
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.GameObjectColor; } }
		public override bool EvaluateRequiresSetup() { return (TargetCollider == null); }
		public override string RequiredTargetText { get { return TargetCollider != null ? TargetCollider.name : "";  } }
		public override string RequiresSetupText { get { return "This feedback requires that a TargetCollider be set to be able to work properly. You can set one below."; } }
		#endif
		public override bool HasAutomatedTargetAcquisition => true;
		protected override void AutomateTargetAcquisition() => TargetCollider = FindAutomatedTarget<Collider>();

        /// 此反馈可能对目标碰撞器状态产生的影响。
        public enum Modes { Enable, Disable, ToggleActive, Trigger, NonTrigger, ToggleTrigger }

		[MMFInspectorGroup("Collider", true, 12, true)]
		/// the collider to act upon
		[Tooltip("要对其执行操作的碰撞器")]
		public Collider TargetCollider;
        /// 此反馈将对目标碰撞器状态产生的影响
        public Modes Mode = Modes.Disable;

		protected bool _initialState;

        /// <summary>
        /// 在播放时，如果需要，我们更改碰撞器的状态
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			if (TargetCollider != null)
			{
				ApplyChanges(Mode);
			}
		}

        /// <summary>
        /// 更改碰撞器的状态
        /// </summary>
        /// <param name="state"></param>
        protected virtual void ApplyChanges(Modes mode)
		{
			switch (mode)
			{
				case Modes.Enable:
					_initialState = TargetCollider.enabled;
					TargetCollider.enabled = true;
					break;
				case Modes.Disable:
					_initialState = TargetCollider.enabled;
					TargetCollider.enabled = false;
					break;
				case Modes.ToggleActive:
					_initialState = TargetCollider.enabled;
					TargetCollider.enabled = !TargetCollider.enabled;
					break;
				case Modes.Trigger:
					_initialState = TargetCollider.isTrigger;
					TargetCollider.isTrigger = true;
					break;
				case Modes.NonTrigger:
					_initialState = TargetCollider.isTrigger;
					TargetCollider.isTrigger = false;
					break;
				case Modes.ToggleTrigger:
					_initialState = TargetCollider.isTrigger;
					TargetCollider.isTrigger = !TargetCollider.isTrigger;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
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

			switch (Mode)
			{
				case Modes.Enable:
					TargetCollider.enabled = _initialState;
					break;
				case Modes.Disable:
					TargetCollider.enabled = _initialState;
					break;
				case Modes.ToggleActive:
					TargetCollider.enabled = _initialState;
					break;
				case Modes.Trigger:
					TargetCollider.isTrigger = _initialState;
					break;
				case Modes.NonTrigger:
					TargetCollider.isTrigger = _initialState;
					break;
				case Modes.ToggleTrigger:
					TargetCollider.isTrigger = _initialState;
					break;
			}
		}
	}
}
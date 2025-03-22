using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此类添加到触发器中，它将允许您在进入时修改时间缩放，并持续指定的时间和设置。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Time Zone")]
	public class TimeZone : ButtonActivated
	{
        /// 此区域可能的模式
        public enum Modes { DurationBased, ExitBased }

		[MMInspectorGroup("Time Zone", true, 18)]

		/// whether this zone will modify time on entry for a certain duration, or until it is exited
		[Tooltip("此区域是否会在进入时修改时间持续一段时间，或者直到退出为止")]
		public Modes Mode = Modes.DurationBased;

		/// the new timescale to apply
		[Tooltip("要应用的新时间缩放")]
		public float TimeScale = 0.5f;
		/// the duration to apply the new timescale for
		[Tooltip("应用新时间缩放的持续时间")]
		public float Duration = 1f;
		/// whether or not the timescale should be lerped
		[Tooltip("是否应该使时间缩放变慢")]
		public bool LerpTimeScale = true;
		/// the speed at which to lerp the timescale
		[Tooltip("使时间缩放变慢的速度")]
		public float LerpSpeed = 5f;

        /// <summary>
        /// 当按下按钮时，我们开始修改时间缩放
        /// </summary>
        public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction();
			ControlTime();
		}

        /// <summary>
        /// 退出时，如果需要，我们重置时间缩放
        /// </summary>
        /// <param name="collider"></param>
        public override void TriggerExitAction(GameObject collider)
		{
			if (Mode == Modes.ExitBased)
			{
				if (!CheckConditions(collider))
				{
					return;
				}

				if (!TestForLastObject(collider))
				{
					return;
				}

				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);
			}
		}

        /// <summary>
        /// 修改时间缩放
        /// </summary>
        public virtual void ControlTime()
		{
			if (Mode == Modes.ExitBased)
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, Duration, LerpTimeScale, LerpSpeed, true);
			}
			else
			{
				MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, Duration, LerpTimeScale, LerpSpeed, false);
			}
		}
	}
}
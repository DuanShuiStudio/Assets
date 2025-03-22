using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此能力添加到角色中，当按下时间控制按钮时，它将能够控制时间
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Time Control")]
	public class CharacterTimeControl : CharacterAbility
	{
		public enum Modes { OneTime, Continuous }
        
		/// the chosen mode for this ability : one time will stop time for the specified duration on button press, even if you release it, while continuous will stop time while the button is pressed, until cooldown consumption duration expiration
		[Tooltip("此技能的选择模式：将在按下一次按钮时停止指定的持续时间，即使你释放它，将继续停止时间，直到冷却消耗持续时间到期")]
		public Modes Mode = Modes.Continuous;
		/// the time scale to switch to when the time control button gets pressed
		[Tooltip("当时间控制按钮被按下时切换到的时间尺度")]
		public float TimeScale = 0.5f;
		/// the duration for which to keep the timescale changed
		[Tooltip("保持时间尺度改变的持续时间")]
		[MMEnumCondition("Mode", (int)Modes.OneTime)]
		public float OneTimeDuration = 1f;
		/// whether or not the timescale should get lerped
		[Tooltip("时间尺度是否应该被平滑")]
		public bool LerpTimeScale = true;
		/// the speed at which to lerp the timescale
		[Tooltip("平滑时间尺度的速度")]
		public float LerpSpeed = 5f;
		/// the cooldown for this ability
		[Tooltip("这个技能的冷却时间")]
		public MMCooldown Cooldown;

		protected bool _timeControlled = false;

        /// <summary>
        /// 输入按键表
        /// </summary>
        protected override void HandleInput()
		{
			base.HandleInput();
			if (!AbilityAuthorized)
			{
				return;
			}
			if (_inputManager.TimeControlButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				TimeControlStart();
			}
			if (_inputManager.TimeControlButton.State.CurrentState == MMInput.ButtonStates.ButtonUp)
			{
				TimeControlStop();
			}
		}

        /// <summary>
        /// 在初始化时，我们初始化冷却时间
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			Cooldown.Initialization();
		}

        /// <summary>
        /// 开始修改时间尺度
        /// </summary>
        public virtual void TimeControlStart()
		{
			if (Cooldown.Ready())
			{
				PlayAbilityStartFeedbacks();
				if (Mode == Modes.Continuous)
				{
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, Cooldown.ConsumptionDuration, LerpTimeScale, LerpSpeed, true);
					Cooldown.Start();
					_timeControlled = true;    
				}
				else
				{
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, TimeScale, OneTimeDuration, LerpTimeScale, LerpSpeed, false);
					Cooldown.Start();
				}
			}            
		}

        /// <summary>
        /// 停止时间控制
        /// </summary>
        public virtual void TimeControlStop()
		{
			Cooldown.Stop();
		}

        /// <summary>
        /// 在更新，我们解冻时间，如果需要的话
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			Cooldown.Update();

			if ((Cooldown.CooldownState != MMCooldown.CooldownStates.Consuming) && _timeControlled)
			{
				if (Mode == Modes.Continuous)
				{
					_timeControlled = false;
					MMTimeScaleEvent.Trigger(MMTimeScaleMethods.Unfreeze, 1f, 0f, false, 0f, false);    
				}
			}
		}

		protected virtual void OnCooldownStateChange(MMCooldown.CooldownStates newState)
		{
			if (newState == MMCooldown.CooldownStates.Stopped)
			{
				StopStartFeedbacks();
				PlayAbilityStopFeedbacks();
			}
		}
		
		protected override void OnEnable()
		{
			base.OnEnable();
			Cooldown.OnStateChange += OnCooldownStateChange;
		}
		
		protected override void OnDisable()
		{
			base.OnDisable();
			Cooldown.OnStateChange -= OnCooldownStateChange;
		}
	}
}
using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// A class used to store the charge properties of the weapons that together make up a charge weapon
    /// 一个用于存储组成蓄力武器的各武器蓄力属性的类
    /// Each charge weapon is made of multiple of these, each representing a step in the charge sequence
	/// 每个蓄力武器由多个这样的部件组成，每个部件代表蓄力序列中的一个步骤
    /// </summary>
    [Serializable]
	public class ChargeWeaponStep
	{
		/// the weapon to cause an attack with at that step
		[Tooltip("在该步骤中用于攻击的武器")]
		public Weapon TargetWeapon;
		/// the duration (in seconds) it should take to keep the charge going to the next step
		[Tooltip("保持蓄力到下一步所需的持续时间（秒）0")]
		public float ChargeDuration = 1f;
		/// if the charge is interrupted at this step, whether or not to trigger this weapon's attack
		[Tooltip("如果在此步骤中蓄力被中断，是否触发该武器的攻击")]
		public bool TriggerIfChargeInterrupted = true;
		/// if this is true, the weapon at this step will be flipped when the charge weapon flips 
		[Tooltip("如果这是真的，那么当蓄力武器翻转时，此步骤中的武器将被翻转")]
		public bool FlipWhenChargeWeaponFlips = true;
		/// a feedback to trigger when this step starts charging
		[Tooltip("一个在此步骤开始蓄力时触发的反馈")]
		public MMFeedbacks ChargeStartFeedbacks;
		/// a feedback to trigger when this step gets interrupted (when the charge is dropped at this step)
		[Tooltip("一个在此步骤被中断时（当蓄力在此步骤被丢弃时）触发的反馈")]
		public MMFeedbacks ChargeInterruptedFeedbacks;
		/// a feedback to trigger when this step completes and the charge potentially moves on to the next step
		[Tooltip("一个在此步骤完成并蓄力且可能移动到下一步时触发的反馈")]
		public MMFeedbacks ChargeCompleteFeedbacks;
        /// 从蓄力武器完全开始到这个武器的蓄力完成的总时间（以秒为单位）
        public virtual float ChargeTotalDuration { get; set; }
        /// 这个步骤的蓄力是否已经开始
        public virtual bool ChargeStarted { get; set; }
        /// 这个步骤的蓄力是否已经完成
        public virtual bool ChargeComplete { get; set; }
	}

    /// <summary>
    /// 将此组件添加到对象中，它将允许您定义一个蓄力步骤序列，每个步骤都会触发其独特的武器，并包含诸如输入模式或条件释放、每个步骤的钩子等选项。这对于像 Megaman 或 Zelda 这样的蓄力武器非常有用
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Charge Weapon")]
	public class ChargeWeapon : Weapon
	{
        /// 这种武器的可能时间表
        public enum TimescaleModes { Scaled, Unscaled }
        /// 蓄力是否应在输入释放时释放，还是在最后一次蓄力持续时间后释放
        public enum ReleaseModes { OnInputRelease, AfterLastChargeDuration }
        /// 当前的增量时间值
        public virtual float DeltaTime => TimescaleMode == TimescaleModes.Scaled ? Time.deltaTime : Time.unscaledDeltaTime;
        /// 当前时间值
        public virtual float CurrentTime => TimescaleMode == TimescaleModes.Scaled ? Time.time : Time.unscaledTime;

		[MMInspectorGroup("Charge Weapon", true, 22)]
		[Header("蓄力序列中的武器列表")]
		/// the list of weapons that make up this charge weapon's sequence of steps
		[Tooltip("构成这个蓄力武器步骤序列的武器列表")]
		public List<ChargeWeaponStep> Weapons;
		
		[Header("Settings设置")]
		/// whether this weapon should trigger its attack when all steps are done charging, or when input gets released
		[Tooltip("这个武器是否应该在所有步骤都蓄力完成后触发攻击，还是在输入释放时触发攻击")]
		public ReleaseModes ReleaseMode = ReleaseModes.OnInputRelease;
		/// whether this weapon's input should run on scaled or unscaled time
		[Tooltip("这个武器的输入是否应该在缩放或未缩放的时间上运行")]
		public TimescaleModes TimescaleMode = TimescaleModes.Scaled;
		/// whether or not the start of the charge should trigger the first step's weapon's attack or not
		[Tooltip("蓄力开始时是否应该触发第一步武器的攻击")]
		public bool AllowInitialShot = true;
		
		[Header("Debug调试")]
		/// the current charge index in the Weapons step list
		[Tooltip("当前在武器步骤列表中的蓄力索引")]
		[MMReadOnly]
		public int CurrentChargeIndex = 0;
		/// whether this weapon is currently charging or not
		[Tooltip("这个武器当前是否正在蓄力")]
		[MMReadOnly] 
		public bool Charging = false;

		protected float _chargingStartedAt = 0f;
		protected int _chargeIndexLastFrame;
		protected int _initialWeaponIndex = 0;

        /// <summary>
        /// 在初始化时，我们初始化我们的持续时间、武器并重置蓄力
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();
			InitializeTotalDurations();
			InitializeWeapons();
			ResetCharge();
		}

        /// <summary>
        /// 遍历所有武器以设置它们的总持续时间（从开始到它们的步骤完成的时间）
        /// </summary>
        public virtual void InitializeTotalDurations()
		{
			float total = 0f;
			if (DelayBeforeUse > 0)
			{
				total += DelayBeforeUse;
				CurrentChargeIndex = -1;
			}
			foreach (ChargeWeaponStep item in Weapons)
			{
				total += item.ChargeDuration;
				item.ChargeTotalDuration = total;
			}
			_chargeIndexLastFrame = CurrentChargeIndex;
			_initialWeaponIndex = CurrentChargeIndex;
		}

        /// <summary>
        /// 重置蓄力，重新初始化所有计数器
        /// </summary>
        public virtual void ResetCharge()
		{
			Charging = false;
			CurrentChargeIndex = _initialWeaponIndex;
			foreach (ChargeWeaponStep item in Weapons)
			{
				item.ChargeStarted = false;
				item.ChargeComplete = false;
			}
		}

        /// <summary>
        /// 为所有步骤初始化所有武器
        /// </summary>
        protected virtual void InitializeWeapons()
		{
			foreach (ChargeWeaponStep item in Weapons)
			{
				item.TargetWeapon.SetOwner(Owner, CharacterHandleWeapon);
				item.TargetWeapon.Initialization();
				item.TargetWeapon.InitializeAnimatorParameters();
			}
		}

        /// <summary>
        /// 在更新时，如果我们正在蓄力，我们处理我们的蓄力以评估当前步骤
        /// </summary>
        protected override void Update()
		{
			base.Update();
			ProcessCharge();
		}

        /// <summary>
        /// 确定当前步骤，如果它与上一帧不同，则开始新的步骤
        /// </summary>
        protected virtual void ProcessCharge()
		{
			if (!Charging)
			{
				return;
			}
			
			CurrentChargeIndex = FindCurrentWeaponIndex();

			if (CurrentChargeIndex != _chargeIndexLastFrame)
			{
				CompleteStepCharge(_chargeIndexLastFrame);
				StartStepCharge(CurrentChargeIndex);
			}

			if ((ReleaseMode == ReleaseModes.AfterLastChargeDuration) && (CurrentChargeIndex == Weapons.Count - 1))
			{
				StopChargeSequence();
			}
			
			_chargeIndexLastFrame = CurrentChargeIndex;
		}

        /// <summary>
        /// 初始化蓄力序列
        /// </summary>
        protected virtual void StartChargeSequence()
		{
			Charging = true;
			_chargingStartedAt = CurrentTime;
			if (WeaponExists(CurrentChargeIndex))
			{
				StartStepCharge(CurrentChargeIndex);
				if (AllowInitialShot)
				{
					ForceWeaponAttack(0);
				}
			}
		}

        /// <summary>
        /// 开始一个蓄力步骤
        /// </summary>
        /// <param name="index"></param>
        protected virtual void StartStepCharge(int index)
		{
			if (!WeaponExists(index))
			{
				return;
			}
			
			Weapons[index].ChargeStarted = true;
			Weapons[index].ChargeStartFeedbacks?.PlayFeedbacks();
		}

        /// <summary>
        /// 停止一个蓄力步骤
        /// </summary>
        /// <param name="index"></param>
        protected virtual void InterruptStepCharge(int index)
		{
			if (!WeaponExists(index))
			{
				return;
			}
			Weapons[index].ChargeStartFeedbacks?.StopFeedbacks();
			Weapons[index].ChargeInterruptedFeedbacks?.PlayFeedbacks();
		}

        /// <summary>
        /// 完成一个蓄力步骤
        /// </summary>
        /// <param name="index"></param>
        protected virtual void CompleteStepCharge(int index)
		{
			if (!WeaponExists(index))
			{
				return;
			}
			
			Weapons[index].ChargeStartFeedbacks?.StopFeedbacks();
			Weapons[index].ChargeComplete = true;
			Weapons[index].ChargeCompleteFeedbacks?.PlayFeedbacks();
		}

        /// <summary>
        /// 停止整个蓄力序列，触发适当的反馈
        /// </summary>
        protected virtual void StopChargeSequence()
		{
			if (!Charging)
			{
				return;
			}
			
			if ((CurrentChargeIndex >= 0) || !AllowInitialShot)
			{
				bool shouldAttack = true;
				if (CurrentChargeIndex < Weapons.Count - 1 && !Weapons[CurrentChargeIndex].ChargeComplete)
				{
					if (!Weapons[CurrentChargeIndex].TriggerIfChargeInterrupted)
					{
						shouldAttack = false;
					}
				}

				if (shouldAttack)
				{
					Weapons[CurrentChargeIndex].ChargeStartFeedbacks?.StopFeedbacks();
					Weapons[CurrentChargeIndex].ChargeCompleteFeedbacks?.StopFeedbacks();
					if (WeaponExists(CurrentChargeIndex - 1))
					{
						Weapons[CurrentChargeIndex - 1].ChargeStartFeedbacks?.StopFeedbacks();
						Weapons[CurrentChargeIndex - 1].ChargeCompleteFeedbacks?.StopFeedbacks();	
					}
					ForceWeaponAttack(CurrentChargeIndex);	
				}
			}

			if (!Weapons[CurrentChargeIndex].ChargeComplete)
			{
				InterruptStepCharge(CurrentChargeIndex);
			}

			ResetCharge();
		}

        /// <summary>
        /// 强制武器在指定步骤打开
        /// </summary>
        /// <param name="index"></param>
        protected virtual void ForceWeaponAttack(int index)
		{
			Weapons[index].TargetWeapon.TurnWeaponOn();
		}

        /// <summary>
        /// 返回蓄力序列中当前武器的索引
        /// </summary>
        /// <returns></returns>
        protected virtual int FindCurrentWeaponIndex()
		{
			float elapsedTime = CurrentTime - _chargingStartedAt;

			if (elapsedTime < DelayBeforeUse)
			{
				return -1;
			}
			
			for (int i = 0; i < Weapons.Count; i++)
			{
				if (Weapons[i].ChargeTotalDuration > elapsedTime)
				{
					return i;
				}
			}
			return Weapons.Count - 1;
		}

        /// <summary>
        /// 如果指定索引处的武器存在，则返回true
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected virtual bool WeaponExists(int index)
		{
			return (index >= 0) && (index < Weapons.Count);
		}

        /// <summary>
        /// 当蓄力武器被激活时，我们开始蓄力
        /// </summary>
        public override void TurnWeaponOn()
		{
			base.TurnWeaponOn();
			StartChargeSequence();
		}

        /// <summary>
        /// 当蓄力武器的输入被释放时，我们停止蓄力
        /// </summary>
        public override void WeaponInputReleased()
		{
			base.WeaponInputReleased();
			StopChargeSequence();
		}

		public override void FlipWeapon()
		{
			base.FlipWeapon();
			for (int i = 0; i < Weapons.Count; i++)
			{
				if (Weapons[i].FlipWhenChargeWeaponFlips)
				{
					Weapons[i].TargetWeapon.Flipped = Flipped;
				}
			}
		}
	}
}

 using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    ///  由DamageResistanceProcessor使用，这个类定义了针对某种类型伤害的抗性。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Health/Damage Resistance")]
	public class DamageResistance : TopDownMonoBehaviour
	{
		public enum DamageModifierModes { Multiplier, Flat }
		public enum KnockbackModifierModes { Multiplier, Flat }

		[Header("General一般")]
		/// The priority of this damage resistance. This will be used to determine in what order damage resistances should be evaluated. Lowest priority means evaluated first.
		[Tooltip("这个伤害抗性的优先级。这将用于确定应该按什么顺序评估伤害抗性。最低优先级意味着首先评估。”")]
		public float Priority = 0;
		/// The label of this damage resistance. Used for organization, and to activate/disactivate a resistance by its label.
		[Tooltip("这个伤害抗性的标签。用于组织，以及通过其标签激活/禁用抗性。")]
		public string Label = "";
		
		[Header("Damage Resistance Settings伤害抗性设置")] 
		/// Whether this resistance impacts base damage or typed damage
		[Tooltip("这种抗性是否影响基础伤害或类型伤害")]
		public DamageTypeModes DamageTypeMode = DamageTypeModes.BaseDamage;
		/// In TypedDamage mode, the type of damage this resistance will interact with
		[Tooltip("在TypedDamage模式下，这个抗性将与之交互的伤害类型")]
		[MMEnumCondition("DamageTypeMode", (int)DamageTypeModes.TypedDamage)]
		public DamageType TypeResistance;
		/// the way to reduce (or increase) received damage. Multiplier will multiply incoming damage by a multiplier, flat will subtract a constant value from incoming damage. 
		[Tooltip("减少（或增加）受到的伤害的方式。乘数将通过一个乘数来乘以进入的伤害，而固定伤害将从进入的伤害中减去一个恒定的值")]
		public DamageModifierModes DamageModifierMode = DamageModifierModes.Multiplier;

		[Header("Damage Modifiers损害修饰符")]
		/// In multiplier mode, the multiplier to apply to incoming damage. 0.5 will reduce it in half, while a value of 2 will create a weakness to the specified damage type, and damages will double.
		[Tooltip("在乘数模式下，乘数适用于传入的伤害。0.5会将其减少一半，而2则会造成特定伤害类型的弱点，并且伤害会翻倍。")]
		[MMEnumCondition("DamageModifierMode", (int)DamageModifierModes.Multiplier)]
		public float DamageMultiplier = 0.25f;
		/// In flat mode, the amount of damage to subtract every time that type of damage is received
		[Tooltip("在固定伤害模式下，每次受到该类型伤害时要减去的伤害量")]
		[MMEnumCondition("DamageModifierMode", (int)DamageModifierModes.Flat)]
		public float FlatDamageReduction = 10f;
		/// whether or not incoming damage of the specified type should be clamped between a min and a max
		[Tooltip("特定类型的传入伤害值是否应在最小和最大之间")] 
		public bool ClampDamage = false;
		/// the values between which to clamp incoming damage
		[Tooltip("数值处于传入的伤害值区间内")]
		[MMVector("Min","Max")]
		public Vector2 DamageModifierClamps = new Vector2(0f,10f);

		[Header("Condition Change条件改变")]
		/// whether or not condition change for that type of damage is allowed or not
		[Tooltip("是否允许更改该类型伤害的条件")]
		public bool PreventCharacterConditionChange = false;
		/// whether or not movement modifiers are allowed for that type of damage or not
		[Tooltip("是否允许为该类型伤害使用移动修饰符")]
		public bool PreventMovementModifier = false;
		
		[Header("Knockback击退")] 
		/// if this is true, knockback force will be ignored and not applied
		[Tooltip("如果这是真的，击退将被忽略而不应用")]
		public bool ImmuneToKnockback = false;
		/// the way to reduce (or increase) received knockback. Multiplier will multiply incoming knockback intensity by a multiplier, flat will subtract a constant value from incoming knockback intensity. 
		[Tooltip("减少（或增加）受到的击退强度的方法。乘数将通过一个乘数来乘以进入的击退强度，而固定伤害将从进入的击退强度中减去一个恒定的值")]
		public KnockbackModifierModes KnockbackModifierMode = KnockbackModifierModes.Multiplier;
		/// In multiplier mode, the multiplier to apply to incoming knockback. 0.5 will reduce it in half, while a value of 2 will create a weakness to the specified damage type, and knockback intensity will double.
		[Tooltip("在乘数模式下，应用于进入击退的乘数。0.5会将其减少一半，而2则会造成特定伤害类型的弱点，并且击退强度会翻倍。")]
		[MMEnumCondition("KnockbackModifierMode", (int)DamageModifierModes.Multiplier)]
		public float KnockbackMultiplier = 1f;
		/// In flat mode, the amount of knockback to subtract every time that type of damage is received
		[Tooltip("在固定伤害模式下，每次受到该类型伤害时需要减去的击退量")]
		[MMEnumCondition("KnockbackModifierMode", (int)DamageModifierModes.Flat)]
		public float FlatKnockbackMagnitudeReduction = 10f;
		/// whether or not incoming knockback of the specified type should be clamped between a min and a max
		[Tooltip("指定类型的传入击退值是否应该处于在min和Max之间")] 
		public bool ClampKnockback = false;
		/// the values between which to clamp incoming knockback magnitude
		[Tooltip("数值处于传入的击退值区间内")]
		[MMCondition("ClampKnockback", true)]
		public float KnockbackMaxMagnitude = 10f;

		[Header("Feedbacks反馈")]
		/// This feedback will only be triggered if damage of the matching type is received
		[Tooltip("只有当收到匹配类型的伤害时才会触发此反馈")]
		public MMFeedbacks OnDamageReceived;
		/// whether or not this feedback can be interrupted (stopped) when that type of damage is interrupted
		[Tooltip("当这种类型的伤害被打断时，这种反馈是否可以被打断（停止）")]
		public bool InterruptibleFeedback = false;
		/// if this is true, the feedback will always be preventively stopped before playing
		[Tooltip("如果这是真的，反馈将始终在播放前被预防性地停止")]
		public bool AlwaysInterruptFeedbackBeforePlay = false;
		/// whether this feedback should play if damage received is zero
		[Tooltip("如果收到的伤害为零，这个反馈是否应该发挥作用")]
		public bool TriggerFeedbackIfDamageIsZero = false;

        /// <summary>
        /// 在清醒时，我们初始化我们的反馈
        /// </summary>
        protected virtual void Awake()
		{
			OnDamageReceived?.Initialization(this.gameObject);
		}

        /// <summary>
        /// 当受到伤害时，通过减少伤害并输出结果伤害
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="type"></param>
        /// <param name="damageApplied"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual float ProcessDamage(float damage, DamageType type, bool damageApplied)
		{
			if (!this.gameObject.activeInHierarchy)
			{
				return damage;
			}
			
			if ((type == null) && (DamageTypeMode != DamageTypeModes.BaseDamage))
			{
				return damage;
			}

			if ((type != null) && (DamageTypeMode == DamageTypeModes.BaseDamage))
			{
				return damage;
			}

			if ((type != null) && (type != TypeResistance))
			{
				return damage;
			}

            // 使用伤害调整或减少
            switch (DamageModifierMode)
			{
				case DamageModifierModes.Multiplier:
					damage = damage * DamageMultiplier;
					break;
				case DamageModifierModes.Flat:
					damage = damage - FlatDamageReduction;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

            // 夹伤害
            damage = ClampDamage ? Mathf.Clamp(damage, DamageModifierClamps.x, DamageModifierClamps.y) : damage;

			if (damageApplied)
			{
				if (!TriggerFeedbackIfDamageIsZero && (damage == 0))
				{
					// 什么都不做
				}
				else
				{
					if (AlwaysInterruptFeedbackBeforePlay)
					{
						OnDamageReceived?.StopFeedbacks();
					}
					OnDamageReceived?.PlayFeedbacks(this.transform.position);	
				}
			}

			return damage;
		}

        /// <summary>
        /// 处理击退输入值并返回可能被伤害抗性修改的值
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="type"></param>
        /// <param name="damageApplied"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public virtual Vector3 ProcessKnockback(Vector3 knockback, DamageType type)
		{
			if (!this.gameObject.activeInHierarchy)
			{
				return knockback;
			}

			if ((type == null) && (DamageTypeMode != DamageTypeModes.BaseDamage))
			{
				return knockback;
			}

			if ((type != null) && (DamageTypeMode == DamageTypeModes.BaseDamage))
			{
				return knockback;
			}

			if ((type != null) && (type != TypeResistance))
			{
				return knockback;
			}

            // 使用伤害调整或减少
            switch (KnockbackModifierMode)
			{
				case KnockbackModifierModes.Multiplier:
					knockback = knockback * KnockbackMultiplier;
					break;
				case KnockbackModifierModes.Flat:
					float magnitudeReduction = Mathf.Clamp(Mathf.Abs(knockback.magnitude) - FlatKnockbackMagnitudeReduction, 0f, Single.MaxValue);
					knockback = knockback.normalized * magnitudeReduction * Mathf.Sign(knockback.magnitude);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			// 夹伤害
			knockback = ClampKnockback ? Vector3.ClampMagnitude(knockback, KnockbackMaxMagnitude) : knockback;

			return knockback;
		}
	}
}

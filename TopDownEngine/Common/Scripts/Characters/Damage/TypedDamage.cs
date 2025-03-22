using System;
using MoreMountains.Tools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个用于存储和定义类型伤害影响的类：造成的伤害、状态或移动速度变化等
    /// </summary>
    [Serializable]
	public class TypedDamage 
	{
		/// the type of damage associated to this definition
		[Tooltip("与此定义相关的伤害类型")]
		public DamageType AssociatedDamageType;
		/// The min amount of health to remove from the player's health
		[Tooltip("从玩家生命值中移除的最小生命值")]
		public float MinDamageCaused = 10f;
		/// The max amount of health to remove from the player's health
		[Tooltip("从玩家生命值中移除的最大生命值")]
		public float MaxDamageCaused = 10f;
		
		/// whether or not this damage, when applied, should force the character into a specified condition
		[Tooltip("这种伤害在施加时是否应该强制角色进入指定的状态")] 
		public bool ForceCharacterCondition = false;
		/// when in forced character condition mode, the condition to which to swap
		[Tooltip("在强制角色状态模式下，要切换到的状态")]
		[MMCondition("ForceCharacterCondition", true)]
		public CharacterStates.CharacterConditions ForcedCondition;
		/// when in forced character condition mode, whether or not to disable gravity
		[Tooltip("在强制角色状态模式下，是否禁用重力")]
		[MMCondition("ForceCharacterCondition", true)]
		public bool DisableGravity = false;
		/// when in forced character condition mode, whether or not to reset controller forces
		[Tooltip("在强制角色状态模式下，是否重置控制器强制")]
		[MMCondition("ForceCharacterCondition", true)]
		public bool ResetControllerForces = false;
		/// when in forced character condition mode, the duration of the effect, after which condition will be reverted 
		[Tooltip("在强制角色状态模式下，效果的持续时间，之后状态将恢复")]
		[MMCondition("ForceCharacterCondition", true)]
		public float ForcedConditionDuration = 3f;
		
		/// whether or not to apply a movement multiplier to the damaged character
		[Tooltip("是否对受伤的角色施加移动倍增器")] 
		public bool ApplyMovementMultiplier = false;
		/// the movement multiplier to apply when ApplyMovementMultiplier is true 
		[Tooltip("当ApplyMovementMultiplier为true时应用的移动乘数")]
		[MMCondition("ApplyMovementMultiplier", true)]
		public float MovementMultiplier = 0.5f;
		/// the duration of the movement multiplier, if ApplyMovementMultiplier is true
		[Tooltip("如果ApplyMovementMultiplier为true，则为移动倍增器的持续时间")]
		[MMCondition("ApplyMovementMultiplier", true)]
		public float MovementMultiplierDuration = 2f;
		
		

		protected int _lastRandomFrame = -1000;
		protected float _lastRandomValue = 0f;

		public virtual float DamageCaused
		{
			get
			{
				if (Time.frameCount != _lastRandomFrame)
				{
					_lastRandomValue = Random.Range(MinDamageCaused, MaxDamageCaused);
					_lastRandomFrame = Time.frameCount;
				}
				return _lastRandomValue;
			}
		} 
	}	
}


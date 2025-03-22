using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 使用此选择器可以中断对拾取它的角色造成的指定类型的所有伤害
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Damage Over Time Interrupter")]
	public class DamageOverTimeInterrupter : PickableItem
	{
		[Header("Damage Over Time Interrupter伤害超时中断")]
		/// whether interrupted damage over time should be of a specific type, or if all damage should be interrupted 
		[Tooltip("伤害时间是否应该被特定类型的伤害中断，或者是否所有伤害都应该被中断")]
		public bool InterruptByTypeOnly = false;
		/// The type of damage over time this should interrupt
		[Tooltip("此伤害时间应中断的伤害类型")]
		[MMCondition("InterruptByTypeOnly", true)]
		public DamageType TargetDamageType;
		/// if this is true, only player characters can pick this up
		[Tooltip("如果为真，只有玩家角色可以拾取此物品")]
		public bool OnlyForPlayerCharacter = true;

        /// <summary>
        /// 当某物与Stimpack碰撞时触发
        /// </summary>
        /// <param name="collider">Other.</param>
        protected override void Pick(GameObject picker)
		{
			Character character = picker.gameObject.MMGetComponentNoAlloc<Character>();
			if (OnlyForPlayerCharacter && (character != null) && (_character.CharacterType != Character.CharacterTypes.Player))
			{
				return;
			}

			Health characterHealth = picker.gameObject.MMGetComponentNoAlloc<Health>();
            // 否则，我们给予玩家生命值
            if (characterHealth != null)
			{
				if (InterruptByTypeOnly)
				{
					characterHealth.InterruptAllDamageOverTimeOfType(TargetDamageType);	
				}
				else
				{
					characterHealth.InterruptAllDamageOverTime();	
				}
			}            
		}
	}
}
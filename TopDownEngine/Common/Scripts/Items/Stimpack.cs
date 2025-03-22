using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个急救包/生命值奖励，拾取时可恢复生命值
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Stimpack")]
	public class Stimpack : PickableItem
	{
		[Header("Stimpack急救包")]
		/// The amount of points to add when collected
		[Tooltip("收集时要增加的生命值数量")]
		public float HealthToGive = 10f;
		/// if this is true, only player characters can pick this up
		[Tooltip("如果这是真的，只有玩家角色可以拾取它")]
		public bool OnlyForPlayerCharacter = true;

        /// <summary>
        /// 当有物体与急救包发生碰撞时触发
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
				characterHealth.ReceiveHealth(HealthToGive, gameObject);
			}            
		}
	}
}
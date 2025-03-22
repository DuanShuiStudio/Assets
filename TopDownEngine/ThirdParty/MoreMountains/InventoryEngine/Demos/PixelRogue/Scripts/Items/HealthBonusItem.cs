using UnityEngine;
using System;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "HealthBonusItem", menuName = "MoreMountains/InventoryEngine/HealthBonusItem", order = 1)]
	[Serializable]
    /// <summary>
    /// 一个生命值物品的演示类
    /// </summary>
    public class HealthBonusItem : InventoryItem 
	{
		[Header("demo-Health Bonus生命值加成")]
        /// 使用该物品时给玩家增加的生命值数量
        public int HealthBonus;

        /// <summary>
        /// 使用该物品时会发生什么
        /// </summary>
        public override bool Use(string playerID)
		{
			base.Use(playerID);
            // 这是你会增加角色生命值的地方
            // 使用类似的东西：
            // Player.Life += HealthValue;
            // 当然，这一切都取决于你的游戏代码库
            Debug.LogFormat("增加角色 "+playerID+ "的生命值通过" + HealthBonus);
			return true;
		}
		
	}
}
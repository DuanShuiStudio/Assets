using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "BombItem", menuName = "MoreMountains/InventoryEngine/BombItem", order = 2)]
	[Serializable]
    /// <summary>
    /// 一个炸弹物品的演示类
    /// </summary>
    public class BombItem : InventoryItem 
	{
        /// <summary>
        /// 当炸弹被使用时，我们会显示一条调试信息以表明它起作用了
        /// 在真实的游戏中，你可能会生成它
        /// </summary>
        public override bool Use(string playerID)
		{
			base.Use(playerID);
			Debug.LogFormat("炸弹爆炸");
			return true;
		}		
	}
}
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
	[CreateAssetMenu(fileName = "ArmorItem", menuName = "MoreMountains/InventoryEngine/ArmorItem", order = 2)]
	[Serializable]
    /// <summary>
    /// 一个示例盔甲物品的演示类
    /// </summary>
    public class ArmorItem : InventoryItem 
	{
		[Header("demo-Armor盔甲")]
		public int ArmorIndex;

        /// <summary>
        /// 当穿上盔甲时会发生什么
        /// </summary>
        public override bool Equip(string playerID)
		{
			base.Equip(playerID);
			TargetInventory(playerID).TargetTransform.GetComponent<InventoryDemoCharacter>().SetArmor(ArmorIndex);
			return true;
		}

        /// <summary>
        /// 当卸下盔甲时会发生什么
        /// </summary>
        public override bool UnEquip(string playerID)
		{
			base.UnEquip(playerID);
			TargetInventory(playerID).TargetTransform.GetComponent<InventoryDemoCharacter>().SetArmor(0);
			return true;
		}		
	}
}
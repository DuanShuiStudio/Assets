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
	[CreateAssetMenu(fileName = "WeaponItem", menuName = "MoreMountains/InventoryEngine/WeaponItem", order = 2)]
	[Serializable]
    /// <summary>
    /// 一个武器物品的演示类。
    /// </summary>
    public class WeaponItem : InventoryItem 
	{
		[Header("demo-Weapon武器")]
        /// 穿上武器时用于显示的精灵
        public Sprite WeaponSprite;

        /// <summary>
        /// 使用该物品时会发生什么
        /// </summary>
        public override bool Equip(string playerID)
		{
			base.Equip(playerID);
			TargetInventory(playerID).TargetTransform.GetComponent<InventoryDemoCharacter>().SetWeapon(WeaponSprite,this);
			return true;
		}

        /// <summary>
        /// 使用该物品时会发生什么
        /// </summary>
        public override bool UnEquip(string playerID)
		{
			base.UnEquip(playerID);
			TargetInventory(playerID).TargetTransform.GetComponent<InventoryDemoCharacter>().SetWeapon(null,this);
			return true;
		}
		
	}
}
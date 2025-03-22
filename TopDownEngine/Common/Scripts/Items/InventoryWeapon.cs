using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[CreateAssetMenu(fileName = "InventoryWeapon", menuName = "MoreMountains/TopDownEngine/InventoryWeapon", order = 2)]
	[Serializable]
    /// <summary>
    /// TopDown引擎中的武器物品
    /// </summary>
    public class InventoryWeapon : InventoryItem 
	{
        /// 可能的自动装备模式
        public enum AutoEquipModes { NoAutoEquip, AutoEquip, AutoEquipIfEmptyHanded }
        
		[Header("Weapon武器")]
		[MMInformation("在这里您需要绑定拾取该物品时要装备的武器", MMInformationAttribute.InformationType.Info,false)]
		/// the weapon to equip
		[Tooltip("要装备的武器")]
		public Weapon EquippableWeapon;
		/// how to equip this weapon when picked : not equip it, automatically equip it, or only equip it if no weapon is currently equipped
		[Tooltip("拾取该武器时如何装备：不装备、自动装备，或仅在当前未装备任何武器时才装备")]
		public AutoEquipModes AutoEquipMode = AutoEquipModes.NoAutoEquip;
		/// the ID of the CharacterHandleWeapon you want this weapon to be equipped to
		[Tooltip("您希望装备此武器的角色的ID")]
		public int HandleWeaponID = 1;

        /// <summary>
        /// 当我们拿起武器时，就装备它
        /// </summary>
        public override bool Equip(string playerID)
		{
			EquipWeapon (EquippableWeapon, playerID);
			return true;
		}

        /// <summary>
        /// 当我们丢弃或卸下武器时，就将其移除
        /// </summary>
        public override bool UnEquip(string playerID)
		{
            // 如果这是当前装备的武器，我们就将其卸下
            if (this.TargetEquipmentInventory(playerID) == null)
			{
				return false;
			}

			if (this.TargetEquipmentInventory(playerID).InventoryContains(this.ItemID).Count > 0)
			{
				EquipWeapon(null, playerID);
			}

			return true;
		}

        /// <summary>
        /// 抓取CharacterHandleWeapon组件并设置武器
        /// </summary>
        /// <param name="newWeapon">New weapon.</param>
        protected virtual void EquipWeapon(Weapon newWeapon, string playerID)
		{
			if (EquippableWeapon == null)
			{
				return;
			}
			if (TargetInventory(playerID).Owner == null)
			{
				return;
			}

			Character character = TargetInventory(playerID).Owner.GetComponentInParent<Character>();

			if (character == null)
			{
				return;
			}

            // 我们将武器装备到所选的CharacterHandleWeapon上
            CharacterHandleWeapon targetHandleWeapon = null;
			CharacterHandleWeapon[] handleWeapons = character.GetComponentsInChildren<CharacterHandleWeapon>();
			foreach (CharacterHandleWeapon handleWeapon in handleWeapons)
			{
				if (handleWeapon.HandleWeaponID == HandleWeaponID)
				{
					targetHandleWeapon = handleWeapon;
				}
			}
			
			if (targetHandleWeapon != null)
			{
				targetHandleWeapon.ChangeWeapon(newWeapon, this.ItemID);
			}
		}
	}
}
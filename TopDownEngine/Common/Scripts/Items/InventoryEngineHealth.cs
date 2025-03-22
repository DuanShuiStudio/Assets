using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[CreateAssetMenu(fileName = "InventoryEngineHealth", menuName = "MoreMountains/TopDownEngine/InventoryEngineHealth", order = 1)]
	[Serializable]
    /// <summary>
    /// 可拾取的生命值物品
    /// </summary>
    public class InventoryEngineHealth : InventoryItem 
	{
		[Header("Health")]
		[MMInformation("这里您需要指定使用此物品时获得的生命值数量", MMInformationAttribute.InformationType.Info,false)]
		/// the amount of health to add to the player when the item is used
		[Tooltip("使用该物品时给玩家增加的生命值数量")]
		public float HealthBonus;

        /// <summary>
        ///使用该物品时，我们会尝试获取角色的生命值组件，如果它存在，我们就增加相应的生命值奖励
        /// </summary>
        public override bool Use(string playerID)
		{
			base.Use(playerID);

			if (TargetInventory(playerID).Owner == null)
			{
				return false;
			}

			Health characterHealth = TargetInventory(playerID).Owner.GetComponent<Health>();
			if (characterHealth != null)
			{
				characterHealth.ReceiveHealth(HealthBonus,TargetInventory(playerID).gameObject);
				return true;
			}
			else
			{
				return false;
			}
		}

	}
}
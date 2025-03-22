using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[CreateAssetMenu(fileName = "InventoryEngineKey", menuName = "MoreMountains/TopDownEngine/InventoryEngineKey", order = 1)]
	[Serializable]
    /// <summary>
    /// 可拾取的钥匙物品
    /// </summary>
    public class InventoryEngineKey : InventoryItem 
	{
        /// <summary>
        /// 使用该物品时，我们会尝试获取角色的生命值组件，如果它存在，我们就增加相应的生命值奖励
        /// </summary>
        public override bool Use(string playerID)
		{
			base.Use(playerID);
			return true;
		}
	}
}
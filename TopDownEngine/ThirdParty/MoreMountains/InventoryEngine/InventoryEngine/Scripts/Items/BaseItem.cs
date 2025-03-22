using UnityEngine;
using System;

namespace MoreMountains.InventoryEngine
{	
	[CreateAssetMenu(fileName = "BaseItem", menuName = "MoreMountains/InventoryEngine/BaseItem", order = 0)]
	[Serializable]
    /// <summary>
    /// 基础项类，当你的对象没有做任何特殊处理时使用。
    /// </summary>
    public class BaseItem : InventoryItem 
	{
				
	}
}
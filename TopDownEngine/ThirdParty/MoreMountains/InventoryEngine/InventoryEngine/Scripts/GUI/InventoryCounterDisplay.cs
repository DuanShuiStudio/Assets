using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 使用这个类来显示一个或多个给定物品在一组或多组给定库存中的总数量
    /// </summary>
    public class InventoryCounterDisplay : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[Header("Items & Inventories物品和库存")]
        /// 要计数的项目列表
        public List<InventoryItem> Item;
        /// 用于计数物品的库存
        public List<Inventory> TargetInventories;

		[Header("Display显示")]
        /// 用于更新目标库存中物品总数量的文本用户界面
        public Text TargetText;
        /// 用于文本的格式
        public string DisplayFormat = "0";

        /// <summary>
        /// 用于更新目标文本的公共方法，该方法会用目标库存中物品的总数量来更新文本
        /// </summary>
        public void UpdateText()
		{
			TargetText.text = ComputeQuantity().ToString(DisplayFormat);
		}

        /// <summary>
        /// 处理库存事件，如有必要则更新文本
        /// </summary>
        /// <param name="inventoryEvent"></param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (!ShouldUpdate(inventoryEvent.TargetInventoryName))
			{
				return;
			}
			
			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.ContentChanged:
					UpdateText();
					break;
				
				case MMInventoryEventType.InventoryLoaded:
					UpdateText();
					break;
			}
		}

        /// <summary>
        /// 计算目标库存中物品的数量
        /// </summary>
        /// <returns></returns>
        public virtual int ComputeQuantity()
		{
			int count = 0;
			foreach (Inventory inventory in TargetInventories)
			{
				foreach (InventoryItem item in Item)
				{
					count += inventory.GetQuantity(item.ItemID);
				}
			}
			return count;
		}

        /// <summary>
        /// 如果传入参数中的库存名称是目标库存之一，则返回真；否则返回假
        /// </summary>
        /// <param name="inventoryName"></param>
        /// <returns></returns>
        public virtual bool ShouldUpdate(string inventoryName)
		{
			bool shouldUpdate = false;
			foreach (Inventory inventory in TargetInventories)
			{
				if (inventory.name == inventoryName)
				{
					shouldUpdate = true;
				}
			}
			return shouldUpdate;
		}

        /// <summary>
        /// 在启用时，我们开始监听MMInventoryEvents
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听MMInventoryEvents
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}	
}


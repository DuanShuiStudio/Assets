using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 这个类展示了如何从任何类中监听 MMInventoryEvents 的示例。
    /// </summary>
    public class EventTester : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
        /// <summary>
        /// 当我们捕获到 MMInventoryEvent 时，我们会根据其类型进行筛选，并显示有关使用物品的信息
        /// </summary>
        /// <param name="inventoryEvent"></param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.ItemUsed)
			{
				MMDebug.DebugLogTime("item used");
				MMDebug.DebugLogTime("ItemID : "+inventoryEvent.EventItem.ItemID);
				MMDebug.DebugLogTime("Item name : "+inventoryEvent.EventItem.ItemName);
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听 MMInventoryEvents
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听 MMInventoryEvents
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}

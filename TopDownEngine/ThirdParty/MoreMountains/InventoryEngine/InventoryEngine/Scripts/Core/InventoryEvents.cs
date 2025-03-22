using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 可能与库存相关的事件
    /// </summary>
    public enum MMInventoryEventType { Pick, Select, Click, Move, UseRequest, ItemUsed, EquipRequest, ItemEquipped, UnEquipRequest, ItemUnEquipped, Drop, Destroy, Error, Redraw, ContentChanged, InventoryOpens, InventoryCloseRequest, InventoryCloses, InventoryLoaded }

    /// <summary>
    /// 在库存引擎中，库存事件被用于告知其他相关类库存发生了某些情况
    /// </summary>
    public struct MMInventoryEvent
	{
        /// 事件的类型
        public MMInventoryEventType InventoryEventType;
        /// 事件中涉及的槽位
        public InventorySlot Slot;
        /// 发生事件的库存的名称
        public string TargetInventoryName;
        /// 事件中涉及的物品
        public InventoryItem EventItem;
        /// 事件中涉及的数量
        public int Quantity;
        /// 库存中发生事件的索引位置
        public int Index;
        /// 触发此事件的玩家的唯一ID
        public string PlayerID;

		public MMInventoryEvent(MMInventoryEventType eventType, InventorySlot slot, string targetInventoryName, InventoryItem eventItem, int quantity, int index, string playerID)
		{
			InventoryEventType = eventType;
			Slot = slot;
			TargetInventoryName = targetInventoryName;
			EventItem = eventItem;
			Quantity = quantity;
			Index = index;
			PlayerID = (playerID != "") ? playerID : "Player1";
		}

		static MMInventoryEvent e;
		public static void Trigger(MMInventoryEventType eventType, InventorySlot slot, string targetInventoryName, InventoryItem eventItem, int quantity, int index, string playerID)
		{
			e.InventoryEventType = eventType;
			e.Slot = slot;
			e.TargetInventoryName = targetInventoryName;
			e.EventItem = eventItem;
			e.Quantity = quantity;
			e.Index = index;
			e.PlayerID = (playerID != "") ? playerID : "Player1";
			MMEventManager.TriggerEvent(e);
		}
	}
}
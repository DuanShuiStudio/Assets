using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 这个测试类让你可以使用库存 API 进行操作
    /// 你可以在 PixelRogueRoom2 演示场景中看到它的实际运行效果
    /// </summary>
    public class InventoryTester : MonoBehaviour
	{
		[Header("Add item添加物品")]
        /// 按下 AddItemTest 按钮时添加的物品
        public InventoryItem AddItem;
        /// 要添加的物品数量
        public int AddItemQuantity;
        /// 要添加物品的库存
        public Inventory AddItemInventory;
        /// 一个测试按钮
        [MMInspectorButton("AddItemTest")]
		public bool AddItemTestButton;
        
		[Header("Add item at在某处添加物品")]
        /// 在特定索引处添加的库存物品
        public InventoryItem AddItemAtItem;
        /// 要添加的物品数量
        public int AddItemAtQuantity;
        /// 应该添加物品的索引位置
        public int AddItemAtIndex;
        /// 要添加物品的库存
        public Inventory AddItemAtInventory;
        /// 一个测试按钮
        [MMInspectorButton("AddItemAtTest")] 
		public bool AddItemAtTestButton;

		[Header("Move Item移动物品")]
        /// 要移动物品的起始索引位置
        public int MoveItemOrigin;
        /// 我们想要将起始索引位置的物品移动到的目标索引位置
        public int MoveItemDestination;
        /// 要在其中移动物品的库存
        public Inventory MoveItemInventory;
        /// 一个测试按钮
        [MMInspectorButton("MoveItemTest")] 
		public bool MoveItemTestButton;

		[Header("Move Item To Inventory将物品移动到库存中")]
        /// 要移动物品的起始索引位置
        public int MoveItemToInventoryOriginIndex;
        /// 我们想要将起始索引位置的物品移动到的目标索引位置
        public int MoveItemToInventoryDestinationIndex = -1;
        /// 要移动物品的源库存
        public Inventory MoveItemToOriginInventory;
        /// 要移动物品到目标库存
        public Inventory MoveItemToDestinationInventory;
        /// 一个测试按钮
        [MMInspectorButton("MoveItemToInventory")] 
		public bool MoveItemToInventoryTestButton;

		[Header("Remove Item移除物品")]
        /// 要移除物品的索引位置
        public int RemoveItemIndex;
        /// 在指定索引处要移除的物品数量
        public int RemoveItemQuantity;
        /// 要移除物品的库存
        public Inventory RemoveItemInventory;
        /// 一个测试按钮
        [MMInspectorButton("RemoveItemTest")] 
		public bool RemoveItemTestButton;

		[Header("Empty Inventory清空库存")]
        /// 要清空的库存
        public Inventory EmptyTargetInventory;
        /// 一个测试按钮
        [MMInspectorButton("EmptyInventoryTest")] 
		public bool EmptyInventoryTestButton;

        /// <summary>
        /// 测试添加物品的方法
        /// </summary>
        protected virtual void AddItemTest()
		{
			AddItemInventory.AddItem(AddItem, AddItemQuantity);
		}

        /// <summary>
        /// 测试在指定索引处添加物品的方法
        /// </summary>
        protected virtual void AddItemAtTest()
		{
			AddItemAtInventory.AddItemAt(AddItemAtItem, AddItemAtQuantity, AddItemAtIndex);
		}

        /// <summary>
        /// 测试将物品从索引 A 移动到索引 B 的方法
        /// </summary>
        protected virtual void MoveItemTest()
		{
			MoveItemInventory.MoveItem(MoveItemOrigin, MoveItemDestination);
		}

        /// <summary>
        /// 测试将物品从索引 A 移动到索引 B 的方法
        /// </summary>
        protected virtual void MoveItemToInventory()
		{
			MoveItemToOriginInventory.MoveItemToInventory(MoveItemToInventoryOriginIndex, MoveItemToDestinationInventory, MoveItemToInventoryDestinationIndex);
		}

        /// <summary>
        /// 测试在指定索引处移除物品的方法
        /// </summary>
        protected virtual void RemoveItemTest()
		{
			RemoveItemInventory.RemoveItem(RemoveItemIndex, RemoveItemQuantity);
		}

        /// <summary>
        /// 测试清空目标库存的方法
        /// </summary>
        protected virtual void EmptyInventoryTest()
		{
			EmptyTargetInventory.EmptyInventory();
		}
	}
}
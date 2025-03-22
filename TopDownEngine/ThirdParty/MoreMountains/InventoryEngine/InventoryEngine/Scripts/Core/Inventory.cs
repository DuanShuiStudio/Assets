using UnityEngine;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;

namespace MoreMountains.InventoryEngine
{
	[Serializable]
    /// <summary>
    /// 基础物品栏类
    /// 将处理存储物品、保存和加载其内容、向其中添加物品、移除物品、装备它们等
    /// </summary>
    public class Inventory : MonoBehaviour, MMEventListener<MMInventoryEvent>, MMEventListener<MMGameEvent>
	{
		public static List<Inventory> RegisteredInventories;
		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		protected static void InitializeStatics()
		{
			RegisteredInventories = null;
		}

        /// 不同的可能的物品栏类型，主要是常规的，装备将具有特殊的行为（用于你放置已装备武器/盔甲等的插槽）。
        public enum InventoryTypes { Main, Equipment }

		[Header("Player ID")] 
		/// a unique ID used to identify the owner of this inventory
		[Tooltip("一个用于识别此物品栏所有者的唯一ID")]
		public string PlayerID = "Player1";

		/// the complete list of inventory items in this inventory
		[Tooltip("这是你的物品栏内容的实时视图。不要通过检查器修改这个列表，它只是为了控制目的而可见")]
		[MMReadOnly]
		public InventoryItem[] Content;

		[Header("物品栏类型")]
        /// 这个物品栏是主要物品栏还是装备物品栏
        [Tooltip("在这里你可以定义你的物品栏类型。主要的有“常规”物品栏。装备物品栏将绑定到某个物品类，并具有专用选项")]
		public InventoryTypes InventoryType = InventoryTypes.Main;

		[Header("Target Transform目标变换。")]
		[Tooltip("目标变换是你场景中的任何变换，从物品栏中掉落的物体将在此生成.")]
		/// the transform at which objects will be spawned when dropped
		public Transform TargetTransform;

		[Header("Persistence持久化")]
		[Tooltip("在这里你可以定义此物品栏是否应对加载和保存事件做出响应。如果你不希望将你的物品栏保存到磁盘，将其设置为false。你也可以在开始时重置它，以确保在此关卡开始时它始终为空")]
        /// 此物品栏是否会被保存和加载
        public bool Persistent = true;
        /// 此物品栏是否应在开始时重置
        public bool ResetThisInventorySaveOnStart = false;
        
		[Header("Debug调试")]
        /// 如果为true，将在其检查器中绘制物品栏的内容
        [Tooltip("Inventory组件就像你的数据库和控制器部分。它不会在屏幕上显示任何东西，你还需要使用InventoryDisplay来显示。在这里你可以决定是否在检查器中输出调试内容（对调试有用）")]
		public bool DrawContentInInspector = false;

        /// 物品栏的所有者（用于游戏中有多个角色的情况）
        public virtual GameObject Owner { get; set; }

        /// 此物品栏中的空闲插槽数量
        public virtual int NumberOfFreeSlots => Content.Length - NumberOfFilledSlots;

        /// 物品栏是否已满（没有剩余的空闲插槽）
        public virtual bool IsFull => NumberOfFreeSlots <= 0;

        /// 已填充插槽的数量
        public int NumberOfFilledSlots
		{
			get
			{
				int numberOfFilledSlots = 0;
				for (int i = 0; i < Content.Length; i++)
				{
					if (!InventoryItem.IsNull(Content[i]))
					{
						numberOfFilledSlots++;
					}
				}
				return numberOfFilledSlots;
			}
		}

		public int NumberOfStackableSlots(string searchedItemID, int maxStackSize)
		{
			int numberOfStackableSlots = 0;
			int i = 0;

			while (i < Content.Length)
			{
				if (InventoryItem.IsNull(Content[i]))
				{
					numberOfStackableSlots += maxStackSize;
				}
				else
				{
					if (Content[i].ItemID == searchedItemID)
					{
						numberOfStackableSlots += maxStackSize - Content[i].Quantity;
					}
				}
				i++;
			}

			return numberOfStackableSlots;
		}

		public static string _resourceItemPath = "Items/";
		public static string _saveFolderName = "InventoryEngine/";
		public static string _saveFileExtension = ".inventory";

        /// <summary>
        /// 返回（如果找到）一个与搜索名称和玩家ID匹配的物品栏
        /// </summary>
        /// <param name="inventoryName"></param>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public static Inventory FindInventory(string inventoryName, string playerID)
		{
			if (inventoryName == null)
			{
				return null;
			}
            
			foreach (Inventory inventory in RegisteredInventories)
			{
				if ((inventory.name == inventoryName) && (inventory.PlayerID == playerID))
				{
					return inventory;
				}
			}
			return null;
		}

        /// <summary>
        /// 在Awake上注册此物品栏
        /// </summary>
        protected virtual void Awake()
		{
			RegisterInventory();
		}

        /// <summary>
        ///注册此物品栏，以便其他脚本稍后可以访问它
        /// </summary>
        protected virtual void RegisterInventory()
		{
			if (RegisteredInventories == null)
			{
				RegisteredInventories = new List<Inventory>();
			}
			if (RegisteredInventories.Count > 0)
			{
				for (int i = RegisteredInventories.Count - 1; i >= 0; i--)
				{
					if (RegisteredInventories[i] == null)
					{
						RegisteredInventories.RemoveAt(i);
					}
				}    
			}
			RegisteredInventories.Add(this);
		}

        /// <summary>
        /// 设置此物品栏的所有者，例如用于应用物品的效果
        /// </summary>
        /// <param name="newOwner">New owner.</param>
        public virtual void SetOwner(GameObject newOwner)
		{
			Owner = newOwner;
		}

        /// <summary>
        /// 尝试添加指定类型的物品。请注意，这是基于名称的
        /// </summary>
        /// <returns><c>true</c>, if item was added, <c>false</c> if it couldn't be added (item null, inventory full).</returns>
        /// <param name="itemToAdd">Item to add.</param>
        public virtual bool AddItem(InventoryItem itemToAdd, int quantity)
		{
            // 如果要添加的物品为null，我们什么也不做并退出
            if (itemToAdd == null)
			{
				Debug.LogWarning(this.name + " : 您要添加到物品栏的物品为null");
				return false;
			}

			List<int> list = InventoryContains(itemToAdd.ItemID);
;
			quantity = CapMaxQuantity(itemToAdd, quantity);

            // 如果物品栏中至少已经有一个这样的物品，并且它是可堆叠的
            if (list.Count > 0 && itemToAdd.MaximumStack > 1)
			{
                // 我们存储与要添加的物品匹配的物品
                for (int i = 0; i < list.Count; i++)
				{
                    // 如果在物品栏中这类物品的其中一个还有空间，我们向其中添加
                    if (Content[list[i]].Quantity < itemToAdd.MaximumStack)
					{
                        // 我们增加物品的数量
                        Content[list[i]].Quantity += quantity;
                        // 如果这超过了最大堆叠量
                        if (Content[list[i]].Quantity > Content[list[i]].MaximumStack)
						{
							InventoryItem restToAdd = itemToAdd;
							int restToAddQuantity = Content[list[i]].Quantity - Content[list[i]].MaximumStack;
                            // 我们限制数量，并将剩余部分作为新物品添加
                            Content[list[i]].Quantity = Content[list[i]].MaximumStack;
							AddItem(restToAdd, restToAddQuantity);
						}
						MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
						return true;
					}
				}
			}
            // 如果我们已经达到了物品栏的最大尺寸，我们就不会添加该物品
            if (NumberOfFilledSlots >= Content.Length)
			{
				return false;
			}
			while (quantity > 0)
			{
				if (quantity > itemToAdd.MaximumStack)
				{
					AddItem(itemToAdd, itemToAdd.MaximumStack);
					quantity -= itemToAdd.MaximumStack;
				}
				else
				{
					AddItemToArray(itemToAdd, quantity);
					quantity = 0;
				}
			}
            // 如果我们仍然在这里，我们在第一个可用插槽中添加物品
            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
			return true;
		}

        /// <summary>
        /// 在所选目的地索引处，将指定数量的指定物品添加到物品栏中
        /// </summary>
        /// <param name="itemToAdd"></param>
        /// <param name="quantity"></param>
        /// <param name="destinationIndex"></param>
        /// <returns></returns>
        public virtual bool AddItemAt(InventoryItem itemToAdd, int quantity, int destinationIndex)
		{
			int tempQuantity = quantity;

			tempQuantity = CapMaxQuantity(itemToAdd, quantity);
			
			if (!InventoryItem.IsNull(Content[destinationIndex]))
			{
				if ((Content[destinationIndex].ItemID != itemToAdd.ItemID) || (Content[destinationIndex].MaximumStack <= 1))
				{
					return false;
				}
				else
				{
					tempQuantity += Content[destinationIndex].Quantity;
				}
			}
			
			if (tempQuantity > itemToAdd.MaximumStack)
			{
				tempQuantity = itemToAdd.MaximumStack;
			}
            
			Content[destinationIndex] = itemToAdd.Copy();
			Content[destinationIndex].Quantity = tempQuantity;

            // 如果我们仍然在这里，我们在第一个可用插槽中添加物品
            MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
			return true;
		}

        /// <summary>
        /// 尝试将第一个参数插槽中的物品移动到第二个插槽
        /// </summary>
        /// <returns><c>true</c>, if item was moved, <c>false</c> otherwise.</returns>
        /// <param name="startIndex">Start index.</param>
        /// <param name="endIndex">End index.</param>
        public virtual bool MoveItem(int startIndex, int endIndex)
		{
			bool swap = false;
            // 如果我们尝试移动的是null，这意味着我们正在尝试移动一个空插槽。
            if (InventoryItem.IsNull(Content[startIndex]))
			{
				Debug.LogWarning("物品栏 : 你正在尝试移动一个空插槽");
				return false;
			}
            // 如果两个对象都是可交换的，我们将交换它们
            if (Content[startIndex].CanSwapObject)
			{
				if (!InventoryItem.IsNull(Content[endIndex]))
				{
					if (Content[endIndex].CanSwapObject)
					{
						swap = true;
					}
				}
			}
            // 如果目标插槽为空
            if (InventoryItem.IsNull(Content[endIndex]))
			{
                // 我们将物品的副本创建到目的地
                Content[endIndex] = Content[startIndex].Copy();
                // 我们移除原始物品
                RemoveItemFromArray(startIndex);
                // 我们提到内容已经改变，并且如果附加了GUI，物品栏可能需要重新绘制
                MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
				return true;
			}
			else
			{
                // 如果我们能够交换对象，我们将尝试进行交换，否则我们返回false，因为我们的目标插槽不是null
                if (swap)
				{
                    // 我们交换物品
                    InventoryItem tempItem = Content[endIndex].Copy();
					Content[endIndex] = Content[startIndex].Copy();
					Content[startIndex] = tempItem;
					MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

        /// <summary>
        /// 这个方法允许您将startIndex处的物品移动到所选的targetInventory，在可选的endIndex处。
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="targetInventory"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public virtual bool MoveItemToInventory(int startIndex, Inventory targetInventory, int endIndex = -1)
		{
            // 如果我们尝试移动的是null，这意味着我们正在尝试移动一个空插槽
            if (InventoryItem.IsNull(Content[startIndex]))
			{
				Debug.LogWarning("物品栏 : 你正在尝试移动一个空插槽");
				return false;
			}

            // 如果我们的目标不为空，我们将退出
            if ( (endIndex >=0) && (!InventoryItem.IsNull(targetInventory.Content[endIndex])) )
			{
				Debug.LogWarning("物品栏 : 目标插槽不为空，无法移动");
				return false;
			}

			InventoryItem itemToMove = Content[startIndex].Copy();

            // 如果我们指定了目标索引，我们将使用它，否则我们正常添加
            if (endIndex >= 0)
			{
				targetInventory.AddItemAt(itemToMove, itemToMove.Quantity, endIndex);    
			}
			else
			{
				targetInventory.AddItem(itemToMove, itemToMove.Quantity);
			}

            // 我们从原始物品栏中移除
            RemoveItem(startIndex, itemToMove.Quantity);

			return true;
		}

        /// <summary>
        /// 从物品栏中移除指定物品
        /// </summary>
        /// <returns><c>true</c>, if item was removed, <c>false</c> otherwise.</returns>
        /// <param name="itemToRemove">Item to remove.</param>
        public virtual bool RemoveItem(int i, int quantity)
		{
			if (i < 0 || i >= Content.Length)
			{
				Debug.LogWarning("物品栏 : 您正尝试从无效索引中移除物品");
				return false;
			}
			if (InventoryItem.IsNull(Content[i]))
			{
				Debug.LogWarning("物品栏 :您正尝试从一个空插槽中移除物品");
				return false;
			}

			quantity = Mathf.Max(0, quantity);
            
			Content[i].Quantity -= quantity;
			if (Content[i].Quantity <= 0)
			{
				bool suppressionSuccessful = RemoveItemFromArray(i);
				MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
				return suppressionSuccessful;
			}
			else
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
				return true;
			}
		}

        /// <summary>
        /// 移除与指定itemID匹配的物品的指定数量。
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public virtual bool RemoveItemByID(string itemID, int quantity)
		{
			if (quantity < 1)
			{
				Debug.LogWarning("物品栏 : 您正尝试移除不正确的数量(" + quantity+") 从你的物品栏.");
				return false;
			}
            
			if (itemID == null || itemID == "")
			{
				Debug.LogWarning("物品栏 : 您正尝试移除一个物品，但未指定itemID");
				return false;
			}

			int quantityLeftToRemove = quantity;
			
            
			List<int> list = InventoryContains(itemID);
			foreach (int index in list)
			{
				int quantityAtIndex = Content[index].Quantity;
				RemoveItem(index, quantityLeftToRemove);
				quantityLeftToRemove -= quantityAtIndex;
				if (quantityLeftToRemove <= 0)
				{
					return true;
				}
			}
			
			return false;
		}

        /// <summary>
        /// 销毁存储在索引i处的物品
        /// </summary>
        /// <returns><c>true</c>, if item was destroyed, <c>false</c> otherwise.</returns>
        /// <param name="i">The index.</param>
        public virtual bool DestroyItem(int i)
		{
			Content[i] = null;

			MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
			return true;
		}

        /// <summary>
        /// 清空物品栏的当前状态
        /// </summary>
        public virtual void EmptyInventory()
		{
			Content = new InventoryItem[Content.Length];

			MMInventoryEvent.Trigger(MMInventoryEventType.ContentChanged, null, this.name, null, 0, 0, PlayerID);
		}

        /// <summary>
        /// 返回可添加到此物品栏中的特定物品的最大值，而不会超过该物品上定义的最大数量
        /// </summary>
        /// <param name="itemToAdd"></param>
        /// <param name="newQuantity"></param>
        /// <returns></returns>
        public virtual int CapMaxQuantity(InventoryItem itemToAdd, int newQuantity)
		{
			return Mathf.Min(newQuantity, itemToAdd.MaximumQuantity - GetQuantity(itemToAdd.ItemID));
		}

        /// <summary>
        /// 将物品添加到内容数组中
        /// </summary>
        /// <returns><c>true</c>, if item to array was added, <c>false</c> otherwise.</returns>
        /// <param name="itemToAdd">Item to add.</param>
        /// <param name="quantity">Quantity.</param>
        protected virtual bool AddItemToArray(InventoryItem itemToAdd, int quantity)
		{
			if (NumberOfFreeSlots == 0)
			{
				return false;
			}
			int i = 0;
			while (i < Content.Length)
			{
				if (InventoryItem.IsNull(Content[i]))
				{
					Content[i] = itemToAdd.Copy();
					Content[i].Quantity = quantity;
					return true;
				}
				i++;
			}
			return false;
		}

        /// <summary>
        /// 从数组中移除索引i处的物品
        /// </summary>
        /// <returns><c>true</c>, if item from array was removed, <c>false</c> otherwise.</returns>
        /// <param name="i">The index.</param>
        protected virtual bool RemoveItemFromArray(int i)
		{
			if (i < Content.Length)
			{
				//Content[i].ItemID = null;
				Content[i] = null;
				return true;
			}
			return false;
		}

        /// <summary>
        /// 将数组调整为指定的新大小
        /// </summary>
        /// <param name="newSize">New size.</param>
        public virtual void ResizeArray(int newSize)
		{
			InventoryItem[] temp = new InventoryItem[newSize];
			for (int i = 0; i < Mathf.Min(newSize, Content.Length); i++)
			{
				temp[i] = Content[i];
			}
			Content = temp;
		}

        /// <summary>
        /// 返回与指定名称匹配的物品的总数量
        /// </summary>
        /// <returns>The quantity.</returns>
        /// <param name="searchedItem">Searched item.</param>
        public virtual int GetQuantity(string searchedItemID)
		{
			List<int> list = InventoryContains(searchedItemID);
			int total = 0;
			foreach (int i in list)
			{
				total += Content[i].Quantity;
			}
			return total;
		}

        /// <summary>
        /// 返回物品栏中所有与指定名称匹配的物品列表
        /// </summary>
        /// <returns>A list of item matching the search criteria.</returns>
        /// <param name="searchedType">The searched type.</param>
        public virtual List<int> InventoryContains(string searchedItemID)
		{
			List<int> list = new List<int>();

			for (int i = 0; i < Content.Length; i++)
			{
				if (!InventoryItem.IsNull(Content[i]))
				{
					if (Content[i].ItemID == searchedItemID)
					{
						list.Add(i);
					}
				}
			}
			return list;
		}

        /// <summary>
        /// 返回物品栏中所有与指定类别匹配的物品列表
        /// </summary>
        /// <returns>A list of item matching the search criteria.</returns>
        /// <param name="searchedType">The searched type.</param>
        public virtual List<int> InventoryContains(MoreMountains.InventoryEngine.ItemClasses searchedClass)
		{
			List<int> list = new List<int>();

			for (int i = 0; i < Content.Length; i++)
			{
				if (InventoryItem.IsNull(Content[i]))
				{
					continue;
				}
				if (Content[i].ItemClass == searchedClass)
				{
					list.Add(i);
				}
			}
			return list;
		}

        /// <summary>
        /// 将物品栏保存到文件中
        /// </summary>
        public virtual void SaveInventory()
		{
			SerializedInventory serializedInventory = new SerializedInventory();
			FillSerializedInventory(serializedInventory);
			MMSaveLoadManager.Save(serializedInventory, DetermineSaveName(), _saveFolderName);
		}

        /// <summary>
        /// 尝试加载如果存在文件的话
        /// </summary>
        public virtual void LoadSavedInventory()
		{
			SerializedInventory serializedInventory = (SerializedInventory)MMSaveLoadManager.Load(typeof(SerializedInventory), DetermineSaveName(), _saveFolderName);
			ExtractSerializedInventory(serializedInventory);
			MMInventoryEvent.Trigger(MMInventoryEventType.InventoryLoaded, null, this.name, null, 0, 0, PlayerID);
		}

        /// <summary>
        /// 为存储填充序列化的物品栏
        /// </summary>
        /// <param name="serializedInventory">Serialized inventory.</param>
        protected virtual void FillSerializedInventory(SerializedInventory serializedInventory)
		{
			serializedInventory.InventoryType = InventoryType;
			serializedInventory.DrawContentInInspector = DrawContentInInspector;
			serializedInventory.ContentType = new string[Content.Length];
			serializedInventory.ContentQuantity = new int[Content.Length];
			for (int i = 0; i < Content.Length; i++)
			{
				if (!InventoryItem.IsNull(Content[i]))
				{
					serializedInventory.ContentType[i] = Content[i].ItemID;
					serializedInventory.ContentQuantity[i] = Content[i].Quantity;
				}
				else
				{
					serializedInventory.ContentType[i] = null;
					serializedInventory.ContentQuantity[i] = 0;
				}
			}
		}

		protected InventoryItem _loadedInventoryItem;

        /// <summary>
        /// 从文件内容中提取序列化的物品栏
        /// </summary>
        /// <param name="serializedInventory">Serialized inventory.</param>
        protected virtual void ExtractSerializedInventory(SerializedInventory serializedInventory)
		{
			if (serializedInventory == null)
			{
				return;
			}

			InventoryType = serializedInventory.InventoryType;
			DrawContentInInspector = serializedInventory.DrawContentInInspector;
			Content = new InventoryItem[serializedInventory.ContentType.Length];
			for (int i = 0; i < serializedInventory.ContentType.Length; i++)
			{
				if ((serializedInventory.ContentType[i] != null) && (serializedInventory.ContentType[i] != ""))
				{
					_loadedInventoryItem = Resources.Load<InventoryItem>(_resourceItemPath + serializedInventory.ContentType[i]);
					if (_loadedInventoryItem == null)
					{
						Debug.LogError("物品栏 : 在资源中找不到任何要加载的物品栏项目/" + _resourceItemPath
							+" named "+serializedInventory.ContentType[i] + ". 确保所有物品定义名称（InventoryItem脚本名称）都是唯一的 " +
                            "objects) 确保它们的名称（InventoryItem脚本名称）与检查器中的ItemID字符串完全相同。请确保它们位于Resources文件夹中/" + _resourceItemPath+" folder. " +
                            "一旦完成，还要确保重置所有已保存的物品栏，因为不匹配的名称和ID可能会导致问题 " +
                            "它们已被损坏.");
					}
					else
					{
						Content[i] = _loadedInventoryItem.Copy();
						Content[i].Quantity = serializedInventory.ContentQuantity[i];
					}
				}
				else
				{
					Content[i] = null;
				}
			}
		}

		protected virtual string DetermineSaveName()
		{
			return gameObject.name + "_" + PlayerID + _saveFileExtension;
		}

        /// <summary>
        /// 销毁任何保存文件
        /// </summary>
        public virtual void ResetSavedInventory()
		{
			MMSaveLoadManager.DeleteSave(DetermineSaveName(), _saveFolderName);
			Debug.LogFormat("物品栏保存文件已删除");
		}

        /// <summary>
        /// 触发使用并可能消耗传递的参数中的物品。您还可以指定物品的插槽（可选）和索引
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="slot">Slot.</param>
        /// <param name="index">Index.</param>
        public virtual bool UseItem(InventoryItem item, int index, InventorySlot slot = null)
		{
			if (InventoryItem.IsNull(item))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
				return false;
			}
			if (!item.IsUsable)
			{
				return false;
			}
			if (item.Use(PlayerID))
			{
                // 从数量中减去1
                if (item.Consumable)
				{
					RemoveItem(index, item.ConsumeQuantity);    
				}
				MMInventoryEvent.Trigger(MMInventoryEventType.ItemUsed, slot, this.name, item.Copy(), 0, index, PlayerID);
			}
			return true;
		}

        /// <summary>
        /// 根据其名称触发物品的使用。如果您不特别关心在有重复项的情况下物品将从哪个插槽中取出，请优先使用此签名而不是上一个
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public virtual bool UseItem(string itemName)
		{
			List<int> list = InventoryContains(itemName);
			if (list.Count > 0)
			{
				UseItem(Content[list[list.Count - 1]], list[list.Count - 1], null);
				return true;
			}
			else
			{
				return false;
			}
		}

        /// <summary>
        /// 在指定插槽处装备物品
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="index">Index.</param>
        /// <param name="slot">Slot.</param>
        public virtual void EquipItem(InventoryItem item, int index, InventorySlot slot = null)
		{
			if (InventoryType == Inventory.InventoryTypes.Main)
			{
				InventoryItem oldItem = null;
				if (InventoryItem.IsNull(item))
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
					return;
				}
                // 如果对象不可装备，我们不执行任何操作并退出
                if (!item.IsEquippable)
				{
					return;
				}
                // 如果目标装备物品栏未设置，我们不执行任何操作并退出
                if (item.TargetEquipmentInventory(PlayerID) == null)
				{
					Debug.LogWarning("物品栏 警告 : " + Content[index].ItemName + "找不到目标装备物品栏.");
					return;
				}
                // 如果对象无法移动，我们播放错误声音并退出
                if (!item.CanMoveObject)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
					return;
				}
                // 如果对象无法装备因为物品栏已满，并且确实已满，我们不执行任何操作并退出。
                if (!item.EquippableIfInventoryIsFull)
				{
					if (item.TargetEquipmentInventory(PlayerID).IsFull)
					{
						return;
					}
				}
                // 如果这是一个单插槽物品栏，我们准备进行交换
                if (item.TargetEquipmentInventory(PlayerID).Content.Length == 1)
				{
					if (!InventoryItem.IsNull(item.TargetEquipmentInventory(PlayerID).Content[0]))
					{
						if (
							(item.CanSwapObject)
							&& (item.TargetEquipmentInventory(PlayerID).Content[0].CanMoveObject)
							&& (item.TargetEquipmentInventory(PlayerID).Content[0].CanSwapObject)
						)
						{
                            // 我们将物品存储在装备物品栏中
                            oldItem = item.TargetEquipmentInventory(PlayerID).Content[0].Copy();
							oldItem.UnEquip(PlayerID);
							MMInventoryEvent.Trigger(MMInventoryEventType.ItemUnEquipped, slot, this.name, oldItem, oldItem.Quantity, index, PlayerID);
							item.TargetEquipmentInventory(PlayerID).EmptyInventory();
						}
					}
				}
                // 我们在目标装备物品栏中添加一个
                item.TargetEquipmentInventory(PlayerID).AddItem(item.Copy(), item.Quantity);
                // 从数量中减去1
                if (item.MoveWhenEquipped)
				{
					RemoveItem(index, item.Quantity);    
				}
				if (oldItem != null)
				{
					oldItem.Swap(PlayerID);
					if (oldItem.MoveWhenEquipped)
					{
						if (oldItem.ForceSlotIndex)
						{
							AddItemAt(oldItem, oldItem.Quantity, oldItem.TargetIndex);    
						}
						else
						{
							AddItem(oldItem, oldItem.Quantity);    
						}	
					}
				}
                // 调用物品的equip方法
                if (!item.Equip(PlayerID))
				{
					return;
				}
				MMInventoryEvent.Trigger(MMInventoryEventType.ItemEquipped, slot, this.name, item, item.Quantity, index, PlayerID);
			}
		}

        /// <summary>
        /// 掉落物品，将其从物品栏中移除，并可能在角色附近的地面上生成一个物品
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="index">Index.</param>
        /// <param name="slot">Slot.</param>
        public virtual void DropItem(InventoryItem item, int index, InventorySlot slot = null)
		{
			if (InventoryItem.IsNull(item))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
				return;
			}
			item.SpawnPrefab(PlayerID);
            
			if (this.name == item.TargetEquipmentInventoryName)
			{
				if (item.UnEquip(PlayerID))
				{
					DestroyItem(index);
				}
			} else
			{
				DestroyItem(index);
			}

		}

		public virtual void DestroyItem(InventoryItem item, int index, InventorySlot slot = null)
		{
			if (InventoryItem.IsNull(item))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
				return;
			}
			DestroyItem(index);
		}

		public virtual void UnEquipItem(InventoryItem item, int index, InventorySlot slot = null)
		{
            // 如果此插槽中没有物品，我们触发一个错误
            if (InventoryItem.IsNull(item))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
				return;
			}
            // 如果我们不在一个装备物品栏中，我们触发一个错误。
            if (InventoryType != InventoryTypes.Equipment)
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, slot, this.name, null, 0, index, PlayerID);
				return;
			}
            // 我们触发物品的unequip效果
            if (!item.UnEquip(PlayerID))
			{
				return;
			}
			MMInventoryEvent.Trigger(MMInventoryEventType.ItemUnEquipped, slot, this.name, item, item.Quantity, index, PlayerID);

            // 如果有目标物品栏，我们将尝试将物品添加回去
            if (item.TargetInventory(PlayerID) != null)
			{
				bool itemAdded = false;
				if (item.ForceSlotIndex)
				{
					itemAdded = item.TargetInventory(PlayerID).AddItemAt(item, item.Quantity, item.TargetIndex);
					if (!itemAdded)
					{
						itemAdded = item.TargetInventory(PlayerID).AddItem(item, item.Quantity);    	
					}
				}
				else
				{
					itemAdded = item.TargetInventory(PlayerID).AddItem(item, item.Quantity);    
				}

                // 如果我们成功地添加了物品
                if (itemAdded)
				{
					DestroyItem(index);
				}
				else
				{
                    // 如果我们无法添加（例如物品栏已满），我们将它掉落到地面上。
                    MMInventoryEvent.Trigger(MMInventoryEventType.Drop, slot, this.name, item, item.Quantity, index, PlayerID);
				}
			}
		}

        /// <summary>
        /// 捕捉物品栏事件并对它们采取行动
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
            // 如果此事件与我们的物品栏显示无关，我们不执行任何操作并退出
            if (inventoryEvent.TargetInventoryName != this.name)
			{
				return;
			}
			if (inventoryEvent.PlayerID != PlayerID)
			{
				return;
			}
			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Pick:
					if (inventoryEvent.EventItem.ForceSlotIndex)
					{
						AddItemAt(inventoryEvent.EventItem, inventoryEvent.Quantity, inventoryEvent.EventItem.TargetIndex);    
					}
					else
					{
						AddItem(inventoryEvent.EventItem, inventoryEvent.Quantity);    
					}
					break;

				case MMInventoryEventType.UseRequest:
					UseItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
					break;

				case MMInventoryEventType.EquipRequest:
					EquipItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
					break;

				case MMInventoryEventType.UnEquipRequest:
					UnEquipItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Destroy:
					DestroyItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Drop:
					DropItem(inventoryEvent.EventItem, inventoryEvent.Index, inventoryEvent.Slot);
					break;
			}
		}

        /// <summary>
        /// 当我们捕捉到一个MMGameEvent时，我们根据它的名称执行相应的操作
        /// </summary>
        /// <param name="gameEvent">Game event.</param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			if ((gameEvent.EventName == "Save") && Persistent)
			{
				SaveInventory();
			}
			if ((gameEvent.EventName == "Load") && Persistent)
			{
				if (ResetThisInventorySaveOnStart)
				{
					ResetSavedInventory();
				}
				LoadSavedInventory();
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听MMGameEvents。您可能需要扩展它以监听其他类型的事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent>();
			this.MMEventStartListening<MMInventoryEvent>();
		}

        /// <summary>
        ///在禁用时，我们停止监听MMGameEvents。您可能需要扩展它以停止监听其他类型的事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent>();
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}
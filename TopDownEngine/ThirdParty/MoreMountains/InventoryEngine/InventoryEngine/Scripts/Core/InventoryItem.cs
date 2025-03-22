using UnityEngine;
using MoreMountains.Tools;
using System;

namespace MoreMountains.InventoryEngine
{
	[Serializable]
	public class InventoryItemDisplayProperties
	{
		[Header("Buttons按钮")]
		public bool DisplayEquipUseButton = true;
		public bool DisplayMoveButton = true;
		public bool DisplayDropButton = true;
		public bool DisplayEquipButton = true;
		public bool DisplayUseButton = true;
		public bool DisplayUnequipButton = true;
		
		[Header("Shortcuts快捷方式")]
		public bool AllowEquipUseShortcut = true;
		public bool AllowMoveShortcut = true;
		public bool AllowDropShortcut = true;
		public bool AllowEquipShortcut = true;
		public bool AllowUseShortcut = true;
	}
	
	[Serializable]
    /// <summary>
    /// 库存项目的基类，旨在被扩展
    /// 将处理基本属性并掉落生成
    /// </summary>
    public class InventoryItem : ScriptableObject 
	{
		[Header("ID and Target标识和目标")]
		/// the (unique) ID of the item
		[Tooltip("该物品的（唯一）ID")]
		public string ItemID;
		/// the inventory name into which this item will be stored
		[Tooltip("将该物品存储到的库存名称")]
		public string TargetInventoryName = "MainInventory";
		/// if this is true, the item won't be added anywhere's there's room in the inventory, but instead at the specified TargetIndex slot
		[Tooltip("如果此选项为真，则该物品不会添加到库存中任何有空间的位置，而是会添加到指定TargetIndex的槽位")]
		public bool ForceSlotIndex = false;
		/// if ForceSlotIndex is true, this is the index at which the item will be added in the target inventory
		[Tooltip("如果ForceSlotIndex为真，这是该物品在目标库存中将被添加的索引")]
		[MMCondition("ForceSlotIndex", true)]
		public int TargetIndex = 0;

		[Header("Permissions权限")]
		/// whether or not this item can be "used" (via the Use method) - important, this is only the INITIAL state of this object, IsUsable is to be used anytime after that
		[Tooltip("无论该物品是否可以通过“使用”方法来使用 - 重要的是，这只是该对象的初始状态，IsUsable属性应在那之后的任何时间使用。")]
		public bool Usable = false;
		/// if this is true, calling Use on that object will consume one unit of it
		[Tooltip("如果此属性为真，调用该对象的Use方法将会消耗其一个单位")]
		[MMCondition("Usable", true)] 
		public bool Consumable = true;
		/// if this item is consumable, determines how many will be consumed per use (usually one)
		[Tooltip("如果此物品是可消耗的，决定每次使用将消耗多少数量（通常为1）")]
		[MMCondition("Consumable", true)] 
		public int ConsumeQuantity = 1;
		/// whether or not this item can be equipped - important, this is only the INITIAL state of this object, IsEquippable is to be used anytime after that
		[Tooltip("此物品是否可以装备 - 重要的是，这只是该对象的初始状态，IsEquippable属性应在那之后的任何时间使用")]
		public bool Equippable = false;
		/// whether or not this item can be equipped if its target inventory is full
		[Tooltip("如果此物品的目标库存已满，它是否可以装备")]
		[MMCondition("Equippable", true)]
		public bool EquippableIfInventoryIsFull = true;
		/// if this is true, this item will be removed from its original inventory when equipped, and moved to its EquipmentInventory
		[Tooltip("如果此属性为真，当该物品被装备时，它将从其原始库存中移除，并移动到其EquipmentInventory中")]
		[MMCondition("Equippable", true)]
		public bool MoveWhenEquipped = true;
		
		/// if this is true, this item can be dropped
		[Tooltip("如果此属性为真，则该物品可以被丢弃")]
		public bool Droppable = true;
		/// if this is true, objects can be moved
		[Tooltip("如果此属性为真，则对象可以被移动")]
		public bool CanMoveObject=true;
		/// if this is true, objects can be swapped with another object
		[Tooltip("如果此属性为真，对象可以与其他对象进行交换")]
		public bool CanSwapObject=true;
		/// a set of properties defining whether or not to show inventory action buttons when that item is selected 
		[Tooltip("一组属性，用于定义当选择该项目时是否显示库存操作按钮")]
		public InventoryItemDisplayProperties DisplayProperties;

        /// 此对象是否可以被使用
        public virtual bool IsUsable {  get { return Usable;  } }
        /// 此对象是否可以被装备
        public virtual bool IsEquippable { get { return Equippable; } }

		[HideInInspector]
        /// 此物品的基础数量
        public int Quantity = 1;

		[Header("Basic info基本信息")]
		/// the name of the item - will be displayed in the details panel
		[Tooltip("物品的名称 - 将在详细信息面板中显示")]
		public string ItemName;
		/// the item's short description to display in the details panel
		[TextArea]
		[Tooltip("物品的简短描述，用于在详细信息面板中显示")]
		public string ShortDescription;
		[TextArea]
		/// the item's long description to display in the details panel
		[Tooltip("物品的详细描述，用于在详细信息面板中显示")]
		public string Description;

		[Header("Image图像")]
		/// the icon that will be shown on the inventory's slot
		[Tooltip("将在库存槽位上显示的图标")]
		public Sprite Icon;

		[Header("Prefab Drop预制件掉落")]
		/// the prefab to instantiate when the item is dropped
		[Tooltip("当物品被丢弃时要实例化的预制件")]
		public GameObject Prefab;
		/// if this is true, the quantity of the object will be forced to PrefabDropQuantity when dropped
		[Tooltip("如果此属性为真，当物品被丢弃时，其数量将被强制设置为PrefabDropQuantity")]
		public bool ForcePrefabDropQuantity = false;
		/// the quantity to force on the spawned item if ForcePrefabDropQuantity is true
		[Tooltip("如果ForcePrefabDropQuantity为真，要在生成的物品上强制设置的数量")]
		[MMCondition("ForcePrefabDropQuantity", true)]
		public int PrefabDropQuantity = 1;
		/// the minimal distance at which the object should be spawned when dropped
		[Tooltip("当物品被丢弃时，应该生成该物品的最小距离")]
		public MMSpawnAroundProperties DropProperties;

		[Header("Inventory Properties库存属性")]
		/// If this object can be stacked (multiple instances in a single inventory slot), you can specify here the maximum size of that stack.
		[Tooltip("如果此对象可以被堆叠（单个库存槽位中的多个实例），您可以在这里指定该堆叠的最大大小")]
		public int MaximumStack = 1;
		/// the maximum quantity allowed of this item in the target inventory
		[Tooltip("此物品在目标库存中允许的最大数量")]
		public int MaximumQuantity = 999999999;
		/// the class of the item
		[Tooltip("此物品的类别")]

        public ItemClasses ItemClass;

		[Header("Equippable可装备的")]
		/// If this item is equippable, you can set here its target inventory name (for example ArmorInventory). Of course you'll need an inventory with a matching name in your scene.
		[Tooltip("如果此物品是可装备的，您可以在这里设置其目标库存名称（例如ArmoryInventory）。当然，您需要在场景中有一个匹配名称的库存")]
		public string TargetEquipmentInventoryName;
		/// the sound the item should play when equipped (optional)
		[Tooltip("当物品被装备时应播放的声音（可选）。")]
		public AudioClip EquippedSound;

		[Header("Usable可使用的")]
		/// If this item can be used, you can set here a sound to play when it gets used, if you don't a default sound will be played.
		[Tooltip("如果此物品可以被使用，您可以在这里设置一个声音来播放，当它被使用时。如果您不设置，将播放默认声音")]
		public AudioClip UsedSound;

		[Header("Sounds声音")]
		/// the sound the item should play when moved (optional)
		[Tooltip("当物品被移动时应播放的声音（可选）")]
		public AudioClip MovedSound;
		/// the sound the item should play when dropped (optional)
		[Tooltip("当物品被丢弃时应播放的声音（可选）")]
		public AudioClip DroppedSound;
		/// if this is set to false, default sounds won't be used and no sound will be played
		[Tooltip("如果此属性设置为假，则不会使用默认声音，也不会播放任何声音")]
		public bool UseDefaultSoundsIfNull = true;

		protected Inventory _targetInventory = null;
		protected Inventory _targetEquipmentInventory = null;

        /// <summary>
        /// 获取目标库存
        /// </summary>
        /// <value>The target inventory.</value>
        public virtual Inventory TargetInventory(string playerID)
		{ 
			if (TargetInventoryName == null)
			{
				return null;
			}
			_targetInventory = Inventory.FindInventory(TargetInventoryName, playerID);
			return _targetInventory;
		}

        /// <summary>
        /// 获取目标装备库存
        /// </summary>
        /// <value>The target equipment inventory.</value>
        public virtual Inventory TargetEquipmentInventory(string playerID)
		{ 
			if (TargetEquipmentInventoryName == null)
			{
				return null;
			}
			_targetEquipmentInventory = Inventory.FindInventory(TargetEquipmentInventoryName, playerID);
			return _targetEquipmentInventory;
		}

        /// <summary>
        /// 确定物品是否为空（null）
        /// </summary>
        /// <returns><c>true</c> if is null the specified item; otherwise, <c>false</c>.</returns>
        /// <param name="item">Item.</param>
        public static bool IsNull(InventoryItem item)
		{
			if (item==null)
			{
				return true;
			}
			if (item.ItemID==null)
			{
				return true;
			}
			if (item.ItemID=="")
			{
				return true;
			}
			return false;
		}

        /// <summary>
        /// 将一个物品复制到一个新的物品中
        /// </summary>
        public virtual InventoryItem Copy()
		{
			string name = this.name;
			InventoryItem clone = UnityEngine.Object.Instantiate(this) as InventoryItem;
			clone.name = name;
			return clone;
		}

        /// <summary>
        /// 生成相关的预制件
        /// </summary>
        public virtual GameObject SpawnPrefab(string playerID)
		{
			if (TargetInventory(playerID) != null)
			{
                // 如果在此槽位为该物品设置了预制件，我们将在指定的偏移量处实例化它
                if (Prefab != null && TargetInventory(playerID).TargetTransform != null)
				{
					GameObject droppedObject=(GameObject)Instantiate(Prefab);
					ItemPicker droppedObjectItemPicker = droppedObject.GetComponent<ItemPicker>(); 
					
					if (droppedObjectItemPicker != null)
					{
						if (ForcePrefabDropQuantity)
						{
							droppedObjectItemPicker.Quantity = PrefabDropQuantity;
							droppedObjectItemPicker.RemainingQuantity = PrefabDropQuantity;	
						}
						else
						{
							droppedObjectItemPicker.Quantity = Quantity;
							droppedObjectItemPicker.RemainingQuantity = Quantity;	
						}
					}

					MMSpawnAround.ApplySpawnAroundProperties(droppedObject, DropProperties,
						TargetInventory(playerID).TargetTransform.position);

					return droppedObject;
				}
			}

			return null;
		}

        /// <summary>
        /// 当对象被拾取时会发生什么 - 覆盖此方法以添加您自己的行为
        /// </summary>
        public virtual bool Pick(string playerID) { return true; }

        /// <summary>
        /// 当对象被使用时会发生什么 - 覆盖此方法以添加您自己的行为
        /// </summary>
        public virtual bool Use(string playerID) { return true; }

        /// <summary>
        /// 当对象被装备时会发生什么 - 覆盖此方法以添加您自己的行为
        /// </summary>
        public virtual bool Equip(string playerID) { return true; }

        /// <summary>
        /// 当对象被取消装备（在丢弃时调用）时会发生什么 - 覆盖此方法以添加您自己的行为
        /// </summary>
        public virtual bool UnEquip(string playerID) { return true; }

        /// <summary>
        /// 当对象被另一个对象交换时会发生什么
        /// </summary>
        public virtual void Swap(string playerID) {}

        /// <summary>
        /// 当对象被丢弃时会发生什么 - 覆盖此方法以添加您自己的行为
        /// </summary>
        public virtual bool Drop(string playerID) { return true; }
	}
}
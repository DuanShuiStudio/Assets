using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 将此组件添加到对象上，使其可以被拾取并添加到库存中
    /// </summary>
    public class ItemPicker : MonoBehaviour 
	{
		[Header("Item to pick要拾取的物品")]
		/// the item that should be picked 
		[MMInformation("将此组件添加到触发器盒子碰撞器2D上，它将使该物品可被拾取，并会将其指定的物品添加到目标库存中。只需将先前创建的物品拖放到下面的槽位即可。有关如何创建物品的更多信息，请查看文档。在这里，您还可以指定在拾取对象时应拾取多少个该物品。", MMInformationAttribute.InformationType.Info,false)]
		public InventoryItem Item ;
		
		[Header("Pick Quantity拾取数量")]
		/// the initial quantity of that item that should be added to the inventory when picked
		[Tooltip("当物品被拾取时应添加到库存中的初始数量")]
		public int Quantity = 1;
		/// the current quantity of that item that should be added to the inventory when picked
		[MMReadOnly]
		[Tooltip("当物品被拾取时应添加到库存中的当前数量")]
		public int RemainingQuantity = 1;
		
		[Header("Conditions条件")]
		/// if you set this to true, a character will be able to pick this item even if its inventory is full
		[Tooltip("如果将其设置为真，即使角色的库存已满，也可以拾取此物品")]
		public bool PickableIfInventoryIsFull = false;
		/// if you set this to true, the object will be disabled when picked
		[Tooltip("如果将其设置为真，当物品被拾取时，该对象将被禁用")]
		public bool DisableObjectWhenDepleted = false;
		/// if this is true, this object will only be allowed to be picked by colliders with a Player tag
		[Tooltip("如果此属性为真，则只有带有Player标签的碰撞器才能拾取该对象")]
		public bool RequirePlayerTag = true;

		protected int _pickedQuantity = 0;
		protected Inventory _targetInventory;

        /// <summary>
        /// 在开始时，我们初始化物品拾取器
        /// </summary>
        protected virtual void Start()
		{
			Initialization ();
		}

        /// <summary>
        /// 在初始化时，我们寻找目标库存
        /// </summary>
        protected virtual void Initialization()
		{
			FindTargetInventory (Item.TargetInventoryName);
			ResetQuantity();
		}

        /// <summary>
        /// 将剩余数量重置为初始数量。
        /// </summary>
        public virtual void ResetQuantity()
		{
			RemainingQuantity = Quantity;
		}

        /// <summary>
        /// 当某物与拾取器碰撞时触发
        /// </summary>
        /// <param name="collider">Other.</param>
        public virtual void OnTriggerEnter(Collider collider)
		{
            // 如果与拾取器碰撞的不是角色行为，则不执行任何操作并退出
            if (RequirePlayerTag && (!collider.CompareTag("Player")))
			{
				return;
			}

			string playerID = "Player1";
			InventoryCharacterIdentifier identifier = collider.GetComponent<InventoryCharacterIdentifier>();
			if (identifier != null)
			{
				playerID = identifier.PlayerID;
			}

			Pick(Item.TargetInventoryName, playerID);
		}

        /// <summary>
        /// 当某物与拾取器碰撞时触发
        /// </summary>
        /// <param name="collider">Other.</param>
        public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
            // 如果与拾取器碰撞的不是角色行为，则不执行任何操作并退出
            if (RequirePlayerTag && (!collider.CompareTag("Player")))
			{
				return;
			}

			string playerID = "Player1";
			InventoryCharacterIdentifier identifier = collider.GetComponent<InventoryCharacterIdentifier>();
			if (identifier != null)
			{
				playerID = identifier.PlayerID;
			}

			Pick(Item.TargetInventoryName, playerID);
		}

        /// <summary>
        /// 拾取此物品并将其添加到目标库存中
        /// </summary>
        public virtual void Pick()
		{
			Pick(Item.TargetInventoryName);
		}

        /// <summary>
        /// 拾取此物品并将其添加到作为参数指定的目标库存中
        /// </summary>
        /// <param name="targetInventoryName">Target inventory name.</param>
        public virtual void Pick(string targetInventoryName, string playerID = "Player1")
		{
			FindTargetInventory(targetInventoryName, playerID);
			if (_targetInventory == null)
			{
				return;
			}

			if (!Pickable()) 
			{
				PickFail ();
				return;
			}

			DetermineMaxQuantity ();
			if (!Application.isPlaying)
			{
				if (!Item.ForceSlotIndex)
				{
					_targetInventory.AddItem(Item, 1);	
				}
				else
				{
					_targetInventory.AddItemAt(Item, 1, Item.TargetIndex);
				}
			}				
			else
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, Item.TargetInventoryName, Item, _pickedQuantity, 0, playerID);
			}				
			if (Item.Pick(playerID))
			{
				RemainingQuantity = RemainingQuantity - _pickedQuantity;
				PickSuccess();
				DisableObjectIfNeeded();
			}			
		}

        /// <summary>
        /// 描述对象成功被拾取时会发生什么
        /// </summary>
        protected virtual void PickSuccess()
		{
			
		}

        /// <summary>
        /// 描述对象未能被拾取时会发生什么（通常是库存已满）
        /// </summary>
        protected virtual void PickFail()
		{

		}

        /// <summary>
        /// 如果需要，禁用该对象
        /// </summary>
        protected virtual void DisableObjectIfNeeded()
		{
			// we desactivate the gameobject
			if (DisableObjectWhenDepleted && RemainingQuantity <= 0)
			{
				gameObject.SetActive(false);	
			}
		}

        /// <summary>
        /// 确定可以从该对象中拾取的最大物品数量
        /// </summary>
        protected virtual void DetermineMaxQuantity()
		{
			int maxQuantity = _targetInventory.CapMaxQuantity(Item, Quantity);
			int stackQuantity = _targetInventory.NumberOfStackableSlots (Item.ItemID, Item.MaximumStack);

			_pickedQuantity = Mathf.Min(maxQuantity, stackQuantity);
			
			if (RemainingQuantity < _pickedQuantity)
			{
				_pickedQuantity = RemainingQuantity;
			}
		}


        /// <summary>
        /// 如果此物品可以被拾取，则返回真；否则返回假
        /// </summary>
        public virtual bool Pickable()
		{
			if (!PickableIfInventoryIsFull && _targetInventory.NumberOfFreeSlots == 0)
			{
                // 我们确保没有可以存储它的地方
                int spaceAvailable = 0;
				List<int> list = _targetInventory.InventoryContains(Item.ItemID);
				if (list.Count > 0)
				{
					foreach (int index in list)
					{
						spaceAvailable += (Item.MaximumStack - _targetInventory.Content[index].Quantity);
					}
				}

				if (Item.Quantity <= spaceAvailable)
				{
					return true;
				}
				else
				{
					return false;	
				}
			}

			return true;
		}

        /// <summary>
        /// 根据其名称查找目标库存
        /// </summary>
        /// <param name="targetInventoryName">Target inventory name.</param>
        public virtual void FindTargetInventory(string targetInventoryName, string playerID = "Player1")
		{
			_targetInventory = null;
			_targetInventory = Inventory.FindInventory(targetInventoryName, playerID);
		}
	}
}
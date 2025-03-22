using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 此类负责处理库存中物品的显示，并会触发与物品相关的各种操作（如装备、使用等）
    /// </summary>
    public class InventorySlot : Button
	{
        /// 在移动物品时，用作槽位背景的精灵图
        public Sprite MovedSprite;
        /// 此槽位所属的库存显示界面
        public InventoryDisplay ParentInventoryDisplay;
        /// 槽位的索引（即其在库存数组中的位置）
        public int Index;
        /// 此槽位当前是否启用并可供交互
        public bool SlotEnabled=true;
		public Image TargetImage;
		public CanvasGroup TargetCanvasGroup;
		public RectTransform TargetRectTransform;
		public RectTransform IconRectTransform;
		public Image IconImage;
		public Text QuantityText;

		public InventoryItem CurrentItem
		{
			get
			{
				if (ParentInventoryDisplay != null)
				{
					return ParentInventoryDisplay.TargetInventory.Content[Index];
				}

				return null;
			}
		}
		
		protected const float _disabledAlpha = 0.5f;
		protected const float _enabledAlpha = 1.0f;

		protected override void Awake()
		{
			base.Awake();
			TargetImage = this.gameObject.GetComponent<Image>();
			TargetCanvasGroup = this.gameObject.GetComponent<CanvasGroup>();
			TargetRectTransform = this.gameObject.GetComponent<RectTransform>();
		}

        /// <summary>
        /// 在启动时，我们开始监听该槽位上的点击事件
        /// </summary>
        protected override void Start()
		{
			base.Start();
			this.onClick.AddListener(SlotClicked);
		}

        /// <summary>
        /// 如果此插槽中有物品，则在其中绘制其图标
        /// </summary>
        /// <param name="item">Item.</param>
        /// <param name="index">Index.</param>
        public virtual void DrawIcon(InventoryItem item, int index)
		{
			if (ParentInventoryDisplay != null)
			{				
				if (!InventoryItem.IsNull(item))
				{
					SetIcon(item.Icon);
					SetQuantity(item.Quantity);
				}
				else
				{
					DisableIconAndQuantity();
				}
			}
		}

		public virtual void SetIcon(Sprite newSprite)
		{
			IconImage.gameObject.SetActive(true);
			IconImage.sprite = newSprite;
		}

		public virtual void SetQuantity(int quantity)
		{
			if (quantity > 1)
			{
				QuantityText.gameObject.SetActive(true);
				QuantityText.text = quantity.ToString();	
			}
			else
			{
				QuantityText.gameObject.SetActive(false);
			}
		}

		public virtual void DisableIconAndQuantity()
		{
			IconImage.gameObject.SetActive(false);
		}

        /// <summary>
        /// 当该插槽被选中（通过鼠标悬停或触摸）时，触发其他类的事件以采取行动
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public override void OnSelect(BaseEventData eventData)
		{
			base.OnSelect(eventData);
			if (ParentInventoryDisplay != null)
			{
				InventoryItem item = ParentInventoryDisplay.TargetInventory.Content[Index];
				MMInventoryEvent.Trigger(MMInventoryEventType.Select, this, ParentInventoryDisplay.TargetInventoryName, item, 0, Index, ParentInventoryDisplay.PlayerID);
			}
		}

        /// <summary>
        /// 当该插槽被点击时，触发其他类的事件以采取行动
        /// </summary>
        public virtual void SlotClicked () 
		{
			if (ParentInventoryDisplay != null)
			{
				InventoryItem item = ParentInventoryDisplay.TargetInventory.Content[Index];
				if (ParentInventoryDisplay.InEquipSelection)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
				}
				MMInventoryEvent.Trigger(MMInventoryEventType.Click, this, ParentInventoryDisplay.TargetInventoryName, item, 0, Index, ParentInventoryDisplay.PlayerID);
				// if we're currently moving an object
				if (InventoryDisplay.CurrentlyBeingMovedItemIndex != -1)
				{
					Move();
				}
			}
		}

        /// <summary>
        /// 选择此插槽中的物品以进行移动，或者将当前选中的物品移动到该插槽
        /// 如果条件允许，此操作也将交换两个对象的位置
        /// </summary>
        public virtual void Move()
		{
			if (!SlotEnabled) { return; }

            // 如果我们尚未移动某个对象
            if (InventoryDisplay.CurrentlyBeingMovedItemIndex == -1)
			{
                // 如果我们所在的插槽为空，我们则不执行任何操作
                if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
					return;
				}
				if (ParentInventoryDisplay.TargetInventory.Content[Index].CanMoveObject)
				{
                    // 我们更改背景图像
                    TargetImage.sprite = ParentInventoryDisplay.MovedSlotImage;
					InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = ParentInventoryDisplay;
					InventoryDisplay.CurrentlyBeingMovedItemIndex = Index;
				}
			}
            // 如果我们正在移动某个对象
            else
            {
				bool moveSuccessful = false;
                // 我们将对象移动到新的插槽
                if (ParentInventoryDisplay == InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay)
				{
					if (!ParentInventoryDisplay.TargetInventory.MoveItem(InventoryDisplay.CurrentlyBeingMovedItemIndex, Index))
					{
                        // 如果无法进行移动（例如目标插槽非空），我们会播放一个声音
                        MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
						moveSuccessful = false;
					}
					else
					{
						moveSuccessful = true;
					}
				}
				else
				{
					if (!ParentInventoryDisplay.AllowMovingObjectsToThisInventory)
					{
						moveSuccessful = false;
					}
					else
					{
						if (!InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay.TargetInventory.MoveItemToInventory(InventoryDisplay.CurrentlyBeingMovedItemIndex, ParentInventoryDisplay.TargetInventory, Index))
						{
                            // 如果无法进行移动（例如目标插槽非空），我们会播放一个声音
                            MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
							moveSuccessful = false;
						}
						else
						{
							moveSuccessful = true;
						}
					}
				}

				if (moveSuccessful)
				{
                    // 如果可以移动，我们会重置当前正在被移动的指针
                    InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
					InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = null;
					MMInventoryEvent.Trigger(MMInventoryEventType.Move, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
				}
			}
		}

        /// <summary>
        /// 消耗此槽位中物品的一个单位，触发音效以及为该物品使用所定义的任何行为
        /// </summary>
        public virtual void Use()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.UseRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
		}

        /// <summary>
        /// 如果可能，装备此物品
        /// </summary>
        public virtual void Equip()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
		}

        /// <summary>
        /// 如果可能，卸载此物品
        /// </summary>
        public virtual void UnEquip()
		{
			if (!SlotEnabled) { return; }
			MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
		}

        /// <summary>
        /// 丢弃此物品
        /// </summary>
        public virtual void Drop()
		{
			if (!SlotEnabled) { return; }
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.Error, this, ParentInventoryDisplay.TargetInventoryName, null, 0, Index, ParentInventoryDisplay.PlayerID);
				return;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].Droppable)
			{
				return;
			}
			if (ParentInventoryDisplay.TargetInventory.Content[Index].Drop(ParentInventoryDisplay.PlayerID))
			{
				InventoryDisplay.CurrentlyBeingMovedItemIndex = -1;
				InventoryDisplay.CurrentlyBeingMovedFromInventoryDisplay = null;
				MMInventoryEvent.Trigger(MMInventoryEventType.Drop, this, ParentInventoryDisplay.TargetInventoryName, ParentInventoryDisplay.TargetInventory.Content[Index], 0, Index, ParentInventoryDisplay.PlayerID);
			}            
		}

        /// <summary>
        /// 禁用该插槽
        /// </summary>
        public virtual void DisableSlot()
		{
			this.interactable = false;
			SlotEnabled = false;
			TargetCanvasGroup.alpha = _disabledAlpha;
		}

        /// <summary>
        /// 启用该插槽
        /// </summary>
        public virtual void EnableSlot()
		{
			this.interactable = true;
			SlotEnabled = true;
			TargetCanvasGroup.alpha = _enabledAlpha;
		}

        /// <summary>
        /// 如果此插槽中的物品可以被装备，则返回 true；否则返回 false
        /// </summary>
        public virtual bool Equippable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].IsEquippable)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

        /// <summary>
        /// 如果此插槽中的物品可以被使用，则返回 true；否则返回 false
        /// </summary>
        public virtual bool Usable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].IsUsable)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

        /// <summary>
        /// 如果此插槽中的物品可以被移动，则返回 true；否则返回 false
        /// </summary>
        public virtual bool Movable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].CanMoveObject)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

        /// <summary>
        /// 如果此插槽中的物品可以被掉落，则返回 true；否则返回 false
        /// </summary>
        public virtual bool Droppable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (!ParentInventoryDisplay.TargetInventory.Content[Index].Droppable)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

        /// <summary>
        /// 如果此插槽中的物品可以被卸载，则返回 true；否则返回 false
        /// </summary>
        public virtual bool Unequippable()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index]))
			{
				return false;
			}
			if (ParentInventoryDisplay.TargetInventory.InventoryType != Inventory.InventoryTypes.Equipment)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public virtual bool EquipUseButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayEquipUseButton;
		}

		public virtual bool MoveButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayMoveButton;
		}

		public virtual bool DropButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayDropButton;
		}

		public virtual bool EquipButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayEquipButton;
		}

		public virtual bool UseButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayUseButton;
		}

		public virtual bool UnequipButtonShouldShow()
		{
			if (InventoryItem.IsNull(ParentInventoryDisplay.TargetInventory.Content[Index])) { return false; }
			return ParentInventoryDisplay.TargetInventory.Content[Index].DisplayProperties.DisplayUnequipButton;
		}
		
	}
}
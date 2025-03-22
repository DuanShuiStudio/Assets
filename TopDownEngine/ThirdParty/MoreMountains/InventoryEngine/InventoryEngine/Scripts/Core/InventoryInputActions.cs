using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 此类允许您将按键绑定到目标库存中的特定槽位，并在按下该按键时关联要执行的操作
    /// 一个典型的用例是武器栏，按下1装备手枪，按下2装备霰弹枪等
    /// 巧合的是，PixelRogueWeaponBar演示场景展示的正是这一功能
    /// </summary>
    public class InventoryInputActions : MonoBehaviour
	{
        /// <summary>
        /// 用于存储槽位/按键/操作绑定的类
        /// </summary>
        [System.Serializable]
		public class InventoryInputActionsBindings
		{
            /// 目标库存中用于绑定操作的槽位
            public int SlotIndex = 0;
            /// 应该触发该操作的按键
            public KeyCode InputBinding = KeyCode.Alpha0;
            /// 也会触发该操作的Alt键
            public KeyCode AltInputBinding = KeyCode.None;
            /// 按下输入时要触发的操作
            public InventoryInputActions.Actions Action = InventoryInputActions.Actions.Equip;
            /// 是否应触发此操作
            public bool Active = true;
		}

        /// <summary>
        /// 激活输入时可能引发的操作
        /// </summary>
        public enum Actions
		{
			Equip,
			Use,
			Drop,
			Unequip
		}

        /// 用于这些绑定的库存名称
        public string TargetInventoryName = "MainInventory";
        /// 与该组件相关联的玩家的唯一ID
        public string PlayerID = "Player1";
        /// 在查找输入时要遍历的绑定列表
        public List<InventoryInputActionsBindings> InputBindings;

		protected Inventory _targetInventory = null;

        /// <summary>
        /// 返回此组件的目标库存
        /// </summary>
        public Inventory TargetInventory
		{
			get
			{
				if (TargetInventoryName == null)
				{
					return null;
				}

				if (_targetInventory == null)
				{
					_targetInventory = Inventory.FindInventory(TargetInventoryName, PlayerID);
				}

				return _targetInventory;
			}
		}

        /// <summary>
        /// 在开始时，我们初始化我们的库存引用
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 确保我们有一个目标库存
        /// </summary>
        protected virtual void Initialization()
		{
			if (TargetInventoryName == "")
			{
				Debug.LogError("这个 " + this.name +
                               " Inventory Input Actions 组件没有设置 TargetInventoryName。您需要从检查器中设置一个，匹配库存的名称");
				return;
			}

			if (TargetInventory == null)
			{
				Debug.LogError("这个 " + this.name +
                               " Inventory Input Actions 组件找不到 TargetInventory。您需要创建一个具有匹配库存名称 (" +
				               TargetInventoryName + ")的库存, 或者将 TargetInventoryName 设置为一个已经存在的库存名称.");
				return;
			}
		}

        /// <summary>
        /// 在更新时，我们查找输入
        /// </summary>
        protected virtual void Update()
		{
			DetectInput();
		}

        /// <summary>
        /// 每帧我们都会为每个绑定查找输入
        /// </summary>
        protected virtual void DetectInput()
		{
			foreach (InventoryInputActionsBindings binding in InputBindings)
			{
				if (binding == null)
				{
					continue;
				}
				if (!binding.Active)
				{
					continue;
				}
				if (Input.GetKeyDown(binding.InputBinding) || Input.GetKeyDown(binding.AltInputBinding))
				{
					ExecuteAction(binding);
				}
			}
		}

        /// <summary>
        /// 为指定的绑定执行相应的操作
        /// </summary>
        /// <param name="binding"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected virtual void ExecuteAction(InventoryInputActionsBindings binding)
		{
			if (binding.SlotIndex > _targetInventory.Content.Length)
			{
				return;
			}
			if (_targetInventory.Content[binding.SlotIndex] == null)
			{
				return;
			}

			switch (binding.Action)
			{
				case Actions.Equip:
					MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, null, _targetInventory.name, _targetInventory.Content[binding.SlotIndex], 0, binding.SlotIndex, _targetInventory.PlayerID);
					break;
				case Actions.Use:
					MMInventoryEvent.Trigger(MMInventoryEventType.UseRequest, null, _targetInventory.name, _targetInventory.Content[binding.SlotIndex], 0, binding.SlotIndex, _targetInventory.PlayerID);
					break;
				case Actions.Drop:
					MMInventoryEvent.Trigger(MMInventoryEventType.Drop, null, _targetInventory.name, _targetInventory.Content[binding.SlotIndex], 0, binding.SlotIndex, _targetInventory.PlayerID);
					break;
				case Actions.Unequip:
					MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, _targetInventory.name, _targetInventory.Content[binding.SlotIndex], 0, binding.SlotIndex, _targetInventory.PlayerID);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
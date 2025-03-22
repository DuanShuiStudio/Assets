using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
	[System.Serializable]
	public struct AutoPickItem
	{
		public InventoryItem Item;
		public int Quantity;
	}

    /// <summary>
    /// 将此组件添加到角色中，它将能够控制库存
    /// 动画器参数：无
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Inventory")] 
	public class CharacterInventory : CharacterAbility, MMEventListener<MMInventoryEvent>
	{
		public enum WeaponRotationModes { Normal, AddEmptySlot, AddInitialWeapon }
        
		[Header("Inventories库存")]
		/// the unique ID of this player as far as the InventoryEngine is concerned. This has to match all its Inventory and InventoryEngine UI components' PlayerID for that player. If you're not going for multiplayer here, just leave Player1.
		[Tooltip("就库存引擎而言，此玩家的唯一ID。这必须与该玩家的所有库存和库存引擎UI组件的PlayerID匹配。如果你不打算在这里使用多人模式，只需留下Player1即可。")]
		public string PlayerID = "Player1";
		/// the name of the main inventory for this character
		[Tooltip("此角色的主要库存名称")]
		public string MainInventoryName;
		/// the name of the inventory where this character stores weapons
		[Tooltip("此角色存储武器的库存名称")]
		public string WeaponInventoryName;
		/// the name of the hotbar inventory for this character
		[Tooltip("此角色的快捷栏库存名称")]
		public string HotbarInventoryName;
		/// a transform to pass to the inventories, will be passed to the inventories and used as reference for drops. If left empty, this.transform will be used.
		[Tooltip("传递给库存的变换，将传递给库存并用作掉落的参考。如果留空，将使用this.transform")]
		public Transform InventoryTransform;

		[Header("Weapon Rotation武器旋转")]
        /// 武器的旋转模式：Normal将循环通过所有武器，adddemptyslot将返回空的手，AddOriginalWeapon将循环回到原来的武器
        [Tooltip("如果这是真的，将添加一个空槽的武器旋转")]
		public WeaponRotationModes WeaponRotationMode = WeaponRotationModes.Normal;

		[Header("Auto Pick自动拾取")]
		/// a list of items to automatically add to this Character's inventories on start
		[Tooltip("在启动时自动添加到此角色库存中的项目列表")]
		public AutoPickItem[] AutoPickItems;
		/// if this is true, auto pick items will only be added if the main inventory is empty
		[Tooltip("如果这个条件为真，自动拾取的物品只有在主库存为空时才会被添加")]
		public bool AutoPickOnlyIfMainInventoryIsEmpty;
		
		[Header("Auto Equip自动装备")]
		/// a weapon to auto equip on start
		[Tooltip("启动时自动装备的武器")]
		public InventoryWeapon AutoEquipWeaponOnStart;
		/// if this is true, auto equip will only occur if the main inventory is empty
		[Tooltip("如果这个条件为真，自动装备只有在主库存为空时才会发生")]
		public bool AutoEquipOnlyIfMainInventoryIsEmpty;
		/// if this is true, auto equip will only occur if the equipment inventory is empty
		[Tooltip("如果这个条件为真，自动装备只有在装备库存为空时才会发生")]
		public bool AutoEquipOnlyIfEquipmentInventoryIsEmpty;
		/// if this is true, auto equip will also happen on respawn
		[Tooltip("如果这个条件为真，自动装备也会在重生时发生")]
		public bool AutoEquipOnRespawn = true;
		/// the target handle weapon ability - if left empty, will pick the first one it finds
		[Tooltip("目标手持武器能力 - 如果留空，将选择它找到的第一个")]
		public CharacterHandleWeapon CharacterHandleWeapon;

		public virtual Inventory MainInventory { get; set; }
		public virtual Inventory WeaponInventory { get; set; }
		public virtual Inventory HotbarInventory { get; set; }
		public virtual List<string> AvailableWeaponsIDs => _availableWeaponsIDs;

		protected List<int> _availableWeapons;
		protected List<string> _availableWeaponsIDs;
		protected string _nextWeaponID;
		protected bool _nextFrameWeapon = false;
		protected string _nextFrameWeaponName;
		protected const string _emptySlotWeaponName = "_EmptySlotWeaponName";
		protected const string _initialSlotWeaponName = "_InitialSlotWeaponName";
		protected bool _initialized = false;
		protected int _initializedFrame = -1;

        /// <summary>
        /// 在init上我们设置我们的能力
        /// </summary>
        protected override void Initialization () 
		{
			base.Initialization();
			Setup ();
		}

        /// <summary>
        /// 抓取所有库存，并填满武器列表
        /// </summary>
        protected virtual void Setup()
		{
			if (InventoryTransform == null)
			{
				InventoryTransform = this.transform;
			}
			GrabInventories ();
			if (CharacterHandleWeapon == null)
			{
				CharacterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon> ();	
			}
			FillAvailableWeaponsLists ();

			if (_initialized)
			{
				return;
			}

			bool mainInventoryEmpty = true;
			if (MainInventory != null)
			{
				mainInventoryEmpty = MainInventory.NumberOfFilledSlots == 0;
			}
			bool canAutoPick = !(AutoPickOnlyIfMainInventoryIsEmpty && !mainInventoryEmpty);
			bool canAutoEquip = !(AutoEquipOnlyIfMainInventoryIsEmpty && !mainInventoryEmpty);

			if (AutoEquipOnlyIfEquipmentInventoryIsEmpty && (WeaponInventory.NumberOfFilledSlots > 0))
			{
				canAutoEquip = false;
			}

            // 如果需要，我们会自动装备物品
            if ((AutoPickItems.Length > 0) && !_initialized && canAutoPick)
			{
				foreach (AutoPickItem item in AutoPickItems)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, item.Item.TargetInventoryName, item.Item, item.Quantity, 0, PlayerID);
				}
			}

            // 如果需要，我们会自动装备武器
            if ((AutoEquipWeaponOnStart != null) && !_initialized && canAutoEquip)
			{
				AutoEquipWeapon();
			}

			_initialized = true;
			_initializedFrame = Time.frameCount;
		}

		protected virtual void AutoEquipWeapon()
		{
			MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null, AutoEquipWeaponOnStart.TargetInventoryName, AutoEquipWeaponOnStart, 1, 0, PlayerID);
			EquipWeapon(AutoEquipWeaponOnStart.ItemID);
		}

		public override void ProcessAbility()
		{
			base.ProcessAbility();
            
			if (_nextFrameWeapon)
			{
				EquipWeapon(_nextFrameWeaponName);
				_nextFrameWeapon = false;
			}
		}

        /// <summary>
        /// 抓取它能找到的任何与检查器中设置的名称匹配的目录
        /// </summary>
        protected virtual void GrabInventories()
		{
			Inventory[] inventories = FindObjectsOfType<Inventory>();
			foreach (Inventory inventory in inventories)
			{
				if (inventory.PlayerID != PlayerID)
				{
					continue;
				}
				if ((MainInventory == null) && (inventory.name == MainInventoryName))
				{
					MainInventory = inventory;
				}
				if ((WeaponInventory == null) && (inventory.name == WeaponInventoryName))
				{
					WeaponInventory = inventory;
				}
				if ((HotbarInventory == null) && (inventory.name == HotbarInventoryName))
				{
					HotbarInventory = inventory;
				}
			}
			if (MainInventory != null) { MainInventory.SetOwner (this.gameObject); MainInventory.TargetTransform = InventoryTransform;}
			if (WeaponInventory != null) { WeaponInventory.SetOwner (this.gameObject); WeaponInventory.TargetTransform = InventoryTransform;}
			if (HotbarInventory != null) { HotbarInventory.SetOwner (this.gameObject); HotbarInventory.TargetTransform = InventoryTransform;}
		}

        /// <summary>
        /// 在手柄输入时，我们注意切换武器按钮，并在需要时切换武器
        /// </summary>
        protected override void HandleInput()
		{
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}
			if (_inputManager.SwitchWeaponButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				SwitchWeapon ();
			}
		}

        /// <summary>
        /// 填满武器列表。武器列表将用于决定我们可以切换到什么武器
        /// </summary>
        protected virtual void FillAvailableWeaponsLists()
		{
			_availableWeaponsIDs = new List<string> ();
			if ((CharacterHandleWeapon == null) || (WeaponInventory == null))
			{
				return;
			}
			_availableWeapons = MainInventory.InventoryContains (ItemClasses.Weapon);
			foreach (int index in _availableWeapons)
			{
				_availableWeaponsIDs.Add (MainInventory.Content [index].ItemID);
			}
			if (!InventoryItem.IsNull(WeaponInventory.Content[0]))
			{
				if ((MainInventory.InventoryContains(WeaponInventory.Content[0].ItemID).Count <= 0) ||
				    WeaponInventory.Content[0].MoveWhenEquipped)
				{
					_availableWeaponsIDs.Add(WeaponInventory.Content[0].ItemID);
				}
			}

			_availableWeaponsIDs.Sort ();
		}

        /// <summary>
        /// 确定下一个武器的名称
        /// </summary>
        protected virtual void DetermineNextWeaponName ()
		{
			if (InventoryItem.IsNull(WeaponInventory.Content[0]))
			{
				_nextWeaponID = _availableWeaponsIDs [0];
				return;
			}

			if ((_nextWeaponID == _emptySlotWeaponName) || (_nextWeaponID == _initialSlotWeaponName))
			{
				_nextWeaponID = _availableWeaponsIDs[0];
				return;
			}

			for (int i = 0; i < _availableWeaponsIDs.Count; i++)
			{
				if (_availableWeaponsIDs[i] == WeaponInventory.Content[0].ItemID)
				{
					if (i == _availableWeaponsIDs.Count - 1)
					{
						switch (WeaponRotationMode)
						{
							case WeaponRotationModes.AddEmptySlot:
								_nextWeaponID = _emptySlotWeaponName;
								return;
							case WeaponRotationModes.AddInitialWeapon:
								_nextWeaponID = _initialSlotWeaponName;
								return;
						}

						_nextWeaponID = _availableWeaponsIDs [0];
					}
					else
					{
						_nextWeaponID = _availableWeaponsIDs [i+1];
					}
				}
			}
		}

        /// <summary>
        /// 用参数中传递的名称装备武器
        /// </summary>
        /// <param name="weaponID"></param>
        public virtual void EquipWeapon(string weaponID)
		{
			if ((weaponID == _emptySlotWeaponName) && (CharacterHandleWeapon != null))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0, PlayerID);
				CharacterHandleWeapon.ChangeWeapon(null, _emptySlotWeaponName, false);
				MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, WeaponInventory.name, null, 0, 0, PlayerID);
				return;
			}

			if ((weaponID == _initialSlotWeaponName) && (CharacterHandleWeapon != null))
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0, PlayerID);
				CharacterHandleWeapon.ChangeWeapon(CharacterHandleWeapon.InitialWeapon, _initialSlotWeaponName, false);
				MMInventoryEvent.Trigger(MMInventoryEventType.Redraw, null, WeaponInventory.name, null, 0, 0, PlayerID);
				return;
			}
			
			for (int i = 0; i < MainInventory.Content.Length ; i++)
			{
				if (InventoryItem.IsNull(MainInventory.Content[i]))
				{
					continue;
				}
				if (MainInventory.Content[i].ItemID == weaponID)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.EquipRequest, null, MainInventory.name, MainInventory.Content[i], 0, i, PlayerID);
					break;
				}
			}
		}

        /// <summary>
        /// 切换到下一个武器
        /// </summary>
        protected virtual void SwitchWeapon()
		{
            // 如果没有角色处理武器组件，我们就不能切换武器，我们什么都不做，然后退出
            if ((CharacterHandleWeapon == null) || (WeaponInventory == null))
			{
				return;
			}

			FillAvailableWeaponsLists ();

            // 如果我们只有0或1件武器，就没有什么可切换的，我们什么也不做，然后退出
            if (_availableWeaponsIDs.Count <= 0)
			{
				return;
			}

			DetermineNextWeaponName ();
			EquipWeapon (_nextWeaponID);
			PlayAbilityStartFeedbacks();
			PlayAbilityStartSfx();
		}

        /// <summary>
        /// 监视InventoryLoaded事件
        /// 当一个库存被加载，如果它是我们的武器库存，我们检查是否已经有武器装备，如果是，我们装备它
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryLoaded)
			{
				if (inventoryEvent.TargetInventoryName == WeaponInventoryName)
				{
					this.Setup ();
					if (WeaponInventory != null)
					{
						if (!InventoryItem.IsNull (WeaponInventory.Content [0]))
						{
							CharacterHandleWeapon.Setup ();
							WeaponInventory.Content [0].Equip (PlayerID);
						}
					}
				}
			}
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.Pick)
			{
				bool isSubclass = (inventoryEvent.EventItem.GetType().IsSubclassOf(typeof(InventoryWeapon)));
				bool isClass = (inventoryEvent.EventItem.GetType() == typeof(InventoryWeapon));
				if (isClass || isSubclass)
				{
					InventoryWeapon inventoryWeapon = (InventoryWeapon)inventoryEvent.EventItem;
					switch (inventoryWeapon.AutoEquipMode)
					{
						case InventoryWeapon.AutoEquipModes.NoAutoEquip:
							// we do nothing
							break;

						case InventoryWeapon.AutoEquipModes.AutoEquip:
							_nextFrameWeapon = true;
							_nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
							break;

						case InventoryWeapon.AutoEquipModes.AutoEquipIfEmptyHanded:
							if (CharacterHandleWeapon.CurrentWeapon == null)
							{
								_nextFrameWeapon = true;
								_nextFrameWeaponName = inventoryEvent.EventItem.ItemID;
							}
							break;
					}
				}
			}
		}
		
		protected override void OnRespawn()
		{
			if (_initializedFrame == Time.frameCount)
			{
				return;
			}
			
			if ((AutoEquipWeaponOnStart == null) || !AutoEquipOnRespawn || (MainInventory == null) || (WeaponInventory == null))
			{
				return;
			}
			
			MMInventoryEvent.Trigger(MMInventoryEventType.Destroy, null, MainInventoryName, AutoEquipWeaponOnStart, 1, 0, PlayerID);
			AutoEquipWeapon();
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			if (WeaponInventory != null)
			{
				MMInventoryEvent.Trigger(MMInventoryEventType.UnEquipRequest, null, WeaponInventoryName, WeaponInventory.Content[0], 0, 0, PlayerID);
			}            
		}

        /// <summary>
        /// 启用后，我们开始监听MMGameEvents。您可能希望将其扩展为侦听其他类型的事件。
        /// </summary>
        protected override void OnEnable()
		{
			base.OnEnable();
			this.MMEventStartListening<MMInventoryEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听MMGameEvents。您可能希望扩展它以停止侦听其他类型的事件。
        /// </summary>
        protected override void OnDisable()
		{
			base.OnDisable ();
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;

namespace MoreMountains.TopDownEngine
{	
	[RequireComponent(typeof(Weapon))]
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Ammo")]
	public class WeaponAmmo : TopDownMonoBehaviour, MMEventListener<MMStateChangeEvent<MoreMountains.TopDownEngine.Weapon.WeaponStates>>, MMEventListener<MMInventoryEvent>, MMEventListener<MMGameEvent>
	{
		[Header("Ammo弹药")]
		
		/// the ID of this ammo, to be matched on the ammo display if you use one
		[Tooltip("这个弹药的ID，如果你使用一个的话，要在弹药显示器上匹配")]
		public string AmmoID;
		/// the name of the inventory where the system should look for ammo
		[Tooltip("系统应查找弹药的库存名称")]
		public string AmmoInventoryName = "MainInventory";
		/// the theoretical maximum of ammo
		[Tooltip("弹药的理论最大值")]
		public int MaxAmmo = 100;
		/// if this is true, everytime you equip this weapon, it'll auto fill with ammo
		[Tooltip("如果这是真的，每次你装备这件武器，它都会自动装满弹药")]
		public bool ShouldLoadOnStart = true;

		/// if this is true, everytime you equip this weapon, it'll auto fill with ammo
		[Tooltip("如果这是真的，每次你装备这件武器，它都会自动装满弹药")]
		public bool ShouldEmptyOnSave = true;

		/// the current amount of ammo available in the inventory
		[MMReadOnly]
		[Tooltip("目前库存中可用的弹药数量")]
		public int CurrentAmmoAvailable;
        /// 储存这件武器弹药的库存
        public virtual Inventory AmmoInventory { get; set; }

		protected Weapon _weapon;
		protected InventoryItem _ammoItem;
		protected bool _emptied = false;

        /// <summary>
        /// 开局，如果我们能找到弹药库存，我们就抓住它
        /// </summary>
        protected virtual void Start()
		{
			_weapon = GetComponent<Weapon> ();
			Inventory[] inventories = FindObjectsOfType<Inventory>();
			foreach (Inventory inventory in inventories)
			{
				CharacterInventory characterInventory = _weapon.Owner.FindAbility<CharacterInventory>();
				if (characterInventory != null)
				{
					if (characterInventory.PlayerID != inventory.PlayerID)
					{
						continue;
					}
				}
				else
				{
					if (inventory.PlayerID != _weapon.Owner.PlayerID) 
					{
						continue;
					}	
				}
				
				if ((AmmoInventory == null) && (inventory.name == AmmoInventoryName))
				{
					AmmoInventory = inventory;
				}
			}
			if (ShouldLoadOnStart)
			{
				LoadOnStart ();	
			}
		}

        /// <summary>
        /// 我们的武器装满了弹药
        /// </summary>
        protected virtual void LoadOnStart()
		{
			FillWeaponWithAmmo ();
		}

        /// <summary>
        /// 更新当前可用弹药计数器
        /// </summary>
        protected virtual void RefreshCurrentAmmoAvailable()
		{
			CurrentAmmoAvailable = AmmoInventory.GetQuantity (AmmoID);
		}

        /// <summary>
        /// 如果这件武器有足够的弹药可以发射，则返回真；否则，返回假
        /// </summary>
        /// <returns></returns>
        public virtual bool EnoughAmmoToFire()
		{
			if (AmmoInventory == null)
			{
				Debug.LogWarning (this.name + " 没能找到相关的库存。场景里有吗？它的名字应该是 '" + AmmoInventoryName + "'.");
				return false;
			}

			RefreshCurrentAmmoAvailable ();

			if (_weapon.MagazineBased)
			{
				if (_weapon.CurrentAmmoLoaded >= _weapon.AmmoConsumedPerShot)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				if (CurrentAmmoAvailable >= _weapon.AmmoConsumedPerShot)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
		}

        /// <summary>
        /// 根据每次射击消耗的弹药量来消耗弹药
        /// </summary>
        protected virtual void ConsumeAmmo()
		{
			if (_weapon.MagazineBased)
			{
				_weapon.CurrentAmmoLoaded = _weapon.CurrentAmmoLoaded - _weapon.AmmoConsumedPerShot;
			}
			else
			{
				for (int i = 0; i < _weapon.AmmoConsumedPerShot; i++)
				{
					AmmoInventory.UseItem (AmmoID);	
					CurrentAmmoAvailable--;
				}	
			}

			if (CurrentAmmoAvailable < _weapon.AmmoConsumedPerShot)
			{
				if (_weapon.AutoDestroyWhenEmpty)
				{
					StartCoroutine(_weapon.WeaponDestruction());
				}
			}
		}

        /// <summary>
        /// 用弹药装满武器
        /// </summary>
        public virtual void FillWeaponWithAmmo()
		{
			if (AmmoInventory != null)
			{
				RefreshCurrentAmmoAvailable ();
			}

			if (_ammoItem == null)
			{
				List<int> list = AmmoInventory.InventoryContains(AmmoID);
				if (list.Count > 0)
				{
					_ammoItem = AmmoInventory.Content[list[list.Count - 1]].Copy();
				}
			}

			if (_weapon.MagazineBased)
			{
				int counter = 0;
				int stock = CurrentAmmoAvailable;
                
				for (int i = _weapon.CurrentAmmoLoaded; i < _weapon.MagazineSize; i++)
				{
					if (stock > 0) 
					{
						stock--;
						counter++;
						
						AmmoInventory.UseItem (AmmoID);	
					}									
				}
				_weapon.CurrentAmmoLoaded += counter;
			}
			
			RefreshCurrentAmmoAvailable();
		}

        /// <summary>
        /// 清空武器的弹匣，并将弹药放回库存
        /// </summary>
        public virtual void EmptyMagazine()
		{
			if (AmmoInventory != null)
			{
				RefreshCurrentAmmoAvailable ();
			}

			if ((_ammoItem == null) || (AmmoInventory == null))
			{
				return;
			}

			if (_emptied)
			{
				return;
			}

			if (_weapon.MagazineBased)
			{
				int stock = _weapon.CurrentAmmoLoaded;
				int counter = 0;
                
				for (int i = 0; i < stock; i++)
				{
					AmmoInventory.AddItem(_ammoItem, 1);
					counter++;
				}
				_weapon.CurrentAmmoLoaded -= counter;

				if (AmmoInventory.Persistent)
				{
					AmmoInventory.SaveInventory();
				}
			}
			RefreshCurrentAmmoAvailable();
			_emptied = true;
		}

        /// <summary>
        /// 当收到武器事件时，我们要么消耗弹药，要么重新装满它
        /// </summary>
        /// <param name="weaponEvent"></param>
        public virtual void OnMMEvent(MMStateChangeEvent<MoreMountains.TopDownEngine.Weapon.WeaponStates> weaponEvent)
		{
            // 如果这个事件与我们无关，我们什么也不做，直接退出
            if (weaponEvent.Target != this.gameObject)
			{
				return;
			}

			switch (weaponEvent.NewState)
			{
				case MoreMountains.TopDownEngine.Weapon.WeaponStates.WeaponUse:
					ConsumeAmmo ();
					break;

				case MoreMountains.TopDownEngine.Weapon.WeaponStates.WeaponReloadStop:
					FillWeaponWithAmmo();
					break;
			}
		}

        /// <summary>
        /// 抓取库存事件，如果需要就刷新弹药
        /// </summary>
        /// <param name="inventoryEvent"></param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Pick:
					if (inventoryEvent.EventItem.ItemClass == ItemClasses.Ammo)
					{
						StartCoroutine(DelayedRefreshCurrentAmmoAvailable());
					}
					break;				
			}
		}

		protected IEnumerator DelayedRefreshCurrentAmmoAvailable()
		{
			yield return null;
			RefreshCurrentAmmoAvailable ();
		}

        /// <summary>
        /// 抓取库存事件，如果需要的话刷新弹药
        /// </summary>
        /// <param name="inventoryEvent"></param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			switch (gameEvent.EventName)
			{
				case "Save":
					if (ShouldEmptyOnSave)
					{
						EmptyMagazine();    
					}
					break;				
			}
		}

		protected void OnDestroy()
		{
            // 销毁时，我们将弹药放回库存
            EmptyMagazine();
		}

        /// <summary>
        /// 启用时，我们开始监听MMGameEvents。你可能需要扩展它来监听其他类型的事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMStateChangeEvent<MoreMountains.TopDownEngine.Weapon.WeaponStates>>();
			this.MMEventStartListening<MMInventoryEvent> ();
			this.MMEventStartListening<MMGameEvent>();
		}

        /// <summary>
        /// 禁用时，我们停止监听MMGameEvents。你可能需要扩展它来停止监听其他类型的事件。
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMStateChangeEvent<MoreMountains.TopDownEngine.Weapon.WeaponStates>>();
			this.MMEventStopListening<MMInventoryEvent> ();
			this.MMEventStopListening<MMGameEvent>();
		}
	}
}
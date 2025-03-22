using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到2D碰撞器上，您将能够让它在某个条件满足时执行一个操作 
    /// 当一个角色装备了指定的钥匙并进入该区域时
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Key Operated Zone")]
	public class KeyOperatedZone : ButtonActivated 
	{
		[MMInspectorGroup("Key", true, 18)]

		/// whether this zone actually requires a key
		[Tooltip("无论这个区域实际上是否需要一把钥匙")]
		public bool RequiresKey = true;
		/// the key ID, that will be checked against the existence (or not) of a key of the same name in the player's inventory
		[Tooltip("钥匙的ID，将会根据玩家物品栏中是否存在相同名字的钥匙来进行判断（存在或不存在）")]
		public string KeyID;
		/// the method that should be triggered when the key is used
		[Tooltip("当钥匙被使用时应该触发的方法")]
		public UnityEvent KeyAction;
        
		protected GameObject _collidingObject;
		protected List<int> _keyList;

        /// <summary>
        /// 在开始的时候，我们初始化我们的对象
        /// </summary>
        protected virtual void Start()
		{
			_keyList = new List<int> ();
		}

        /// <summary>
        /// 当进入（某个区域）时，我们存储发生碰撞的对象
        /// </summary>
        /// <param name="collider">Something colliding with the water.</param>
        protected override void OnTriggerEnter2D(Collider2D collider)
		{
			_collidingObject = collider.gameObject;
			base.OnTriggerEnter2D (collider);
		}

		protected override void OnTriggerEnter(Collider collider)
		{
			_collidingObject = collider.gameObject;
			base.OnTriggerEnter(collider);
		}

        /// <summary>
        /// 当按钮被按下时，我们检查物品栏中是否有钥匙
        /// </summary>
        public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				PromptError();
				return;
			}

			if (_collidingObject == null) { return; }

			if (RequiresKey)
			{
				CharacterInventory characterInventory = _collidingObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterInventory> ();
				if (characterInventory == null)
				{
					PromptError();
					return;
				}	

				_keyList.Clear ();
				_keyList = characterInventory.MainInventory.InventoryContains (KeyID);
				if (_keyList.Count == 0)
				{
					PromptError();
					return;
				}
				else
				{
					base.TriggerButtonAction ();
					characterInventory.MainInventory.UseItem(KeyID);
				}
			}

			TriggerKeyAction ();
			ActivateZone ();
		}

        /// <summary>
        /// 调用与该钥匙相关联的方法
        /// </summary>
        protected virtual void TriggerKeyAction()
		{
			if (KeyAction != null)
			{
				KeyAction.Invoke ();
			}
		}
	}
}
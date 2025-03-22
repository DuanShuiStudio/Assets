using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到场景中的某个对象上，使其表现得像一个宝箱。你需要一个钥匙操作区域来打开它，并在其上放置物品选择器来填充其内容
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Inventory Engine Chest")]
	public class InventoryEngineChest : TopDownMonoBehaviour 
	{
		protected Animator _animator;
		protected ItemPicker[] _itemPickerList;

        /// <summary>
        /// 开始时，我们获取动画器和物品选择器列表
        /// </summary>
        protected virtual void Start()
		{
			_animator = GetComponent<Animator> ();
			_itemPickerList = GetComponents<ItemPicker> ();
		}

        /// <summary>
        /// 打开箱子的公共方法，通常由相关联的钥匙操作区域调用
        /// </summary>
        public virtual void OpenChest()
		{
			TriggerOpeningAnimation ();
			PickChestContents ();
		}

        /// <summary>
        /// 触发开启动画
        /// </summary>
        protected virtual void TriggerOpeningAnimation()
		{
			if (_animator == null)
			{
				return;
			}
			_animator.SetTrigger ("OpenChest");
		}

        /// <summary>
        /// 将所有相关拾取器中的物品放入玩家的背包中
        /// </summary>
        protected virtual void PickChestContents()
		{
			if (_itemPickerList.Length == 0)
			{
				return;
			}
			foreach (ItemPicker picker in _itemPickerList)
			{
				picker.Pick ();
			}
		}
			
	}
}
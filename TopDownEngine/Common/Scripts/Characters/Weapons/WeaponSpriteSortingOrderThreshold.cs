using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个类将根据武器的当前旋转角度修改与其排序顺序相关联的精灵。
    /// 在2D武器中，根据这个角度使武器位于角色的前方或后方是非常有用的
    /// </summary>
    [RequireComponent(typeof(WeaponAim2D))]
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Sprite Sorting Order Threshold")]
	public class WeaponSpriteSortingOrderThreshold : TopDownMonoBehaviour
	{
		/// the angle threshold at which to switch the sorting order
		[Tooltip("切换排序顺序的角度阈值")]
		public float Threshold = 0f;
		/// the sorting order to apply when the weapon's rotation is below threshold
		[Tooltip("当武器的旋转角度低于阈值时要应用的排序顺序")]
		public int BelowThresholdSortingOrder = 1;
		/// the sorting order to apply when the weapon's rotation is above threshold
		[Tooltip("当武器的旋转角度高于阈值时要应用的排序顺序")]
		public int AboveThresholdSortingOrder = -1;
		/// the sprite whose sorting order we want to modify
		[Tooltip("我们想要修改其排序顺序的精灵")]
		public SpriteRenderer Sprite;

		protected WeaponAim2D _weaponAim2D;

        /// <summary>
        /// 在唤醒（On Awake）时，我们获取我们的武器瞄准组件
        /// </summary>
        protected virtual void Awake()
		{
			_weaponAim2D = this.gameObject.GetComponent<WeaponAim2D>();
		}

        /// <summary>
        /// 在更新（On Update）时，我们根据当前武器的角度改变我们的排序顺序
        /// </summary>
        protected virtual void Update()
		{
			if ((_weaponAim2D == null) || (Sprite == null)) 
			{
				return;
			}

			Sprite.sortingOrder = (_weaponAim2D.CurrentAngleRelative > Threshold) ? AboveThresholdSortingOrder : BelowThresholdSortingOrder;
		}
	}
}
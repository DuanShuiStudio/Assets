using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个动作是用来强迫你的角色切换到另一种武器。只要把一个预制武器拖到它的NewWeapon槽中，你就可以开始了。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Change Weapon")]
	//[RequireComponent(typeof(CharacterHandleWeapon))]
	public class AIActionChangeWeapon : AIAction
	{
		/// the new weapon to equip when that action is performed
		[Tooltip("执行该动作时要装备的新武器")]
		public Weapon NewWeapon;

		protected CharacterHandleWeapon _characterHandleWeapon;
		protected int _change = 0;

        /// <summary>
        /// 在init中，我们获取CharacterHandleWeapon能力
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
		}

        /// <summary>
        /// 在PerformAction中，我们改变武器
        /// </summary>
        public override void PerformAction()
		{
			ChangeWeapon();
		}

        /// <summary>
        /// 执行武器更换
        /// </summary>
        protected virtual void ChangeWeapon()
		{
			if (_change < 1)
			{
				if (NewWeapon == null)
				{
					_characterHandleWeapon.ChangeWeapon(NewWeapon, "");
				}
				else
				{
					_characterHandleWeapon.ChangeWeapon(NewWeapon, NewWeapon.name);
				}
                
				_change++;
			}
		}

        /// <summary>
        /// 重置计数器
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			_change = 0;
		}
	}
}
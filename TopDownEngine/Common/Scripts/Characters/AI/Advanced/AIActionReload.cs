using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 用于请求武器重新装填的AIACtion
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Reload")]
	public class AIActionReload : AIAction
	{
		public bool OnlyReloadOnceInThisSate = true;

		protected CharacterHandleWeapon _characterHandleWeapon;
		protected bool _reloadedOnce = false;

        /// <summary>
        /// 在init中，我们获取组件
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
		}

        /// <summary>
        /// 请求重新加载
        /// </summary>
        public override void PerformAction()
		{
			if (OnlyReloadOnceInThisSate && _reloadedOnce)
			{
				return;
			}
			if (_characterHandleWeapon == null)
			{
				return;
			}
			_characterHandleWeapon.Reload();
			_reloadedOnce = true;
		}

        /// <summary>
        /// 在进入状态时，我们重置计数器
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			_reloadedOnce = false;
		}
	}
}
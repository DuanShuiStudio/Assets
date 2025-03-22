using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果需要重新加载，这个Decision将返回true
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Reload Needed")]
	public class AIDecisionReloadNeeded : AIDecision
	{
		protected CharacterHandleWeapon _characterHandleWeapon;

        /// <summary>
        /// 在Init中，我们存储了CharacterHandleWeapon
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
		}

        /// <summary>
        /// 在决定时，如果需要重新加载，则返回true
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			if (_characterHandleWeapon == null)
			{
				return false;
			}

			if (_characterHandleWeapon.CurrentWeapon == null)
			{
				return false;
			}

			return _characterHandleWeapon.CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadNeeded;
		}
	}
}
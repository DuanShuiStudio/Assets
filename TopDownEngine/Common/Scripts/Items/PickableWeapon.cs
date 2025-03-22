using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此类添加到可收集物中，以便玩家在
    /// 将此类添加到可收集时更换武器
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Pickable Weapon")]
	public class PickableWeapon : PickableItem
	{
		[Header("Pickable Weapon可拾取的武器")]
		/// the new weapon the player gets when collecting this object
		[Tooltip("玩家在收集此对象时获得的新武器")]
		public Weapon WeaponToGive;
		/// the ID of the CharacterHandleWeapon ability you want this weapon to go to (1 by default)
		[Tooltip("你希望此武器归属的角色处理武器能力的ID（默认为1）")]
		public int HandleWeaponID = 1;


		protected CharacterHandleWeapon _characterHandleWeapon;


        /// <summary>
        /// 当武器被拾取时会发生什么。
        /// </summary>
        protected override void Pick(GameObject picker)
		{
			Character character = _collidingObject.gameObject.MMGetComponentNoAlloc<Character>();

			if (character == null)
			{
				return;
			}
			
			if (_characterHandleWeapon != null)
			{
				_characterHandleWeapon.ChangeWeapon(WeaponToGive, WeaponToGive.WeaponName);
			}
		}

        /// <summary>
        /// 检查该对象是否可被拾取
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        protected override bool CheckIfPickable()
		{
			_character = _collidingObject.GetComponent<Character>();

            // 如果与硬币碰撞的不是角色行为，我们就不采取任何行动并退出
            if ((_character == null) || (_character.FindAbility<CharacterHandleWeapon>() == null))
			{
				return false;
			}
			if (_character.CharacterType != Character.CharacterTypes.Player)
			{
				return false;
			}
            // 我们将武器装备到选定的角色上
            CharacterHandleWeapon[] handleWeapons = _character.GetComponentsInChildren<CharacterHandleWeapon>();
			foreach (CharacterHandleWeapon handleWeapon in handleWeapons)
			{
				if ((handleWeapon.HandleWeaponID == HandleWeaponID) && (handleWeapon.CanPickupWeapons))
				{
					_characterHandleWeapon = handleWeapon;
				}
			}

			if (_characterHandleWeapon == null)
			{
				return false;
			}
			return true;
		}
	}
}
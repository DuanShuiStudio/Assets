using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Coin manager
	/// </summary>
	[AddComponentMenu("TopDown Engine/Items/Explodudes Bomb Picker")]
	public class ExplodudesBombPicker : PickableItem
	{
		[Header("demo-Explodudes Bomb Picker炸弹拾取者爆炸")]
		/// The amount of points to add when collected
		[Tooltip("demo-收集时要增加的分数")]
		public int BombsToAdd = 1;

		protected CharacterHandleWeapon _characterHandleWeapon;
		protected ExplodudesWeapon _explodudesWeapon;

        /// <summary>
        /// 当某物与硬币碰撞时触发
        /// </summary>
        /// <param name="collider">Other.</param>
        protected override void Pick(GameObject picker) 
		{
			_characterHandleWeapon = picker.MMGetComponentNoAlloc<CharacterHandleWeapon>();
			if (_characterHandleWeapon == null)
			{
				return;
			}
			_explodudesWeapon = _characterHandleWeapon.CurrentWeapon.GetComponent<ExplodudesWeapon>();
			if (_explodudesWeapon == null)
			{
				return;
			}
			_explodudesWeapon.MaximumAmountOfBombsAtOnce += BombsToAdd;
			_explodudesWeapon.RemainingBombs += BombsToAdd;
		}
	}
}
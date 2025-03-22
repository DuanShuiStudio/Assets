using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这个组件添加到一个带有WeaponAutoAim（2D或3D）的武器上，它会在可选的延迟后自动向目标射击
    /// 为了防止/停止自动射击，只需禁用这个组件，然后重新启用它以恢复自动射击
    /// </summary>
    public class WeaponAutoShoot : TopDownMonoBehaviour
	{
		[Header("Auto Shoot自动射击")]
		/// the delay (in seconds) between acquiring a target and starting shooting at it
		[Tooltip("在获取目标和开始向其射击之间的延迟（以秒为单位）。")]
		public float DelayBeforeShootAfterAcquiringTarget = 0.1f;
		/// if this is true, the weapon will only auto shoot if its owner is idle 
		[Tooltip("如果为真，武器只有在其拥有者空闲时才会自动射击")]
		public bool OnlyAutoShootIfOwnerIsIdle = false;
		
		protected WeaponAutoAim _weaponAutoAim;
		protected Weapon _weapon;
		protected bool _hasWeaponAndAutoAim;
		protected float _targetAcquiredAt;
		protected Transform _lastTarget;

        /// <summary>
        /// 在唤醒时，我们初始化我们的组件
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 获取自动瞄准和武器
        /// </summary>
        protected virtual void Initialization()
		{
			_weaponAutoAim = this.gameObject.GetComponent<WeaponAutoAim>();
			_weapon = this.gameObject.GetComponent<Weapon>();
			if (_weaponAutoAim == null)
			{
				Debug.LogWarning(this.name + " : 这个对象上的WeaponAutoShoot功能要求你在武器上添加一个WeaponAutoAim2D或WeaponAutoAim3D组件.");
				return;
			}
			_hasWeaponAndAutoAim = (_weapon != null) && (_weaponAutoAim != null);
		}

        /// <summary>
        /// 你可以使用一个公共方法来更新缓存的武器
        /// </summary>
        /// <param name="newWeapon"></param>
        public virtual void SetCurrentWeapon(Weapon newWeapon)
		{
			_weapon = newWeapon;
		}

        /// <summary>
        /// 在更新时，我们处理自动射击
        /// </summary>
        protected virtual void LateUpdate()
		{
			HandleAutoShoot();
		}

        /// <summary>
        /// 如果这个武器可以自动射击，返回真；否则，返回假
        /// </summary>
        /// <returns></returns>
        public virtual bool CanAutoShoot()
		{
			if (!_hasWeaponAndAutoAim)
			{
				return false;
			}

			if (OnlyAutoShootIfOwnerIsIdle)
			{
				if (_weapon.Owner.MovementState.CurrentState != CharacterStates.MovementStates.Idle)
				{
					return false;
				}
			}

			return true;
		}

        /// <summary>
        /// 检查我们是否有足够时间的目标，并在需要时进行射击
        /// </summary>
        protected virtual void HandleAutoShoot()
		{
			if (!CanAutoShoot())
			{
				return;
			}

			if (_weaponAutoAim.Target != null)
			{
				if (_lastTarget != _weaponAutoAim.Target)
				{
					_targetAcquiredAt = Time.time;
				}

				if (Time.time - _targetAcquiredAt >= DelayBeforeShootAfterAcquiringTarget)
				{
					_weapon.WeaponInputStart();    
				}
				_lastTarget = _weaponAutoAim.Target;
			}
		}
	}    
}
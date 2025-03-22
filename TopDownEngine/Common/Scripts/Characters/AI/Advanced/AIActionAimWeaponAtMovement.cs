using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 不射击时，将武器瞄准当前的运动
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Aim Weapon At Movement")]
	public class AIActionAimWeaponAtMovement : AIAction
	{
		/// if this is true, this action will only be performed if the brain doesn't have a Target. This can be used to combine this action with the AIActionAimWeaponAtTarget2D action for example
		[Tooltip("如果这个值为真，那么只有在大脑没有目标的情况下才会执行这个动作。例如，这可以用来将此动作与 AIActionAimWeaponAtTarget2D 动作结合起来")]
		public bool OnlyIfTargetIsNull = false;
		
		protected TopDownController _controller;
		protected CharacterHandleWeapon _characterHandleWeapon;
		protected WeaponAim _weaponAim;
		protected AIActionShoot2D _aiActionShoot2D;
		protected AIActionShoot3D _aiActionShoot3D;
		protected Vector3 _weaponAimDirection;

        /// <summary>
        /// 在init中，我们获取组件
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
			_aiActionShoot2D = this.gameObject.GetComponent<AIActionShoot2D>();
			_aiActionShoot3D = this.gameObject.GetComponent<AIActionShoot3D>();
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
		}

        /// <summary>
        /// 如果我们不射击，我们瞄准当前的运动
        /// </summary>
        public override void PerformAction()
		{
			if (!Shooting())
			{
				_weaponAimDirection = _controller.CurrentDirection;
				if (_weaponAim == null)
				{
					GrabWeaponAim();
				}
				if (_weaponAim == null)
				{
					return;
				}
				UpdateAim();
			}
		}

        /// <summary>
        /// 设置武器瞄准方向
        /// </summary>
        void UpdateAim()
		{
			if (OnlyIfTargetIsNull && (_brain.Target != null))
			{
				return;
			}
			
			_weaponAimDirection = _controller.CurrentDirection;
			if (_weaponAim != null)
			{
				_weaponAim.SetCurrentAim(_weaponAimDirection);
			}
		}

        /// <summary>
        /// 如果射击返回true，否则返回false
        /// </summary>
        /// <returns></returns>
        protected bool Shooting()
		{
			if (_aiActionShoot2D != null)
			{
				return _aiActionShoot2D.ActionInProgress;
			}
			if (_aiActionShoot3D != null)
			{
				return _aiActionShoot3D.ActionInProgress;
			}
			return false;
		}

        /// <summary>
        /// 抓取并存储武器瞄准组件
        /// </summary>
        protected virtual void GrabWeaponAim()
		{
			if ((_characterHandleWeapon != null) && (_characterHandleWeapon.CurrentWeapon != null))
			{
				_weaponAim = _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
			}            
		}

        /// <summary>
        /// 当进入这个州时，我们拿起武器
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			GrabWeaponAim();
			enabled = true;
		}

        /// <summary>
        /// 在退出时，我们自我禁用
        /// </summary>
        public override void OnExitState()
		{
			base.OnExitState();
			enabled = false;
		}
		
		
		private void Update()
		{
			if (!Shooting())
			{
				UpdateAim();
			}
		}
	}
}
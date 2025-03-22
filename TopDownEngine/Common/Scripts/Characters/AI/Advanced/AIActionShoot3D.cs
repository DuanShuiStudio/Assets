using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 使用当前装备的武器射击的动作。如果你的武器处于自动模式，会一直射击直到你退出该状态，并且在半自动模式下只会射击一次。你可以选择让角色面对（左/右）目标，并瞄准它（如果武器有一个武器瞄准组件）。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Shoot 3D")]
	//[RequireComponent(typeof(CharacterOrientation3D))]
	//[RequireComponent(typeof(CharacterHandleWeapon))]
	public class AIActionShoot3D : AIAction
	{
		public enum AimOrigins { Transform, SpawnPosition }
        
		[Header("Binding绑定")] 
		/// the CharacterHandleWeapon ability this AI action should pilot. If left blank, the system will grab the first one it finds.
		[Tooltip("这个AI动作应该控制的CharacterHandleWeapon能力。如果留空，系统将抓取它找到的第一个")]
		public CharacterHandleWeapon TargetHandleWeaponAbility;
        
		[Header("Behaviour行为")]
		/// if true the Character will aim at the target when shooting
		[Tooltip("如果为真，角色在射击时会瞄准目标")]
		public bool AimAtTarget = true;
		/// the point to consider as the aim origin
		[Tooltip("在计算朝向目标的瞄准方向时，我们将考虑的起点")]
		public AimOrigins AimOrigin = AimOrigins.Transform;
		/// an offset to apply to the aim (useful to aim at the head/torso/etc automatically)
		[Tooltip("用于瞄准的偏移量（用于自动瞄准头部/躯干等）")]
		public Vector3 ShootOffset;
		/// if this is set to true, vertical aim will be locked to remain horizontal
		[Tooltip("如果设置为true，垂直瞄准将被锁定为保持水平")]
		public bool LockVerticalAim = false;

		protected CharacterOrientation3D _orientation3D;
		protected Character _character;
		protected WeaponAim _weaponAim;
		protected ProjectileWeapon _projectileWeapon;
		protected Vector3 _weaponAimDirection;
		protected int _numberOfShoots = 0;
		protected bool _shooting = false;
		protected Weapon _targetWeapon;

        /// <summary>
        /// 在init中，我们获取CharacterHandleWeapon能力
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_character = GetComponentInParent<Character>();
			_orientation3D = _character?.FindAbility<CharacterOrientation3D>();
			if (TargetHandleWeaponAbility == null)
			{
				TargetHandleWeaponAbility = _character?.FindAbility<CharacterHandleWeapon>();
			}
		}

        /// <summary>
        /// 在表演行动中，我们面对并瞄准，如果需要，我们开枪
        /// </summary>
        public override void PerformAction()
		{
			MakeChangesToTheWeapon();
			TestAimAtTarget();
			Shoot();
		}

        /// <summary>
        /// 对武器进行修改，以确保它能与AI脚本一起工作
        /// </summary>
        protected virtual void MakeChangesToTheWeapon()
		{
			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				TargetHandleWeaponAbility.CurrentWeapon.TimeBetweenUsesReleaseInterruption = true;
			}
		}

        /// <summary>
        /// 如果需要，设置当前目标
        /// </summary>
        protected virtual void Update()
		{
			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				if (_weaponAim != null)
				{
					if (_shooting)
					{
						if (LockVerticalAim)
						{
							_weaponAimDirection.y = 0;
						}

						if (AimAtTarget)
						{
							_weaponAim.SetCurrentAim(_weaponAimDirection);    
						}
					}
				}
			}
		}

        /// <summary>
        /// 如果需要，瞄准目标
        /// </summary>
        protected virtual void TestAimAtTarget()
		{
			if (!AimAtTarget || (_brain.Target == null))
			{
				return;
			}

			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				if (_weaponAim == null)
				{
					_weaponAim = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
				}

				if (_weaponAim != null)
				{
					if (_projectileWeapon != null)
					{
						if (AimOrigin == AimOrigins.Transform)
						{
							_weaponAimDirection = _brain.Target.position + ShootOffset - _character.transform.position;   
						}
						else if (AimOrigin == AimOrigins.SpawnPosition)
						{
							_projectileWeapon.DetermineSpawnPosition();
							_weaponAimDirection = _brain.Target.position + ShootOffset - _projectileWeapon.SpawnPosition;    
						}
					}
					else
					{
						_weaponAimDirection = _brain.Target.position + ShootOffset - _character.transform.position;
					}                    
				}                
			}
			
			_shooting = true;
		}

        /// <summary>
        /// 激活武器
        /// </summary>
        protected virtual void Shoot()
		{
			if (_numberOfShoots < 1)
			{
				_targetWeapon = TargetHandleWeaponAbility.CurrentWeapon;
				TargetHandleWeaponAbility.ShootStart();
				_numberOfShoots++;
			}

			if ((_targetWeapon == null) || (TargetHandleWeaponAbility.CurrentWeapon != _targetWeapon))
			{
				OnEnterState();
			}
		}

        /// <summary>
        /// 当进入状态时，我们重置射击计数器并拿起武器
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			_numberOfShoots = 0;
			_weaponAim = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
			_projectileWeapon = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<ProjectileWeapon>();
		}

        /// <summary>
        /// 当退出状态时，我们确保不再射击
        /// </summary>
        public override void OnExitState()
		{
			base.OnExitState();
			if (TargetHandleWeaponAbility != null)
			{
				TargetHandleWeaponAbility.ForceStop();    
			}
			_shooting = false;
		}
	}
}
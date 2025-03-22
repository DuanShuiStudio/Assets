using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 使用当前装备的武器射击的动作。如果你的武器处于自动模式，会一直射击直到你退出该状态，并且在半自动模式下只会射击一次。
	/// 你可以选择让角色面对（左/右）目标，并瞄准它（如果武器有一个武器瞄准组件）。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Shoot 2D")]
	//[RequireComponent(typeof(CharacterOrientation2D))]
	//[RequireComponent(typeof(CharacterHandleWeapon))]
	public class AIActionShoot2D : AIAction
	{
		public enum AimOrigins { Transform, SpawnPoint }
        
		[Header("Binding绑定")] 
		/// the CharacterHandleWeapon ability this AI action should pilot. If left blank, the system will grab the first one it finds.
		[Tooltip("这个AI动作应该控制的CharacterHandleWeapon能力。如果留空，系统将抓取它找到的第一个")]
		public CharacterHandleWeapon TargetHandleWeaponAbility;

		[Header("Behaviour行为")] 
		/// the origin we'll take into account when computing the aim direction towards the target
		[Tooltip("在计算朝向目标的瞄准方向时，我们将考虑的起点")]
		public AimOrigins AimOrigin = AimOrigins.Transform;
		/// if true, the Character will face the target (left/right) when shooting
		[Tooltip("如果为真，当射击时角色将面向目标（左/右）")]
		public bool FaceTarget = true;
		/// if true the Character will aim at the target when shooting
		[Tooltip("如果为真，角色在射击时会瞄准目标")]
		public bool AimAtTarget = false;
		/// an offset to apply to the aim (useful to aim at the head/torso/etc automatically)
		[Tooltip("应用于瞄准的偏移量（用于自动瞄准头部/躯干等")]
		public Vector3 ShootOffset;
		/// whether or not to only perform aim when in this state
		[Tooltip("是否只在此状态下执行瞄准")]
		[MMCondition("AimAtTarget")]
		public bool OnlyAimWhenInState = false;

		protected CharacterOrientation2D _orientation2D;
		protected Character _character;
		protected WeaponAim _weaponAim;
		protected ProjectileWeapon _projectileWeapon;
		protected Vector3 _weaponAimDirection;
		protected int _numberOfShoots = 0;
		protected bool _shooting = false;

        /// <summary>
        /// 在init中，我们获取CharacterHandleWeapon能力
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_character = GetComponentInParent<Character>();
			_orientation2D = _character?.FindAbility<CharacterOrientation2D>();
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
			TestFaceTarget();
			TestAimAtTarget();
			Shoot();
		}

        /// <summary>
        /// 如果需要，设置当前目标
        /// </summary>
        protected virtual void Update()
		{
			if (OnlyAimWhenInState && !_shooting)
			{
				return;
			}
			
			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				if (_weaponAim != null)
				{
					if (_shooting)
					{
						_weaponAim.SetCurrentAim(_weaponAimDirection);
					}
					else
					{
						if (_orientation2D != null)
						{
							if (_orientation2D.IsFacingRight)
							{
								_weaponAim.SetCurrentAim(Vector3.right);
							}
							else
							{
								_weaponAim.SetCurrentAim(Vector3.left);
							}
						}                        
					}
				}
			}
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
        /// 如果需要，面对目标
        /// </summary>
        protected virtual void TestFaceTarget()
		{
			if (!FaceTarget)
			{
				return;
			}

			if (this.transform.position.x > _brain.Target.position.x)
			{
				_orientation2D.FaceDirection(-1);
			}
			else
			{
				_orientation2D.FaceDirection(1);
			}            
		}

        /// <summary>
        /// 如果需要，瞄准目标
        /// </summary>
        protected virtual void TestAimAtTarget()
		{
			if (!AimAtTarget)
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
					if ((AimOrigin == AimOrigins.SpawnPoint) && (_projectileWeapon != null))
					{
						_projectileWeapon.DetermineSpawnPosition();
						_weaponAimDirection = _brain.Target.position + ShootOffset - _projectileWeapon.SpawnPosition;
					}
					else
					{
						_weaponAimDirection = _brain.Target.position + ShootOffset - _character.transform.position;
					}                    
				}                
			}
		}

        /// <summary>
        /// 激活武器
        /// </summary>
        protected virtual void Shoot()
		{
			if (_numberOfShoots < 1)
			{
				TargetHandleWeaponAbility.ShootStart();
				_numberOfShoots++;
			}
		}

        /// <summary>
        /// 当进入状态时，我们重置射击计数器并拿起武器
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			_numberOfShoots = 0;
			_shooting = true;
			if (TargetHandleWeaponAbility.CurrentWeapon != null)
			{
				_weaponAim = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
				_projectileWeapon = TargetHandleWeaponAbility.CurrentWeapon.gameObject.MMGetComponentNoAlloc<ProjectileWeapon>();	
			}
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
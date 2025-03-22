using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将武器瞄准当前目标
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Aim Weapon At Target 2D")]
	public class AIActionAimWeaponAtTarget2D : AIAction
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
		/// if true, the Character will face the target (left/right)
		[Tooltip("如果为真，角色将面向目标（左/右）")]
		public bool FaceTarget = true;
		/// if true the Character will aim at the target
		[Tooltip("如果是真的，角色将瞄准目标")]
		public bool AimAtTarget = true;

		protected CharacterOrientation2D _orientation2D;
		protected Character _character;
		protected WeaponAim _weaponAim;
		protected ProjectileWeapon _projectileWeapon;
		protected Vector3 _weaponAimDirection;

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
			if (_brain.Target == null)
			{
				return;
			}
			TestFaceTarget();
			TestAimAtTarget();
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
						_weaponAimDirection = _brain.Target.position - _projectileWeapon.SpawnPosition;
					}
					else
					{
						_weaponAimDirection = _brain.Target.position - _character.transform.position;
					}                    
				}                
			}
			
			_weaponAim.SetCurrentAim(_weaponAimDirection);
		}

	}
}
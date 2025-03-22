using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = System.Random;

namespace MoreMountains.TopDownEngine
{
	public class HitscanWeapon : Weapon
	{
        /// 这种武器激光瞄准器可能运行的模式，默认为3D
        public enum Modes { TwoD, ThreeD }

		[MMInspectorGroup("Hitscan Spawn", true, 23)]
		/// the offset position at which the projectile will spawn
		[Tooltip("投射物将生成的偏移位置")]
		public Vector3 ProjectileSpawnOffset = Vector3.zero;
		/// the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile
		[Tooltip("在生成投射物时随机（或不）应用到每个角度上的扩散（以度为单位）")]
		public Vector3 Spread = Vector3.zero;
		/// whether or not the weapon should rotate to align with the spread angle
		[Tooltip("武器是否应该旋转以对齐扩散角度")]
		public bool RotateWeaponOnSpread = false;
		/// whether or not the spread should be random (if not it'll be equally distributed)
		[Tooltip("扩散是否应该是随机的（如果不是，则将均匀分布")]
		public bool RandomSpread = true;
		/// the projectile's spawn position
		[MMReadOnly]
		[Tooltip("投射物的生成位置")]
		public Vector3 SpawnPosition = Vector3.zero;

		[MMInspectorGroup("Hitscan", true, 24)]
		/// whether this hitscan should work in 2D or 3D
		[Tooltip("这个点射应该在2D还是3D中工作")]
		public Modes Mode = Modes.ThreeD;
		/// the layer(s) on which to hitscan ray should collide
		[Tooltip("点射射线应该碰撞的层")]
		public LayerMask HitscanTargetLayers;
		/// the maximum distance of this weapon, after that bullets will be considered lost
		[Tooltip("这种武器的最大距离，超过该距离子弹将被视为丢失")]
		public float HitscanMaxDistance = 100f;
		/// the min amount of damage to apply to a damageable (something with a Health component) every time there's a hit
		[FormerlySerializedAs("DamageCaused")] 
		[Tooltip("每次命中时对可损伤物（具有生命值组件的物体）应用的最小伤害量")]
		public float MinDamageCaused = 5;
		/// the maximum amount of damage to apply to a damageable (something with a Health component) every time there's a hit 
		[Tooltip("每次命中时对可损伤物（具有生命值组件的物体）应用的最大伤害量")]
		public float MaxDamageCaused = 5;
		/// the duration of the invincibility after a hit (to prevent insta death in the case of rapid fire)
		[Tooltip("每次命中后无敌的持续时间（以防止在快速射击的情况下瞬间死亡）")]
		public float DamageCausedInvincibilityDuration = 0.2f;
		/// a list of typed damage definitions that will be applied on top of the base damage
		[Tooltip("一系列类型化的伤害定义，将应用在基础伤害之上")]
		public List<TypedDamage> TypedDamages;

		[MMInspectorGroup("Knockback", true, 29)]
		/// the type of knockback to apply when causing damage
		[Tooltip("造成伤害时应用的击退类型")]
		public DamageOnTouch.KnockbackStyles DamageCausedKnockbackType = DamageOnTouch.KnockbackStyles.NoKnockback;
		/// The force to apply to the object that gets damaged
		[Tooltip("对受损对象应用的力")]
		public Vector3 DamageCausedKnockbackForce = new Vector3(10, 10, 10);
		
		[MMInspectorGroup("Hit Damageable", true, 25)]
		/// a MMFeedbacks to move to the position of the hit and to play when hitting something with a Health component
		[Tooltip("一个 MMFeedbacks，用于在击中具有生命值组件的对象时移动到命中位置并播放")]
		public MMFeedbacks HitDamageable;
		/// a particle system to move to the position of the hit and to play when hitting something with a Health component
		[Tooltip("一个粒子系统，用于在击中具有生命值组件的对象时移动到命中位置并播放")]
		public ParticleSystem DamageableImpactParticles;
        
		[MMInspectorGroup("Hit Non Damageable", true, 26)]
		/// a MMFeedbacks to move to the position of the hit and to play when hitting something without a Health component
		[Tooltip("一个 MMFeedbacks，用于在击中没有生命值组件的对象时移动到命中位置并播放")]
		public MMFeedbacks HitNonDamageable;
		/// a particle system to move to the position of the hit and to play when hitting something without a Health component
		[Tooltip("一个粒子系统，用于在击中没有生命值组件的对象时移动到命中位置并播放")]
		public ParticleSystem NonDamageableImpactParticles;

		protected Vector3 _flippedProjectileSpawnOffset;
		protected Vector3 _randomSpreadDirection;
		protected bool _initialized = false;
		protected Transform _projectileSpawnTransform;
		public virtual RaycastHit _hit { get; protected set; }
		public virtual RaycastHit2D _hit2D { get; protected set; }
		public virtual Vector3 _origin { get; protected set; }
		protected Vector3 _destination;
		protected Vector3 _direction;
		protected GameObject _hitObject = null;
		protected Vector3 _hitPoint;
		protected Health _health;
		protected Vector3 _damageDirection;
		protected Vector3 _knockbackRelativePosition = Vector3.zero;
		protected Vector3 _knockbackForce = Vector3.zero;
		protected TopDownController _knockbackTopDownController;

		[MMInspectorButton("TestShoot")]
        /// 一个按钮来测试射击方法
        public bool TestShootButton;

        /// <summary>
        /// 一个触发武器的测试方法
        /// </summary>
        protected virtual void TestShoot()
		{
			if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
			{
				WeaponInputStart();
			}
			else
			{
				WeaponInputStop();
			}
		}

        /// <summary>
        /// 初始化这个武器
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();
			_weaponAim = GetComponent<WeaponAim>();

			if (!_initialized)
			{
				if (FlipWeaponOnCharacterFlip)
				{
					_flippedProjectileSpawnOffset = ProjectileSpawnOffset;
					_flippedProjectileSpawnOffset.y = -_flippedProjectileSpawnOffset.y;
				}
				_initialized = true;
			}
		}

        /// <summary>
        /// 每次使用武器时调用
        /// </summary>
        public override void WeaponUse()
		{
			base.WeaponUse();

			DetermineSpawnPosition();
			DetermineDirection();
			SpawnProjectile(SpawnPosition, true);
			HandleDamage();
		}

        /// <summary>
        /// 确定我们需要投射的射线的方向
        /// </summary>
        protected virtual void DetermineDirection()
		{
			if (RandomSpread)
			{
				_randomSpreadDirection.x = UnityEngine.Random.Range(-Spread.x, Spread.x);
				_randomSpreadDirection.y = UnityEngine.Random.Range(-Spread.y, Spread.y);
				_randomSpreadDirection.z = UnityEngine.Random.Range(-Spread.z, Spread.z);
			}
			else
			{
                
				_randomSpreadDirection = Vector3.zero;
			}
            
			Quaternion spread = Quaternion.Euler(_randomSpreadDirection);
            
			if (Owner.CharacterDimension == Character.CharacterDimensions.Type3D)
			{
				_randomSpreadDirection = spread * transform.forward;
			}
			else
			{
				_randomSpreadDirection = spread * transform.right * (Flipped ? -1 : 1);
			}
            
			if (RotateWeaponOnSpread)
			{
				this.transform.rotation = this.transform.rotation * spread;
			}
		}

        /// <summary>
        /// 生成一个新对象，并对其进行定位/调整大小
        /// </summary>
        public virtual void SpawnProjectile(Vector3 spawnPosition, bool triggerObjectActivation = true)
		{
			_hitObject = null;

            // 我们在该方向上投射一条射线。
            if (Mode == Modes.ThreeD)
			{
				// if 3D
				_origin = SpawnPosition;
				_hit = MMDebug.Raycast3D(_origin, _randomSpreadDirection, HitscanMaxDistance, HitscanTargetLayers, Color.red, true);

                // 如果我们击中了某物，我们的目标就是射线投射命中的位置
                if (_hit.transform != null)
				{
					_hitObject = _hit.collider.gameObject;
					_hitPoint = _hit.point;

				}
                // 否则，我们就在我们的武器前面绘制我们的激光
                else
                {
					_hitObject = null;
				}
			}
			else
			{
                // if 2D

                //_direction = this.Flipped ? Vector3.left : Vector3.right;

                // 我们在武器前方投射一条射线来检测障碍物
                _origin = SpawnPosition;
				_hit2D = MMDebug.RayCast(_origin, _randomSpreadDirection, HitscanMaxDistance, HitscanTargetLayers, Color.red, true);
				if (_hit2D)
				{
					_hitObject = _hit2D.collider.gameObject;
					_hitPoint = _hit2D.point;
				}
                // 否则，我们就在我们的武器前面绘制我们的激光
                else
                {
					_hitObject = null;
				}
			}      
		}

        /// <summary>
        /// 处理伤害和相关的反馈
        /// </summary>
        protected virtual void HandleDamage()
		{
			if (_hitObject == null)
			{
				return;
			}

			_health = _hitObject.MMGetComponentNoAlloc<Health>();

			if (_health == null)
			{
                // 击中不可损伤物
                if (HitNonDamageable != null)
				{
					HitNonDamageable.transform.position = _hitPoint;
					HitNonDamageable.transform.LookAt(this.transform);
					HitNonDamageable.PlayFeedbacks();
				}

				if (NonDamageableImpactParticles != null)
				{
					NonDamageableImpactParticles.transform.position = _hitPoint;
					NonDamageableImpactParticles.transform.LookAt(this.transform);
					NonDamageableImpactParticles.Play();
				}
			}
			else
			{
                // 击中可损伤物
                _damageDirection = (_hitObject.transform.position - this.transform.position).normalized;
                
				float randomDamage = UnityEngine.Random.Range(MinDamageCaused, Mathf.Max(MaxDamageCaused, MinDamageCaused));
				_health.Damage(randomDamage, this.gameObject, DamageCausedInvincibilityDuration, DamageCausedInvincibilityDuration, _damageDirection, TypedDamages);

				if (HitDamageable != null)
				{
					HitDamageable.transform.position = _hitPoint;
					HitDamageable.transform.LookAt(this.transform);
					HitDamageable.PlayFeedbacks();
				}
                
				if (DamageableImpactParticles != null)
				{
					DamageableImpactParticles.transform.position = _hitPoint;
					DamageableImpactParticles.transform.LookAt(this.transform);
					DamageableImpactParticles.Play();
				}

				ApplyKnockback();
			}
		}

        /// <summary>
        /// 如有必要，对被击中的目标应用击退效果
        /// </summary>
        protected virtual void ApplyKnockback()
		{
			if (DamageCausedKnockbackType == DamageOnTouch.KnockbackStyles.AddForce)
			{
				_knockbackTopDownController = _hitObject.MMGetComponentNoAlloc<TopDownController>();
				if (_knockbackTopDownController == null)
				{
					return;
				}
				_knockbackForce = DamageCausedKnockbackForce * _health.KnockbackForceMultiplier;
				_knockbackForce = _health.ComputeKnockbackForce(_knockbackForce, TypedDamages);
				if (Mode == Modes.ThreeD)
				{
					_knockbackRelativePosition = _hitPoint - Owner.transform.position;
					_knockbackForce = Quaternion.LookRotation(_knockbackRelativePosition) * _knockbackForce;
				}
				else 
				{
					_knockbackRelativePosition = _hitPoint - Owner.transform.position;
					_knockbackForce = Vector3.RotateTowards(_knockbackForce, _knockbackRelativePosition.normalized, 10f, 0f);
				}
				_knockbackTopDownController.Impact(_knockbackForce.normalized, _knockbackForce.magnitude);
			}
		}

        /// <summary>
        /// 根据生成偏移和武器是否翻转来确定生成位置
        /// </summary>
        public virtual void DetermineSpawnPosition()
		{
			if (Flipped)
			{
				if (FlipWeaponOnCharacterFlip)
				{
					SpawnPosition = this.transform.position - this.transform.rotation * _flippedProjectileSpawnOffset;
				}
				else
				{
					SpawnPosition = this.transform.position - this.transform.rotation * ProjectileSpawnOffset;
				}
			}
			else
			{
				SpawnPosition = this.transform.position + this.transform.rotation * ProjectileSpawnOffset;
			}

			if (WeaponUseTransform != null)
			{
				SpawnPosition = WeaponUseTransform.position;
			}
		}

        /// <summary>
        /// 当武器被选择时，在生成位置绘制一个圆圈
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
		{
			DetermineSpawnPosition();

			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(SpawnPosition, 0.2f);
		}

	}
}
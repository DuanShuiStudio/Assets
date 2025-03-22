using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一种武器类别，特别允许创建各种枪械，从霰弹枪到机枪，经由等离子枪或者火箭发射器
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Projectile Weapon")]
	public class ProjectileWeapon : Weapon, MMEventListener<TopDownEngineEvent>
	{
		[MMInspectorGroup("Projectiles", true, 22)]
		/// the offset position at which the projectile will spawn
		[Tooltip("弹药生成的偏移位置")]
		public Vector3 ProjectileSpawnOffset = Vector3.zero;
		/// in the absence of a character owner, the default direction of the projectiles
		[Tooltip("在没有角色拥有者的情况下，弹药的默认方向")]
		public Vector3 DefaultProjectileDirection = Vector3.forward;
		/// the number of projectiles to spawn per shot
		[Tooltip("每次射击生成的弹药数量")]
		public int ProjectilesPerShot = 1;

		[Header("Spawn Transforms生成变换")]
		/// a list of transforms that can be used a spawn points, instead of the ProjectileSpawnOffset. Will be ignored if left emtpy
		[Tooltip("可以用作生成点的变换列表，而不是ProjectileSpawnOffset。如果留空将被忽略")]
		public List<Transform> SpawnTransforms = new List<Transform>();
        /// 生成变换可以操作的模式列表
        public enum SpawnTransformsModes { Random, Sequential }
		/// the selected mode for spawn transforms. Sequential will go through the list sequentially, while Random will pick a random one every shot
		[Tooltip("生成变换的选择模式。顺序模式将按顺序遍历列表，而随机模式将在每次射击时随机选择一个")]
		public SpawnTransformsModes SpawnTransformsMode = SpawnTransformsModes.Sequential; 
        
		[Header("Spread子弹散布程度")]
		/// the spread (in degrees) to apply randomly (or not) on each angle when spawning a projectile
		[Tooltip("生成弹药时，每个角度的随机（或不随机）散布范围（以度为单位）")]
		public Vector3 Spread = Vector3.zero;
		/// whether or not the weapon should rotate to align with the spread angle
		[Tooltip("武器是否应旋转以与散布角度对齐")]
		public bool RotateWeaponOnSpread = false;
		/// whether or not the spread should be random (if not it'll be equally distributed)
		[Tooltip("散布是否应该是随机的（如果不是，则均匀分布）")]
		public bool RandomSpread = true;
		/// the projectile's spawn position
		[MMReadOnly]
		[Tooltip("弹药生成的位置")]
		public Vector3 SpawnPosition = Vector3.zero;

		/// the object pooler used to spawn projectiles, if left empty, this component will try to find one on its game object
		[Tooltip("用于生成弹药的对象池，如果为空，该组件将尝试在其游戏对象上找到它")]
		public MMObjectPooler ObjectPooler;
        
		[Header("Spawn Feedbacks生成反馈")]
		public List<MMFeedbacks> SpawnFeedbacks = new List<MMFeedbacks>();

		protected Vector3 _flippedProjectileSpawnOffset;
		protected Vector3 _randomSpreadDirection;
		protected bool _poolInitialized = false;
		protected Transform _projectileSpawnTransform;
		protected int _spawnArrayIndex = 0;

		[MMInspectorButton("TestShoot")]
        /// 测试射击方法的按钮
        public bool TestShootButton;

        /// <summary>
        /// 触发武器的测试方法
        /// </summary>
        protected virtual void TestShoot()
		{
			if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
			{
				WeaponInputStart ();				
			}
			else
			{
				WeaponInputStop ();
			}
		}

        /// <summary>
        /// 初始化这个武器
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();            
			_weaponAim = GetComponent<WeaponAim> ();

			if (!_poolInitialized)
			{
				if (ObjectPooler == null)
				{
					ObjectPooler = GetComponent<MMObjectPooler>();	
				}
				if (ObjectPooler == null)
				{
					Debug.LogWarning(this.name + " : no object pooler (simple or multiple) is attached to this Projectile Weapon, it won't be able to shoot anything.");
					return;
				}
				if (FlipWeaponOnCharacterFlip)
				{
					_flippedProjectileSpawnOffset = ProjectileSpawnOffset;
					_flippedProjectileSpawnOffset.y = -_flippedProjectileSpawnOffset.y;
				}
				_poolInitialized = true;
			}
		}

        /// <summary>
        /// 每次使用武器时调用
        /// </summary>
        public override void WeaponUse()
		{
			base.WeaponUse();

			DetermineSpawnPosition();

			for (int i = 0; i < ProjectilesPerShot; i++)
			{
				SpawnProjectile(SpawnPosition, i, ProjectilesPerShot, true);
				PlaySpawnFeedbacks();
			}
		}

        /// <summary>
        /// 生成新对象并定位/调整其大小。
        /// </summary>
        public virtual GameObject SpawnProjectile(Vector3 spawnPosition, int projectileIndex, int totalProjectiles, bool triggerObjectActivation = true)
		{
            /// 我们从池中获取下一个对象并确保它不为空
            GameObject nextGameObject = ObjectPooler.GetPooledGameObject();

            // 必要的检查
            if (nextGameObject == null) { return null; }
			if (nextGameObject.GetComponent<MMPoolableObject>() == null)
			{
				throw new Exception(gameObject.name + " is trying to spawn objects that don't have a PoolableObject component.");
			}
            // 我们定位对象
            nextGameObject.transform.position = spawnPosition;
			if (_projectileSpawnTransform != null)
			{
				nextGameObject.transform.position = _projectileSpawnTransform.position;
			}
            // 我们设置它的方向

            Projectile projectile = nextGameObject.GetComponent<Projectile>();
			if (projectile != null)
			{
				projectile.SetWeapon(this);
				if (Owner != null)
				{
					projectile.SetOwner(Owner.gameObject);
				}
			}
            // 我们激活对象
            nextGameObject.gameObject.SetActive(true);

			if (projectile != null)
			{
				if (RandomSpread)
				{
					_randomSpreadDirection.x = UnityEngine.Random.Range(-Spread.x, Spread.x);
					_randomSpreadDirection.y = UnityEngine.Random.Range(-Spread.y, Spread.y);
					_randomSpreadDirection.z = UnityEngine.Random.Range(-Spread.z, Spread.z);
				}
				else
				{
					if (totalProjectiles > 1)
					{
						_randomSpreadDirection.x = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.x, Spread.x);
						_randomSpreadDirection.y = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.y, Spread.y);
						_randomSpreadDirection.z = MMMaths.Remap(projectileIndex, 0, totalProjectiles - 1, -Spread.z, Spread.z);
					}
					else
					{
						_randomSpreadDirection = Vector3.zero;
					}
				}

				Quaternion spread = Quaternion.Euler(_randomSpreadDirection);

				if (Owner == null)
				{
					projectile.SetDirection(spread * transform.rotation * DefaultProjectileDirection, transform.rotation, true);
				}
				else
				{
					if (Owner.CharacterDimension == Character.CharacterDimensions.Type3D) // if we're in 3D
					{
						projectile.SetDirection(spread * transform.forward, transform.rotation, true);
					}
					else // if we're in 2D
					{
						Vector3 newDirection = (spread * transform.right) * (Flipped ? -1 : 1);
						if (Owner.Orientation2D != null)
						{
							projectile.SetDirection(newDirection, spread * transform.rotation, Owner.Orientation2D.IsFacingRight);
						}
						else
						{
							projectile.SetDirection(newDirection, spread * transform.rotation, true);
						}
					}
				}                

				if (RotateWeaponOnSpread)
				{
					this.transform.rotation = this.transform.rotation * spread;
				}
			}

			if (triggerObjectActivation)
			{
				if (nextGameObject.GetComponent<MMPoolableObject>() != null)
				{
					nextGameObject.GetComponent<MMPoolableObject>().TriggerOnSpawnComplete();
				}
			}
			return (nextGameObject);
		}

        /// <summary>
        /// 这个方法负责在弹药生成时播放反馈
        /// </summary>
        protected virtual void PlaySpawnFeedbacks()
		{
			if (SpawnFeedbacks.Count > 0)
			{
				SpawnFeedbacks[_spawnArrayIndex]?.PlayFeedbacks();
			}

			_spawnArrayIndex++;
			if (_spawnArrayIndex >= SpawnTransforms.Count)
			{
				_spawnArrayIndex = 0;
			}
		}

        /// <summary>
        /// 设置一个强制的弹药生成位置
        /// </summary>
        /// <param name="newSpawnTransform"></param>
        public virtual void SetProjectileSpawnTransform(Transform newSpawnTransform)
		{
			_projectileSpawnTransform = newSpawnTransform;
		}

        /// <summary>
        /// 根据生成偏移和武器是否翻转来决定生成位置。
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

			if (SpawnTransforms.Count > 0)
			{
				if (SpawnTransformsMode == SpawnTransformsModes.Random)
				{
					_spawnArrayIndex = Random.Range(0, SpawnTransforms.Count);
					SpawnPosition = SpawnTransforms[_spawnArrayIndex].position;
				}
				else
				{
					SpawnPosition = SpawnTransforms[_spawnArrayIndex].position;
				}
			}
		}

        /// <summary>
        /// 当武器被选中时，在生成位置绘制一个圆圈
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
		{
			DetermineSpawnPosition ();

			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(SpawnPosition, 0.2f);	
		}

		public void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.LevelStart:
					_poolInitialized = false;
					Initialization();
					break;
			}
		}

        /// <summary>
        /// 启用时，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

        /// <summary>
        /// 禁用时，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}
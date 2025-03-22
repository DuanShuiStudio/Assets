using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;
using MoreMountains.Feedbacks;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到对象中，它将对与之碰撞的对象造成损坏。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Damage/Damage On Touch")]
	public class DamageOnTouch : MMMonoBehaviour
	{
		[Flags]
		public enum TriggerAndCollisionMask
		{
			IgnoreAll = 0,
			OnTriggerEnter = 1 << 0,
			OnTriggerStay = 1 << 1,
			OnTriggerEnter2D = 1 << 6,
			OnTriggerStay2D = 1 << 7,

			All_3D = OnTriggerEnter | OnTriggerStay,
			All_2D = OnTriggerEnter2D | OnTriggerStay2D,
			All = All_3D | All_2D
		}

		/// 添加击退的方法 : noKnockback：什么也不做, set force设置力, or add force或添加力
		public enum KnockbackStyles
		{
			NoKnockback,
			AddForce
		}

        /// 可能的击退方向
        public enum KnockbackDirections
		{
			BasedOnOwnerPosition,
			BasedOnSpeed,
			BasedOnDirection,
			BasedOnScriptDirection
		}

        /// 确定损伤方向的可能方法
        public enum DamageDirections
		{
			BasedOnOwnerPosition,
			BasedOnVelocity,
			BasedOnScriptDirection
		}

		public const TriggerAndCollisionMask AllowedTriggerCallbacks = TriggerAndCollisionMask.OnTriggerEnter
		                                                                  | TriggerAndCollisionMask.OnTriggerStay
		                                                                  | TriggerAndCollisionMask.OnTriggerEnter2D
		                                                                  | TriggerAndCollisionMask.OnTriggerStay2D;

		[MMInspectorGroup("Targets", true, 3)]
		[MMInformation(
            "此组件将使您的对象对与其碰撞的对象造成损害。在这里你可以定义哪些图层会受到伤害的影响（对于一个标准的敌人，选择Player），要给予多少伤害，以及应该对受到伤害的对象施加多少力。你也可以指定命中后无敌应该持续多长时间（以秒为单位）。",
			MMInformationAttribute.InformationType.Info, false)]
		/// the layers that will be damaged by this object
		[Tooltip("将被该对象损坏的层")]
		public LayerMask TargetLayerMask;
		/// the owner of the DamageOnTouch zone
		[MMReadOnly] [Tooltip("DamageOnTouch区域的所有者")]
		public GameObject Owner;

		/// Defines on what triggers the damage should be applied, by default on enter and stay (both 2D and 3D) but this field will let you exclude triggers if needed
		[Tooltip(
            "定义了应该应用的伤害触发器，默认情况下是进入和停留（2D和3D），但如果需要，这个字段可以让你排除触发器")]
		public TriggerAndCollisionMask TriggerFilter = AllowedTriggerCallbacks;

		[MMInspectorGroup("Damage Caused", true, 8)]
		/// The min amount of health to remove from the player's health
		[FormerlySerializedAs("DamageCaused")]
		[Tooltip("从玩家生命值中移除的最小生命值")]
		public float MinDamageCaused = 10f;
		/// The max amount of health to remove from the player's health
		[Tooltip("从玩家生命值中移除的最大生命值")]
		public float MaxDamageCaused = 10f;
		/// a list of typed damage definitions that will be applied on top of the base damage
		[Tooltip("将应用于基础伤害之上的类型伤害定义列表")]
		public List<TypedDamage> TypedDamages;
		/// how to determine the damage direction passed to the Health damage method, usually you'll use velocity for moving damage areas (projectiles) and owner position for melee weapons
		[Tooltip("如何确定传递到Health damage方法的伤害方向，通常你会使用速度来处理移动伤害区域（如投射物）和近战武器的所有者位置。")]
		public DamageDirections DamageDirectionMode = DamageDirections.BasedOnVelocity;
		
		[Header("Knockback击退")]
		/// the type of knockback to apply when causing damage
		[Tooltip("造成伤害时使用的击退类型")]
		public KnockbackStyles DamageCausedKnockbackType = KnockbackStyles.AddForce;
		/// The direction to apply the knockback 
		[Tooltip(" 运用击退的方向")]
		public KnockbackDirections DamageCausedKnockbackDirection = KnockbackDirections.BasedOnOwnerPosition;
		/// The force to apply to the object that gets damaged - this force will be rotated based on your knockback direction mode. So for example in 3D if you want to be pushed back the opposite direction, focus on the z component, with a force of 0,0,20 for example
		[Tooltip("对被损坏的对象施加的力——这个力将根据你的击退方向模式进行旋转。例如，在3D中，如果你想要朝相反方向被推回去，可以专注于z分量，例如使用0,0,20的力")]
		public Vector3 DamageCausedKnockbackForce = new Vector3(10, 10, 10);
		
		[Header("Invincibility无敌")]
		/// The duration of the invincibility frames after the hit (in seconds)
		[Tooltip("命中后无敌帧的持续时间（以秒为单位）")]
		public float InvincibilityDuration = 0.5f;

		[Header("Damage over time超时伤害")]
		/// Whether or not this damage on touch zone should apply damage over time
		[Tooltip("这种伤害在接触区域是否应该应用持续伤害")]
		public bool RepeatDamageOverTime = false;
		/// if in damage over time mode, how many times should damage be repeated?
		[Tooltip("如果在持续伤害模式下，伤害应该重复多少次？")] 
		[MMCondition("RepeatDamageOverTime", true)]
		public int AmountOfRepeats = 3;
		/// if in damage over time mode, the duration, in seconds, between two damages
		[Tooltip("如果在持续伤害模式下，两次伤害之间的持续时间（以秒为单位）")]
		[MMCondition("RepeatDamageOverTime", true)]
		public float DurationBetweenRepeats = 1f;
		/// if in damage over time mode, whether or not it can be interrupted (by calling the Health:InterruptDamageOverTime method
		[Tooltip("如果在持续伤害模式下，它是否可以被中断（通过调用Health:InterruptDamageOverTime方法）")] 
		[MMCondition("RepeatDamageOverTime", true)]
		public bool DamageOverTimeInterruptible = true;
		/// if in damage over time mode, the type of the repeated damage 
		[Tooltip("如果在持续伤害模式下，重复伤害的类型")] 
		[MMCondition("RepeatDamageOverTime", true)]
		public DamageType RepeatedDamageType;

		[MMInspectorGroup("Damage Taken", true, 69)]
		[MMInformation(
            "在对它碰撞的物体施加伤害之后，你可以让这个物体伤害自己。" +
            "例如，子弹击中墙壁后会爆炸。在这里，你可以定义每次撞击物体所造成的伤害，" +
            "或者只在击中可损坏或不可损坏的物体时。请注意，此对象也需要Health组件才能发挥作用。",
			MMInformationAttribute.InformationType.Info, false)]
		/// The Health component on which to apply damage taken. If left empty, will attempty to grab one on this object.
		[Tooltip("要应用所受伤害的生命值组件。如果为空，将尝试抓取此对象上的一个生命值组件")]
		public Health DamageTakenHealth;
		/// The amount of damage taken every time, whether what we collide with is damageable or not
		[Tooltip("每次所受的伤害量，无论我们碰撞的是什么是否可造成伤害")]
		public float DamageTakenEveryTime = 0;
		/// The amount of damage taken when colliding with a damageable object
		[Tooltip("与可损坏物体碰撞时所受的伤害")]
		public float DamageTakenDamageable = 0;
		/// The amount of damage taken when colliding with something that is not damageable
		[Tooltip("与不可损坏的物体碰撞时所受的伤害")]
		public float DamageTakenNonDamageable = 0;
		/// the type of knockback to apply when taking damage
		[Tooltip("当受到伤害时使用的击退类型")]
		public KnockbackStyles DamageTakenKnockbackType = KnockbackStyles.NoKnockback;
		/// The force to apply to the object that gets damaged
		[Tooltip("施加在被损坏物体上的力")]
		public Vector3 DamageTakenKnockbackForce = Vector3.zero;
		/// The duration of the invincibility frames after the hit (in seconds)
		[Tooltip("命中后无敌帧的持续时间（以秒为单位）")]
		public float DamageTakenInvincibilityDuration = 0.5f;

		[MMInspectorGroup("Feedbacks", true, 18)]
		/// the feedback to play when hitting a Damageable
		[Tooltip("在击中可造成伤害的对象时播放的反馈")]
		public MMFeedbacks HitDamageableFeedback;
		/// the feedback to play when hitting a non Damageable
		[Tooltip("在击中不可造成伤害的对象时播放的反馈")]
		public MMFeedbacks HitNonDamageableFeedback;
		/// the feedback to play when hitting anything
		[Tooltip("在击中任何对象时播放的反馈")]
		public MMFeedbacks HitAnythingFeedback;

        /// 在击中可造成伤害的对象时触发的事件
        public UnityEvent<Health> HitDamageableEvent;
        /// 在击中不可造成伤害的对象时触发的事件
        public UnityEvent<GameObject> HitNonDamageableEvent;
        /// 在击中任何对象时触发的事件
        public UnityEvent<GameObject> HitAnythingEvent;

        // 存储
        protected Vector3 _lastPosition, _lastDamagePosition, _velocity, _knockbackForce, _damageDirection;
		protected float _startTime = 0f;
		protected Health _colliderHealth;
		protected TopDownController _topDownController;
		protected TopDownController _colliderTopDownController;
		protected List<GameObject> _ignoredGameObjects;
		protected Vector3 _knockbackForceApplied;
		protected CircleCollider2D _circleCollider2D;
		protected BoxCollider2D _boxCollider2D;
		protected SphereCollider _sphereCollider;
		protected BoxCollider _boxCollider;
		protected Color _gizmosColor;
		protected Vector3 _gizmoSize;
		protected Vector3 _gizmoOffset;
		protected Transform _gizmoTransform;
		protected bool _twoD = false;
		protected bool _initializedFeedbacks = false;
		protected Vector3 _positionLastFrame;
		protected Vector3 _knockbackScriptDirection;
		protected Vector3 _relativePosition;
		protected Vector3 _damageScriptDirection;
		protected Health _collidingHealth;

		#region Initialization
		
		/// <summary>
		/// On Awake we initialize our damage on touch area
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
		}

		/// <summary>
		/// OnEnable we set the start time to the current timestamp
		/// </summary>
		protected virtual void OnEnable()
		{
			_startTime = Time.time;
			_lastPosition = transform.position;
			_lastDamagePosition = transform.position;
		}

		/// <summary>
		/// Initializes ignore list, feedbacks, colliders and grabs components
		/// </summary>
		public virtual void Initialization()
		{
			InitializeIgnoreList();
			GrabComponents();
			InitalizeGizmos();
			InitializeColliders();
			InitializeFeedbacks();
		}

		/// <summary>
		/// Stores components
		/// </summary>
		protected virtual void GrabComponents()
		{
			if (DamageTakenHealth == null)
			{
				DamageTakenHealth = GetComponent<Health>();	
			}
			_topDownController = GetComponent<TopDownController>();
			_boxCollider = GetComponent<BoxCollider>();
			_sphereCollider = GetComponent<SphereCollider>();
			_boxCollider2D = GetComponent<BoxCollider2D>();
			_circleCollider2D = GetComponent<CircleCollider2D>();
			_lastDamagePosition = transform.position;
		}

		/// <summary>
		/// Initializes colliders, setting them as trigger if needed
		/// </summary>
		protected virtual void InitializeColliders()
		{
			_twoD = _boxCollider2D != null || _circleCollider2D != null;
			if (_boxCollider2D != null)
			{
				SetGizmoOffset(_boxCollider2D.offset);
				_boxCollider2D.isTrigger = true;
			}

			if (_boxCollider != null)
			{
				SetGizmoOffset(_boxCollider.center);
				_boxCollider.isTrigger = true;
			}

			if (_sphereCollider != null)
			{
				SetGizmoOffset(_sphereCollider.center);
				_sphereCollider.isTrigger = true;
			}

			if (_circleCollider2D != null)
			{
				SetGizmoOffset(_circleCollider2D.offset);
				_circleCollider2D.isTrigger = true;
			}
		}

		/// <summary>
		/// Initializes the _ignoredGameObjects list if needed
		/// </summary>
		protected virtual void InitializeIgnoreList()
		{
			if (_ignoredGameObjects == null) _ignoredGameObjects = new List<GameObject>();
		}

		/// <summary>
		/// Initializes feedbacks
		/// </summary>
		public virtual void InitializeFeedbacks()
		{
			if (_initializedFeedbacks) return;

			HitDamageableFeedback?.Initialization(this.gameObject);
			HitNonDamageableFeedback?.Initialization(this.gameObject);
			HitAnythingFeedback?.Initialization(this.gameObject);
			_initializedFeedbacks = true;
		}

		/// <summary>
		/// On disable we clear our ignore list
		/// </summary>
		protected virtual void OnDisable()
		{
			ClearIgnoreList();
		}

		/// <summary>
		/// On validate we ensure our inspector is in sync
		/// </summary>
		protected virtual void OnValidate()
		{
			TriggerFilter &= AllowedTriggerCallbacks;
		}
		
		#endregion

		#region Gizmos

		/// <summary>
		/// Initializes gizmo colors & settings
		/// </summary>
		protected virtual void InitalizeGizmos()
		{
			_gizmosColor = Color.red;
			_gizmosColor.a = 0.25f;
		}
		
		/// <summary>
		/// A public method letting you (re)define gizmo size
		/// </summary>
		/// <param name="newGizmoSize"></param>
		public virtual void SetGizmoSize(Vector3 newGizmoSize)
		{
			_boxCollider2D = GetComponent<BoxCollider2D>();
			_boxCollider = GetComponent<BoxCollider>();
			_sphereCollider = GetComponent<SphereCollider>();
			_circleCollider2D = GetComponent<CircleCollider2D>();
			_gizmoSize = newGizmoSize;
		}

		/// <summary>
		/// A public method letting you specify a gizmo offset
		/// </summary>
		/// <param name="newOffset"></param>
		public virtual void SetGizmoOffset(Vector3 newOffset)
		{
			_gizmoOffset = newOffset;
		}
		
		/// <summary>
		/// draws a cube or sphere around the damage area
		/// </summary>
		protected virtual void OnDrawGizmos()
		{
			Gizmos.color = _gizmosColor;

			if (_boxCollider2D != null)
			{
				if (_boxCollider2D.enabled)
				{
					MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, false);
				}
				else
				{
					MMDebug.DrawGizmoCube(transform, _gizmoOffset, _boxCollider2D.size, true);
				}
			}

			if (_circleCollider2D != null)
			{
				Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
				Gizmos.matrix = rotationMatrix;
				if (_circleCollider2D.enabled)
				{
					Gizmos.DrawSphere( (Vector2)_gizmoOffset, _circleCollider2D.radius);
				}
				else
				{
					Gizmos.DrawWireSphere((Vector2)_gizmoOffset, _circleCollider2D.radius);
				}
			}

			if (_boxCollider != null)
			{
				if (_boxCollider.enabled)
					MMDebug.DrawGizmoCube(transform,
						_gizmoOffset,
						_boxCollider.size,
						false);
				else
					MMDebug.DrawGizmoCube(transform,
						_gizmoOffset,
						_boxCollider.size,
						true);
			}

			if (_sphereCollider != null)
			{
				if (_sphereCollider.enabled)
					Gizmos.DrawSphere(transform.position, _sphereCollider.radius);
				else
					Gizmos.DrawWireSphere(transform.position, _sphereCollider.radius);
			}
		}

		#endregion

		#region PublicAPIs

		/// <summary>
		/// When knockback is in script direction mode, lets you specify the direction of the knockback
		/// </summary>
		/// <param name="newDirection"></param>
		public virtual void SetKnockbackScriptDirection(Vector3 newDirection)
		{
			_knockbackScriptDirection = newDirection;
		}

		/// <summary>
		/// When damage direction is in script mode, lets you specify the direction of damage
		/// </summary>
		/// <param name="newDirection"></param>
		public virtual void SetDamageScriptDirection(Vector3 newDirection)
		{
			_damageDirection = newDirection;
		}

		/// <summary>
		/// Adds the gameobject set in parameters to the ignore list
		/// </summary>
		/// <param name="newIgnoredGameObject">New ignored game object.</param>
		public virtual void IgnoreGameObject(GameObject newIgnoredGameObject)
		{
			InitializeIgnoreList();
			_ignoredGameObjects.Add(newIgnoredGameObject);
		}
		
		/// <summary>
		/// Removes the object set in parameters from the ignore list
		/// </summary>
		/// <param name="ignoredGameObject">Ignored game object.</param>
		public virtual void StopIgnoringObject(GameObject ignoredGameObject)
		{
			if (_ignoredGameObjects != null) _ignoredGameObjects.Remove(ignoredGameObject);
		}

		/// <summary>
		/// Clears the ignore list.
		/// </summary>
		public virtual void ClearIgnoreList()
		{
			InitializeIgnoreList();
			_ignoredGameObjects.Clear();
		}

		#endregion

		#region Loop

		/// <summary>
		/// During last update, we store the position and velocity of the object
		/// </summary>
		protected virtual void Update()
		{
			ComputeVelocity();
		}

		/// <summary>
		/// On Late Update we store our position
		/// </summary>
		protected void LateUpdate()
		{
			_positionLastFrame = transform.position;
		}

		/// <summary>
		/// Computes the velocity based on the object's last position
		/// </summary>
		protected virtual void ComputeVelocity()
		{
			if (Time.deltaTime != 0f)
			{
				_velocity = (_lastPosition - (Vector3)transform.position) / Time.deltaTime;

				if (Vector3.Distance(_lastDamagePosition, transform.position) > 0.5f)
				{
					_lastDamagePosition = transform.position;
				}

				_lastPosition = transform.position;
			}
		}

		/// <summary>
		/// Determine the damage direction to pass to the Health Damage method
		/// </summary>
		protected virtual void DetermineDamageDirection()
		{
			switch (DamageDirectionMode)
			{
				case DamageDirections.BasedOnOwnerPosition:
					if (Owner == null)
					{
						Owner = gameObject;
					}
					if (_twoD)
					{
						_damageDirection = _collidingHealth.transform.position - Owner.transform.position;
						_damageDirection.z = 0;
					}
					else
					{
						_damageDirection = _collidingHealth.transform.position - Owner.transform.position;
					}
					break;
				case DamageDirections.BasedOnVelocity:
					_damageDirection = transform.position - _lastDamagePosition;
					break;
				case DamageDirections.BasedOnScriptDirection:
					_damageDirection = _damageScriptDirection;
					break;
			}

			_damageDirection = _damageDirection.normalized;
		}

		#endregion

		#region CollisionDetection

		/// <summary>
		/// When a collision with the player is triggered, we give damage to the player and knock it back
		/// </summary>
		/// <param name="collider">what's colliding with the object.</param>
		public virtual void OnTriggerStay2D(Collider2D collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerStay2D)) return;
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger enter 2D, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>S
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerEnter2D)) return;
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger stay, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerStay(Collider collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerStay)) return;
			Colliding(collider.gameObject);
		}

		/// <summary>
		/// On trigger enter, we call our colliding endpoint
		/// </summary>
		/// <param name="collider"></param>
		public virtual void OnTriggerEnter(Collider collider)
		{
			if (0 == (TriggerFilter & TriggerAndCollisionMask.OnTriggerEnter)) return;
			Colliding(collider.gameObject);
		}

        #endregion

        /// <summary>
        /// 碰撞时，我们施加适当的伤害
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void Colliding(GameObject collider)
		{
			if (!EvaluateAvailability(collider))
			{
				return;
			}

            //  缓存重置
            _colliderTopDownController = null;
			_colliderHealth = collider.gameObject.MMGetComponentNoAlloc<Health>();

            // 如果我们撞击的物体是可损坏的
            if (_colliderHealth != null)
			{
				if (_colliderHealth.CurrentHealth > 0)
				{
					OnCollideWithDamageable(_colliderHealth);
				}
			}
            else // 如果我们碰撞的物体不会被破坏
            {
				OnCollideWithNonDamageable();
				HitNonDamageableEvent?.Invoke(collider);
			}

			OnAnyCollision(collider);
			HitAnythingEvent?.Invoke(collider);
			HitAnythingFeedback?.PlayFeedbacks(transform.position);
		}

        /// <summary>
        /// 检查是否应该对这个对象进行损坏
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected virtual bool EvaluateAvailability(GameObject collider)
		{
            // 如果我们不活动，我们什么也不做
            if (!isActiveAndEnabled) { return false; }

            // 如果我们碰撞的对象是忽略列表的一部分，我们不做任何事情并退出
            if (_ignoredGameObjects.Contains(collider)) { return false; }

            // 如果我们碰撞的不是目标层的一部分，我们不做任何事情并退出
            if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask)) { return false; }

            // 如果我们是在第一帧，我们不会造成伤害
            if (Time.time == 0f) { return false; }

			return true;
		}

        /// <summary>
        /// 描述与可损坏物体碰撞时发生的情况
        /// </summary>
        /// <param name="health">Health.</param>
        protected virtual void OnCollideWithDamageable(Health health)
		{
			_collidingHealth = health;

			if (health.CanTakeDamageThisFrame())
			{
                // 如果我们碰撞的是一个TopDownController，我们就施加一个回退力
                _colliderTopDownController = health.gameObject.MMGetComponentNoAlloc<TopDownController>();
				if (_colliderTopDownController == null)
				{
					_colliderTopDownController = health.gameObject.GetComponentInParent<TopDownController>();
				}

				HitDamageableFeedback?.PlayFeedbacks(this.transform.position);
				HitDamageableEvent?.Invoke(_colliderHealth);

                // 我们把伤害施加在与我们相撞的物体上
                float randomDamage =
					UnityEngine.Random.Range(MinDamageCaused, Mathf.Max(MaxDamageCaused, MinDamageCaused));

				ApplyKnockback(randomDamage, TypedDamages);

				DetermineDamageDirection();

				if (RepeatDamageOverTime)
				{
					_colliderHealth.DamageOverTime(randomDamage, gameObject, InvincibilityDuration,
						InvincibilityDuration, _damageDirection, TypedDamages, AmountOfRepeats, DurationBetweenRepeats,
						DamageOverTimeInterruptible, RepeatedDamageType);
				}
				else
				{
					_colliderHealth.Damage(randomDamage, gameObject, InvincibilityDuration, InvincibilityDuration,
						_damageDirection, TypedDamages);
				}
			}

            // 我们使用自我伤害
            if (DamageTakenEveryTime + DamageTakenDamageable > 0 && !_colliderHealth.PreventTakeSelfDamage)
			{
				SelfDamage(DamageTakenEveryTime + DamageTakenDamageable);
			}
		}

		#region Knockback

		/// <summary>
		/// Applies knockback if needed
		/// </summary>
		protected virtual void ApplyKnockback(float damage, List<TypedDamage> typedDamages)
		{
			if (ShouldApplyKnockback(damage, typedDamages))
			{
				_knockbackForce = DamageCausedKnockbackForce * _colliderHealth.KnockbackForceMultiplier;
				_knockbackForce = _colliderHealth.ComputeKnockbackForce(_knockbackForce, typedDamages);

				if (_twoD) // if we're in 2D
				{
					ApplyKnockback2D();
				}
				else // if we're in 3D
				{
					ApplyKnockback3D();
				}
				
				if (DamageCausedKnockbackType == KnockbackStyles.AddForce)
				{
					_colliderTopDownController.Impact(_knockbackForce.normalized, _knockbackForce.magnitude);
				}
			}
		}

		/// <summary>
		/// Determines whether or not knockback should be applied
		/// </summary>
		/// <returns></returns>
		protected virtual bool ShouldApplyKnockback(float damage, List<TypedDamage> typedDamages)
		{
			if (_colliderHealth.ImmuneToKnockbackIfZeroDamage)
			{
				if (_colliderHealth.ComputeDamageOutput(damage, typedDamages, false) == 0)
				{
					return false;
				}
			}
			
			return (_colliderTopDownController != null)
			       && (DamageCausedKnockbackForce != Vector3.zero)
			       && !_colliderHealth.Invulnerable
			       && _colliderHealth.CanGetKnockback(typedDamages);
		}

		/// <summary>
		/// Applies knockback if we're in a 2D context
		/// </summary>
		protected virtual void ApplyKnockback2D()
		{
			switch (DamageCausedKnockbackDirection)
			{
				case KnockbackDirections.BasedOnSpeed:
					var totalVelocity = _colliderTopDownController.Speed + _velocity;
					_knockbackForce = Vector3.RotateTowards(_knockbackForce,
						totalVelocity.normalized, 10f, 0f);
					break;
				case KnockbackDirections.BasedOnOwnerPosition:
					if (Owner == null)
					{
						Owner = gameObject;
					}
					_relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
					_knockbackForce = Vector3.RotateTowards(_knockbackForce, _relativePosition.normalized, 10f, 0f);
					break;
				case KnockbackDirections.BasedOnDirection:
					var direction = transform.position - _positionLastFrame;
					_knockbackForce = direction * _knockbackForce.magnitude;
					break;
				case KnockbackDirections.BasedOnScriptDirection:
					_knockbackForce = _knockbackScriptDirection * _knockbackForce.magnitude;
					break;
			}
		}

		/// <summary>
		/// Applies knockback if we're in a 3D context
		/// </summary>
		protected virtual void ApplyKnockback3D()
		{
			switch (DamageCausedKnockbackDirection)
			{
				case KnockbackDirections.BasedOnSpeed:
					var totalVelocity = _colliderTopDownController.Speed + _velocity;
					_knockbackForce = _knockbackForce * totalVelocity.magnitude;
					break;
				case KnockbackDirections.BasedOnOwnerPosition:
					if (Owner == null)
					{
						Owner = gameObject;
					}
					_relativePosition = _colliderTopDownController.transform.position - Owner.transform.position;
					_knockbackForce = Quaternion.LookRotation(_relativePosition) * _knockbackForce;
					break;
				case KnockbackDirections.BasedOnDirection:
					var direction = transform.position - _positionLastFrame;
					_knockbackForce = direction * _knockbackForce.magnitude;
					break;
				case KnockbackDirections.BasedOnScriptDirection:
					_knockbackForce = _knockbackScriptDirection * _knockbackForce.magnitude;
					break;
			}
		}

        #endregion


        /// <summary>
        /// 描述与不可损坏物体碰撞时发生的情况
        /// </summary>
        protected virtual void OnCollideWithNonDamageable()
		{
			float selfDamage = DamageTakenEveryTime + DamageTakenNonDamageable; 
			if (selfDamage > 0)
			{
				SelfDamage(selfDamage);
			}
			HitNonDamageableFeedback?.PlayFeedbacks(transform.position);
		}

        /// <summary>
        /// 描述与任何物体碰撞时可能发生的情况
        /// </summary>
        protected virtual void OnAnyCollision(GameObject other)
		{
		}

        /// <summary>
        /// 对自身造成伤害
        /// </summary>
        /// <param name="damage">Damage.</param>
        protected virtual void SelfDamage(float damage)
		{
			if (DamageTakenHealth != null)
			{
				_damageDirection = Vector3.up;
				DamageTakenHealth.Damage(damage, gameObject, 0f, DamageTakenInvincibilityDuration, _damageDirection);
			}

			// 如果我们碰撞的是一个TopDownController，我们就施加一个回退力
			if ((_topDownController != null) && (_colliderTopDownController != null))
			{
				Vector3 totalVelocity = _colliderTopDownController.Speed + _velocity;
				Vector3 knockbackForce =
					Vector3.RotateTowards(DamageTakenKnockbackForce, totalVelocity.normalized, 10f, 0f);

				if (DamageTakenKnockbackType == KnockbackStyles.AddForce)
				{
					_topDownController.AddForce(knockbackForce);
				}
			}
		}
	}
}
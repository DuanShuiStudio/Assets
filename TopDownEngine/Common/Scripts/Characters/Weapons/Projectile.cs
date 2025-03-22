using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 与抛射物武器一起使用的抛射物类
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Projectile")]
	public class Projectile : MMPoolableObject  
	{
		public enum MovementVectors { Forward, Right, Up}
		
		[Header("Movement移动")]
		/// if true, the projectile will rotate at initialization towards its rotation
		[Tooltip("如果为真，抛射物将在初始化时旋转到其旋转方向")]
		public bool FaceDirection = true;
		/// if true, the projectile will rotate towards movement
		[Tooltip("如果为真，抛射物将朝向移动方向旋转")]
		public bool FaceMovement = false;
		/// if FaceMovement is true, the projectile's vector specified below will be aligned to the movement vector, usually you'll want to go with Forward in 3D, Right in 2D
		[Tooltip("如果FaceMovement为真，下面指定的抛射物向量将与移动向量对齐，通常在3D中你会选择Forward，在2D中选择Right")]
		[MMCondition("FaceMovement", true)]
		public MovementVectors MovementVector = MovementVectors.Forward;

		/// the speed of the object (relative to the level's speed)
		[Tooltip("对象的速度（相对于关卡的速度）")]
		public float Speed = 0;
		/// the acceleration of the object over time. Starts accelerating on enable.
		[Tooltip("对象随时间的加速度。在启用时开始加速")]
		public float Acceleration = 0;
		/// the current direction of the object
		[Tooltip("对象的当前方向")]
		public Vector3 Direction = Vector3.left;
		/// if set to true, the spawner can change the direction of the object. If not the one set in its inspector will be used.
		[Tooltip("如果设置为true，生成器可以改变对象的方向。如果不是，将使用其检查器中设置的那个方向")]
		public bool DirectionCanBeChangedBySpawner = true;
		/// the flip factor to apply if and when the projectile is mirrored
		[Tooltip("如果抛射物被镜像时应用的翻转因子")]
		public Vector3 FlipValue = new Vector3(-1,1,1);
		/// set this to true if your projectile's model (or sprite) is facing right, false otherwise
		[Tooltip("如果你的抛射物模型（或精灵）面向右侧，设置为true，否则设置为false")]
		public bool ProjectileIsFacingRight = true;

		[Header("Spawn生成")]
		[MMInformation("在这里，您可以定义一个初始延迟（以秒为单位），在此期间，此对象不会受到或造成损坏。此延迟从对象启用时开始。你还可以定义投射物是否应该伤害它们的主人（比如火箭等）", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the initial delay during which the projectile can't be destroyed
		[Tooltip("抛射物不能被摧毁的初始延迟")]
		public float InitialInvulnerabilityDuration=0f;
		/// should the projectile damage its owner?
		[Tooltip("抛射物是否应该伤害它的主人?")]
		public bool DamageOwner = false;

        /// 返回关联的触碰区域的伤害
        public virtual DamageOnTouch TargetDamageOnTouch { get { return _damageOnTouch; } }
		public virtual Weapon SourceWeapon { get { return _weapon; } }

		protected Weapon _weapon;
		protected GameObject _owner;
		protected Vector3 _movement;
		protected float _initialSpeed;
		protected SpriteRenderer _spriteRenderer;
		protected DamageOnTouch _damageOnTouch;
		protected WaitForSeconds _initialInvulnerabilityDurationWFS;
		protected Collider _collider;
		protected Collider2D _collider2D;
		protected Rigidbody _rigidBody;
		protected Rigidbody2D _rigidBody2D;
		protected bool _facingRightInitially;
		protected bool _initialFlipX;
		protected Vector3 _initialLocalScale;
		protected bool _shouldMove = true;
		protected Health _health;
		protected bool _spawnerIsFacingRight;

        /// <summary>
        /// 在唤醒时，我们存储对象的初始速度
        /// </summary>
        protected virtual void Awake ()
		{
			_facingRightInitially = ProjectileIsFacingRight;
			_initialSpeed = Speed;
			_health = GetComponent<Health> ();
			_collider = GetComponent<Collider> ();
			_collider2D = GetComponent<Collider2D>();
			_spriteRenderer = GetComponent<SpriteRenderer> ();
			_damageOnTouch = GetComponent<DamageOnTouch>();
			_rigidBody = GetComponent<Rigidbody> ();
			_rigidBody2D = GetComponent<Rigidbody2D> ();
			_initialInvulnerabilityDurationWFS = new WaitForSeconds (InitialInvulnerabilityDuration);
			if (_spriteRenderer != null) {	_initialFlipX = _spriteRenderer.flipX ;		}
			_initialLocalScale = transform.localScale;
		}

        /// <summary>
        /// 处理抛射物的初始无敌状态
        /// </summary>
        /// <returns>The invulnerability.</returns>
        protected virtual IEnumerator InitialInvulnerability()
		{
			if (_damageOnTouch == null) { yield break; }
			if (_weapon == null) { yield break; }

			_damageOnTouch.ClearIgnoreList();
			if (_weapon.Owner != null)
			{
				_damageOnTouch.IgnoreGameObject(_weapon.Owner.gameObject);	
			}
			yield return _initialInvulnerabilityDurationWFS;
			if (DamageOwner)
			{
				_damageOnTouch.StopIgnoringObject(_weapon.Owner.gameObject);
			}
		}

        /// <summary>
        /// 初始化抛射物
        /// </summary>
        protected virtual void Initialization()
		{
			Speed = _initialSpeed;
			ProjectileIsFacingRight = _facingRightInitially;
			if (_spriteRenderer != null) {	_spriteRenderer.flipX = _initialFlipX;	}
			transform.localScale = _initialLocalScale;	
			_shouldMove = true;
			_damageOnTouch?.InitializeFeedbacks();

			if (_collider != null)
			{
				_collider.enabled = true;
			}
			if (_collider2D != null)
			{
				_collider2D.enabled = true;
			}
		}

        /// <summary>
        /// 在update()中，我们根据关卡的速度和对象的速度移动对象，并应用加速度
        /// </summary>
        protected virtual void FixedUpdate ()
		{
			base.Update ();
			if (_shouldMove)
			{
				Movement();
			}
		}

        /// <summary>
        /// 处理抛射物的运动，每一帧
        /// </summary>
        public virtual void Movement()
		{
			_movement = Direction * (Speed / 10) * Time.deltaTime;
			//transform.Translate(_movement,Space.World);
			if (_rigidBody != null)
			{
				_rigidBody.MovePosition (this.transform.position + _movement);
			}
			if (_rigidBody2D != null)
			{
				_rigidBody2D.MovePosition(this.transform.position + _movement);
			}
            // 我们应用加速度来增加速度
            Speed += Acceleration * Time.deltaTime;
		}

        /// <summary>
        /// 设置抛射物的方向
        /// </summary>
        /// <param name="newDirection">New direction.</param>
        /// <param name="newRotation">New rotation.</param>
        /// <param name="spawnerIsFacingRight">If set to <c>true</c> spawner is facing right.</param>
        public virtual void SetDirection(Vector3 newDirection, Quaternion newRotation, bool spawnerIsFacingRight = true)
		{
			_spawnerIsFacingRight = spawnerIsFacingRight;

			if (DirectionCanBeChangedBySpawner)
			{
				Direction = newDirection;
			}
			if (ProjectileIsFacingRight != spawnerIsFacingRight)
			{
				Flip ();
			}
			if (FaceDirection)
			{
				transform.rotation = newRotation;
			}

			if (_damageOnTouch != null)
			{
				_damageOnTouch.SetKnockbackScriptDirection(newDirection);
			}

			if (FaceMovement)
			{
				switch (MovementVector)
				{
					case MovementVectors.Forward:
						transform.forward = newDirection;
						break;
					case MovementVectors.Right:
						transform.right = newDirection;
						break;
					case MovementVectors.Up:
						transform.up = newDirection;
						break;
				}
			}
		}

        /// <summary>
        /// 翻转抛射物
        /// </summary>
        protected virtual void Flip()
		{
			if (_spriteRenderer != null)
			{
				_spriteRenderer.flipX = !_spriteRenderer.flipX;
			}	
			else
			{
				this.transform.localScale = Vector3.Scale(this.transform.localScale,FlipValue) ;
			}
		}

        /// <summary>
        /// 翻转抛射物
        /// </summary>
        protected virtual void Flip(bool state)
		{
			if (_spriteRenderer != null)
			{
				_spriteRenderer.flipX = state;
			}
			else
			{
				this.transform.localScale = Vector3.Scale(this.transform.localScale, FlipValue);
			}
		}

        /// <summary>
        /// 设置抛射物的父类武器
        /// </summary>
        /// <param name="newWeapon">New weapon.</param>
        public virtual void SetWeapon(Weapon newWeapon)
		{
			_weapon = newWeapon;
		}

        /// <summary>
        /// 将抛射物的DamageOnTouch造成的伤害设置为指定值
        /// </summary>
        /// <param name="newDamage"></param>
        public virtual void SetDamage(float minDamage, float maxDamage)
		{
			if (_damageOnTouch != null)
			{
				_damageOnTouch.MinDamageCaused = minDamage;
				_damageOnTouch.MaxDamageCaused = maxDamage;
			}
		}

        /// <summary>
        /// "设置抛射物的主人
        /// </summary>
        /// <param name="newOwner">New owner.</param>
        public virtual void SetOwner(GameObject newOwner)
		{
			_owner = newOwner;
			DamageOnTouch damageOnTouch = this.gameObject.MMGetComponentNoAlloc<DamageOnTouch>();
			if (damageOnTouch != null)
			{
				damageOnTouch.Owner = newOwner;
				if (!DamageOwner)
				{
					damageOnTouch.ClearIgnoreList();
					damageOnTouch.IgnoreGameObject(newOwner);
				}
			}
		}

        /// <summary>
        /// 返回抛射物的当前主人
        /// </summary>
        /// <returns></returns>
        public virtual GameObject GetOwner()
		{
			return _owner;
		}

        /// <summary>
        /// 在死亡时，禁用碰撞器并阻止移动
        /// </summary>
        public virtual void StopAt()
		{
			if (_collider != null)
			{
				_collider.enabled = false;
			}
			if (_collider2D != null)
			{
				_collider2D.enabled = false;
			}
			
			_shouldMove = false;
		}

        /// <summary>
        /// 在死亡时，我们停止我们的抛射物
        /// </summary>
        protected virtual void OnDeath()
		{
			StopAt ();
		}

        /// <summary>
        /// 在启用时，我们触发短暂的无敌状态
        /// </summary>
        protected override void OnEnable ()
		{
			base.OnEnable ();

			Initialization();
			if (InitialInvulnerabilityDuration>0)
			{
				StartCoroutine(InitialInvulnerability());
			}

			if (_health != null)
			{
				_health.OnDeath += OnDeath;
			}
		}

        /// <summary>
        /// 在禁用时，我们将我们的OnDeath方法连接到生命值组件
        /// </summary>
        protected override void OnDisable()
		{
			base.OnDisable ();
			if (_health != null)
			{
				_health.OnDeath -= OnDeath;
			}			
		}
	}	
}
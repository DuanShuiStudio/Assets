using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using UnityEngine.Serialization;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个基本的近战武器类，当使用武器时会激活‘伤害区域’
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Melee Weapon")]
	public class MeleeWeapon : Weapon
	{
        /// 近战武器伤害区域的可能形状
        public enum MeleeDamageAreaShapes { Rectangle, Circle, Box, Sphere }
		public enum MeleeDamageAreaModes { Generated, Existing }

		[MMInspectorGroup("Damage Area", true, 22)]
		/// the possible modes to handle the damage area. In Generated, the MeleeWeapon will create it, in Existing, you can bind an existing damage area - usually nested under the weapon
		[Tooltip("处理伤害区域的可能模式。在生成模式下，近战武器将创建它；在现有模式下，你可以绑定一个现有的伤害区域 - 通常嵌套在武器下")]
		public MeleeDamageAreaModes MeleeDamageAreaMode = MeleeDamageAreaModes.Generated;
		/// the shape of the damage area (rectangle or circle)
		[Tooltip("伤害区域的形状（矩形或圆形）")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public MeleeDamageAreaShapes DamageAreaShape = MeleeDamageAreaShapes.Rectangle;
		/// the offset to apply to the damage area (from the weapon's attachment position
		[Tooltip("对伤害区域应用的偏移（从武器的连接位置开始）")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public Vector3 AreaOffset = new Vector3(1, 0);
		/// the size of the damage area
		[Tooltip("伤害区域的大小")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public Vector3 AreaSize = new Vector3(1, 1);
		/// the trigger filters this melee weapon should apply damage on (by default, it'll apply damage on everything, but you can change this to only apply when targets enter the area, for example)
		[Tooltip("这个近战武器应该应用伤害的触发过滤器（例如，默认情况下，它会对所有事物应用伤害，但你可以更改为只在目标进入区域时应用")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public DamageOnTouch.TriggerAndCollisionMask TriggerFilter = DamageOnTouch.AllowedTriggerCallbacks;
		/// the feedback to play when hitting a Damageable
		[Tooltip("在击中可伤害物时播放的反馈")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public MMFeedbacks HitDamageableFeedback;
		/// the feedback to play when hitting a non Damageable
		[Tooltip("在击中不可伤害物时播放的反馈")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Generated)]
		public MMFeedbacks HitNonDamageableFeedback;
		/// an existing damage area to activate/handle as the weapon is used
		[Tooltip("作为武器使用时激活/处理的现有伤害区域")]
		[MMEnumCondition("MeleeDamageAreaMode", (int)MeleeDamageAreaModes.Existing)]
		public DamageOnTouch ExistingDamageArea;

		[MMInspectorGroup("Damage Area Timing", true, 23)]
        
		/// the initial delay to apply before triggering the damage area
		[Tooltip("在触发伤害区域之前应用的初始延迟")]
		public float InitialDelay = 0f;
		/// the duration during which the damage area is active
		[Tooltip("伤害区域活跃的持续时间")]
		public float ActiveDuration = 1f;

		[MMInspectorGroup("Damage Caused", true, 24)]

		/// the layers that will be damaged by this object
		[Tooltip("这个对象将造成伤害的层")]
		public LayerMask TargetLayerMask;
		/// The min amount of health to remove from the player's health
		[FormerlySerializedAs("DamageCaused")] 
		[Tooltip("从玩家的生命值中移除的最小生命值")]
		public float MinDamageCaused = 10f;
		/// The max amount of health to remove from the player's health
		[FormerlySerializedAs("DamageCaused")] 
		[Tooltip("从玩家的生命值中移除的最大生命值")]
		public float MaxDamageCaused = 10f;
		/// the kind of knockback to apply
		[Tooltip("要应用的击退类型")]
		public DamageOnTouch.KnockbackStyles Knockback;
		/// The force to apply to the object that gets damaged
		[Tooltip("对受损对象应用的力")]
		public Vector3 KnockbackForce = new Vector3(10, 2, 0);
		/// The direction in which to apply the knockback 
		[Tooltip("应用击退的方向")]
		public DamageOnTouch.KnockbackDirections KnockbackDirection = DamageOnTouch.KnockbackDirections.BasedOnOwnerPosition;
		/// The duration of the invincibility frames after the hit (in seconds)
		[Tooltip("击中后的无敌帧持续时间（以秒为单位）")]
		public float InvincibilityDuration = 0.5f;
		/// if this is true, the owner can be damaged by its own weapon's damage area (usually false)
		[Tooltip("如果这为真，所有者可以被其自己的武器的伤害区域所伤害（通常为假）")]
		public bool CanDamageOwner = false;

		protected Collider _damageAreaCollider;
		protected Collider2D _damageAreaCollider2D;
		protected bool _attackInProgress = false;
		protected Color _gizmosColor;
		protected Vector3 _gizmoSize;
		protected CircleCollider2D _circleCollider2D;
		protected BoxCollider2D _boxCollider2D;
		protected BoxCollider _boxCollider;
		protected SphereCollider _sphereCollider;
		protected Vector3 _gizmoOffset;
		protected DamageOnTouch _damageOnTouch;
		protected GameObject _damageArea;
		protected Coroutine _attackCoroutine;

		/// <summary>
		/// 初始化
		/// </summary>
		public override void Initialization()
		{
			base.Initialization();

			if (_damageArea == null)
			{
				CreateDamageArea();
				DisableDamageArea();
			}
			if (Owner != null)
			{
				_damageOnTouch.Owner = Owner.gameObject;
			}            
		}

		/// <summary>
		/// 创建一个伤害区域
		/// </summary>
		protected virtual void CreateDamageArea()
		{
			if ((MeleeDamageAreaMode == MeleeDamageAreaModes.Existing) && (ExistingDamageArea != null))
			{
				_damageArea = ExistingDamageArea.gameObject;
				_damageAreaCollider = _damageArea.gameObject.GetComponent<Collider>();
				_damageAreaCollider2D = _damageArea.gameObject.GetComponent<Collider2D>();
				_damageOnTouch = ExistingDamageArea;
				return;
			}
			
			_damageArea = new GameObject();
			_damageArea.name = this.name + "DamageArea";
			_damageArea.transform.position = this.transform.position;
			_damageArea.transform.rotation = this.transform.rotation;
			_damageArea.transform.SetParent(this.transform);
			_damageArea.transform.localScale = Vector3.one;
			_damageArea.layer = this.gameObject.layer;
            
			if (DamageAreaShape == MeleeDamageAreaShapes.Rectangle)
			{
				_boxCollider2D = _damageArea.AddComponent<BoxCollider2D>();
				_boxCollider2D.offset = AreaOffset;
				_boxCollider2D.size = AreaSize;
				_damageAreaCollider2D = _boxCollider2D;
				_damageAreaCollider2D.isTrigger = true;
			}
			if (DamageAreaShape == MeleeDamageAreaShapes.Circle)
			{
				_circleCollider2D = _damageArea.AddComponent<CircleCollider2D>();
				_circleCollider2D.transform.position = this.transform.position;
				_circleCollider2D.offset = AreaOffset;
				_circleCollider2D.radius = AreaSize.x / 2;
				_damageAreaCollider2D = _circleCollider2D;
				_damageAreaCollider2D.isTrigger = true;
			}

			if ((DamageAreaShape == MeleeDamageAreaShapes.Rectangle) || (DamageAreaShape == MeleeDamageAreaShapes.Circle))
			{
				Rigidbody2D rigidBody = _damageArea.AddComponent<Rigidbody2D>();
				rigidBody.isKinematic = true;
				rigidBody.sleepMode = RigidbodySleepMode2D.NeverSleep;
			}            

			if (DamageAreaShape == MeleeDamageAreaShapes.Box)
			{
				_boxCollider = _damageArea.AddComponent<BoxCollider>();
				_boxCollider.center = AreaOffset;
				_boxCollider.size = AreaSize;
				_damageAreaCollider = _boxCollider;
				_damageAreaCollider.isTrigger = true;
			}
			if (DamageAreaShape == MeleeDamageAreaShapes.Sphere)
			{
				_sphereCollider = _damageArea.AddComponent<SphereCollider>();
				_sphereCollider.transform.position = this.transform.position + this.transform.rotation * AreaOffset;
				_sphereCollider.radius = AreaSize.x / 2;
				_damageAreaCollider = _sphereCollider;
				_damageAreaCollider.isTrigger = true;
			}

			if ((DamageAreaShape == MeleeDamageAreaShapes.Box) || (DamageAreaShape == MeleeDamageAreaShapes.Sphere))
			{
				Rigidbody rigidBody = _damageArea.AddComponent<Rigidbody>();
				rigidBody.isKinematic = true;

				rigidBody.gameObject.AddComponent<MMRagdollerIgnore>();
			}

			_damageOnTouch = _damageArea.AddComponent<DamageOnTouch>();
			_damageOnTouch.SetGizmoSize(AreaSize);
			_damageOnTouch.SetGizmoOffset(AreaOffset);
			_damageOnTouch.TargetLayerMask = TargetLayerMask;
			_damageOnTouch.MinDamageCaused = MinDamageCaused;
			_damageOnTouch.MaxDamageCaused = MaxDamageCaused;
			_damageOnTouch.DamageDirectionMode = DamageOnTouch.DamageDirections.BasedOnOwnerPosition;
			_damageOnTouch.DamageCausedKnockbackType = Knockback;
			_damageOnTouch.DamageCausedKnockbackForce = KnockbackForce;
			_damageOnTouch.DamageCausedKnockbackDirection = KnockbackDirection;
			_damageOnTouch.InvincibilityDuration = InvincibilityDuration;
			_damageOnTouch.HitDamageableFeedback = HitDamageableFeedback;
			_damageOnTouch.HitNonDamageableFeedback = HitNonDamageableFeedback;
			_damageOnTouch.TriggerFilter = TriggerFilter;
            
			if (!CanDamageOwner && (Owner != null))
			{
				_damageOnTouch.IgnoreGameObject(Owner.gameObject);    
			}
		}

        /// <summary>
        /// 当使用武器时，我们触发攻击程序
        /// </summary>
        public override void WeaponUse()
		{
			base.WeaponUse();
			_attackCoroutine = StartCoroutine(MeleeWeaponAttack());
		}

        /// <summary>
        /// 触发一次攻击，开启伤害区域然后再关闭
        /// </summary>
        /// <returns>The weapon attack.</returns>
        protected virtual IEnumerator MeleeWeaponAttack()
		{
			if (_attackInProgress) { yield break; }

			_attackInProgress = true;
			yield return new WaitForSeconds(InitialDelay);
			EnableDamageArea();
			yield return new WaitForSeconds(ActiveDuration);
			DisableDamageArea();
			_attackInProgress = false;
		}

        /// <summary>
        /// 在中断时，如有必要，我们停止伤害区域的序列
        /// </summary>
        public override void Interrupt()
		{
			base.Interrupt();
			if (_attackCoroutine != null)
			{
				StopCoroutine(_attackCoroutine);
			}	
		}

        /// <summary>
        /// 启用伤害区域
        /// </summary>
        protected virtual void EnableDamageArea()
		{
			if (_damageAreaCollider2D != null)
			{
				_damageAreaCollider2D.enabled = true;
			}
			if (_damageAreaCollider != null)
			{
				_damageAreaCollider.enabled = true;
			}
		}


        /// <summary>
        /// 禁用伤害区域
        /// </summary>
        protected virtual void DisableDamageArea()
		{
			if (_damageAreaCollider2D != null)
			{
				_damageAreaCollider2D.enabled = false;
			}
			if (_damageAreaCollider != null)
			{
				_damageAreaCollider.enabled = false;
			}
		}

        /// <summary>
        /// 当选中时，我们绘制一堆小工具
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
		{
			if (!Application.isPlaying)
			{
				DrawGizmos();
			}            
		}

        /// <summary>
        /// 绘制伤害区域的小工具
        /// </summary>
        protected virtual void DrawGizmos()
		{
			if (MeleeDamageAreaMode == MeleeDamageAreaModes.Existing)
			{
				return;
			}
			
			if (DamageAreaShape == MeleeDamageAreaShapes.Box)
			{
				Gizmos.DrawWireCube(this.transform.position + AreaOffset, AreaSize);
			}

			if (DamageAreaShape == MeleeDamageAreaShapes.Circle)
			{
				Gizmos.DrawWireSphere(this.transform.position + AreaOffset, AreaSize.x / 2);
			}

			if (DamageAreaShape == MeleeDamageAreaShapes.Rectangle)
			{
				MMDebug.DrawGizmoRectangle(this.transform.position + AreaOffset, AreaSize, Color.red);
			}

			if (DamageAreaShape == MeleeDamageAreaShapes.Sphere)
			{
				Gizmos.DrawWireSphere(this.transform.position + AreaOffset, AreaSize.x / 2);
			}
		}

        /// <summary>
        /// 在禁用时，我们将标志设置为false
        /// </summary>
        protected virtual void OnDisable()
		{
			_attackInProgress = false;
		}
	}
}
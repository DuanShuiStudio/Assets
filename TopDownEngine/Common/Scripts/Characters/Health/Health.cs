using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 每次运行生命值更改时触发的事件，供其他类收听
    /// </summary>
    public struct HealthChangeEvent
	{
		public Health AffectedHealth;
		public float NewHealth;
		
		public HealthChangeEvent(Health affectedHealth, float newHealth)
		{
			AffectedHealth = affectedHealth;
			NewHealth = newHealth;
		}

		static HealthChangeEvent e;
		public static void Trigger(Health affectedHealth, float newHealth)
		{
			e.AffectedHealth = affectedHealth;
			e.NewHealth = newHealth;
			MMEventManager.TriggerEvent(e);
		}
	}

    /// <summary>
    /// 这个类管理一个对象的生命值，引导它的潜在生命条，处理当它受到伤害时发生的事情，
    /// 它死后会发生什么。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Core/Health")] 
	public class Health : TopDownMonoBehaviour
	{
		[MMInspectorGroup("Bindings", true, 3)]

		/// the model to disable (if set so)
		[Tooltip("要禁用的模型（如果这样设置）")]
		public GameObject Model;
		
		[MMInspectorGroup("Status", true, 29)]

		/// the current health of the character
		[MMReadOnly]
		[Tooltip("角色的当前生命值")]
		public float CurrentHealth ;
		/// If this is true, this object can't take damage at this time
		[MMReadOnly]
		[Tooltip("如果这是真的，这个物体此时不会受到伤害")]
		public bool Invulnerable = false;	

		[MMInspectorGroup("Health", true, 5)]

		[MMInformation("将这个组件添加到一个对象中，它就会有生命值，可能会受到伤害甚至死亡。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the initial amount of health of the object
		[Tooltip("对象的初始运行状况")]
		public float InitialHealth = 10;
		/// the maximum amount of health of the object
		[Tooltip("对象的生命值上限")]
		public float MaximumHealth = 10;
		/// if this is true, health values will be reset everytime this character is enabled (usually at the start of a scene)
		[Tooltip("如果这是真的，每次这个角色被启用时（通常在场景开始时），生命值将被重置。")]
		public bool ResetHealthOnEnable = true;

		[MMInspectorGroup("Damage", true, 6)]

		[MMInformation("在这里，你可以指定一个效果和声音特效来实例化当物体被损坏时，以及当物体被击中时应该闪烁多长时间（只适用于精灵）。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// whether or not this Health object can be damaged 
		[Tooltip("这个生命值对象是否可以受到伤害")]
		public bool ImmuneToDamage = false;
		/// the feedback to play when getting damage
		[Tooltip("当受到伤害时的反馈")]
		public MMFeedbacks DamageMMFeedbacks;
		/// if this is true, the damage value will be passed to the MMFeedbacks as its Intensity parameter, letting you trigger more intense feedbacks as damage increases
		[Tooltip("如果这是真的，伤害值将作为强度参数传递给MMFeedbacks，让你可以随着伤害的增加触发更强烈的反馈")]
		public bool FeedbackIsProportionalToDamage = false;
		/// if you set this to true, other objects damaging this one won't take any self damage
		[Tooltip("如果你把这个设置为真，其他对象对这个造成伤害时不会受到任何自伤")]
		public bool PreventTakeSelfDamage = false;
		
		[MMInspectorGroup("Knockback", true, 63)]
		
		/// whether or not this object is immune to damage knockback
		[Tooltip("这个对象是否免疫伤害击退")]
		public bool ImmuneToKnockback = false;
		/// whether or not this object is immune to damage knockback if the damage received is zero
		[Tooltip("如果受到的伤害为零，这个对象是否免疫伤害击退")]
		public bool ImmuneToKnockbackIfZeroDamage = false;
		/// a multiplier applied to the incoming knockback forces. 0 will cancel all knockback, 0.5 will cut it in half, 1 will have no effect, 2 will double the knockback force, etc
		[Tooltip("施加在即将到来的反击力上的乘数。0将取消所有反伤力，0.5将其减半，1将无效，2将回弹力加倍，以此类推")]
		public float KnockbackForceMultiplier = 1f;

		[MMInspectorGroup("Death", true, 53)]

		[MMInformation("在这里，您可以设置对象死亡时实例化的效果、应用于它的力（需要自上而下的控制器）、要为游戏分数添加多少点以及角色应该在哪里重生（仅适用于非玩家角色）。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// whether or not this object should get destroyed on death
		[Tooltip("这个物体死后是否应该被销毁")]
		public bool DestroyOnDeath = true;
		/// the time (in seconds) before the character is destroyed or disabled
		[Tooltip("角色被销毁或禁用之前的时间（秒）")]
		public float DelayBeforeDestruction = 0f;
		/// the points the player gets when the object's health reaches zero
		[Tooltip("当物体的生命值为零时，玩家获得的点数")]
		public int PointsWhenDestroyed;
		/// if this is set to false, the character will respawn at the location of its death, otherwise it'll be moved to its initial position (when the scene started)
		[Tooltip("如果设置为false，角色将在死亡位置重生，否则将移动到初始位置（场景开始时）")]
		public bool RespawnAtInitialLocation = false;
		/// if this is true, the controller will be disabled on death
		[Tooltip("如果这是真的，控制器将在死亡时禁用")]
		public bool DisableControllerOnDeath = true;
		/// if this is true, the model will be disabled instantly on death (if a model has been set)
		[Tooltip("如果这是真的，模型将在死亡时立即禁用（如果已设置模型）")]
		public bool DisableModelOnDeath = true;
		/// if this is true, collisions will be turned off when the character dies
		[Tooltip("如果这是真的，当角色死亡时，碰撞将被关闭")]
		public bool DisableCollisionsOnDeath = true;
		/// if this is true, collisions will also be turned off on child colliders when the character dies
		[Tooltip("如果这是真的，当角色死亡时，子碰撞器上的碰撞也将关闭")]
		public bool DisableChildCollisionsOnDeath = false;
		/// whether or not this object should change layer on death
		[Tooltip("此对象是否应在死亡时更改图层")]
		public bool ChangeLayerOnDeath = false;
		/// whether or not this object should change layer on death
		[Tooltip("此对象是否应在死亡时递归更改图层")]
		public bool ChangeLayersRecursivelyOnDeath = false;
		/// the layer we should move this character to on death
		[Tooltip("我们应该把这个角色移到死亡的那一层")]
		public MMLayer LayerOnDeath;
		/// the feedback to play when dying
		[Tooltip("死亡时的反馈")]
		public MMFeedbacks DeathMMFeedbacks;

		/// if this is true, color will be reset on revive
		[Tooltip("如果这是真的，颜色将在恢复时重置")]
		public bool ResetColorOnRevive = true;
		/// the name of the property on your renderer's shader that defines its color 
		[Tooltip("渲染器着色器上定义其颜色的属性的名称")]
		[MMCondition("ResetColorOnRevive", true)]
		public string ColorMaterialPropertyName = "_Color";
		/// if this is true, this component will use material property blocks instead of working on an instance of the material.
		[Tooltip("如果这是真的，则此组件将使用材质属性块，而不是处理材质的实例。")] 
		public bool UseMaterialPropertyBlocks = false;
        
		[MMInspectorGroup("Shared Health and Damage Resistance", true, 12)]
		/// another Health component (usually on another character) towards which all health will be redirected
		[Tooltip("另一个生命值组件（通常在另一个角色上），所有生命值都将重定向到该组件")]
		public Health MasterHealth;
		/// a DamageResistanceProcessor this Health will use to process damage when it's received
		[Tooltip("一个损伤抵抗处理器，此生命值将在收到损伤时用于处理损伤")]
		public DamageResistanceProcessor TargetDamageResistanceProcessor;

		[MMInspectorGroup("Animator", true, 14)]
		/// the target animator to pass a Death animation parameter to. The Health component will try to auto bind this if left empty
		[Tooltip("将死亡动画参数传递给的目标动画器。如果留空，生命值组件将尝试自动绑定此参数")]
		public Animator TargetAnimator;
		/// if this is true, animator logs for the associated animator will be turned off to avoid potential spam
		[Tooltip("如果这是真的，则相关动画器的动画日志将被关闭，以避免潜在的垃圾邮件")]
		public bool DisableAnimatorLogs = true;
        
		public virtual float LastDamage { get; set; }
		public virtual Vector3 LastDamageDirection { get; set; }
		public virtual bool Initialized => _initialized;

        // 击打标志
        public delegate void OnHitDelegate();
		public OnHitDelegate OnHit;

        // 重生标志
        public delegate void OnReviveDelegate();
		public OnReviveDelegate OnRevive;

        // 死亡标志
        public delegate void OnDeathDelegate();
		public OnDeathDelegate OnDeath;

		protected Vector3 _initialPosition;
		protected Renderer _renderer;
		protected Character _character;
		protected CharacterMovement _characterMovement;
		protected TopDownController _controller;
		
		protected MMHealthBar _healthBar;
		protected Collider2D _collider2D;
		protected Collider _collider3D;
		protected CharacterController _characterController;
		protected bool _initialized = false;
		protected Color _initialColor;
		protected AutoRespawn _autoRespawn;
		protected int _initialLayer;
		protected MaterialPropertyBlock _propertyBlock;
		protected bool _hasColorProperty = false;

		protected const string _deathAnimatorParameterName = "Death";
		protected const string _healthAnimatorParameterName = "Health";
		protected const string _healthAsIntAnimatorParameterName = "HealthAsInt";
		protected int _deathAnimatorParameter;
		protected int _healthAnimatorParameter;
		protected int _healthAsIntAnimatorParameter;

		protected class InterruptiblesDamageOverTimeCoroutine
		{
			public Coroutine DamageOverTimeCoroutine;
			public DamageType DamageOverTimeType;
		}
		
		protected List<InterruptiblesDamageOverTimeCoroutine> _interruptiblesDamageOverTimeCoroutines;
		protected List<InterruptiblesDamageOverTimeCoroutine> _damageOverTimeCoroutines;

		#region Initialization
		
		/// <summary>
		/// On Awake, we initialize our health
		/// </summary>
		protected virtual void Awake()
		{
			Initialization();
			InitializeCurrentHealth();
		}

		/// <summary>
		/// On Start we grab our animator
		/// </summary>
		protected virtual void Start()
		{
			GrabAnimator();
		}
		
		/// <summary>
		/// Grabs useful components, enables damage and gets the inital color
		/// </summary>
		public virtual void Initialization()
		{
			_character = this.gameObject.GetComponentInParent<Character>(); 

			if (Model != null)
			{
				Model.SetActive(true);
			}        
            
			if (gameObject.GetComponentInParent<Renderer>() != null)
			{
				_renderer = GetComponentInParent<Renderer>();				
			}
			if (_character != null)
			{
				_characterMovement = _character.FindAbility<CharacterMovement>();
				if (_character.CharacterModel != null)
				{
					if (_character.CharacterModel.GetComponentInChildren<Renderer> ()!= null)
					{
						_renderer = _character.CharacterModel.GetComponentInChildren<Renderer> ();	
					}
				}	
			}
			if (_renderer != null)
			{
				if (UseMaterialPropertyBlocks && (_propertyBlock == null))
				{
					_propertyBlock = new MaterialPropertyBlock();
				}
	            
				if (ResetColorOnRevive)
				{
					if (UseMaterialPropertyBlocks)
					{
						if (_renderer.sharedMaterial.HasProperty(ColorMaterialPropertyName))
						{
							_hasColorProperty = true; 
							_initialColor = _renderer.sharedMaterial.GetColor(ColorMaterialPropertyName);
						}
					}
					else
					{
						if (_renderer.material.HasProperty(ColorMaterialPropertyName))
						{
							_hasColorProperty = true;
							_initialColor = _renderer.material.GetColor(ColorMaterialPropertyName);
						} 
					}
				}
			}

			_interruptiblesDamageOverTimeCoroutines = new List<InterruptiblesDamageOverTimeCoroutine>();
			_damageOverTimeCoroutines = new List<InterruptiblesDamageOverTimeCoroutine>();
			_initialLayer = gameObject.layer;
			
			_deathAnimatorParameter = Animator.StringToHash(_deathAnimatorParameterName);
			_healthAnimatorParameter = Animator.StringToHash(_healthAnimatorParameterName);
			_healthAsIntAnimatorParameter = Animator.StringToHash(_healthAsIntAnimatorParameterName);

			_autoRespawn = this.gameObject.GetComponentInParent<AutoRespawn>();
			_healthBar = this.gameObject.GetComponentInParent<MMHealthBar>();
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
			_characterController = this.gameObject.GetComponentInParent<CharacterController>();
			_collider2D = this.gameObject.GetComponentInParent<Collider2D>();
			_collider3D = this.gameObject.GetComponentInParent<Collider>();

			DamageMMFeedbacks?.Initialization(this.gameObject);
			DeathMMFeedbacks?.Initialization(this.gameObject);

			StoreInitialPosition();
			_initialized = true;
			
			DamageEnabled();
		}
		
		/// <summary>
		/// Grabs the target animator
		/// </summary>
		protected virtual void GrabAnimator()
		{
			if (TargetAnimator == null)
			{
				BindAnimator();
			}

			if ((TargetAnimator != null) && DisableAnimatorLogs)
			{
				TargetAnimator.logWarnings = false;
			}
			UpdateHealthAnimationParameters();
		}

		/// <summary>
		/// Finds and binds an animator if possible
		/// </summary>
		protected virtual void BindAnimator()
		{
			if (_character != null)
			{
				if (_character.CharacterAnimator != null)
				{
					TargetAnimator = _character.CharacterAnimator;
				}
				else
				{
					TargetAnimator = GetComponent<Animator>();
				}
			}
			else
			{
				TargetAnimator = GetComponent<Animator>();
			}    
		}

		/// <summary>
		/// Stores the initial position for further use
		/// </summary>
		public virtual void StoreInitialPosition()
		{
			_initialPosition = this.transform.position;
		}
		
		/// <summary>
		/// Initializes health to either initial or current values
		/// </summary>
		public virtual void InitializeCurrentHealth()
		{
			if (MasterHealth == null)
			{
				SetHealth(InitialHealth);	
			}
			else
			{
				if (MasterHealth.Initialized)
				{
					SetHealth(MasterHealth.CurrentHealth);
				}
				else
				{
					SetHealth(MasterHealth.InitialHealth);
				}
			}
		}

		/// <summary>
		/// When the object is enabled (on respawn for example), we restore its initial health levels
		/// </summary>
		protected virtual void OnEnable()
		{
			if (ResetHealthOnEnable)
			{
				InitializeCurrentHealth();
			}
			if (Model != null)
			{
				Model.SetActive(true);
			}            
			DamageEnabled();
		}
		
		/// <summary>
		/// On Disable, we prevent any delayed destruction from running
		/// </summary>
		protected virtual void OnDisable()
		{
			CancelInvoke();
		}

        #endregion

        /// <summary>
        /// 如果此生命值组件可能在此帧中损坏，则返回true，否则返回false
        /// </summary>
        /// <returns></returns>
        public virtual bool CanTakeDamageThisFrame()
		{
            // 如果对象是无敌的，我们什么都不做，然后退出
            if (Invulnerable || ImmuneToDamage)
			{
				return false;
			}

			if (!this.enabled)
			{
				return false;
			}

            // 如果我们生命值已经低于零，我们什么都不做，退出
            if ((CurrentHealth <= 0) && (InitialHealth != 0))
			{
				return false;
			}

			return true;
		}

        /// <summary>
        /// 当物体受伤害时调用
        /// </summary>
        /// <param name="damage">The amount of health points that will get lost.</param>
        /// <param name="instigator">The object that caused the damage.</param>
        /// <param name="flickerDuration">The time (in seconds) the object should flicker after taking the damage - not used anymore, kept to not break retrocompatibility</param>
        /// <param name="invincibilityDuration">The duration of the short invincibility following the hit.</param>
        public virtual void Damage(float damage, GameObject instigator, float flickerDuration, float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null)
		{
			if (!CanTakeDamageThisFrame())
			{
				return;
			}

			damage = ComputeDamageOutput(damage, typedDamages, true);

            // 我们通过伤害来降低角色的生命值
            float previousHealth = CurrentHealth;
			if (MasterHealth != null)
			{
				previousHealth = MasterHealth.CurrentHealth;
				MasterHealth.SetHealth(MasterHealth.CurrentHealth - damage);
			}
			else
			{
				SetHealth(CurrentHealth - damage);	
			}

			LastDamage = damage;
			LastDamageDirection = damageDirection;
			if (OnHit != null)
			{
				OnHit();
			}

            // 我们防止角色与投射物、玩家和敌人碰撞
            if (invincibilityDuration > 0)
			{
				DamageDisabled();
				StartCoroutine(DamageEnabled(invincibilityDuration));	
			}

            // 我们触发了伤害事件
            MMDamageTakenEvent.Trigger(this, instigator, CurrentHealth, damage, previousHealth, typedDamages);

            // 我们更新我们的动画器
            if (TargetAnimator != null)
			{
				TargetAnimator.SetTrigger("Damage");
			}

			// 我们播放我们的反馈
			if (FeedbackIsProportionalToDamage)
			{
				DamageMMFeedbacks?.PlayFeedbacks(this.transform.position, damage);    
			}
			else
			{
				DamageMMFeedbacks?.PlayFeedbacks(this.transform.position);
			}

            // 我们更新生命条
            UpdateHealthBar(true);

            // 我们处理任何条件状态更改
            ComputeCharacterConditionStateChanges(typedDamages);
			ComputeCharacterMovementMultipliers(typedDamages);

            // 如果生命已达到零，我们将其生命值设置为零（对生命条有用）
            if (MasterHealth != null)
			{
				if (MasterHealth.CurrentHealth <= 0)
				{
					MasterHealth.CurrentHealth = 0;
					MasterHealth.Kill();
				}
			}
			else
			{
				if (CurrentHealth <= 0)
				{
					CurrentHealth = 0;
					Kill();
				}
					
			}
		}

        /// <summary>
        /// 随着时间的推移，中断所有伤害，无论类型如何
        /// </summary>
        public virtual void InterruptAllDamageOverTime()
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _interruptiblesDamageOverTimeCoroutines)
			{
				StopCoroutine(coroutine.DamageOverTimeCoroutine);
			}
			_interruptiblesDamageOverTimeCoroutines.Clear();
		}

        /// <summary>
        /// 中断所有持续伤害，包括那些通常在死亡时无法中断的伤害
        /// </summary>
        public virtual void StopAllDamageOverTime()
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _damageOverTimeCoroutines)
			{
				StopCoroutine(coroutine.DamageOverTimeCoroutine);
			}
			_damageOverTimeCoroutines.Clear();
		}

        /// <summary>
        /// 中断指定类型随时间推移的所有伤害
        /// </summary>
        /// <param name="damageType"></param>
        public virtual void InterruptAllDamageOverTimeOfType(DamageType damageType)
		{
			foreach (InterruptiblesDamageOverTimeCoroutine coroutine in _interruptiblesDamageOverTimeCoroutines)
			{
				if (coroutine.DamageOverTimeType == damageType)
				{
					StopCoroutine(coroutine.DamageOverTimeCoroutine);	
				}
			}
			TargetDamageResistanceProcessor?.InterruptDamageOverTime(damageType);
		}

        /// <summary>
        /// 在指定的重复次数（包括首次造成伤害）内，以指定的间隔持续造成伤害，这样在检查器中更容易进行快速计算。
        /// 您可以选择决定您的伤害是可中断的，在这种情况下，调用InterruptAllDamageOverTime（）将停止应用这些伤害，例如用于治疗毒药。
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="instigator"></param>
        /// <param name="flickerDuration"></param>
        /// <param name="invincibilityDuration"></param>
        /// <param name="damageDirection"></param>
        /// <param name="typedDamages"></param>
        /// <param name="amountOfRepeats"></param>
        /// <param name="durationBetweenRepeats"></param>
        /// <param name="interruptible"></param>
        public virtual void DamageOverTime(float damage, GameObject instigator, float flickerDuration,
			float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null,
			int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, DamageType damageType = null)
		{
			if (ComputeDamageOutput(damage, typedDamages, false) == 0)
			{
				return;
			}
			
			InterruptiblesDamageOverTimeCoroutine damageOverTime = new InterruptiblesDamageOverTimeCoroutine();
			damageOverTime.DamageOverTimeType = damageType;
			damageOverTime.DamageOverTimeCoroutine = StartCoroutine(DamageOverTimeCo(damage, instigator, flickerDuration,
				invincibilityDuration, damageDirection, typedDamages, amountOfRepeats, durationBetweenRepeats,
				interruptible));
			_damageOverTimeCoroutines.Add(damageOverTime);
			if (interruptible)
			{
				_interruptiblesDamageOverTimeCoroutines.Add(damageOverTime);
			}
		}

        /// <summary>
        /// 一个协程，用来在一段时间内应用伤害
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="instigator"></param>
        /// <param name="flickerDuration"></param>
        /// <param name="invincibilityDuration"></param>
        /// <param name="damageDirection"></param>
        /// <param name="typedDamages"></param>
        /// <param name="amountOfRepeats"></param>
        /// <param name="durationBetweenRepeats"></param>
        /// <param name="interruptible"></param>
        /// <param name="damageType"></param>
        /// <returns></returns>
        protected virtual IEnumerator DamageOverTimeCo(float damage, GameObject instigator, float flickerDuration,
			float invincibilityDuration, Vector3 damageDirection, List<TypedDamage> typedDamages = null,
			int amountOfRepeats = 0, float durationBetweenRepeats = 1f, bool interruptible = true, DamageType damageType = null)
		{
			for (int i = 0; i < amountOfRepeats; i++)
			{
				Damage(damage, instigator, flickerDuration, invincibilityDuration, damageDirection, typedDamages);
				yield return MMCoroutine.WaitFor(durationBetweenRepeats);
			}
		}

        /// <summary>
        /// 返回在处理潜在抗性后这个生命值应该承受的伤害
        /// </summary>
        /// <param name="damage"></param>
        /// <returns></returns>
        public virtual float ComputeDamageOutput(float damage, List<TypedDamage> typedDamages = null, bool damageApplied = false)
		{
			if (Invulnerable || ImmuneToDamage)
			{
				return 0;
			}
			
			float totalDamage = 0f;
            // 我们通过我们的潜在抗性来处理伤害
            if (TargetDamageResistanceProcessor != null)
			{
				if (TargetDamageResistanceProcessor.isActiveAndEnabled)
				{
					totalDamage = TargetDamageResistanceProcessor.ProcessDamage(damage, typedDamages, damageApplied);	
				}
			}
			else
			{
				totalDamage = damage;
				if (typedDamages != null)
				{
					foreach (TypedDamage typedDamage in typedDamages)
					{
						totalDamage += typedDamage.DamageCaused;
					}
				}
			}
			return totalDamage;
		}

        /// <summary>
        /// 通过抗性处理并在需要时应用状态变化
        /// </summary>
        /// <param name="typedDamages"></param>
        protected virtual void ComputeCharacterConditionStateChanges(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (_character == null))
			{
				return;
			}
			
			foreach (TypedDamage typedDamage in typedDamages)
			{
				if (typedDamage.ForceCharacterCondition)
				{
					if (TargetDamageResistanceProcessor != null)
					{
						if (TargetDamageResistanceProcessor.isActiveAndEnabled)
						{
							bool checkResistance =
								TargetDamageResistanceProcessor.CheckPreventCharacterConditionChange(typedDamage.AssociatedDamageType);
							if (checkResistance)
							{
								continue;		
							}
						}
					}
					_character.ChangeCharacterConditionTemporarily(typedDamage.ForcedCondition, typedDamage.ForcedConditionDuration, typedDamage.ResetControllerForces, typedDamage.DisableGravity);	
				}
			}
			
		}

        /// <summary>
        /// 通过抗性列表处理并在需要时应用移动倍率
        /// </summary>
        /// <param name="typedDamages"></param>
        protected virtual void ComputeCharacterMovementMultipliers(List<TypedDamage> typedDamages)
		{
			if ((typedDamages == null) || (_character == null))
			{
				return;
			}
			
			foreach (TypedDamage typedDamage in typedDamages)
			{
				if (typedDamage.ApplyMovementMultiplier)
				{
					if (TargetDamageResistanceProcessor != null)
					{
						if (TargetDamageResistanceProcessor.isActiveAndEnabled)
						{
							bool checkResistance =
								TargetDamageResistanceProcessor.CheckPreventMovementModifier(typedDamage.AssociatedDamageType);
							if (checkResistance)
							{
								continue;		
							}
						}
					}
					_characterMovement?.ApplyMovementMultiplier(typedDamage.MovementMultiplier,
						typedDamage.MovementMultiplierDuration);
				}
			}
		}

        /// <summary>
        /// 通过处理抗性来确定新的击退力
        /// </summary>
        /// <param name="knockbackForce"></param>
        /// <param name="typedDamages"></param>
        /// <returns></returns>
        public virtual Vector3 ComputeKnockbackForce(Vector3 knockbackForce, List<TypedDamage> typedDamages = null)
		{
			return (TargetDamageResistanceProcessor == null) ? knockbackForce : TargetDamageResistanceProcessor.ProcessKnockbackForce(knockbackForce, typedDamages);;

		}

        /// <summary>
        /// 如果这个生命值可以被击退，则返回真，否则返回假
        /// </summary>
        /// <param name="typedDamages"></param>
        /// <returns></returns>
        public virtual bool CanGetKnockback(List<TypedDamage> typedDamages) 
		{
			if (ImmuneToKnockback)
			{
				return false;
			}
			if (TargetDamageResistanceProcessor != null)
			{
				if (TargetDamageResistanceProcessor.isActiveAndEnabled)
				{
					bool checkResistance = TargetDamageResistanceProcessor.CheckPreventKnockback(typedDamages);
					if (checkResistance)
					{
						return false;
					}
				}
			}
			return true;
		}

        /// <summary>
        /// 杀死角色，实例化死亡效果，处理得分等。
        /// </summary>
        public virtual void Kill()
		{
			if (ImmuneToDamage)
			{
				return;
			}
	        
			if (_character != null)
			{
                // 我们将它的死亡状态设置为真
                _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Dead);
				_character.Reset();

				if (_character.CharacterType == Character.CharacterTypes.Player)
				{
					TopDownEngineEvent.Trigger(TopDownEngineEventTypes.PlayerDeath, _character);
				}
			}
			SetHealth(0);

            // 我们防止进一步的伤害。
            StopAllDamageOverTime();
			DamageDisabled();

			DeathMMFeedbacks?.PlayFeedbacks(this.transform.position);

            // 如果需要，增加得分
            if (PointsWhenDestroyed != 0)
			{
                // 我们发送一个新的得分事件，让游戏管理器捕捉到（其他可能监听它的类也会收到这个事件）
                TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsWhenDestroyed);
			}

			if (TargetAnimator != null)
			{
				TargetAnimator.SetTrigger(_deathAnimatorParameter);
			}
            // 从现在开始，我们让它忽略碰撞
            if (DisableCollisionsOnDeath)
			{
				if (_collider2D != null)
				{
					_collider2D.enabled = false;
				}
				if (_collider3D != null)
				{
					_collider3D.enabled = false;
				}

                // 如果我们有一个控制器，移除碰撞，恢复潜在重生所需的参数，并施加死亡力
                if (_controller != null)
				{				
					_controller.CollisionsOff();						
				}

				if (DisableChildCollisionsOnDeath)
				{
					foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
					{
						collider.enabled = false;
					}
					foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
					{
						collider.enabled = false;
					}
				}
			}

			if (ChangeLayerOnDeath)
			{
				gameObject.layer = LayerOnDeath.LayerIndex;
				if (ChangeLayersRecursivelyOnDeath)
				{
					this.transform.ChangeLayersRecursively(LayerOnDeath.LayerIndex);
				}
			}
            
			OnDeath?.Invoke();
			MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Death);

			if (DisableControllerOnDeath && (_controller != null))
			{
				_controller.enabled = false;
			}

			if (DisableControllerOnDeath && (_characterController != null))
			{
				_characterController.enabled = false;
			}

			if (DisableModelOnDeath && (Model != null))
			{
				Model.SetActive(false);
			}

			if (DelayBeforeDestruction > 0f)
			{
				Invoke ("DestroyObject", DelayBeforeDestruction);
			}
			else
			{
				// finally we destroy the object
				DestroyObject();	
			}
		}

        /// <summary>
        /// 复活这个对象
        /// </summary>
        public virtual void Revive()
		{
			if (!_initialized)
			{
				return;
			}

			if (_collider2D != null)
			{
				_collider2D.enabled = true;
			}
			if (_collider3D != null)
			{
				_collider3D.enabled = true;
			}
			if (DisableChildCollisionsOnDeath)
			{
				foreach (Collider2D collider in this.gameObject.GetComponentsInChildren<Collider2D>())
				{
					collider.enabled = true;
				}
				foreach (Collider collider in this.gameObject.GetComponentsInChildren<Collider>())
				{
					collider.enabled = true;
				}
			}
			if (ChangeLayerOnDeath)
			{
				gameObject.layer = _initialLayer;
				if (ChangeLayersRecursivelyOnDeath)
				{
					this.transform.ChangeLayersRecursively(_initialLayer);
				}
			}
			if (_characterController != null)
			{
				_characterController.enabled = true;
			}
			if (_controller != null)
			{
				_controller.enabled = true;
				_controller.CollisionsOn();
				_controller.Reset();
			}
			if (_character != null)
			{
				_character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
			}
			if (ResetColorOnRevive && (_renderer != null))
			{
				if (UseMaterialPropertyBlocks)
				{
					_renderer.GetPropertyBlock(_propertyBlock);
					_propertyBlock.SetColor(ColorMaterialPropertyName, _initialColor);
					_renderer.SetPropertyBlock(_propertyBlock);    
				}
				else
				{
					_renderer.material.SetColor(ColorMaterialPropertyName, _initialColor);
				}
			}            

			if (RespawnAtInitialLocation)
			{
				transform.position = _initialPosition;
			}
			if (_healthBar != null)
			{
				_healthBar.Initialization();
			}

			Initialization();
			InitializeCurrentHealth();
			OnRevive?.Invoke();
			MMLifeCycleEvent.Trigger(this, MMLifeCycleEventTypes.Revive);
		}

        /// <summary>
        /// 根据角色的设置销毁对象，或者尝试销毁
        /// </summary>
        protected virtual void DestroyObject()
		{
			if (_autoRespawn == null)
			{
				if (DestroyOnDeath)
				{
					if (_character != null)
					{
						_character.gameObject.SetActive(false);
					}
					else
					{
						gameObject.SetActive(false);	
					}
				}                
			}
			else
			{
				_autoRespawn.Kill();
			}
		}

		#region HealthManipulationAPIs
		

		/// <summary>
		/// Sets the current health to the specified new value, and updates the health bar
		/// </summary>
		/// <param name="newValue"></param>
		public virtual void SetHealth(float newValue)
		{
			CurrentHealth = newValue;
			UpdateHealthBar(false);
			HealthChangeEvent.Trigger(this, newValue);
		}
		
		/// <summary>
		/// Called when the character gets health (from a stimpack for example)
		/// </summary>
		/// <param name="health">The health the character gets.</param>
		/// <param name="instigator">The thing that gives the character health.</param>
		public virtual void ReceiveHealth(float health,GameObject instigator)
		{
			// this function adds health to the character's Health and prevents it to go above MaxHealth.
			if (MasterHealth != null)
			{
				MasterHealth.SetHealth(Mathf.Min (CurrentHealth + health,MaximumHealth));	
			}
			else
			{
				SetHealth(Mathf.Min (CurrentHealth + health,MaximumHealth));	
			}
			UpdateHealthBar(true);
		}
		
		/// <summary>
		/// Resets the character's health to its max value
		/// </summary>
		public virtual void ResetHealthToMaxHealth()
		{
			SetHealth(MaximumHealth);
		}
		
		/// <summary>
		/// Forces a refresh of the character's health bar
		/// </summary>
		public virtual void UpdateHealthBar(bool show)
		{
			UpdateHealthAnimationParameters();
			
			if (_healthBar != null)
			{
				_healthBar.UpdateBar(CurrentHealth, 0f, MaximumHealth, show);
			}

			if (MasterHealth == null)
			{
				if (_character != null)
				{
					if (_character.CharacterType == Character.CharacterTypes.Player)
					{
						// We update the health bar
						if (GUIManager.HasInstance)
						{
							GUIManager.Instance.UpdateHealthBar(CurrentHealth, 0f, MaximumHealth, _character.PlayerID);
						}
					}
				}    
			}
		}

		protected virtual void UpdateHealthAnimationParameters()
		{
			if (TargetAnimator != null)
			{
				TargetAnimator.SetFloat(_healthAnimatorParameter, CurrentHealth);
				TargetAnimator.SetInteger(_healthAsIntAnimatorParameter, (int)CurrentHealth);
			}
		}

		#endregion
		
		#region DamageDisablingAPIs

		/// <summary>
		/// Prevents the character from taking any damage
		/// </summary>
		public virtual void DamageDisabled()
		{
			Invulnerable = true;
		}

		/// <summary>
		/// Allows the character to take damage
		/// </summary>
		public virtual void DamageEnabled()
		{
			Invulnerable = false;
		}

		/// <summary>
		/// makes the character able to take damage again after the specified delay
		/// </summary>
		/// <returns>The layer collision.</returns>
		public virtual IEnumerator DamageEnabled(float delay)
		{
			yield return new WaitForSeconds (delay);
			Invulnerable = false;
		}

		#endregion
	}
}
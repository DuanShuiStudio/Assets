using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Linq;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个用来处理角色技能的类。
    /// </summary>
    //[RequireComponent(typeof(Character))]
    public class CharacterAbility : TopDownMonoBehaviour 
	{
		/// the sound fx to play when the ability starts
		[Tooltip("技能开始时播放的声音效果")]
		public AudioClip AbilityStartSfx;
		/// the sound fx to play while the ability is running
		[Tooltip("技能运行时播放的声音效果")]
		public AudioClip AbilityInProgressSfx;
		/// the sound fx to play when the ability stops
		[Tooltip("技能停止时播放的声音效果")]
		public AudioClip AbilityStopSfx;

		/// the feedbacks to play when the ability starts
		[Tooltip("技能开始时播放的反馈")]
		public MMFeedbacks AbilityStartFeedbacks;
		/// the feedbacks to play when the ability stops
		[Tooltip("技能停止时播放的反馈")]
		public MMFeedbacks AbilityStopFeedbacks;
                
		[Header("Permission权限")]
		/// if true, this ability can perform as usual, if not, it'll be ignored. You can use this to unlock abilities over time for example
		[Tooltip("如果为真，则此能力可以正常执行，否则将被忽略。例如，你可以使用它随着时间解锁能力")]
		public bool AbilityPermitted = true;
        
		/// an array containing all the blocking movement states. If the Character is in one of these states and tries to trigger this ability, it won't be permitted. Useful to prevent this ability from being used while Idle or Swimming, for example.
		[Tooltip("包含所有阻碍移动状态的数组。如果角色处于其中一种状态并试图触发此能力，则不允许使用。例如，可以防止在空闲或游泳时使用此技能。")]
		public CharacterStates.MovementStates[] BlockingMovementStates;
		/// an array containing all the blocking condition states. If the Character is in one of these states and tries to trigger this ability, it won't be permitted. Useful to prevent this ability from being used while dead, for example.
		[Tooltip("包含所有阻碍条件状态的数组。如果角色处于其中一种状态并试图触发此能力，则不允许使用。例如，防止这个技能在死后被使用。")]
		public CharacterStates.CharacterConditions[] BlockingConditionStates;
		/// an array containing all the blocking weapon states. If one of the character's weapons is in one of these states and yet the character tries to trigger this ability, it won't be permitted. Useful to prevent this ability from being used while attacking, for example.
		[Tooltip("包含所有阻碍武器状态的数组。如果角色的某件武器处于这些状态之一，但角色却试图触发这种能力，那么这是不被允许的。例如，用于防止在攻击时使用此技能。")]
		public Weapon.WeaponStates[] BlockingWeaponStates;

		public virtual bool AbilityAuthorized
		{
			get
			{
				if (_character != null)
				{
					if ((BlockingMovementStates != null) && (BlockingMovementStates.Length > 0))
					{
						for (int i = 0; i < BlockingMovementStates.Length; i++)
						{
							if (BlockingMovementStates[i] == (_character.MovementState.CurrentState))
							{
								return false;
							}    
						}
					}

					if ((BlockingConditionStates != null) && (BlockingConditionStates.Length > 0))
					{
						for (int i = 0; i < BlockingConditionStates.Length; i++)
						{
							if (BlockingConditionStates[i] == (_character.ConditionState.CurrentState))
							{
								return false;
							}    
						}
					}
					
					if ((BlockingWeaponStates != null) && (BlockingWeaponStates.Length > 0))
					{
						for (int i = 0; i < BlockingWeaponStates.Length; i++)
						{
							foreach (CharacterHandleWeapon handleWeapon in _handleWeaponList)
							{
								if (handleWeapon.CurrentWeapon != null)
								{
									if (BlockingWeaponStates[i] == (handleWeapon.CurrentWeapon.WeaponState.CurrentState))
									{
										return false;
									}
								}
							}
						}
					}
				}
				return AbilityPermitted;
			}
		}

        /// 此能力是否已初始化
        public virtual bool AbilityInitialized { get { return _abilityInitialized; } }
		
		public delegate void AbilityEvent();
		public AbilityEvent OnAbilityStart;
		public AbilityEvent OnAbilityStop;
        
		protected Character _character;
		protected TopDownController _controller;
		protected TopDownController2D _controller2D;
		protected TopDownController3D _controller3D;
		protected GameObject _model;
		protected Health _health;
		protected CharacterMovement _characterMovement;
		protected InputManager _inputManager;
		protected Animator _animator = null;
		protected CharacterStates _state;
		protected SpriteRenderer _spriteRenderer;
		protected MMStateMachine<CharacterStates.MovementStates> _movement;
		protected MMStateMachine<CharacterStates.CharacterConditions> _condition;
		protected AudioSource _abilityInProgressSfx;
		protected bool _abilityInitialized = false;
		protected float _verticalInput;
		protected float _horizontalInput;
		protected bool _startFeedbackIsPlaying = false;
		protected List<CharacterHandleWeapon> _handleWeaponList;

        /// 此方法仅用于在功能检查器的开头显示帮助框文本
        public virtual string HelpBoxText() { return ""; }

        /// <summary>
        /// 唤醒的时候，我们开始预先初始化我们的能力
        /// </summary>
        protected virtual void Awake()
		{
			PreInitialization ();
		}

        /// <summary>
        /// 在Start（）中，我们调用能力的初始化
        /// </summary>
        protected virtual void Start () 
		{
			Initialization();
		}

        /// <summary>
        /// 您可以覆盖的方法，以便在实际初始化之前进行初始化
        /// </summary>
        protected virtual void PreInitialization()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			BindAnimator();
		}

        /// <summary>
        /// 获取和存储组件以供以后使用
        /// </summary>
        protected virtual void Initialization()
		{
			BindAnimator();
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
			_controller2D = this.gameObject.GetComponentInParent<TopDownController2D>();
			_controller3D = this.gameObject.GetComponentInParent<TopDownController3D>();
			_model = _character.CharacterModel;
			_characterMovement = _character?.FindAbility<CharacterMovement>();
			_spriteRenderer = this.gameObject.GetComponentInParent<SpriteRenderer>();
			_health = _character.CharacterHealth;
			_handleWeaponList = _character?.FindAbilities<CharacterHandleWeapon>();
			_inputManager = _character.LinkedInputManager;
			_state = _character.CharacterState;
			_movement = _character.MovementState;
			_condition = _character.ConditionState;
			_abilityInitialized = true;
		}

        /// <summary>
        /// 在任何需要强制（再次）初始化此功能的时候调用此方法。
        /// </summary>
        public virtual void ForceInitialization()
		{
			Initialization();
		}

        /// <summary>
        /// 从角色绑定动画器并初始化动画器参数
        /// </summary>
        protected virtual void BindAnimator()
		{
			if (_character._animator == null)
			{
				_character.AssignAnimator();
			}

			_animator = _character._animator;

			if (_animator != null)
			{
				InitializeAnimatorParameters();
			}
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected virtual void InitializeAnimatorParameters()
		{

		}

        /// <summary>
        /// 检查输入管理器是否存在的内部方法
        /// </summary>
        protected virtual void InternalHandleInput()
		{
			if (_inputManager == null) { return; }
			_horizontalInput = _inputManager.PrimaryMovement.x;
			_verticalInput = _inputManager.PrimaryMovement.y;
			HandleInput();
		}

        /// <summary>
        /// 在功能周期的最开始调用，并打算被覆盖，查找输入并在满足条件时调用方法
        /// </summary>
        protected virtual void HandleInput()
		{

		}

        /// <summary>
        /// 重置此技能的所有输入。可以覆盖能力特定指令
        /// </summary>
        public virtual void ResetInput()
		{
			_horizontalInput = 0f;
			_verticalInput = 0f;
		}


        /// <summary>
        /// 在你的能力释放3次中的第一次. 参照 EarlyUpdate()如果它存在的话
        /// The first of the 3 passes you can have in your ability. Think of it as EarlyUpdate()
        /// </summary>
        public virtual void EarlyProcessAbility()
		{
			InternalHandleInput();
		}

        /// <summary>
        /// 在你的能力释放3次中的第二次. 参照 Update()
        /// The second of the 3 passes you can have in your ability. Think of it as Update()
        /// </summary>
        public virtual void ProcessAbility()
		{
			
		}

        /// <summary>
        /// 在你的能力释放3次中的最后一次. 参照 LateUpdate()
        /// The last of the 3 passes you can have in your ability. Think of it as LateUpdate()
        /// </summary>
        public virtual void LateProcessAbility()
		{
			
		}

        /// <summary>
        /// 重写此命令以向角色的动画器发送参数。在Early， normal和Late process（）之后，每个周期由Character类调用一次。
        /// </summary>
        public virtual void UpdateAnimator()
		{

		}

        /// <summary>
        /// 修改该能力的权限状态
        /// </summary>
        /// <param name="abilityPermitted">If set to <c>true</c> ability permitted.</param>
        public virtual void PermitAbility(bool abilityPermitted)
		{
			AbilityPermitted = abilityPermitted;
		}

        /// <summary>
        /// 重写此选项以指定角色翻转时该能力应发生的情况
        /// </summary>
        public virtual void Flip()
		{
			
		}

        /// <summary>
        /// 重写此功能以重置此技能的参数。当角色被杀死时，它将自动被调用，以期待其重生。
        /// </summary>
        public virtual void ResetAbility()
		{
			
		}

        /// <summary>
        /// 使用参数中设置的输入管理器更改对输入管理器的引用
        /// </summary>
        /// <param name="newInputManager"></param>
        public virtual void SetInputManager(InputManager newInputManager)
		{
			_inputManager = newInputManager;
		}

        /// <summary>
        /// 播放能力开始音效
        /// </summary>
        public virtual void PlayAbilityStartSfx()
		{
			if (AbilityStartSfx!=null)
			{
				AudioSource tmp = new AudioSource();
				MMSoundManagerSoundPlayEvent.Trigger(AbilityStartSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);	
			}
		}

        /// <summary>
        /// 播放技能使用的音效
        /// </summary>
        public virtual void PlayAbilityUsedSfx()
		{
			if (AbilityInProgressSfx != null) 
			{	
				if (_abilityInProgressSfx == null)
				{
					_abilityInProgressSfx = MMSoundManagerSoundPlayEvent.Trigger(AbilityInProgressSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position, true);
				}
			}
		}

        /// <summary>
        /// 停止使用技能的声音效果
        /// </summary>
        public virtual void StopAbilityUsedSfx()
		{
			if (_abilityInProgressSfx != null)
			{
				MMSoundManagerSoundControlEvent.Trigger(MMSoundManagerSoundControlEventTypes.Free, 0, _abilityInProgressSfx);
				_abilityInProgressSfx = null;
			}
		}

        /// <summary>
        /// 播放能力停止音效
        /// </summary>
        public virtual void PlayAbilityStopSfx()
		{
			if (AbilityStopSfx!=null) 
			{	
				MMSoundManagerSoundPlayEvent.Trigger(AbilityStopSfx, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
			}
		}

        /// <summary>
        /// 播放能力开始音效
        /// </summary>
        public virtual void PlayAbilityStartFeedbacks()
		{
			AbilityStartFeedbacks?.PlayFeedbacks(this.transform.position);
			_startFeedbackIsPlaying = true;
			OnAbilityStart?.Invoke();
		}

        /// <summary>
        /// 停止使用技能的声音效果
        /// </summary>
        public virtual void StopStartFeedbacks()
		{
			AbilityStartFeedbacks?.StopFeedbacks();
			_startFeedbackIsPlaying = false;
		}

        /// <summary>
        /// 播放能力停止音效
        /// </summary>
        public virtual void PlayAbilityStopFeedbacks()
		{
			AbilityStopFeedbacks?.PlayFeedbacks();
			OnAbilityStop?.Invoke();
		}

        /// <summary>
        /// 向列表注册一个新的动画器参数
        /// </summary>
        /// <param name="parameterName">Parameter name.</param>
        /// <param name="parameterType">Parameter type.</param>
        protected virtual void RegisterAnimatorParameter(string parameterName, AnimatorControllerParameterType parameterType, out int parameter)
		{
			parameter = Animator.StringToHash(parameterName);

			if (_animator == null) 
			{
				return;
			}
			if (_animator.MMHasParameterOfType(parameterName, parameterType))
			{
				if (_character != null)
				{
					_character._animatorParameters.Add(parameter);	
				}
			}
		}

        /// <summary>
        /// 重写这一点来描述当角色重生时该能力应该发生什么
        /// </summary>
        protected virtual void OnRespawn()
		{
		}

        /// <summary>
        /// 重写这一点来描述当角色重生时该能力应该发生什么
        /// </summary>
        protected virtual void OnDeath()
		{
			StopAbilityUsedSfx ();
			StopStartFeedbacks();
		}

        /// <summary>
        /// 重写这一点来描述当角色受到攻击时该技能应该发生什么
        /// </summary>
        protected virtual void OnHit()
		{

		}

        /// <summary>
		/// 在enable上，我们绑定重生委托
        /// On enable, we bind our respawn delegate
        /// </summary>
        protected virtual void OnEnable()
		{
			if (_health == null)
			{
				_health = this.gameObject.GetComponentInParent<Character>().CharacterHealth;
			}

			if (_health == null)
			{
				_health = this.gameObject.GetComponentInParent<Health>();
			}

			if (_health != null)
			{
				_health.OnRevive += OnRespawn;
				_health.OnDeath += OnDeath;
				_health.OnHit += OnHit;
			}
		}

        /// <summary>
        /// 在禁用时，我们取消绑定respawn委托
        /// On disable, we unbind our respawn delegate
        /// </summary>
        protected virtual void OnDisable()
		{
			if (_health != null)
			{
				_health.OnRevive -= OnRespawn;
				_health.OnDeath -= OnDeath;
				_health.OnHit -= OnHit;
			}	
		}
	}
}
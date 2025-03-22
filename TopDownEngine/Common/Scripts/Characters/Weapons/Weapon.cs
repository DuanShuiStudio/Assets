using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个基础类旨在被扩展（请参见ProjectileWeapon.cs以了解示例），它处理射速（实际使用率）和弹药装填
    /// </summary>
    [SelectionBase]
	public class Weapon : MMMonoBehaviour 
	{
		[MMInspectorGroup("ID", true, 7)]
		/// the name of the weapon, only used for debugging
		[Tooltip("武器的名称，仅用于调试")]
		public string WeaponName;
        /// 触发器的可能使用模式（半自动：玩家需要释放触发器才能再次射击，自动：玩家可以按住触发器连续射击）
        public enum TriggerModes { SemiAuto, Auto }

        /// 武器可能处于的状态
        public enum WeaponStates { WeaponIdle, WeaponStart, WeaponDelayBeforeUse, WeaponUse, WeaponDelayBetweenUses, WeaponStop, WeaponReloadNeeded, WeaponReloadStart, WeaponReload, WeaponReloadStop, WeaponInterrupted }

        /// 武器当前是否处于活动状态
        [MMReadOnly]
		[Tooltip("武器当前是否处于活动状态")]
		public bool WeaponCurrentlyActive = true;

		[MMInspectorGroup("Use", true, 10)]
		/// if this is true, this weapon will be able to read input (usually via the CharacterHandleWeapon ability), otherwise player input will be disabled
		[Tooltip("如果此选项为真，则该武器将能够读取输入（通常通过CharacterHandleWeapon能力），否则玩家输入将被禁用")]
		public bool InputAuthorized = true;
		/// is this weapon on semi or full auto ?
		[Tooltip("这把武器是半自动还是全自动的？")]
		public TriggerModes TriggerMode = TriggerModes.Auto;
		/// the delay before use, that will be applied for every shot
		[Tooltip("每次射击前都会应用的使用延迟")]
		public float DelayBeforeUse = 0f;
		/// whether or not the delay before used can be interrupted by releasing the shoot button (if true, releasing the button will cancel the delayed shot)
		[Tooltip("使用前的延迟是否可以被释放射击按钮中断（如果为真，则释放按钮将取消延迟射击）")]
		public bool DelayBeforeUseReleaseInterruption = true;
		/// the time (in seconds) between two shots		
		[Tooltip("两次射击之间的时间间隔（以秒为单位）")]
		public float TimeBetweenUses = 1f;
		/// whether or not the time between uses can be interrupted by releasing the shoot button (if true, releasing the button will cancel the time between uses)
		[Tooltip("射击间隔时间是否可以被释放射击按钮中断（如果为真，则释放按钮将取消使用之间的时间）")]
		public bool TimeBetweenUsesReleaseInterruption = true;

		[Header("Burst Mode连发模式")] 
		/// if this is true, the weapon will activate repeatedly for every shoot request
		[Tooltip("如果此选项为真，则武器将针对每个射击请求重复激活")]
		public bool UseBurstMode = false;
		/// the amount of 'shots' in a burst sequence
		[Tooltip("连发序列中的“射击”数量")]
		public int BurstLength = 3;
		/// the time between shots in a burst sequence (in seconds)
		[Tooltip("连发序列中每次射击之间的时间间隔（以秒为单位）")]
		public float BurstTimeBetweenShots = 0.1f;

		[MMInspectorGroup("Magazine", true, 11)]
		/// whether or not the weapon is magazine based. If it's not, it'll just take its ammo inside a global pool
		[Tooltip("武器是否基于弹匣。如果不是，它将从全局弹药池中获取弹药")]
		public bool MagazineBased = false;
		/// the size of the magazine
		[Tooltip("弹匣的大小")]
		public int MagazineSize = 30;
		/// if this is true, pressing the fire button when a reload is needed will reload the weapon. Otherwise you'll need to press the reload button
		[Tooltip("如果此选项为真，当需要装填弹药时按下射击按钮将装填武器。否则你需要按下装填按钮")]
		public bool AutoReload;
		/// if this is true, reload will automatically happen right after the last bullet is shot, without the need for input
		[Tooltip("如果此选项为真，装填将在最后一发子弹射出后自动进行，无需输入")]
		public bool NoInputReload = false;
		/// the time it takes to reload the weapon
		[Tooltip("装填武器所需的时间")]
		public float ReloadTime = 2f;
		/// the amount of ammo consumed everytime the weapon fires
		[Tooltip("每次武器射击时消耗的弹药量")]
		public int AmmoConsumedPerShot = 1;
		/// if this is set to true, the weapon will auto destroy when there's no ammo left
		[Tooltip("如果此选项设置为真，当弹药用尽时武器将自动销毁")]
		public bool AutoDestroyWhenEmpty;
		/// the delay (in seconds) before weapon destruction if empty
		[Tooltip("如果弹药用尽，武器销毁前的延迟时间（以秒为单位）")]
		public float AutoDestroyWhenEmptyDelay = 1f;
		/// if this is true, the weapon won't try and reload if the ammo is empty, when using WeaponAmmo
		[Tooltip("当使用WeaponAmmo时，如果弹药为空，此选项为真则武器不会尝试装填")]
		public bool PreventReloadIfAmmoEmpty = false;
		/// the current amount of ammo loaded inside the weapon
		[MMReadOnly]
		[Tooltip("当前武器内装载的弹药数量")]
		public int CurrentAmmoLoaded = 0;

		[MMInspectorGroup("Position", true, 12)]
		/// an offset that will be applied to the weapon once attached to the center of the WeaponAttachment transform.
		[Tooltip("一旦连接到武器附件变换的中心，将应用于武器的偏移")]
		public Vector3 WeaponAttachmentOffset = Vector3.zero;
		/// should that weapon be flipped when the character flips?
		[Tooltip("当角色翻转时，该武器是否也应翻转？")]
		public bool FlipWeaponOnCharacterFlip = true;
		/// the FlipValue will be used to multiply the model's transform's localscale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs
		[Tooltip("FlipValue将用于在翻转时乘以模型变换的局部比例。通常是-1.1,1,1，但可以随意更改以适应您的型号规格")]
		public Vector3 RightFacingFlipValue = new Vector3(1, 1, 1);
		/// the FlipValue will be used to multiply the model's transform's localscale on flip. Usually it's -1,1,1, but feel free to change it to suit your model's specs
		[Tooltip("FlipValue将用于在翻转时乘以模型变换的局部比例。通常是-1.1,1,1，但可以随意更改以适应您的型号规格")]
		public Vector3 LeftFacingFlipValue = new Vector3(-1, 1, 1);
		/// a transform to use as the spawn point for weapon use (if null, only offset will be considered, otherwise the transform without offset)
		[Tooltip("用作武器使用生成点的变换（如果为空，则仅考虑偏移量，否则为不带偏移量的变换）")]
		public Transform WeaponUseTransform;
		/// if this is true, the weapon will flip to match the character's orientation
		[Tooltip("如果此选项为真，武器将翻转以匹配角色的方向")]
		public bool WeaponShouldFlip = true;

		[MMInspectorGroup("IK", true, 13)]
		/// the transform to which the character's left hand should be attached to
		[Tooltip("角色左手应附加到的变换")]
		public Transform LeftHandHandle;
		/// the transform to which the character's right hand should be attached to
		[Tooltip("角色右手应附加到的变换")]
		public Transform RightHandHandle;

		[MMInspectorGroup("Movement", true, 14)]
		/// if this is true, a multiplier will be applied to movement while the weapon is active
		[Tooltip("若此选项为真，则在武器激活时将对移动应用一个倍数")]
		public bool ModifyMovementWhileAttacking = false;
		/// the multiplier to apply to movement while attacking
		[Tooltip("攻击时应用于移动的倍数")]
		public float MovementMultiplier = 0f;
		/// if this is true all movement will be prevented (even flip) while the weapon is active
		[Tooltip("若此选项为真，则在武器激活时将阻止所有移动（甚至翻转）")]
		public bool PreventAllMovementWhileInUse = false;
		/// if this is true all aim will be prevented while the weapon is active
		[Tooltip("若此选项为真，则在武器激活时将阻止所有瞄准")]
		public bool PreventAllAimWhileInUse = false;

		[MMInspectorGroup("Recoil", true, 15)]
		/// the force to apply to push the character back when shooting - positive values will push the character back, negative values will launch it forward, turning that recoil into a thrust
		[Tooltip("射击时应用于将角色推回的力量 - 正值将把角色向后推，负值将向前发射，将后坐力转变为推力")]
		public float RecoilForce = 0f;

		[MMInspectorGroup("Animation", true, 16)]
		/// the other animators (other than the Character's) that you want to update every time this weapon gets used
		[Tooltip("每次使用此武器时都要更新的其他动画（除角色的动画外）")]
		public List<Animator> Animators;
		/// If this is true, sanity checks will be performed to make sure animator parameters exist before updating them. Turning this to false will increase performance but will throw errors if you're trying to update non existing parameters. Make sure your animator has the required parameters.
		[Tooltip("如果此选项为真，将执行健全性检查以确保在更新动画参数之前它们存在。将其设置为假会增加性能，但如果尝试更新不存在的参数，则会抛出错误。请确保您的动画具有所需的参数")]
		public bool PerformAnimatorSanityChecks = false;
		/// if this is true, the weapon's animator(s) will mirror the animation parameter of the owner character (that way your weapon's animator will be able to "know" if the character is walking, jumping, etc)
		[Tooltip("如果此选项为真，武器的动画将镜像拥有者角色的动画参数（这样您的武器动画就能“知道”角色是否在行走、跳跃等）)")]
		public bool MirrorCharacterAnimatorParameters = false;

		[MMInspectorGroup("Animation Parameters Names", true, 17)]
		/// the ID of the weapon to pass to the animator
		[Tooltip("要传递给动画的武器ID")]
		public int WeaponAnimationID = 0;
		/// the name of the weapon's idle animation parameter : this will be true all the time except when the weapon is being used
		[Tooltip("武器空闲动画参数的名称：除了武器正在使用时，此值将一直为真")]
		public string IdleAnimationParameter;
		/// the name of the weapon's start animation parameter : true at the frame where the weapon starts being used
		[Tooltip("武器开始动画参数的名称：在武器开始被使用的帧处为真")]
		public string StartAnimationParameter;
		/// the name of the weapon's delay before use animation parameter : true when the weapon has been activated but hasn't been used yet
		[Tooltip("武器使用前延迟动画参数的名称：在武器被激活但尚未使用时为真")]
		public string DelayBeforeUseAnimationParameter;
		/// the name of the weapon's single use animation parameter : true at each frame the weapon activates (shoots)
		[Tooltip("武器单次使用动画参数的名称：在武器激活（射击）的每一帧为真")]
		public string SingleUseAnimationParameter;
		/// the name of the weapon's in use animation parameter : true at each frame the weapon has started firing but hasn't stopped yet
		[Tooltip("武器使用中动画参数的名称：在武器开始射击但尚未停止的每一帧为真")]
		public string UseAnimationParameter;
		/// the name of the weapon's delay between each use animation parameter : true when the weapon is in use
		[Tooltip("武器每次使用之间的延迟动画参数的名称：在武器使用时为真")]
		public string DelayBetweenUsesAnimationParameter;
		/// the name of the weapon stop animation parameter : true after a shot and before the next one or the weapon's stop 
		[Tooltip("武器停止动画参数的名称：在射击后和下一次射击或武器停止前为真 ")]
		public string StopAnimationParameter;
		/// the name of the weapon reload start animation parameter
		[Tooltip("武器开始装填动画参数的名称")]
		public string ReloadStartAnimationParameter;
		/// the name of the weapon reload animation parameter
		[Tooltip("武器装填动画参数的名称")]
		public string ReloadAnimationParameter;
		/// the name of the weapon reload end animation parameter
		[Tooltip("武器装填结束动画参数的名称")]
		public string ReloadStopAnimationParameter;
		/// the name of the weapon's angle animation parameter
		[Tooltip("武器角度动画参数的名称")]
		public string WeaponAngleAnimationParameter;
		/// the name of the weapon's angle animation parameter, adjusted so it's always relative to the direction the character is currently facing
		[Tooltip("武器角度动画参数的名称，经过调整使其始终相对于角色当前面对的方向")]
		public string WeaponAngleRelativeAnimationParameter;
		/// the name of the parameter to send to true as long as this weapon is equipped, used or not. While all the other parameters defined here are updated by the Weapon class itself, and passed to the weapon and character, this one will be updated by CharacterHandleWeapon only."
		[Tooltip("只要装备了这件武器，无论是否使用，就会将此参数发送为真。虽然这里定义的所有其他参数都是由武器类本身更新并传递给武器和角色的，但这个参数只会由CharacterHandleWeapon更新")]
		public string EquippedAnimationParameter;
		/// the name of the parameter to send to true when the weapon gets interrupted. While all the other parameters defined here are updated by the Weapon class itself, and passed to the weapon and character, this one will be updated by CharacterHandleWeapon only."
		[Tooltip("当武器被中断、使用或未使用时，将此参数发送为真。虽然这里定义的所有其他参数都是由武器类本身更新并传递给武器和角色的，但这个参数只会由CharacterHandleWeapon更新。")]
		public string InterruptedAnimationParameter;
        
		[MMInspectorGroup("Feedbacks", true, 18)]
		/// the feedback to play when the weapon starts being used
		[Tooltip("当武器开始使用时要播放的反馈")]
		public MMFeedbacks WeaponStartMMFeedback;
		/// the feedback to play while the weapon is in use
		[Tooltip("在武器使用中播放的反馈")]
		public MMFeedbacks WeaponUsedMMFeedback;
		/// if set, this feedback will be used randomly instead of WeaponUsedMMFeedback
		[Tooltip("如果设置了这个反馈，它将随机使用，而不是WeaponUsedMMFeedback")]
		public MMFeedbacks WeaponUsedMMFeedbackAlt;
		/// the feedback to play when the weapon stops being used
		[Tooltip("当武器停止使用时要播放的反馈")]
		public MMFeedbacks WeaponStopMMFeedback;
		/// the feedback to play when the weapon gets reloaded
		[Tooltip("当武器重新装填时要播放的反馈")]
		public MMFeedbacks WeaponReloadMMFeedback;
		/// the feedback to play when the weapon gets reloaded
		[Tooltip("当武器重新装填时要播放的反馈")]
		public MMFeedbacks WeaponReloadNeededMMFeedback;
		/// the feedback to play when the weapon can't reload as there's no more ammo available. You'll need PreventReloadIfAmmoEmpty to be true for this to work
		[Tooltip("当武器没有更多弹药可用而无法重新装填时，要播放的反馈。要使这个功能正常工作，需要将PreventReloadIfAmmoEmpty设置为真")]
		public MMFeedbacks WeaponReloadImpossibleMMFeedback;
        
		[MMInspectorGroup("Settings", true, 19)]
		/// If this is true, the weapon will initialize itself on start, otherwise it'll have to be init manually, usually by the CharacterHandleWeapon class
		[Tooltip("如果此选项为真，武器将在启动时自行初始化；否则，需要手动初始化，通常由CharacterHandleWeapon类完成")]
		public bool InitializeOnStart = false;
		/// whether or not this weapon can be interrupted 
		[Tooltip("此武器是否可以被中断")]
		public bool Interruptable = false;

        /// 此武器对应的库存项名称。由InventoryEngineWeapon自动设置（如果需要）
        public virtual string WeaponID { get; set; }
        /// 武器的拥有者
        public virtual Character Owner { get; protected set; }
        /// 武器拥有者的CharacterHandleWeapon组件
        public virtual CharacterHandleWeapon CharacterHandleWeapon { get; set; }
		/// if true, the weapon is flipped
		[MMReadOnly]
		[Tooltip("若为真，则武器现在被翻转")]
		public bool Flipped;
        /// 与此武器相关联的武器弹药组件
        public virtual WeaponAmmo WeaponAmmo { get; protected set; }
        /// 武器的状态机
        public MMStateMachine<WeaponStates> WeaponState;

		protected SpriteRenderer _spriteRenderer;
		protected WeaponAim _weaponAim;
		protected float _movementMultiplierStorage = 1f;

		public float MovementMultiplierStorage
		{
			get => _movementMultiplierStorage;
			set => _movementMultiplierStorage = value;
		}
		
		public bool IsComboWeapon { get; set; }
		public bool IsAutoComboWeapon { get; set; }
		
		protected Animator _ownerAnimator;
		protected WeaponPreventShooting _weaponPreventShooting;
		protected float _delayBeforeUseCounter = 0f;
		protected float _delayBetweenUsesCounter = 0f;
		protected float _reloadingCounter = 0f;
		protected bool _triggerReleased = false;
		protected bool _reloading = false;
		protected ComboWeapon _comboWeapon;
		protected TopDownController _controller;
		protected CharacterMovement _characterMovement;
		protected Vector3 _weaponOffset;
		protected Vector3 _weaponAttachmentOffset;
		protected Transform _weaponAttachment;
		protected List<HashSet<int>> _animatorParameters;
		protected HashSet<int> _ownerAnimatorParameters;
		protected bool _controllerIs3D = false;
        
		protected const string _aliveAnimationParameterName = "Alive";
		protected int _idleAnimationParameter;
		protected int _startAnimationParameter;
		protected int _delayBeforeUseAnimationParameter;
		protected int _singleUseAnimationParameter;
		protected int _useAnimationParameter;
		protected int _delayBetweenUsesAnimationParameter;
		protected int _stopAnimationParameter;
		protected int _reloadStartAnimationParameter;
		protected int _reloadAnimationParameter;
		protected int _reloadStopAnimationParameter;
		protected int _weaponAngleAnimationParameter;
		protected int _weaponAngleRelativeAnimationParameter;
		protected int _aliveAnimationParameter;
		protected int _comboInProgressAnimationParameter;
		protected int _equippedAnimationParameter;
		protected int _interruptedAnimationParameter;
		protected float _lastShootRequestAt = -float.MaxValue;
		protected float _lastTurnWeaponOnAt = -float.MaxValue;
		protected bool _movementSpeedMultiplierSet = false;

        /// <summary>
        /// 在开始时，我们初始化我们的武器
        /// </summary>
        protected virtual void Start()
		{
			if (InitializeOnStart)
			{
				Initialization();
			}
		}

        /// <summary>
        /// 初始化这个武器
        /// </summary>
        public virtual void Initialization()
		{
			Flipped = false;
			_spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
			_comboWeapon = this.gameObject.GetComponent<ComboWeapon>();
			_weaponPreventShooting = this.gameObject.GetComponent<WeaponPreventShooting>();

			WeaponState = new MMStateMachine<WeaponStates>(gameObject, true);
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
			WeaponAmmo = GetComponent<WeaponAmmo>();
			_animatorParameters = new List<HashSet<int>>();
			_weaponAim = GetComponent<WeaponAim>();
			InitializeAnimatorParameters();
			if (WeaponAmmo == null)
			{
				CurrentAmmoLoaded = MagazineSize;
			}
			InitializeFeedbacks();       
		}

		protected virtual void InitializeFeedbacks()
		{
			WeaponStartMMFeedback?.Initialization(this.gameObject);
			WeaponUsedMMFeedback?.Initialization(this.gameObject);
			WeaponUsedMMFeedbackAlt?.Initialization(this.gameObject);
			WeaponStopMMFeedback?.Initialization(this.gameObject);
			WeaponReloadNeededMMFeedback?.Initialization(this.gameObject);
			WeaponReloadMMFeedback?.Initialization(this.gameObject);
		}

        /// <summary>
        /// 如果它是一个组合武器，则初始化这个组合武器
        /// </summary>
        public virtual void InitializeComboWeapons()
		{
			IsComboWeapon = false;
			IsAutoComboWeapon = false;
			if (_comboWeapon != null)
			{
				IsComboWeapon = true;
				IsAutoComboWeapon = (_comboWeapon.InputMode == ComboWeapon.InputModes.Auto);
				_comboWeapon.Initialization();
			}
		}

        /// <summary>
        /// 设置武器的拥有者
        /// </summary>
        /// <param name="newOwner">New owner.</param>
        public virtual void SetOwner(Character newOwner, CharacterHandleWeapon handleWeapon)
		{
			Owner = newOwner;
			if (Owner != null)
			{
				CharacterHandleWeapon = handleWeapon;
				_characterMovement = Owner.GetComponent<Character>()?.FindAbility<CharacterMovement>();
				_controller = Owner.GetComponent<TopDownController>();

				_controllerIs3D = Owner.GetComponent<TopDownController3D>() != null;

				if (CharacterHandleWeapon.AutomaticallyBindAnimator)
				{
					if (CharacterHandleWeapon.CharacterAnimator != null)
					{
						_ownerAnimator = CharacterHandleWeapon.CharacterAnimator;
					}
					if (_ownerAnimator == null)
					{
						_ownerAnimator = CharacterHandleWeapon.gameObject.GetComponentInParent<Character>().CharacterAnimator;
					}
					if (_ownerAnimator == null)
					{
						_ownerAnimator = CharacterHandleWeapon.gameObject.GetComponentInParent<Animator>();
					}
				}
			}
		}

        /// <summary>
        /// 通过输入调用，打开武器
        /// </summary>
        public virtual void WeaponInputStart()
		{
			if (_reloading)
			{
				return;
			}

			if (WeaponState.CurrentState == WeaponStates.WeaponIdle)
			{
				_triggerReleased = false;
				TurnWeaponOn();
			}
		}

        /// <summary>
        /// 描述当武器的输入被释放时会发生什么
        /// </summary>
        public virtual void WeaponInputReleased()
		{
			
		}

        /// <summary>
        /// 描述当武器开始使用时会发生什么
        /// </summary>
        public virtual void TurnWeaponOn()
		{
			if (!InputAuthorized && (Time.time - _lastTurnWeaponOnAt < TimeBetweenUses))
			{
				return;
			}

			_lastTurnWeaponOnAt = Time.time;
			
			TriggerWeaponStartFeedback();
			WeaponState.ChangeState(WeaponStates.WeaponStart);
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
			{
				_movementMultiplierStorage = _characterMovement.MovementSpeedMultiplier;
				_characterMovement.MovementSpeedMultiplier = MovementMultiplier;
				_movementSpeedMultiplierSet = true;
			}
			if (_comboWeapon != null)
			{
				_comboWeapon.WeaponStarted(this);
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null) && (_controller != null))
			{
				_characterMovement.SetMovement(Vector2.zero);
				_characterMovement.MovementForbidden = true;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.AimControlActive = false;
			}
		}

        /// <summary>
        /// 在更新时，我们检查武器是否或应该被使用
        /// </summary>
        protected virtual void Update()
		{
			FlipWeapon();
			ApplyOffset();       
		}

        /// <summary>
        /// 在LateUpdate时，处理武器状态
        /// </summary>
        protected virtual void LateUpdate()
		{     
			ProcessWeaponState();
		}

        /// <summary>
        /// 在每次LateUpdate时调用，处理武器的状态机
        /// </summary>
        protected virtual void ProcessWeaponState()
		{
			if (WeaponState == null) { return; }
			
			UpdateAnimator();

			switch (WeaponState.CurrentState)
			{
				case WeaponStates.WeaponIdle:
					CaseWeaponIdle();
					break;

				case WeaponStates.WeaponStart:
					CaseWeaponStart();
					break;

				case WeaponStates.WeaponDelayBeforeUse:
					CaseWeaponDelayBeforeUse();
					break;

				case WeaponStates.WeaponUse:
					CaseWeaponUse();
					break;

				case WeaponStates.WeaponDelayBetweenUses:
					CaseWeaponDelayBetweenUses();
					break;

				case WeaponStates.WeaponStop:
					CaseWeaponStop();
					break;

				case WeaponStates.WeaponReloadNeeded:
					CaseWeaponReloadNeeded();
					break;

				case WeaponStates.WeaponReloadStart:
					CaseWeaponReloadStart();
					break;

				case WeaponStates.WeaponReload:
					CaseWeaponReload();
					break;

				case WeaponStates.WeaponReloadStop:
					CaseWeaponReloadStop();
					break;

				case WeaponStates.WeaponInterrupted:
					CaseWeaponInterrupted();
					break;
			}
		}

        /// <summary>
        /// 如果武器处于空闲状态，我们重置移动倍数
        /// </summary>
        public virtual void CaseWeaponIdle()
		{
				ResetMovementMultiplier();	
		}

        /// <summary>
        /// 当武器开始使用时，我们根据武器的设置切换到延迟或射击
        /// </summary>
        public virtual void CaseWeaponStart()
		{
			if (DelayBeforeUse > 0)
			{
				_delayBeforeUseCounter = DelayBeforeUse;
				WeaponState.ChangeState(WeaponStates.WeaponDelayBeforeUse);
			}
			else
			{
				StartCoroutine(ShootRequestCo());
			}
		}

        /// <summary>
        /// 如果我们处于使用前的延迟状态，我们会等待延迟结束，然后请求射击
        /// </summary>
        public virtual void CaseWeaponDelayBeforeUse()
		{
			_delayBeforeUseCounter -= Time.deltaTime;
			if (_delayBeforeUseCounter <= 0)
			{
				StartCoroutine(ShootRequestCo());
			}
		}

        /// <summary>
        /// 在武器使用时，我们使用武器，然后切换到每次使用之间的延迟
        /// </summary>
        public virtual void CaseWeaponUse()
		{
			WeaponUse();
			_delayBetweenUsesCounter = TimeBetweenUses;
			WeaponState.ChangeState(WeaponStates.WeaponDelayBetweenUses);
		}

        /// <summary>
        /// 在每次使用之间的延迟时，我们要么关闭武器，要么发出射击请求
        /// </summary>
        public virtual void CaseWeaponDelayBetweenUses()
		{
			if (_triggerReleased && TimeBetweenUsesReleaseInterruption)
			{
				TurnWeaponOff();
				return;
			}
            
			_delayBetweenUsesCounter -= Time.deltaTime;
			if (_delayBetweenUsesCounter <= 0)
			{
				if ((TriggerMode == TriggerModes.Auto) && !_triggerReleased)
				{
					StartCoroutine(ShootRequestCo());
				}
				else
				{
					TurnWeaponOff();
				}
			}
		}

        /// <summary>
        /// 在武器停止时，我们切换到空闲状态
        /// </summary>
        public virtual void CaseWeaponStop()
		{
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

        /// <summary>
        /// 如果需要重新装填，我们会提及这一点并切换到空闲状态
        /// </summary>
        public virtual void CaseWeaponReloadNeeded()
		{
			ReloadNeeded();
			ResetMovementMultiplier();
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

        /// <summary>
        /// 在重新装填开始时，我们重新装填武器并切换到重新装填状态
        /// </summary>
        public virtual void CaseWeaponReloadStart()
		{
			ReloadWeapon();
			_reloadingCounter = ReloadTime;
			WeaponState.ChangeState(WeaponStates.WeaponReload);
		}

        /// <summary>
        /// 在重新装填时，我们重置移动倍数，并在重新装填延迟结束后切换到重新装填停止状态。
        /// </summary>
        public virtual void CaseWeaponReload()
		{
			ResetMovementMultiplier();
			_reloadingCounter -= Time.deltaTime;
			if (_reloadingCounter <= 0)
			{
				WeaponState.ChangeState(WeaponStates.WeaponReloadStop);
			}
		}

        /// <summary>
        /// 在重新装填停止时，我们切换到空闲状态并加载弹药
        /// </summary>
        public virtual void CaseWeaponReloadStop()
		{
			_reloading = false;
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
			if (WeaponAmmo == null)
			{
				CurrentAmmoLoaded = MagazineSize;
			}
		}

        /// <summary>
        /// 在武器被中断时，我们关闭武器并切换回空闲状态
        /// </summary>
        public virtual void CaseWeaponInterrupted()
		{
			TurnWeaponOff();
			ResetMovementMultiplier();
			if ((WeaponState.CurrentState == WeaponStates.WeaponReload)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStart)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStop))
			{
				return;
			}
			WeaponState.ChangeState(WeaponStates.WeaponIdle);
		}

        /// <summary>
        /// 调用此方法以中断武器
        /// </summary>
        public virtual void Interrupt()
		{
			if ((WeaponState.CurrentState == WeaponStates.WeaponReload)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStart)
			    || (WeaponState.CurrentState == WeaponStates.WeaponReloadStop))
			{
				return;
			}
			
			if (Interruptable)
			{
				WeaponState.ChangeState(WeaponStates.WeaponInterrupted);
			}
		}




        /// <summary>
        ///确定武器是否可以射击
        /// </summary>
        public virtual IEnumerator ShootRequestCo()
		{
			if (Time.time - _lastShootRequestAt < TimeBetweenUses)
			{
				yield break;
			}
			
			int remainingShots = UseBurstMode ? BurstLength : 1;
			float interval = UseBurstMode ? BurstTimeBetweenShots : 1;

			while (remainingShots > 0)
			{
				ShootRequest();
				_lastShootRequestAt = Time.time;
				remainingShots--;
				yield return MMCoroutine.WaitFor(interval);
			}
		}

		public virtual void ShootRequest()
		{
            // 如果我们有武器弹药组件，我们就确定我们是否有足够的弹药来射击
            if (_reloading)
			{
				return;
			}

			if (_weaponPreventShooting != null)
			{
				if (!_weaponPreventShooting.ShootingAllowed())
				{
					return;
				}
			}

			if (MagazineBased)
			{
				if (WeaponAmmo != null)
				{
					if (WeaponAmmo.EnoughAmmoToFire())
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
					}
					else
					{
						if (AutoReload && MagazineBased)
						{
							InitiateReloadWeapon();
						}
						else
						{
							WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
						}
					}
				}
				else
				{
					if (CurrentAmmoLoaded > 0)
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
						CurrentAmmoLoaded -= AmmoConsumedPerShot;
					}
					else
					{
						if (AutoReload)
						{
							InitiateReloadWeapon();
						}
						else
						{
							WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
						}
					}
				}
			}
			else
			{
				if (WeaponAmmo != null)
				{
					if (WeaponAmmo.EnoughAmmoToFire())
					{
						WeaponState.ChangeState(WeaponStates.WeaponUse);
					}
					else
					{
						WeaponState.ChangeState(WeaponStates.WeaponReloadNeeded);
					}
				}
				else
				{
					WeaponState.ChangeState(WeaponStates.WeaponUse);
				}
			}
		}

        /// <summary>
        /// 当武器被使用的时候，播放相应的声音
        /// </summary>
        public virtual void WeaponUse()
		{
			ApplyRecoil();
			TriggerWeaponUsedFeedback();
		}

        /// <summary>
        /// 如有必要，应用后坐力
        /// </summary>
        protected virtual void ApplyRecoil()
		{
			if ((RecoilForce != 0f) && (_controller != null))
			{
				if (Owner != null)
				{
					if (!_controllerIs3D)
					{
						if (Flipped)
						{
							_controller.Impact(this.transform.right, RecoilForce);
						}
						else
						{
							_controller.Impact(-this.transform.right, RecoilForce);
						}
					}
					else
					{
						_controller.Impact(-this.transform.forward, RecoilForce);
					}
				}                
			}
		}

        /// <summary>
        /// 由输入调用，如果处于自动模式则关闭武器
        /// </summary>
        public virtual void WeaponInputStop()
		{
			if (_reloading)
			{
				return;
			}
			_triggerReleased = true;
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking))
			{
				_characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
				_movementMultiplierStorage = 1f;
			}
		}

        /// <summary>
        /// 关闭武器
        /// </summary>
        public virtual void TurnWeaponOff()
		{
			if ((WeaponState.CurrentState == WeaponStates.WeaponIdle || WeaponState.CurrentState == WeaponStates.WeaponStop))
			{
				return;
			}
			_triggerReleased = true;

			TriggerWeaponStopFeedback();
			WeaponState.ChangeState(WeaponStates.WeaponStop);
			ResetMovementMultiplier();
			if (_comboWeapon != null)
			{
				_comboWeapon.WeaponStopped(this);
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null))
			{
				_characterMovement.MovementForbidden = false;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.AimControlActive = true;
			}

			if (NoInputReload)
			{
				bool needToReload = false;
				if (WeaponAmmo != null)
				{
					needToReload = !WeaponAmmo.EnoughAmmoToFire();
				}
				else
				{
					needToReload = (CurrentAmmoLoaded <= 0);
				}
                
				if (needToReload)
				{
					InitiateReloadWeapon();
				}
			}
		}

		protected virtual void ResetMovementMultiplier()
		{
			if ((_characterMovement != null) && (ModifyMovementWhileAttacking) && _movementSpeedMultiplierSet)
			{
				_characterMovement.MovementSpeedMultiplier = _movementMultiplierStorage;
				_movementMultiplierStorage = 1f;
				_movementSpeedMultiplierSet = false;
			}
		}

        /// <summary>
        /// 描述武器需要重新装填时会发生什么情况
        /// </summary>
        public virtual void ReloadNeeded()
		{
			TriggerWeaponReloadNeededFeedback();
		}

        /// <summary>
        /// 开始重新装填
        /// </summary>
        public virtual void InitiateReloadWeapon()
		{
			if (PreventReloadIfAmmoEmpty && WeaponAmmo && WeaponAmmo.CurrentAmmoAvailable == 0)
			{
				WeaponReloadImpossibleMMFeedback?.PlayFeedbacks();
				return;
			}

            // 如果已经在重新装填中，我们不做任何操作并退出
            if (_reloading || !MagazineBased)
			{
				return;
			}
			if (PreventAllMovementWhileInUse && (_characterMovement != null))
			{
				_characterMovement.MovementForbidden = false;
			}
			if (PreventAllAimWhileInUse && (_weaponAim != null))
			{
				_weaponAim.AimControlActive = true;
			}
			WeaponState.ChangeState(WeaponStates.WeaponReloadStart);
			_reloading = true;
		}

        /// <summary>
        /// 重新装填武器
        /// </summary>
        /// <param name="ammo">Ammo.</param>
        protected virtual void ReloadWeapon()
		{
			if (MagazineBased)
			{
				TriggerWeaponReloadFeedback();
			}
		}

        /// <summary>
        /// 翻转武器.
        /// </summary>
        public virtual void FlipWeapon()
		{
			if (!WeaponShouldFlip)
			{
				return;
			}
			
			if (Owner == null)
			{
				return;
			}

			if (Owner.Orientation2D == null)
			{
				return;
			}

			if (FlipWeaponOnCharacterFlip)
			{
				Flipped = !Owner.Orientation2D.IsFacingRight;
				if (_spriteRenderer != null)
				{
					_spriteRenderer.flipX = Flipped;
				}
				else
				{
					transform.localScale = Flipped ? LeftFacingFlipValue : RightFacingFlipValue;
				}
			}

			if (_comboWeapon != null)
			{
				_comboWeapon.FlipUnusedWeapons();
			}
		}

        /// <summary>
        /// 销毁武器
        /// </summary>
        /// <returns>The destruction.</returns>
        public virtual IEnumerator WeaponDestruction()
		{
			yield return new WaitForSeconds(AutoDestroyWhenEmptyDelay);
            // 如果我们没有弹药了，并且需要销毁我们的武器，我们就这么做
            TurnWeaponOff();
			Destroy(this.gameObject);

			if (WeaponID != null)
			{
                // 我们把它从库存中移除
                List<int> weaponList = Owner.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterInventory>().WeaponInventory.InventoryContains(WeaponID);
				if (weaponList.Count > 0)
				{
					Owner.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterInventory>().WeaponInventory.DestroyItem(weaponList[0]);
				}
			}
		}

        /// <summary>
        /// 应用在检查器中指定的偏移量
        /// </summary>
        public virtual void ApplyOffset()
		{

			if (!WeaponCurrentlyActive)
			{
				return;
			}
            
			_weaponAttachmentOffset = WeaponAttachmentOffset;

			if (Owner == null)
			{
				return;
			}

			if (Owner.Orientation2D != null)
			{
				if (Flipped)
				{
					_weaponAttachmentOffset.x = -WeaponAttachmentOffset.x;
				}

                // 我们应用偏移量
                if (transform.parent != null)
				{
					_weaponOffset = transform.parent.position + _weaponAttachmentOffset;
					transform.position = _weaponOffset;
				}
			}
			else
			{
				if (transform.parent != null)
				{
					_weaponOffset = _weaponAttachmentOffset;
					transform.localPosition = _weaponOffset;
				}
			}           
		}

        /// <summary>
        /// 播放武器的启动声音
        /// </summary>
        protected virtual void TriggerWeaponStartFeedback()
		{
			WeaponStartMMFeedback?.PlayFeedbacks(this.transform.position);
		}

        /// <summary>
        /// 播放武器的使用声音
        /// </summary>
        protected virtual void TriggerWeaponUsedFeedback()
		{
			if (WeaponUsedMMFeedbackAlt != null)
			{
				int random = MMMaths.RollADice(2);
				if (random > 1)
				{
					WeaponUsedMMFeedbackAlt?.PlayFeedbacks(this.transform.position);
				}
				else
				{
					WeaponUsedMMFeedback?.PlayFeedbacks(this.transform.position);
				}
			}
			else
			{
				WeaponUsedMMFeedback?.PlayFeedbacks(this.transform.position);    
			}
            
		}

        /// <summary>
        /// 播放武器的停止声音
        /// </summary>
        protected virtual void TriggerWeaponStopFeedback()
		{            
			WeaponStopMMFeedback?.PlayFeedbacks(this.transform.position);
		}

        /// <summary>
        /// 播放武器需要重新装填的声音
        /// </summary>
        protected virtual void TriggerWeaponReloadNeededFeedback()
		{
			WeaponReloadNeededMMFeedback?.PlayFeedbacks(this.transform.position);
		}

        /// <summary>
        /// 播放武器重新装填的声音
        /// </summary>
        protected virtual void TriggerWeaponReloadFeedback()
		{
			WeaponReloadMMFeedback?.PlayFeedbacks(this.transform.position);
		}

        /// <summary>
        /// 如果它们存在，将所需的动画参数添加到动画参数列表中
        /// </summary>
        public virtual void InitializeAnimatorParameters()
		{
			if (Animators.Count > 0)
			{
				for (int i = 0; i < Animators.Count; i++)
				{
					_animatorParameters.Add(new HashSet<int>());
					AddParametersToAnimator(Animators[i], _animatorParameters[i]);
					if (!PerformAnimatorSanityChecks)
					{
						Animators[i].logWarnings = false;
					}

					if (MirrorCharacterAnimatorParameters)
					{
						MMAnimatorMirror mirror = Animators[i].gameObject.AddComponent<MMAnimatorMirror>();
						mirror.SourceAnimator = _ownerAnimator;
						mirror.TargetAnimator = Animators[i];
						mirror.Initialization();
					}
				}                
			}            

			if (_ownerAnimator != null)
			{
				_ownerAnimatorParameters = new HashSet<int>();
				AddParametersToAnimator(_ownerAnimator, _ownerAnimatorParameters);
				if (!PerformAnimatorSanityChecks)
				{
					_ownerAnimator.logWarnings = false;
				}
			}
		}

		protected virtual void AddParametersToAnimator(Animator animator, HashSet<int> list)
		{
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, EquippedAnimationParameter, out _equippedAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleAnimationParameter, out _weaponAngleAnimationParameter, AnimatorControllerParameterType.Float, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, WeaponAngleRelativeAnimationParameter, out _weaponAngleRelativeAnimationParameter, AnimatorControllerParameterType.Float, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, IdleAnimationParameter, out _idleAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StartAnimationParameter, out _startAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBeforeUseAnimationParameter, out _delayBeforeUseAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, DelayBetweenUsesAnimationParameter, out _delayBetweenUsesAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, StopAnimationParameter, out _stopAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStartAnimationParameter, out _reloadStartAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadStopAnimationParameter, out _reloadStopAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, ReloadAnimationParameter, out _reloadAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, SingleUseAnimationParameter, out _singleUseAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, UseAnimationParameter, out _useAnimationParameter, AnimatorControllerParameterType.Bool, list);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, InterruptedAnimationParameter, out _interruptedAnimationParameter, AnimatorControllerParameterType.Bool, list);

			if (_comboWeapon != null)
			{
				MMAnimatorExtensions.AddAnimatorParameterIfExists(animator, _comboWeapon.ComboInProgressAnimationParameter, out _comboInProgressAnimationParameter, AnimatorControllerParameterType.Bool, list);
			}
		}

        /// <summary>
        /// 覆盖此方法以将参数发送到角色的动画师。这是由角色每周期调用一次。
        /// class, after Early, normal and Late process().
        /// </summary>
        public virtual void UpdateAnimator()
		{
			for (int i = 0; i < Animators.Count; i++)
			{
				UpdateAnimator(Animators[i], _animatorParameters[i]);
			}

			if ((_ownerAnimator != null) && (WeaponState != null) && (_ownerAnimatorParameters != null))
			{
				UpdateAnimator(_ownerAnimator, _ownerAnimatorParameters);
			}
		}

		protected virtual void UpdateAnimator(Animator animator, HashSet<int> list)
		{
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _equippedAnimationParameter, true, list);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _idleAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _startAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStart), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBeforeUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _useAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse || WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _singleUseAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _delayBetweenUsesAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _stopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponStop), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStartAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload), list, PerformAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(animator, _reloadStopAnimationParameter, (WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop), list, PerformAnimatorSanityChecks);

			if (WeaponState.CurrentState == Weapon.WeaponStates.WeaponInterrupted)
			{
				MMAnimatorExtensions.UpdateAnimatorTrigger(animator, _interruptedAnimationParameter, list, PerformAnimatorSanityChecks);
			}
			
			if (Owner != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(animator, _aliveAnimationParameter, (Owner.ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead), list, PerformAnimatorSanityChecks);
			}

			if (_weaponAim != null)
			{
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, _weaponAim.CurrentAngle, list, PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, _weaponAim.CurrentAngleRelative, list, PerformAnimatorSanityChecks);
			}
			else
			{
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(animator, _weaponAngleRelativeAnimationParameter, 0f, list, PerformAnimatorSanityChecks);
			}

			if (_comboWeapon != null)
			{
				MMAnimatorExtensions.UpdateAnimatorBool(animator, _comboInProgressAnimationParameter, _comboWeapon.ComboInProgress, list, PerformAnimatorSanityChecks);
			}
		}
	}
}
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此职业添加到角色中，使其可以使用武器
    /// 注意，该组件将触发动画（如果它们的参数存在于Animator中），基于
    /// 当前武器的动画
    /// 动画器参数：从武器的检查器中定义
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Handle Weapon")]
	public class CharacterHandleWeapon : CharacterAbility
	{
        /// 此方法仅用于在功能检查器的开头显示帮助框文本
        public override string HelpBoxText() { return "这个组件将允许你的角色拾取和使用武器。武器的作用在武器类中定义。这只是描述握着武器的“手”的行为，而不是武器本身。在这里，你可以为角色设置初始武器，允许拾取武器，并指定武器附件（角色内部的转换，可以只是一个空的子游戏对象，或者是模型的子部分）。"; }

		[Header("Weapon武器")]

		/// the initial weapon owned by the character
		[Tooltip("角色拥有的初始武器")]
		public Weapon InitialWeapon;
		/// if this is set to true, the character can pick up PickableWeapons
		[Tooltip("如果这个设置为真，角色可以捡起可拾取的武器")]
		public bool CanPickupWeapons = true;

		[Header("Feedbacks反馈")]
		/// a feedback that gets triggered at the character level everytime the weapon is used
		[Tooltip("每次使用武器时在角色级别触发的反馈")]
		public MMFeedbacks WeaponUseFeedback;

		[Header("Binding绑定")]
		/// the position the weapon will be attached to. If left blank, will be this.transform.
		[Tooltip("武器将附加到的位置。如果留空，将是this.transform")]
		public Transform WeaponAttachment;
		/// the position from which projectiles will be spawned (can be safely left empty)
		[Tooltip("投射物将从中生成的位置（可以安全地留空）")]
		public Transform ProjectileSpawn;
		/// if this is true this animator will be automatically bound to the weapon
		[Tooltip("如果这是真的，这个动画将自动绑定到武器")]
		public bool AutomaticallyBindAnimator = true;
		/// the ID of the AmmoDisplay this ability should update
		[Tooltip("这个能力应该更新的AmmoDisplay的ID")]
		public int AmmoDisplayID = 0;
		/// if this is true, IK will be automatically setup if possible
		[Tooltip("如果这是真的，IK反向动力将自动设置，如果可能的话")]
		public bool AutoIK = true;

		[Header("Input输入")]
		/// if this is true you won't have to release your fire button to auto reload
		[Tooltip("如果这个条件为真，你将不必松开开火按钮来自动装填")]
		public bool ContinuousPress = false;
		/// whether or not this character getting hit should interrupt its attack (will only work if the weapon is marked as interruptable)
		[Tooltip("这个角色受到攻击是否应该打断其攻击（仅当武器被标记为可中断时才会生效")]
		public bool GettingHitInterruptsAttack = false;
		/// whether or not pushing the secondary axis above its threshold should cause the weapon to shoot
		[Tooltip("推动辅助轴超过其阈值是否应导致武器射击")]
		public bool UseSecondaryAxisThresholdToShoot = false;
		/// if this is true, the ForcedWeaponAimControl mode will be applied to all weapons equipped by this character
		[Tooltip("如果这个条件为真，ForcedWeaponAimControl模式将应用于此角色装备的所有武器")]
		public bool ForceWeaponAimControl = false;
		/// if ForceWeaponAimControl is true, the AimControls mode to apply to all weapons equipped by this character
		[Tooltip("ForceWeaponAimControl的默认值是什么？\r\n如何关闭AimControls模式？\r\n哪些角色可以应用ForceWeaponAimControl？\r\n\r\n如果ForceWeaponAimControl为真，应用于此角色装备的所有武器的AimControls模式")]
		[MMCondition("ForceWeaponAimControl", true)]
		public WeaponAim.AimControls ForcedWeaponAimControl = WeaponAim.AimControls.PrimaryMovement;
		/// if this is true, the character will continuously fire its weapon
		[Tooltip("如果这是真的，角色将持续发射武器")]
		public bool ForceAlwaysShoot = false;

		[Header("Buffering缓冲")]
		/// whether or not attack input should be buffered, letting you prepare an attack while another is being performed, making it easier to chain them
		[Tooltip("是否应该缓冲攻击输入，让您在执行另一个攻击时准备一个攻击，从而更容易进行连击")]
		public bool BufferInput;
		/// if this is true, every new input will prolong the buffer
		[MMCondition("BufferInput", true)]
		[Tooltip("如果这个条件为真，每个新的输入都会延长缓冲时间")]
		public bool NewInputExtendsBuffer;
		/// the maximum duration for the buffer, in seconds
		[MMCondition("BufferInput", true)]
		[Tooltip("缓冲区的最大持续时间，以秒为单位")]
		public float MaximumBufferDuration = 0.25f;
		/// if this is true, and if this character is using GridMovement, then input will only be triggered when on a perfect tile
		[MMCondition("BufferInput", true)]
		[Tooltip("如果这个条件为真，且此角色正在使用GridMovement，那么输入将仅在完美的格子上触发")]
		public bool RequiresPerfectTile = false;
        
		[Header("Debug调试")]

		/// the weapon currently equipped by the Character
		[MMReadOnly]
		[Tooltip("角色当前装备的武器")]
		public Weapon CurrentWeapon;

        /// 这个CharacterHandleWeapon的ID /索引。这将被用来决定什么处理武器能力应该装备武器。
        ///  如果你创造了更多的“处理武器”技能，请确保覆盖并增加它
        public virtual int HandleWeaponID { get { return 1; } }

        /// 当武器被使用时更新的动画
        public virtual Animator CharacterAnimator { get; set; }
        /// 武器的武器瞄准组件，如果有的话
        public virtual WeaponAim WeaponAimComponent { get { return _weaponAim; } }

		public delegate void OnWeaponChangeDelegate();
        /// 一个你可以钩到的委托，用来通知武器的变化
        public OnWeaponChangeDelegate OnWeaponChange;

		protected float _fireTimer = 0f;
		protected float _secondaryHorizontalMovement;
		protected float _secondaryVerticalMovement;
		protected WeaponAim _weaponAim;
		protected ProjectileWeapon _projectileWeapon;
		protected WeaponIK _weaponIK;
		protected Transform _leftHandTarget = null;
		protected Transform _rightHandTarget = null;
		protected float _bufferEndsAt = 0f;
		protected bool _buffering = false;
		protected const string _weaponEquippedAnimationParameterName = "WeaponEquipped";
		protected const string _weaponEquippedIDAnimationParameterName = "WeaponEquippedID";
		protected int _weaponEquippedAnimationParameter;
		protected int _weaponEquippedIDAnimationParameter;
		protected CharacterGridMovement _characterGridMovement;
		protected List<WeaponModel> _weaponModels;

        /// <summary>
        /// 设置武器附件
        /// </summary>
        protected override void PreInitialization()
		{
			base.PreInitialization();
            // 如果没有设置武器附件，则填充
            if (WeaponAttachment == null)
			{
				WeaponAttachment = transform;
			}
		}

		// 初始化
		protected override void Initialization()
		{
			base.Initialization();
			Setup();
		}

        /// <summary>
        /// 抓取各种组件并初始化
        /// </summary>
        public virtual void Setup()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			_characterGridMovement = _character?.FindAbility<CharacterGridMovement>();
			_weaponModels = new List<WeaponModel>();
			foreach (WeaponModel model in _character.gameObject.GetComponentsInChildren<WeaponModel>())
			{
				_weaponModels.Add(model);
			}
			CharacterAnimator = _animator;
            // 如果没有设置武器附件，则填充
            if (WeaponAttachment == null)
			{
				WeaponAttachment = transform;
			}
			if ((_animator != null) && (AutoIK))
			{
				_weaponIK = _animator.GetComponent<WeaponIK>();
			}
            // 我们设置了初始武器
            if (InitialWeapon != null)
			{
				if (CurrentWeapon != null)
				{
					if (CurrentWeapon.name != InitialWeapon.name)
					{
						ChangeWeapon(InitialWeapon, InitialWeapon.WeaponName, false);    
					}
				}
				else
				{
					ChangeWeapon(InitialWeapon, InitialWeapon.WeaponName, false);    
				}
			}
		}

        /// <summary>
        /// 每一帧我们都会检查是否需要更新弹药显示
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			HandleCharacterState();
			HandleFeedbacks();
			UpdateAmmoDisplay();
			HandleBuffer();
		}

        /// <summary>
        /// 检查角色状态并停止射击，如果不是在正常状态
        /// </summary>
        protected virtual void HandleCharacterState()
		{
			if (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				ShootStop();
			}
		}

        /// <summary>
        /// 必要时触发武器使用反馈
        /// </summary>
        protected virtual void HandleFeedbacks()
		{
			if (CurrentWeapon != null)
			{
				if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse)
				{
					WeaponUseFeedback?.PlayFeedbacks();
				}
			}
		}

        /// <summary>
        /// 获取输入并根据按下的内容触发方法
        /// </summary>
        protected override void HandleInput()
		{
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			    || (CurrentWeapon == null))
			{
				return;
			}

			bool inputAuthorized = true;
			if (CurrentWeapon != null)
			{
				inputAuthorized = CurrentWeapon.InputAuthorized;
			}

			if (ForceAlwaysShoot)
			{
				ShootStart();
			}
			
			if (inputAuthorized && ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonDown)))
			{
				ShootStart();
			}

			bool buttonPressed =
				(_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed) ||
				(_inputManager.ShootAxis == MMInput.ButtonStates.ButtonPressed); 
                
			if (inputAuthorized && ContinuousPress && (CurrentWeapon.TriggerMode == Weapon.TriggerModes.Auto) && buttonPressed)
			{
				ShootStart();
			}
			
			if (inputAuthorized && ContinuousPress && (CurrentWeapon.IsAutoComboWeapon) && buttonPressed)
			{
				ShootStart();
			}
            
			if (_inputManager.ReloadButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				Reload();
			}

			if (inputAuthorized && ((_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.ButtonUp) || (_inputManager.ShootAxis == MMInput.ButtonStates.ButtonUp)))
			{
				ShootStop();
				CurrentWeapon.WeaponInputReleased();
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses)
			    && ((_inputManager.ShootAxis == MMInput.ButtonStates.Off) && (_inputManager.ShootButton.State.CurrentState == MMInput.ButtonStates.Off))
			    && !(UseSecondaryAxisThresholdToShoot && (_inputManager.SecondaryMovement.magnitude > _inputManager.Threshold.magnitude)))
			{
				CurrentWeapon.WeaponInputStop();
			}

			if (inputAuthorized && UseSecondaryAxisThresholdToShoot && (_inputManager.SecondaryMovement.magnitude > _inputManager.Threshold.magnitude))
			{
				ShootStart();
			}
		}

        /// <summary>
        /// 触发攻击，如果武器是闲置的，输入已被缓冲
        /// </summary>
        protected virtual void HandleBuffer()
		{
			if (CurrentWeapon == null)
			{
				return;
			}

            // 如果我们当前正在缓冲输入，如果武器现在是空闲的
            if (_buffering && (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle))
			{
				// and if our buffer is still valid, we trigger an attack
				if (Time.time < _bufferEndsAt)
				{
					ShootStart();
				}
				else
				{
					_buffering = false;
				}                
			}
		}

        /// <summary>
        /// 使角色开始射击
        /// </summary>
        public virtual void ShootStart()
		{
            // 如果在权限中启用了拍摄操作，我们继续，如果没有，我们什么都不做。如果玩家死了，我们什么也不做。
            if (!AbilityAuthorized
			    || (CurrentWeapon == null)
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}

            //  如果我们决定缓冲输入，如果武器现在正在使用
            if (BufferInput && (CurrentWeapon.WeaponState.CurrentState != Weapon.WeaponStates.WeaponIdle))
			{
                // 如果我们还没有缓冲，或者每个新输入都扩展了缓冲区，我们将缓冲状态改为true
                ExtendBuffer();
			}

			if (BufferInput && RequiresPerfectTile && (_characterGridMovement != null))            
			{
				if (!_characterGridMovement.PerfectTile)
				{
					ExtendBuffer();
					return;
				}
				else
				{
					_buffering = false;
				}
			}
			PlayAbilityStartFeedbacks();
			CurrentWeapon.WeaponInputStart();
		}

        /// <summary>
        ///如果需要，扩展缓冲区的持续时间
        /// </summary>
        protected virtual void ExtendBuffer()
		{
			if (!_buffering || NewInputExtendsBuffer)
			{
				_buffering = true;
				_bufferEndsAt = Time.time + MaximumBufferDuration;
			}
		}

        /// <summary>
        /// 使角色停止射击
        /// </summary>
        public virtual void ShootStop()
		{
            // 如果在权限中启用了拍摄操作，我们继续，如果没有，我们什么都不做
            if (!AbilityAuthorized
			    || (CurrentWeapon == null))
			{
				return;
			}

			if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponIdle)
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReload)
			    || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStart)
			    || (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponReloadStop))
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBeforeUse) && (!CurrentWeapon.DelayBeforeUseReleaseInterruption))
			{
				return;
			}

			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses) && (!CurrentWeapon.TimeBetweenUsesReleaseInterruption))
			{
				return;
			}

			if (CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponUse) 
			{
				return;
			}

			ForceStop();
		}

        /// <summary>
        /// 强制武器停止
        /// </summary>
        public virtual void ForceStop()
		{
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
			if (CurrentWeapon != null)
			{
				CurrentWeapon.TurnWeaponOff();    
			}
		}

        /// <summary>
        /// 重新装填武器
        /// </summary>
        public virtual void Reload()
		{
			if (CurrentWeapon != null)
			{
				CurrentWeapon.InitiateReloadWeapon();
			}
		}

        /// <summary>
        /// 将角色的当前武器更改为传递的参数
        /// </summary>
        /// <param name="newWeapon">The new weapon.</param>
        public virtual void ChangeWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{
            // 如果角色已经拥有武器，我们就会让它停止射击
            if (CurrentWeapon != null)
			{
				CurrentWeapon.TurnWeaponOff();
				if (!combo)
				{
					ShootStop();

					if (_weaponAim != null) { _weaponAim.RemoveReticle(); }
					if (_character._animator != null)
					{
						AnimatorControllerParameter[] parameters = _character._animator.parameters;
						foreach(AnimatorControllerParameter parameter in parameters)
						{
							if (parameter.name == CurrentWeapon.EquippedAnimationParameter)
							{
								MMAnimatorExtensions.UpdateAnimatorBool(_animator, CurrentWeapon.EquippedAnimationParameter, false);
							}
						}
					}
					Destroy(CurrentWeapon.gameObject);
				}
			}

			if (newWeapon != null)
			{
				InstantiateWeapon(newWeapon, weaponID, combo);
			}
			else
			{
				CurrentWeapon = null;
				HandleWeaponModel(null, null);
			}

			if (OnWeaponChange != null)
			{
				OnWeaponChange();
			}
		}

        /// <summary>
        /// 实例化指定的武器
        /// </summary>
        /// <param name="newWeapon"></param>
        /// <param name="weaponID"></param>
        /// <param name="combo"></param>
        protected virtual void InstantiateWeapon(Weapon newWeapon, string weaponID, bool combo = false)
		{
			if (!combo)
			{
				CurrentWeapon = (Weapon)Instantiate(newWeapon, WeaponAttachment.transform.position + newWeapon.WeaponAttachmentOffset, WeaponAttachment.transform.rotation);
			}

			CurrentWeapon.name = newWeapon.name;
			CurrentWeapon.transform.parent = WeaponAttachment.transform;
			CurrentWeapon.transform.localPosition = newWeapon.WeaponAttachmentOffset;
			CurrentWeapon.SetOwner(_character, this);
			CurrentWeapon.WeaponID = weaponID;
			CurrentWeapon.FlipWeapon();
			_weaponAim = CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();

			HandleWeaponAim();

            // 我们处理（可选）反向运动学（IK）
            HandleWeaponIK();

            // 我们处理武器模型
            HandleWeaponModel(newWeapon, weaponID, combo, CurrentWeapon);

            // 我们关掉枪的发射器。
            CurrentWeapon.Initialization();
			CurrentWeapon.InitializeComboWeapons();
			CurrentWeapon.InitializeAnimatorParameters();
			InitializeAnimatorParameters();
		}

        /// <summary>
        /// 如果可能的话应用目标
        /// </summary>
        protected virtual void HandleWeaponAim()
		{
			if ((_weaponAim != null) && (_weaponAim.enabled))
			{
				if (ForceWeaponAimControl)
				{
					_weaponAim.AimControl = ForcedWeaponAimControl;
				}
				_weaponAim.ApplyAim();
			}
		}

        /// <summary>
        /// 如果需要，设置IK句柄
        /// </summary>
        protected virtual void HandleWeaponIK()
		{
			if (_weaponIK != null)
			{
				_weaponIK.SetHandles(CurrentWeapon.LeftHandHandle, CurrentWeapon.RightHandHandle);
			}
			_projectileWeapon = CurrentWeapon.gameObject.MMFGetComponentNoAlloc<ProjectileWeapon>();
			if (_projectileWeapon != null)
			{
				_projectileWeapon.SetProjectileSpawnTransform(ProjectileSpawn);
			}
		}

		protected virtual void HandleWeaponModel(Weapon newWeapon, string weaponID, bool combo = false, Weapon weapon = null)
		{
			if (_weaponModels == null)
			{
				return;
			}

			bool handlesSet = false;
			
			foreach (WeaponModel model in _weaponModels)
			{
				if (model.Owner == this)
				{
					model.Hide();	
					if (model.UseIK && !handlesSet)
					{
						_weaponIK.SetHandles(null, null);
					}
				}
				
				if (model.WeaponID == weaponID)
				{
					model.Show(this);
					if (model.UseIK)
					{
						_weaponIK.SetHandles(model.LeftHandHandle, model.RightHandHandle);
						handlesSet = true;
					}
					if (weapon != null)
					{
						if (model.BindFeedbacks)
						{
							weapon.WeaponStartMMFeedback = model.WeaponStartMMFeedback;
							weapon.WeaponUsedMMFeedback = model.WeaponUsedMMFeedback;
							weapon.WeaponStopMMFeedback = model.WeaponStopMMFeedback;
							weapon.WeaponReloadMMFeedback = model.WeaponReloadMMFeedback;
							weapon.WeaponReloadNeededMMFeedback = model.WeaponReloadNeededMMFeedback;
						}
						if (model.AddAnimator)
						{
							weapon.Animators.Add(model.TargetAnimator);
						}
						if (model.OverrideWeaponUseTransform)
						{
							weapon.WeaponUseTransform = model.WeaponUseTransform;
						}
					}
				}
			}
		}

        /// <summary>
        /// 如果需要，翻转当前的武器
        /// </summary>
        public override void Flip()
		{
		}

        /// <summary>
        /// 更新弹药显示栏和文字。
        /// </summary>
        public virtual void UpdateAmmoDisplay()
		{
			if ((GUIManager.HasInstance) && (_character.CharacterType == Character.CharacterTypes.Player))
			{
				if (CurrentWeapon == null)
				{
					GUIManager.Instance.SetAmmoDisplays(false, _character.PlayerID, AmmoDisplayID);
					return;
				}

				if (!CurrentWeapon.MagazineBased && (CurrentWeapon.WeaponAmmo == null))
				{
					GUIManager.Instance.SetAmmoDisplays(false, _character.PlayerID, AmmoDisplayID);
					return;
				}

				if (CurrentWeapon.WeaponAmmo == null)
				{
					GUIManager.Instance.SetAmmoDisplays(true, _character.PlayerID, AmmoDisplayID);
					GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, 0, 0, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, _character.PlayerID, AmmoDisplayID, false);
					return;
				}
				else
				{
					GUIManager.Instance.SetAmmoDisplays(true, _character.PlayerID, AmmoDisplayID); 
					GUIManager.Instance.UpdateAmmoDisplays(CurrentWeapon.MagazineBased, CurrentWeapon.WeaponAmmo.CurrentAmmoAvailable + CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.WeaponAmmo.MaxAmmo, CurrentWeapon.CurrentAmmoLoaded, CurrentWeapon.MagazineSize, _character.PlayerID, AmmoDisplayID, true);
					return;
				}
			}
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			if (CurrentWeapon == null)
			{ return; }

			RegisterAnimatorParameter(_weaponEquippedAnimationParameterName, AnimatorControllerParameterType.Bool, out _weaponEquippedAnimationParameter);
			RegisterAnimatorParameter(_weaponEquippedIDAnimationParameterName, AnimatorControllerParameterType.Int, out _weaponEquippedIDAnimationParameter);
		}

        /// <summary>
        /// 重写此命令以向角色的动画器发送参数。这是每循环一次，由字符调用
        /// 类, 在Early, normal and Late process()后.
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _weaponEquippedAnimationParameter, (CurrentWeapon != null), _character._animatorParameters, _character.RunAnimatorSanityChecks);
			if (CurrentWeapon == null)
			{
				MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, -1, _character._animatorParameters, _character.RunAnimatorSanityChecks);
				return;
			}
			else
			{
				MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _weaponEquippedIDAnimationParameter, CurrentWeapon.WeaponAnimationID, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			}
		}

		protected override void OnHit()
		{
			base.OnHit();
			if (GettingHitInterruptsAttack && (CurrentWeapon != null))
			{
				CurrentWeapon.Interrupt();
			}
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			ShootStop();
			if (CurrentWeapon != null)
			{
				ChangeWeapon(null, "");
			}
		}

		protected override void OnRespawn()
		{
			base.OnRespawn();
			Setup();
		}
	}
}
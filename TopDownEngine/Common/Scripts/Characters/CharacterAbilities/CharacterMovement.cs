using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这种能力添加到角色中，让它能够处理x和z方向（3D）和x和y方向（2D）的地面移动（行走，可能会跑，爬行等）
    /// 动画器参数 : Speed (float), Walking (bool)
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Movement")] 
	public class CharacterMovement : CharacterAbility 
	{
        /// 角色可能的旋转模式
        public enum Movements { Free, Strict2DirectionsHorizontal, Strict2DirectionsVertical, Strict4Directions, Strict8Directions }

        /// 当前参考移动速度
        public virtual float MovementSpeed { get; set; }
        /// 如果这是真的，移动将被禁止（以及翻转）
        public virtual bool MovementForbidden { get; set; }

		[Header("Direction方向")]

		/// whether the character can move freely, in 2D only, in 4 or 8 cardinal directions
		[Tooltip("角色是否可以自由移动，仅在2D中，以4个或8个基本方向")]
		public Movements Movement = Movements.Free;

		[Header("Settings设置")]

		/// whether or not movement input is authorized at that time
		[Tooltip("是否允许在那时进行移动输入")]
		public bool InputAuthorized = true;
		/// whether or not input should be analog
		[Tooltip("是否应该使用模拟输入")]
		public bool AnalogInput = false;
		/// whether or not input should be set from another script
		[Tooltip("是否应该从另一个脚本设置输入")]
		public bool ScriptDrivenInput = false;

		[Header("Speed速度")]

		/// the speed of the character when it's walking
		[Tooltip("角色行走时的速度")]
		public float WalkSpeed = 6f;
		/// whether or not this component should set the controller's movement
		[Tooltip("这个组件是否应该设置控制器的移动")]
		public bool ShouldSetMovement = true;
		/// the speed threshold after which the character is not considered idle anymore
		[Tooltip("速度阈值，超过该阈值后角色不再被认为是空闲的")]
		public float IdleThreshold = 0.05f;

		[Header("Acceleration加速度")]

		/// the acceleration to apply to the current speed / 0f : no acceleration, instant full speed
		[Tooltip("加速度适用于当前速度/ 0f：无加速度，瞬间全速")]
		public float Acceleration = 10f;
		/// the deceleration to apply to the current speed / 0f : no deceleration, instant stop
		[Tooltip("减速适用于当前速度/ 0f：不减速，瞬间停止")]
		public float Deceleration = 10f;

		/// whether or not to interpolate movement speed
		[Tooltip("是否插值移动速度")]
		public bool InterpolateMovementSpeed = false;
		public virtual float MovementSpeedMaxMultiplier { get; set; } = float.MaxValue;
		private float _movementSpeedMultiplier;
        /// 要应用于水平运动的乘数
        public float MovementSpeedMultiplier
		{
			get => Mathf.Min(_movementSpeedMultiplier, MovementSpeedMaxMultiplier);
			set => _movementSpeedMultiplier = value;
		}
        /// 由上下文元素（移动区域等）应用于水平移动的乘数
        public Stack<float> ContextSpeedStack = new Stack<float>();
		public virtual float ContextSpeedMultiplier => ContextSpeedStack.Count > 0 ? ContextSpeedStack.Peek() : 1;

		[Header("Walk Feedback行走反馈")]
		/// the particles to trigger while walking
		[Tooltip("行走时触发的粒子")]
		public ParticleSystem[] WalkParticles;

		[Header("Touch The Ground Feedback触地反馈")]
		/// the particles to trigger when touching the ground
		[Tooltip("触地时触发的粒子")]
		public ParticleSystem[] TouchTheGroundParticles;
		/// the sfx to trigger when touching the ground
		[Tooltip("触地时触发的声音效果")]
		public AudioClip[] TouchTheGroundSfx;

		protected float _movementSpeed;
		protected float _horizontalMovement;
		protected float _verticalMovement;
		protected Vector3 _movementVector;
		protected Vector2 _currentInput = Vector2.zero;
		protected Vector2 _normalizedInput;
		protected Vector2 _lerpedInput = Vector2.zero;
		protected float _acceleration = 0f;
		protected bool _walkParticlesPlaying = false;

		protected const string _speedAnimationParameterName = "Speed";
		protected const string _walkingAnimationParameterName = "Walking";
		protected const string _idleAnimationParameterName = "Idle";
		protected int _speedAnimationParameter;
		protected int _walkingAnimationParameter;
		protected int _idleAnimationParameter;

        /// <summary>
        /// 在初始化中，我们将移动速度设置为WalkSpeed。
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization ();
			ResetAbility();
		}

        /// <summary>
        /// 重置角色移动状态和速度
        /// </summary>
        public override void ResetAbility()
        {
	        base.ResetAbility();
			MovementSpeed = WalkSpeed;
			if (ContextSpeedStack != null)
			{
				ContextSpeedStack.Clear();
			}
			if ((_movement != null) && (_movement.CurrentState != CharacterStates.MovementStates.FallingDownHole))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
			}
			MovementSpeedMultiplier = 1f;
			MovementForbidden = false;

			foreach (ParticleSystem system in TouchTheGroundParticles)
			{
				if (system != null)
				{
					system.Stop();
				}				
			}
			foreach (ParticleSystem system in WalkParticles)
			{
				if (system != null)
				{
					system.Stop();
				}				
			}
		}

        /// <summary>
        /// 3次传递中的第2次。可以把它看作Update（）
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			
			HandleFrozen();
			
			if (!AbilityAuthorized
			    || ((_condition.CurrentState != CharacterStates.CharacterConditions.Normal) && (_condition.CurrentState != CharacterStates.CharacterConditions.ControlledMovement)))
			{
				if (AbilityAuthorized)
				{
					StopAbilityUsedSfx();
				}
				return;
			}
			HandleDirection();
			HandleMovement();
			Feedbacks ();
		}

        /// <summary>
        /// 在能力周期的最开始调用，并打算被重写，查找输入和调用
        /// 满足条件时，执行方法
        /// </summary>
        protected override void HandleInput()
		{
			if (ScriptDrivenInput)
			{
				return;
			}

			if (InputAuthorized)
			{
				_horizontalMovement = _horizontalInput;
				_verticalMovement = _verticalInput;
			}
			else
			{
				_horizontalMovement = 0f;
				_verticalMovement = 0f;
			}	
		}

        /// <summary>
        /// 设置水平移动值。
        /// </summary>
        /// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
        public virtual void SetMovement(Vector2 value)
		{
			_horizontalMovement = value.x;
			_verticalMovement = value.y;
		}

        /// <summary>
        /// 设置移动的水平部分
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetHorizontalMovement(float value)
		{
			_horizontalMovement = value;
		}

        /// <summary>
        /// 设置移动的垂直部分
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetVerticalMovement(float value)
		{
			_verticalMovement = value;
		}

        /// <summary>
        /// 在指定的持续时间内应用移动倍增器
        /// </summary>
        /// <param name="movementMultiplier"></param>
        /// <param name="duration"></param>
        public virtual void ApplyMovementMultiplier(float movementMultiplier, float duration)
		{
			StartCoroutine(ApplyMovementMultiplierCo(movementMultiplier, duration));
		}

        /// <summary>
        /// 一种协同程序，用于仅在特定持续时间内应用移动倍增器
        /// </summary>
        /// <param name="movementMultiplier"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        protected virtual IEnumerator ApplyMovementMultiplierCo(float movementMultiplier, float duration)
		{
			if (_characterMovement == null)
			{
				yield break;
			}
			SetContextSpeedMultiplier(movementMultiplier);
			yield return MMCoroutine.WaitFor(duration);
			ResetContextSpeedMultiplier();
		}

        /// <summary>
        /// 堆叠一个新的上下文速度倍增器
        /// </summary>
        /// <param name="newMovementSpeedMultiplier"></param>
        public virtual void SetContextSpeedMultiplier(float newMovementSpeedMultiplier)
		{
			ContextSpeedStack.Push(newMovementSpeedMultiplier);
		}

        /// <summary>
        /// 将上下文速度乘法器反转到之前的值
        /// </summary>
        public virtual void ResetContextSpeedMultiplier()
		{
			if (ContextSpeedStack.Count <= 0)
			{
				return;
			}
			
			ContextSpeedStack.Pop();
		}

        /// <summary>
        /// 根据选择的移动模式修改玩家的输入
        /// </summary>
        protected virtual void HandleDirection()
		{
			switch (Movement)
			{
				case Movements.Free:
					// do nothing
					break;
				case Movements.Strict2DirectionsHorizontal:
					_verticalMovement = 0f;
					break;
				case Movements.Strict2DirectionsVertical:
					_horizontalMovement = 0f;
					break;
				case Movements.Strict4Directions:
					if (Mathf.Abs(_horizontalMovement) > Mathf.Abs(_verticalMovement))
					{
						_verticalMovement = 0f;
					}
					else
					{
						_horizontalMovement = 0f;
					}
					break;
				case Movements.Strict8Directions:
					_verticalMovement = Mathf.Round(_verticalMovement);
					_horizontalMovement = Mathf.Round(_horizontalMovement);
					break;
			}
		}

        /// <summary>
        /// 在Update（）中调用，处理水平移动
        /// </summary>
        protected virtual void HandleMovement()
		{
            // 如果我们不再走路，我们就停止走路的声音
            if ((_movement.CurrentState != CharacterStates.MovementStates.Walking) && _startFeedbackIsPlaying)
			{
				StopStartFeedbacks();
			}

            // 如果我们不再走路，我们就停止走路的声音
            if (_movement.CurrentState != CharacterStates.MovementStates.Walking && _abilityInProgressSfx != null)
			{
				StopAbilityUsedSfx();
			}

			if (_movement.CurrentState == CharacterStates.MovementStates.Walking && _abilityInProgressSfx == null)
			{
				PlayAbilityUsedSfx();
			}

            // 如果移动被阻止，或者角色死亡/冻结/无法移动，我们就退出游戏，什么也不做
            if ( !AbilityAuthorized
			     || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal) )
			{
				return;				
			}
            
			CheckJustGotGrounded();

			if (MovementForbidden)
			{
				_horizontalMovement = 0f;
				_verticalMovement = 0f;
			}

            // 如果角色没有接地，但目前处于空闲或行走状态，我们将其状态更改为Falling
            if (!_controller.Grounded
			    && (_condition.CurrentState == CharacterStates.CharacterConditions.Normal)
			    && (
				    (_movement.CurrentState == CharacterStates.MovementStates.Walking)
				    || (_movement.CurrentState == CharacterStates.MovementStates.Idle)
			    ))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Falling);
			}

			if (_controller.Grounded && (_movement.CurrentState == CharacterStates.MovementStates.Falling))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
			}

			if ( _controller.Grounded
			     && (_controller.CurrentMovement.magnitude > IdleThreshold)
			     && ( _movement.CurrentState == CharacterStates.MovementStates.Idle))
			{				
				_movement.ChangeState(CharacterStates.MovementStates.Walking);	
				PlayAbilityStartSfx();	
				PlayAbilityUsedSfx();
				PlayAbilityStartFeedbacks();
			}

            // 如果我们在走而不再移动，我们就会回到Idle状态
            if ((_movement.CurrentState == CharacterStates.MovementStates.Walking) 
			    && (_controller.CurrentMovement.magnitude <= IdleThreshold))
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
				PlayAbilityStopSfx();
				PlayAbilityStopFeedbacks();
			}

			if (ShouldSetMovement)
			{
				SetMovement ();	
			}
		}

        /// <summary>
        /// 描述当角色处于冻结状态时会发生什么
        /// </summary>
        protected virtual void HandleFrozen()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			if (_condition.CurrentState == CharacterStates.CharacterConditions.Frozen)
			{
				_horizontalMovement = 0f;
				_verticalMovement = 0f;
				SetMovement();
			}
		}

        /// <summary>
        /// 移动控制器
        /// </summary>
        protected virtual void SetMovement()
		{
			_movementVector = Vector3.zero;
			_currentInput = Vector2.zero;

			_currentInput.x = _horizontalMovement;
			_currentInput.y = _verticalMovement;
            
			_normalizedInput = _currentInput.normalized;

			float interpolationSpeed = 1f;
            
			if ((Acceleration == 0) || (Deceleration == 0))
			{
				_lerpedInput = AnalogInput ? _currentInput : _normalizedInput;
			}
			else
			{
				if (_normalizedInput.magnitude == 0)
				{
					_acceleration = Mathf.Lerp(_acceleration, 0f, Deceleration * Time.deltaTime);
					_lerpedInput = Vector2.Lerp(_lerpedInput, _lerpedInput * _acceleration, Time.deltaTime * Deceleration);
					interpolationSpeed = Deceleration;
				}
				else
				{
					_acceleration = Mathf.Lerp(_acceleration, 1f, Acceleration * Time.deltaTime);
					_lerpedInput = AnalogInput ? Vector2.ClampMagnitude (_currentInput, _acceleration) : Vector2.ClampMagnitude(_normalizedInput, _acceleration);
					interpolationSpeed = Acceleration;
				}
			}		
			
			_movementVector.x = _lerpedInput.x;
			_movementVector.y = 0f;
			_movementVector.z = _lerpedInput.y;

			if (InterpolateMovementSpeed)
			{
				_movementSpeed = Mathf.Lerp(_movementSpeed, MovementSpeed * ContextSpeedMultiplier * MovementSpeedMultiplier, interpolationSpeed * Time.deltaTime);
			}
			else
			{
				_movementSpeed = MovementSpeed * MovementSpeedMultiplier * ContextSpeedMultiplier;
			}

			_movementVector *= _movementSpeed;

			if (_movementVector.magnitude > MovementSpeed * ContextSpeedMultiplier * MovementSpeedMultiplier)
			{
				_movementVector = Vector3.ClampMagnitude(_movementVector, MovementSpeed);
			}

			if ((_currentInput.magnitude <= IdleThreshold) && (_controller.CurrentMovement.magnitude < IdleThreshold))
			{
				_movementVector = Vector3.zero;
			}
            
			_controller.SetMovement (_movementVector);
		}

        /// <summary>
        /// 每一帧，检查我们是否碰到地面，如果是，改变状态并触发粒子效果
        /// </summary>
        protected virtual void CheckJustGotGrounded()
		{
			// if the character just got grounded
			if (_controller.JustGotGrounded)
			{
				_movement.ChangeState(CharacterStates.MovementStates.Idle);
			}
		}

        /// <summary>
        /// 行走时播放粒子，着陆时播放粒子和声音
        /// </summary>
        protected virtual void Feedbacks ()
		{
			if (_controller.Grounded)
			{
				if (_controller.CurrentMovement.magnitude > IdleThreshold)	
				{			
					foreach (ParticleSystem system in WalkParticles)
					{				
						if (!_walkParticlesPlaying && (system != null))
						{
							system.Play();		
						}
						_walkParticlesPlaying = true;
					}	
				}
				else
				{
					foreach (ParticleSystem system in WalkParticles)
					{						
						if (_walkParticlesPlaying && (system != null))
						{
							system.Stop();		
							_walkParticlesPlaying = false;
						}
					}
				}
			}
			else
			{
				foreach (ParticleSystem system in WalkParticles)
				{						
					if (_walkParticlesPlaying && (system != null))
					{
						system.Stop();		
						_walkParticlesPlaying = false;
					}
				}
			}

			if (_controller.JustGotGrounded)
			{
				foreach (ParticleSystem system in TouchTheGroundParticles)
				{
					if (system != null)
					{
						system.Clear();
						system.Play();
					}					
				}
				foreach (AudioClip clip in TouchTheGroundSfx)
				{
					MMSoundManagerSoundPlayEvent.Trigger(clip, MMSoundManager.MMSoundManagerTracks.Sfx, this.transform.position);
				}
			}
		}

        /// <summary>
        /// 重置这个角色的速度
        /// </summary>
        public virtual void ResetSpeed()
		{
			MovementSpeed = WalkSpeed;
		}

        /// <summary>
        /// 在重生时，重置速度
        /// </summary>
        protected override void OnRespawn()
		{
			ResetSpeed();
			MovementForbidden = false;
		}

		protected override void OnDeath()
		{
			base.OnDeath();
			DisableWalkParticles();
		}

        /// <summary>
        /// 禁用所有可能正在播放的行走粒子系统
        /// </summary>
        protected virtual void DisableWalkParticles()
		{
			if (WalkParticles.Length > 0)
			{
				foreach (ParticleSystem walkParticle in WalkParticles)
				{
					if (walkParticle != null)
					{
						walkParticle.Stop();
					}
				}
			}
		}

        /// <summary>
        /// 在禁用时，我们确保关闭任何可能仍在播放的内容
        /// </summary>
        protected override void OnDisable()
		{
			base.OnDisable ();
			DisableWalkParticles();
			PlayAbilityStopSfx();
			PlayAbilityStopFeedbacks();
			StopAbilityUsedSfx();
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter (_speedAnimationParameterName, AnimatorControllerParameterType.Float, out _speedAnimationParameter);
			RegisterAnimatorParameter (_walkingAnimationParameterName, AnimatorControllerParameterType.Bool, out _walkingAnimationParameter);
			RegisterAnimatorParameter (_idleAnimationParameterName, AnimatorControllerParameterType.Bool, out _idleAnimationParameter);
		}

        /// <summary>
        /// 将当前速度和Walking状态的当前值发送给动画器
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _speedAnimationParameter, Mathf.Abs(_controller.CurrentMovement.magnitude),_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _walkingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Walking),_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Idle),_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}
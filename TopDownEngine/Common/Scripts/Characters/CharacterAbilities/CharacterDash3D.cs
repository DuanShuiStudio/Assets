using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此能力添加到3D角色中，它将能够冲刺（在指定时间内覆盖指定距离）。
    ///
    /// 动画参数：
    /// Dashing : 如果角色当前正在奔跑，则为True
    /// DashStarted : 冲刺开始时为True
    /// DashingDirectionX : 冲刺方向的x分量，标准化
    /// DashingDirectionY : 冲刺方向的y分量，标准化
    /// DashingDirectionZ : 冲刺方向的z分量，标准化
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Dash 3D")]
	public class CharacterDash3D : CharacterAbility
	{
        /// 可能的短跑模式（固定=总是相同的方向）
        public enum DashModes { Fixed, MainMovement, SecondaryMovement, MousePosition, ModelDirection, Script }
        /// 冲刺可能出现的空间，无论是世界坐标还是本地坐标
        public enum DashSpaces { World, Local }

		/// the current dash mode
		[Tooltip("当前的冲刺模式(固定：总是相同的方向，主移动：通常是你的左摇杆，次移动：通常是你的右摇杆，鼠标位置：光标的位置")]
		public DashModes DashMode = DashModes.MainMovement;

		[Header("Dash冲刺")]
        /// 冲刺应该出现的空间，可以是局部的，也可以是全局的
        public DashSpaces DashSpace = DashSpaces.World;
		/// the direction of the dash, relative to the character
		[Tooltip("相对于角色的冲刺方向")]
		public Vector3 DashDirection = Vector3.forward;
		/// the distance to cover
		[Tooltip("冲刺距离")]
		public float DashDistance = 10f;
		/// the duration of the dash
		[Tooltip("冲刺的持续时间，以秒为单位")]
		public float DashDuration = 0.5f;
		/// the curve to apply to the dash's acceleration
		[Tooltip("应用于冲刺加速度的曲线")]
		public AnimationCurve DashCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
		/// if this is true, dash will be allowed while jumping, otherwise it'll be ignored
		[Tooltip("如果为真，则跳跃时允许使用冲刺，否则将被忽略")]
		public bool AllowDashWhenJumping = false;

		[Header("Cooldown冷却")]
		/// this ability's cooldown
		[Tooltip("这个技能的冷却时间")]
		public MMCooldown Cooldown;
        
		[Header("Damage伤害")] 
		/// if this is true, this character won't receive any damage while a dash is in progress
		[Tooltip("如果这是真的，这个角色在冲刺的过程中不会受到任何伤害")]
		public bool InvincibleWhileDashing = false; 

		[Header("Feedbacks反馈")]
		/// the feedbacks to play when dashing
		[Tooltip("冲刺时播放的反馈")]
		public MMFeedbacks DashFeedback;

		protected bool _dashing;
		protected bool _dashStartedThisFrame;
		protected float _dashTimer;
		protected Vector3 _dashOrigin;
		protected Vector3 _dashDestination;
		protected Vector3 _newPosition;
		protected Vector3 _oldPosition;
		protected Vector3 _dashAngle = Vector3.zero;
		protected Vector3 _inputDirection;
		protected Vector3 _dashAnimParameterDirection;
		protected Plane _playerPlane;
		protected Camera _mainCamera;
		protected const string _dashingAnimationParameterName = "Dashing";
		protected const string _dashStartedAnimationParameterName = "DashStarted";
		protected const string _dashingDirectionXAnimationParameterName = "DashingDirectionX";
		protected const string _dashingDirectionYAnimationParameterName = "DashingDirectionY";
		protected const string _dashingDirectionZAnimationParameterName = "DashingDirectionZ";
		protected int _dashingAnimationParameter;
		protected int _dashStartedAnimationParameter;
		protected int _dashingDirectionXAnimationParameter;
		protected int _dashingDirectionYAnimationParameter;
		protected int _dashingDirectionZAnimationParameter;
		protected CharacterOrientation3D _characterOrientation3D;

        /// <summary>
        /// 在init中，我们初始化冷却时间和反馈
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_playerPlane = new Plane(Vector3.up, Vector3.zero);
			_characterOrientation3D = _character.FindAbility<CharacterOrientation3D>();
			_mainCamera = Camera.main;
			Cooldown.Initialization();
			DashFeedback?.Initialization(this.gameObject);

			if (GUIManager.HasInstance && _character.CharacterType == Character.CharacterTypes.Player)
			{
				GUIManager.Instance.SetDashBar(true, _character.PlayerID);
				UpdateDashBar();
			}
		}

        /// <summary>
        /// 监视输入并在需要时启动冲刺
        /// </summary>
        protected override void HandleInput()
		{
			base.HandleInput();
			if (!AbilityAuthorized
			    || (!Cooldown.Ready())
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}

			if (!AllowDashWhenJumping && (_movement.CurrentState == CharacterStates.MovementStates.Jumping))
			{
				return;
			}

			if (_inputManager.DashButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				DashStart();
			}
		}

		/// <summary>
		/// 启动一个冲刺
		/// </summary>
		public virtual void DashStart()
		{
			if (!Cooldown.Ready())
			{
				return;
			}
			Cooldown.Start();

			_movement.ChangeState(CharacterStates.MovementStates.Dashing);
			_dashing = true;
			_dashTimer = 0f;
			_dashOrigin = this.transform.position;
			_controller.FreeMovement = false;
			_controller3D.DetachFromMovingPlatform();
			DashFeedback?.PlayFeedbacks(this.transform.position);
			PlayAbilityStartFeedbacks();
			_dashStartedThisFrame = true;

			if (InvincibleWhileDashing)
			{
				_health.DamageDisabled();
			}

			HandleDashMode();
		}

		protected virtual void HandleDashMode()
		{
			float angle  = 0f;
			switch (DashMode)
			{
				case DashModes.MainMovement:
					angle = Vector3.SignedAngle(this.transform.forward, _controller.CurrentDirection.normalized, Vector3.up);
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					_dashAngle.y = angle;
					_dashDestination = MMMaths.RotatePointAroundPivot(_dashDestination, this.transform.position, _dashAngle);
					break;

				case DashModes.Fixed:
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					break;

				case DashModes.SecondaryMovement:
					_inputDirection = _character.LinkedInputManager.SecondaryMovement;
					_inputDirection.z = _inputDirection.y;
					_inputDirection.y = 0;

					angle = Vector3.SignedAngle(this.transform.forward, _inputDirection.normalized, Vector3.up);
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					_dashAngle.y = angle;
					_dashDestination = MMMaths.RotatePointAroundPivot(_dashDestination, this.transform.position, _dashAngle);

					_controller.CurrentDirection = (_dashDestination - this.transform.position).normalized;
					break;
				
				case DashModes.ModelDirection:
					_dashDestination = this.transform.position + _characterOrientation3D.ModelDirection.normalized * DashDistance;
					break;

				case DashModes.MousePosition:
					Ray ray = _mainCamera.ScreenPointToRay(InputManager.Instance.MousePosition);
					Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
					float distance;
					_playerPlane.SetNormalAndPosition(_playerPlane.normal, this.transform.position);
					if (_playerPlane.Raycast(ray, out distance))
					{
						_inputDirection = ray.GetPoint(distance);
					}

					angle = Vector3.SignedAngle(this.transform.forward, (_inputDirection - this.transform.position).normalized, Vector3.up);
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					_dashAngle.y = angle;
					_dashDestination = MMMaths.RotatePointAroundPivot(_dashDestination, this.transform.position, _dashAngle);

					_controller.CurrentDirection = (_dashDestination - this.transform.position).normalized;
					break;
				
				case DashModes.Script:
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					break;
			}
		}

		/// <summary>
		/// 结束冲刺
		/// </summary>
		public virtual void DashStop()
		{
			Cooldown.Stop();
			_movement.ChangeState(CharacterStates.MovementStates.Idle);
			_dashing = false;
			_controller.FreeMovement = true;
			DashFeedback?.StopFeedbacks();
			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
			if (InvincibleWhileDashing)
			{
				_health.DamageEnabled();
			}
		}

        /// <summary>
        /// 在处理能力上，如果我们正在奔跑，我们就会移动我们的角色
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			Cooldown.Update();
			UpdateDashBar();

			if (_dashing)
			{
				if (_dashTimer < DashDuration)
				{
					_dashAnimParameterDirection = (_dashDestination - _dashOrigin).normalized;
					if (DashSpace == DashSpaces.World)
					{
						_newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, DashCurve.Evaluate(_dashTimer / DashDuration));	
						_dashTimer += Time.deltaTime;
						_controller.MovePosition(_newPosition);
					}
					else
					{
						_oldPosition = _dashTimer == 0 ? _dashOrigin : _newPosition;
						_newPosition = Vector3.Lerp(_dashOrigin, _dashDestination, DashCurve.Evaluate(_dashTimer / DashDuration));
						_dashTimer += Time.deltaTime;
						_controller.MovePosition(this.transform.position + _newPosition - _oldPosition);
					}
				}
				else
				{
					DashStop();                   
				}
			}
		}

        /// <summary>
        /// 更新冲刺线的GUI
        /// </summary>
        protected virtual void UpdateDashBar()
		{
			if ((GUIManager.HasInstance) && (_character.CharacterType == Character.CharacterTypes.Player))
			{
				GUIManager.Instance.UpdateDashBars(Cooldown.CurrentDurationLeft, 0f, Cooldown.ConsumptionDuration, _character.PlayerID);
			}
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_dashingAnimationParameterName, AnimatorControllerParameterType.Bool, out _dashingAnimationParameter);
			RegisterAnimatorParameter(_dashStartedAnimationParameterName, AnimatorControllerParameterType.Bool, out _dashStartedAnimationParameter);
			RegisterAnimatorParameter(_dashingDirectionXAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionXAnimationParameter);
			RegisterAnimatorParameter(_dashingDirectionYAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionYAnimationParameter);
			RegisterAnimatorParameter(_dashingDirectionZAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionZAnimationParameter);
		}

        /// <summary>
        /// 在每个循环结束时，我们将运行状态发送给角色的动画师
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _dashingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Dashing), _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _dashStartedAnimationParameter, _dashStartedThisFrame, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionXAnimationParameter, _dashAnimParameterDirection.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionYAnimationParameter, _dashAnimParameterDirection.y, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionZAnimationParameter, _dashAnimParameterDirection.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);

			_dashStartedThisFrame = false;
		}
	}
}
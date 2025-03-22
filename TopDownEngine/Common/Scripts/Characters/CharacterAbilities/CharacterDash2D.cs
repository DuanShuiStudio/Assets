using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using MoreMountains.InventoryEngine;
using MoreMountains.Feedbacks;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这种能力添加到角色中，它将能够在2D中冲刺，在特定的持续时间内覆盖特定的距离
    ///
    /// 动画参数：
    /// Dashing : 如果角色当前正在奔跑，则为True
    /// DashingDirectionX : 冲刺方向的x分量，标准化
    /// DashingDirectionY :冲刺方向的y分量，标准化
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Dash 2D")]
	public class CharacterDash2D : CharacterAbility 
	{
        /// 可能的短跑模式（固定：总是相同的方向）
        public enum DashModes { Fixed, MainMovement, SecondaryMovement, MousePosition, Script }
        /// 冲刺可能出现的空间，无论是世界坐标还是本地坐标
        public enum DashSpaces { World, Local }

		/// the dash mode to apply the dash in
		[Tooltip("应用冲刺的冲刺模式")]
		public DashModes DashMode = DashModes.MainMovement;

		[Header("Dash冲刺")]
        /// 猛冲应该出现的空间，可以是局部的，也可以是全局的
        public DashSpaces DashSpace = DashSpaces.World;
		/// the dash direction
		[Tooltip("冲刺方向")]
		public Vector3 DashDirection = Vector3.forward;
		/// the distance the dash should last for
		[Tooltip("冲刺应该持续的距离")]
		public float DashDistance = 6f;
		/// the duration of the dash, in seconds
		[Tooltip("冲刺的持续时间，以秒为单位")]
		public float DashDuration = 0.2f;
		/// the animation curve to apply to the dash acceleration
		[Tooltip("应用于冲刺加速的动画曲线")]
		public AnimationCurve DashCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        
		[Header("Cooldown冷却")]

		/// this ability's cooldown
		[Tooltip("这个技能的冷却时间")]
		public MMCooldown Cooldown;
        
        
		[Header("Damage伤害")] 
		/// if this is true, this character won't receive any damage while a dash is in progress
		[Tooltip("如果这是真的，这个角色在冲刺的过程中不会受到任何伤害")]
		public bool InvincibleWhileDashing = false; 

		[Header("Feedback反馈")]
		/// the feedbacks to play when dashing
		[Tooltip("冲刺时播放的反馈")]
		public MMFeedbacks DashFeedback;

		protected bool _dashing;
		protected float _dashTimer;
		protected Vector3 _dashOrigin;
		protected Vector3 _dashDestination;
		protected Vector3 _newPosition;
		protected Vector3 _oldPosition;
		protected  Vector3 _dashAnimParameterDirection;
		protected Vector3 _dashAngle = Vector3.zero;
		protected Vector3 _inputPosition;
		protected Camera _mainCamera;
		protected const string _dashingAnimationParameterName = "Dashing";
		protected const string _dashingDirectionXAnimationParameterName = "DashingDirectionX";
		protected const string _dashingDirectionYAnimationParameterName = "DashingDirectionY";
		protected int _dashingAnimationParameter;
		protected int _dashingDirectionXAnimationParameter;
		protected int _dashingDirectionYAnimationParameter;

        /// <summary>
        /// 在init中，我们停止我们的粒子，并初始化我们的冲刺线
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization ();
			Cooldown.Initialization();

			_mainCamera = Camera.main;

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
			if (_inputManager.DashButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				DashStart();
			}
		}

		/// <summary>
		/// 初始化冲刺
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
			DashFeedback?.PlayFeedbacks(this.transform.position);
			PlayAbilityStartFeedbacks();
            
			if (InvincibleWhileDashing)
			{
				_health.DamageDisabled();
			}

			HandleDashMode();
		}

		protected virtual void HandleDashMode()
		{
			switch (DashMode)
			{
				case DashModes.MainMovement:
					_dashDestination = this.transform.position + _controller.CurrentDirection.normalized * DashDistance;
					break;

				case DashModes.Fixed:
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					break;

				case DashModes.SecondaryMovement:
					_dashDestination = this.transform.position + (Vector3)_character.LinkedInputManager.SecondaryMovement.normalized * DashDistance;
					break;

				case DashModes.MousePosition:
					_inputPosition = _mainCamera.ScreenToWorldPoint(InputManager.Instance.MousePosition);
					_inputPosition.z = this.transform.position.z;
					_dashDestination = this.transform.position + (_inputPosition - this.transform.position).normalized * DashDistance;
					break;
				
				case DashModes.Script:
					_dashDestination = this.transform.position + DashDirection.normalized * DashDistance;
					break;
			}  
		}

		/// <summary>
		/// 停止冲刺
		/// </summary>
		public virtual void DashStop()
		{
			DashFeedback?.StopFeedbacks(this.transform.position);

			StopStartFeedbacks();
			PlayAbilityStopFeedbacks();
            
			if (InvincibleWhileDashing)
			{
				_health.DamageEnabled();
			}

			_movement.ChangeState(CharacterStates.MovementStates.Idle);
			_dashing = false;
			_controller.FreeMovement = true;
		}

        /// <summary>
        /// 更新时，按需移动角色
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility ();
			Cooldown.Update();
			UpdateDashBar();

			if (_dashing)
			{
				if (_dashTimer < DashDuration)
				{
					_dashAnimParameterDirection = (_dashDestination - _dashOrigin).normalized;
					if (DashSpace == DashSpaces.World)
					{
						_newPosition = Vector3.Lerp (_dashOrigin, _dashDestination, DashCurve.Evaluate (_dashTimer/DashDuration));
						_dashTimer += Time.deltaTime;
						_controller.MovePosition (_newPosition);
					}
					else
					{
						_oldPosition = _dashTimer == 0 ? _dashOrigin : _newPosition;
						_newPosition = Vector3.Lerp (_dashOrigin, _dashDestination, DashCurve.Evaluate (_dashTimer/DashDuration));
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
			RegisterAnimatorParameter (_dashingAnimationParameterName, AnimatorControllerParameterType.Bool, out _dashingAnimationParameter);
			RegisterAnimatorParameter(_dashingDirectionXAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionXAnimationParameter);
			RegisterAnimatorParameter(_dashingDirectionYAnimationParameterName, AnimatorControllerParameterType.Float, out _dashingDirectionYAnimationParameter);
		}

        /// <summary>
        /// 在每个循环结束时，我们将运行状态发送给角色的动画师
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _dashingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Dashing),_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionXAnimationParameter, _dashAnimParameterDirection.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _dashingDirectionYAnimationParameter, _dashAnimParameterDirection.y, _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}
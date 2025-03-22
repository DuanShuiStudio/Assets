using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此能力添加到角色中，它将能够旋转以面对移动方向或武器的旋转
    /// </summary>
    [MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Orientation 3D")]
	public class CharacterOrientation3D : CharacterAbility
	{
        /// 可能的旋转模式
        public enum RotationModes { None, MovementDirection, WeaponDirection, Both }
        /// 可能的旋转速度
        public enum RotationSpeeds { Instant, Smooth, SmoothAbsolute }

		[Header("Rotation Mode旋转模式")]

 
        [Tooltip("角色是否应该面对移动方向，武器方向，或者两者都面对，或者什么都不面对")]
		public RotationModes RotationMode = RotationModes.None;
        
        [Tooltip("如果为false，则不会发生旋转")]
		public bool CharacterRotationAuthorized = true;

		[Header("Movement Direction")]

        /// 如果这是真的，我们就把模型朝这个方向旋转
        [Tooltip("如果这是真的，我们就把模型朝这个方向旋转")]
		public bool ShouldRotateToFaceMovementDirection = true;

		[MMCondition("ShouldRotateToFaceMovementDirection", true)]
		[Tooltip("当前旋转方式")]
		public RotationSpeeds MovementRotationSpeed = RotationSpeeds.Instant;
		/// the object we want to rotate towards direction. If left empty, we'll use the Character's model
		[MMCondition("ShouldRotateToFaceMovementDirection", true)]
		[Tooltip("我们想要旋转的物体的方向。如果留空，我们将使用角色的模型")]
		public GameObject MovementRotatingModel;
		/// the speed at which to rotate towards direction (smooth and absolute only)
		[MMCondition("ShouldRotateToFaceMovementDirection", true)]
		[Tooltip("向某一方向旋转的速度（仅为平稳和绝对）")]
		public float RotateToFaceMovementDirectionSpeed = 10f;
		/// the threshold after which we start rotating (absolute mode only)
		[MMCondition("ShouldRotateToFaceMovementDirection", true)]
		[Tooltip("我们开始旋转的阈值（仅限绝对模式）")]
		public float AbsoluteThresholdMovement = 0.5f;
		/// the direction of the model
		[MMReadOnly]
		[Tooltip("模型的方向")]
		public Vector3 ModelDirection;
		/// the direction of the model in angle values
		[MMReadOnly]
		[Tooltip("角度值中模型的方向")]
		public Vector3 ModelAngles;

		[Header("Weapon Direction武器方向")]

		/// If this is true, we'll rotate our model towards the weapon's direction
		[Tooltip("如果这是真的，我们将把模型旋转到武器的方向")]
		public bool ShouldRotateToFaceWeaponDirection = true;
		/// the current rotation mode
		[MMCondition("ShouldRotateToFaceWeaponDirection", true)]
		[Tooltip("当前旋转方式")]
		public RotationSpeeds WeaponRotationSpeed = RotationSpeeds.Instant;
		/// the object we want to rotate towards direction. If left empty, we'll use the Character's model
		[MMCondition("ShouldRotateToFaceWeaponDirection", true)]
		[Tooltip("我们想要旋转的物体的方向。如果留空，我们将使用角色的模型")]
		public GameObject WeaponRotatingModel;
		/// the speed at which to rotate towards direction (smooth and absolute only)
		[MMCondition("ShouldRotateToFaceWeaponDirection", true)]
		[Tooltip("向某一方向旋转的速度（仅为平稳和绝对）")]
		public float RotateToFaceWeaponDirectionSpeed = 10f;
		/// the threshold after which we start rotating (absolute mode only)
		[MMCondition("ShouldRotateToFaceWeaponDirection", true)]
		[Tooltip("我们开始旋转的阈值（仅限绝对模式）")]
		public float AbsoluteThresholdWeapon = 0.5f;
		/// the threshold after which we start rotating (absolute mode only)
		[MMCondition("ShouldRotateToFaceWeaponDirection", true)]
		[Tooltip("我们开始旋转的阈值（仅限绝对模式）")]
		public bool LockVerticalRotation = true;

		[Header("Animation动画")]

		/// the speed at which the instant rotation animation parameter float resets to 0
		[Tooltip("即时旋转动画参数float重置为0的速度")]
		public float RotationSpeedResetSpeed = 2f;
		/// the speed at which the YRotationOffsetSmoothed should lerp
		[Tooltip("YRotationOffsetSmoothed应该运行的速度")]
		public float RotationOffsetSmoothSpeed = 1f;

		[Header("Forced Rotation强制轮换制度")]

		/// whether the character is being applied a forced rotation
		[Tooltip("角色是否被强制旋转")]
		public bool ForcedRotation = false;
		/// the forced rotation applied by an external script
		[MMCondition("ForcedRotation", true)]
		[Tooltip("由外部脚本应用的强制旋转")]
		public Vector3 ForcedRotationDirection;

		public virtual Vector3 RelativeSpeed { get { return _relativeSpeed; } }
		public virtual Vector3 RelativeSpeedNormalized { get { return _relativeSpeedNormalized; } }
		public virtual float RotationSpeed { get { return _rotationSpeed; } }
		public virtual Vector3 CurrentDirection { get { return _currentDirection; } }

		protected CharacterHandleWeapon _characterHandleWeapon;
		protected CharacterRun _characterRun;
		protected Vector3 _lastRegisteredVelocity;
		protected Vector3 _rotationDirection;
		protected Vector3 _lastMovement = Vector3.zero;
		protected Vector3 _lastAim = Vector3.zero;
		protected Vector3 _relativeSpeed;
		protected Vector3 _remappedSpeed = Vector3.zero;
		protected Vector3 _relativeMaximum;
		protected Vector3 _relativeSpeedNormalized;
		protected bool _secondaryMovementTriggered = false;
		protected Quaternion _tmpRotation;
		protected Quaternion _newMovementQuaternion;
		protected Quaternion _newWeaponQuaternion;
		protected bool _shouldRotateTowardsWeapon;
		protected float _rotationSpeed;
		protected float _modelAnglesYLastFrame;
		protected float _yRotationOffset;
		protected float _yRotationOffsetSmoothed;
		protected Vector3 _currentDirection;
		protected Vector3 _weaponRotationDirection;
		protected Vector3 _positionLastFrame;
		protected Vector3 _newSpeed;
		protected bool _controllerIsNull;
		protected const string _relativeForwardSpeedAnimationParameterName = "RelativeForwardSpeed";
		protected const string _relativeLateralSpeedAnimationParameterName = "RelativeLateralSpeed";
		protected const string _remappedForwardSpeedAnimationParameterName = "RemappedForwardSpeedNormalized";
		protected const string _remappedLateralSpeedAnimationParameterName = "RemappedLateralSpeedNormalized";
		protected const string _relativeForwardSpeedNormalizedAnimationParameterName = "RelativeForwardSpeedNormalized";
		protected const string _relativeLateralSpeedNormalizedAnimationParameterName = "RelativeLateralSpeedNormalized";
		protected const string _remappedSpeedNormalizedAnimationParameterName = "RemappedSpeedNormalized";
		protected const string _rotationSpeeddAnimationParameterName = "YRotationSpeed";
		protected const string _yRotationOffsetAnimationParameterName = "YRotationOffset";
		protected const string _yRotationOffsetSmoothedAnimationParameterName = "YRotationOffsetSmoothed";
		protected int _relativeForwardSpeedAnimationParameter;
		protected int _relativeLateralSpeedAnimationParameter;
		protected int _remappedForwardSpeedAnimationParameter;
		protected int _remappedLateralSpeedAnimationParameter;
		protected int _relativeForwardSpeedNormalizedAnimationParameter;
		protected int _relativeLateralSpeedNormalizedAnimationParameter;
		protected int _remappedSpeedNormalizedAnimationParameter;
		protected int _rotationSpeeddAnimationParameter;
		protected int _yRotationOffsetAnimationParameter;
		protected int _yRotationOffsetSmoothedAnimationParameter;

        /// <summary>
        /// 在init上，如果需要，我们抓取模型
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();

			if ((_model == null) && (MovementRotatingModel == null) && (WeaponRotatingModel == null))
			{
				Debug.LogError("CharacterOrientation3D on "+this.name+ " : 你需要在你的角色组件上设置一个CharacterModel，和/或在你的CharacterOrientation3D检查器上指定MovementRotatingModel和WeaponRotatingModel。查看文档以了解更多相关信息。");
			}

			if (MovementRotatingModel == null)
			{
				MovementRotatingModel = _model;
			}
			_characterRun = _character?.FindAbility<CharacterRun>();
			_characterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();
			if (WeaponRotatingModel == null)
			{
				WeaponRotatingModel = _model;
			}
			_controllerIsNull = _controller == null;
		}

        /// <summary>
        /// 每一帧我们都朝这个方向旋转
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();

			if (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				return;
			}

			if ((MovementRotatingModel == null) && (WeaponRotatingModel == null))
			{
				return;
			}

			if (!AbilityAuthorized)
			{
				return;
			}

			if (GameManager.Instance.Paused)
			{
				return;
			}
			if (CharacterRotationAuthorized)
			{
				RotateToFaceMovementDirection();
				RotateToFaceWeaponDirection();
				RotateModel();
			}
		}


		protected virtual void FixedUpdate()
		{
			ComputeRelativeSpeeds();
		}


        /// <summary>
        /// 旋转玩家模型以面对当前方向
        /// </summary>
        protected virtual void RotateToFaceMovementDirection()
		{
            // 如果我们不应该面对我们的方向，我们什么也不做，然后退出
            if (!ShouldRotateToFaceMovementDirection) { return; }
			if ((RotationMode != RotationModes.MovementDirection) && (RotationMode != RotationModes.Both)) { return; }

			_currentDirection = ForcedRotation || _controllerIsNull ? ForcedRotationDirection : _controller.CurrentDirection;

            // 如果旋转模式是即时的，我们只需旋转面对我们的方向
            if (MovementRotationSpeed == RotationSpeeds.Instant)
			{
				if (_currentDirection != Vector3.zero)
				{
					_newMovementQuaternion = Quaternion.LookRotation(_currentDirection);
				}
			}

            // 如果旋转模式是平滑的，我们就朝着我们的方向旋转
            if (MovementRotationSpeed == RotationSpeeds.Smooth)
			{
				if (_currentDirection != Vector3.zero)
				{
					_tmpRotation = Quaternion.LookRotation(_currentDirection);
					_newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceMovementDirectionSpeed);
				}
			}

            // 如果旋转模式是平滑的，即使输入已经释放，我们也会朝着我们的方向旋转
            if (MovementRotationSpeed == RotationSpeeds.SmoothAbsolute)
			{
				if (_currentDirection.normalized.magnitude >= AbsoluteThresholdMovement)
				{
					_lastMovement = _currentDirection;
				}
				if (_lastMovement != Vector3.zero)
				{
					_tmpRotation = Quaternion.LookRotation(_lastMovement);
					_newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceMovementDirectionSpeed);
				}
			}
            
			ModelDirection = MovementRotatingModel.transform.forward.normalized;
			ModelAngles = MovementRotatingModel.transform.eulerAngles;
		}

        /// <summary>
        /// 旋转角色，使其面对武器的方向
        /// </summary>
        protected virtual void RotateToFaceWeaponDirection()
		{
			_newWeaponQuaternion = Quaternion.identity;
			_weaponRotationDirection = Vector3.zero;
			_shouldRotateTowardsWeapon = false;

            // 如果我们不应该面对我们的方向，我们什么也不做，然后退出
            if (!ShouldRotateToFaceWeaponDirection) { return; }
			if ((RotationMode != RotationModes.WeaponDirection) && (RotationMode != RotationModes.Both)) { return; }
			if (_characterHandleWeapon == null) { return; }
			if (_characterHandleWeapon.WeaponAimComponent == null) { return; }

			_shouldRotateTowardsWeapon = true;

			_rotationDirection = _characterHandleWeapon.WeaponAimComponent.CurrentAim.normalized;

			if (LockVerticalRotation)
			{
				_rotationDirection.y = 0;
			}

			_weaponRotationDirection = _rotationDirection;

			MMDebug.DebugDrawArrow(this.transform.position, _rotationDirection, Color.red);

            // 如果旋转模式是即时的，我们只需旋转面对我们的方向
            if (WeaponRotationSpeed == RotationSpeeds.Instant)
			{
				if (_rotationDirection != Vector3.zero)
				{
					_newWeaponQuaternion = Quaternion.LookRotation(_rotationDirection);
				}
			}

            // 如果旋转模式是平滑的，我们就朝着我们的方向旋转
            if (WeaponRotationSpeed == RotationSpeeds.Smooth)
			{
				if (_rotationDirection != Vector3.zero)
				{
					_tmpRotation = Quaternion.LookRotation(_rotationDirection);
					_newWeaponQuaternion = Quaternion.Slerp(WeaponRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceWeaponDirectionSpeed);
				}
			}

            // 如果旋转模式是平滑的，即使输入已经释放，我们也会朝着我们的方向旋转
            if (WeaponRotationSpeed == RotationSpeeds.SmoothAbsolute)
			{
				if (_rotationDirection.normalized.magnitude >= AbsoluteThresholdWeapon)
				{
					_lastMovement = _rotationDirection;
				}
				if (_lastMovement != Vector3.zero)
				{
					_tmpRotation = Quaternion.LookRotation(_lastMovement);
					_newWeaponQuaternion = Quaternion.Slerp(WeaponRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceWeaponDirectionSpeed);
				}
			}
		}

        /// <summary>
        /// 根据需要旋转模型
        /// </summary>
        protected virtual void RotateModel()
		{
			MovementRotatingModel.transform.rotation = _newMovementQuaternion;

			if (_shouldRotateTowardsWeapon && (_weaponRotationDirection != Vector3.zero))
			{
				WeaponRotatingModel.transform.rotation = _newWeaponQuaternion;
			}
		}

        /// <summary>
        /// 计算相对速度
        /// </summary>
        protected virtual void ComputeRelativeSpeeds()
		{
			if ((MovementRotatingModel == null) && (WeaponRotatingModel == null))
			{
				return;
			}
            
			if (Time.deltaTime != 0f)
			{
				_newSpeed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
			}

            // 相对速度
            if ((_characterHandleWeapon == null) || (_characterHandleWeapon.CurrentWeapon == null))
			{
				_relativeSpeed = MovementRotatingModel.transform.InverseTransformVector(_newSpeed);
			}
			else
			{
				_relativeSpeed = WeaponRotatingModel.transform.InverseTransformVector(_newSpeed);
			}

            //重新映射速度

            float maxSpeed = 0f;
			if (_characterMovement != null)
			{
				maxSpeed = _characterMovement.WalkSpeed;
			}
			if (_characterRun != null)
			{
				maxSpeed = _characterRun.RunSpeed;
			}
            
			_relativeMaximum = _character.transform.TransformVector(Vector3.one);
			
			_remappedSpeed.x = MMMaths.Remap(_relativeSpeed.x, 0f, maxSpeed, 0f, _relativeMaximum.x);
			_remappedSpeed.y = MMMaths.Remap(_relativeSpeed.y, 0f, maxSpeed, 0f, _relativeMaximum.y);
			_remappedSpeed.z = MMMaths.Remap(_relativeSpeed.z, 0f, maxSpeed, 0f, _relativeMaximum.z);

            // 归一化相对速度
            _relativeSpeedNormalized = _relativeSpeed.normalized;
			_yRotationOffset = _modelAnglesYLastFrame - ModelAngles.y;

			_yRotationOffsetSmoothed = Mathf.Lerp(_yRotationOffsetSmoothed, _yRotationOffset, RotationOffsetSmoothSpeed * Time.deltaTime);

            // 旋转速度
            if (Mathf.Abs(_modelAnglesYLastFrame - ModelAngles.y) > 1f)
			{
				_rotationSpeed = Mathf.Abs(_modelAnglesYLastFrame - ModelAngles.y);
			}
			else
			{
				_rotationSpeed -= Time.time * RotationSpeedResetSpeed;
			}
			if (_rotationSpeed <= 0f)
			{
				_rotationSpeed = 0f;
			}

			_modelAnglesYLastFrame = ModelAngles.y;
			_positionLastFrame = this.transform.position;
		}

        /// <summary>
        /// 强迫角色的模型朝向指定的方向
        /// </summary>
        /// <param name="direction"></param>
        public virtual void Face(Character.FacingDirections direction)
		{
			switch (direction)
			{
				case Character.FacingDirections.East:
					_newMovementQuaternion = Quaternion.LookRotation(Vector3.right);
					break;
				case Character.FacingDirections.North:
					_newMovementQuaternion = Quaternion.LookRotation(Vector3.forward);
					break;
				case Character.FacingDirections.South:
					_newMovementQuaternion = Quaternion.LookRotation(Vector3.back);
					break;
				case Character.FacingDirections.West:
					_newMovementQuaternion = Quaternion.LookRotation(Vector3.left);
					break;
			}
		}

        /// <summary>
        /// 强制角色的模型朝向指定的角度
        /// </summary>
        /// <param name="angles"></param>
        public virtual void Face(Vector3 angles)
		{
			_newMovementQuaternion = Quaternion.LookRotation(Quaternion.Euler(angles) * Vector3.forward);
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_rotationSpeeddAnimationParameterName, AnimatorControllerParameterType.Float, out _rotationSpeeddAnimationParameter);
			RegisterAnimatorParameter(_relativeForwardSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeForwardSpeedAnimationParameter);
			RegisterAnimatorParameter(_relativeLateralSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeLateralSpeedAnimationParameter);
			RegisterAnimatorParameter(_remappedForwardSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _remappedForwardSpeedAnimationParameter);
			RegisterAnimatorParameter(_remappedLateralSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _remappedLateralSpeedAnimationParameter);
			RegisterAnimatorParameter(_relativeForwardSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeForwardSpeedNormalizedAnimationParameter);
			RegisterAnimatorParameter(_relativeLateralSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeLateralSpeedNormalizedAnimationParameter);
			RegisterAnimatorParameter(_remappedSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float, out _remappedSpeedNormalizedAnimationParameter);
			RegisterAnimatorParameter(_yRotationOffsetAnimationParameterName, AnimatorControllerParameterType.Float, out _yRotationOffsetAnimationParameter);
			RegisterAnimatorParameter(_yRotationOffsetSmoothedAnimationParameterName, AnimatorControllerParameterType.Float, out _yRotationOffsetSmoothedAnimationParameter);
		}

        /// <summary>
        /// 将当前速度和Walking状态的当前值发送给动画器
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _rotationSpeeddAnimationParameter, _rotationSpeed, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeForwardSpeedAnimationParameter, _relativeSpeed.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeLateralSpeedAnimationParameter, _relativeSpeed.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _remappedForwardSpeedAnimationParameter, _remappedSpeed.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _remappedLateralSpeedAnimationParameter, _remappedSpeed.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeForwardSpeedNormalizedAnimationParameter, _relativeSpeedNormalized.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeLateralSpeedNormalizedAnimationParameter, _relativeSpeedNormalized.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _remappedSpeedNormalizedAnimationParameter, _remappedSpeed.magnitude, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _yRotationOffsetAnimationParameter, _yRotationOffset, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _yRotationOffsetSmoothedAnimationParameter, _yRotationOffsetSmoothed, _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}
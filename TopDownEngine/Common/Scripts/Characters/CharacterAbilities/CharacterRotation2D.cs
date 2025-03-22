using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这种能力添加到角色中，它将旋转或翻转以面对移动方向或武器方向，或两者都有，或没有
    /// 只将此能力添加到2D角色中
    /// </summary>
    [MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Rotation 2D")]
	public class CharacterRotation2D : CharacterAbility
	{
        /// 可能的旋转模式
        public enum RotationModes { None, MovementDirection, WeaponDirection, Both }
        /// 可能的旋转速度
        public enum RotationSpeeds { Instant, Smooth, SmoothAbsolute }

		[Header("Rotation Mode旋转模式")]

		/// whether the character should face movement direction, weapon direction, or both, or none
		[Tooltip("角色是否应该面向移动方向、武器方向，或者两者都面向，或者都不面向")]
		public RotationModes RotationMode = RotationModes.None;

		/// whether the character is being applied a forced rotation
		[Tooltip("角色是否正在被施加强制旋转")]
		public bool ForcedRotation = false;
		/// the forced rotation applied by an external script
		[MMCondition("ForcedRotation", true)]
		[Tooltip("由外部脚本施加的强制旋转")]
		public Vector3 ForcedRotationDirection;
		/// whether or not the forced rotation should impact weapon direction
		[MMCondition("ForcedRotation", true)]
		[Tooltip("是否应将强制旋转应用于武器方向")]
		public bool ForcedRotationImpactsWeapon;

		[Header("Movement Direction运动的方向")]

		/// If this is true, we'll rotate our model towards the direction
		[Tooltip("如果这个条件为真，我们将旋转模型朝向该方向")]
		public bool ShouldRotateToFaceMovementDirection = true;
		/// the current rotation mode
		[Tooltip("当前旋转方式")]
		public RotationSpeeds MovementRotationSpeed = RotationSpeeds.Instant;
		/// the object we want to rotate towards direction. If left empty, we'll use the Character's model
		[Tooltip("我们要旋转朝向方向的对象。如果为空，我们将使用角色的模型")]
		public GameObject MovementRotatingModel;
		/// the speed at which to rotate towards direction (smooth and absolute only)
		[Tooltip("向某一方向旋转的速度（仅为smooth光滑和absolute绝对）")]
		public float RotateToFaceMovementDirectionSpeed = 10f;
		/// the threshold after which we start rotating (absolute mode only)
		[Tooltip("我们开始旋转的阈值（仅限absolute绝对模式）")]
		public float AbsoluteThresholdMovement = 0.5f;
		/// the direction of the model
		[MMReadOnly]
		[Tooltip("模型的方向")]
		public Vector3 ModelDirection;
		/// the direction of the model in angle values
		[MMReadOnly]
		[Tooltip("模型的方向以角度值表示")]
		public Vector3 ModelAngles;

		[Header("Weapon Direction武器方向")]

		/// If this is true, we'll rotate our model towards the weapon's direction
		[Tooltip("如果这个条件为真，我们将旋转模型朝向武器的方向")]
		public bool ShouldRotateToFaceWeaponDirection = true;
		/// the current rotation mode
		[Tooltip("当前旋转方式")]
		public RotationSpeeds WeaponRotationSpeed = RotationSpeeds.Instant;
		/// the object we want to rotate towards direction. If left empty, we'll use the Character's model
		[Tooltip("我们要旋转朝向方向的对象。如果为空，我们将使用角色的模型")]
		public GameObject WeaponRotatingModel;
		/// the speed at which to rotate towards direction (smooth and absolute only)
		[Tooltip("向某一方向旋转的速度（仅为smooth光滑和absolute绝对）")]
		public float RotateToFaceWeaponDirectionSpeed = 10f;
		/// the threshold after which we start rotating (absolute mode only)
		[Tooltip("我们开始旋转的阈值（仅限absolute绝对模式）")]
		public float AbsoluteThresholdWeapon = 0.5f;

		protected CharacterHandleWeapon _characterHandleWeapon;
		protected Vector3 _lastRegisteredVelocity;
		protected Vector3 _rotationDirection;
		protected Vector3 _lastMovement = Vector3.zero;
		protected Vector3 _lastAim = Vector3.zero;
		protected Vector3 _relativeSpeed;
		protected Vector3 _relativeSpeedNormalized;
		protected bool _secondaryMovementTriggered = false;
		protected Quaternion _tmpRotation;
		protected Quaternion _newMovementQuaternion;
		protected Quaternion _newWeaponQuaternion;
		protected bool _shouldRotateTowardsWeapon;
		protected const string _relativeForwardSpeedAnimationParameterName = "RelativeForwardSpeed";
		protected const string _relativeLateralSpeedAnimationParameterName = "RelativeLateralSpeed";
		protected const string _relativeForwardSpeedNormalizedAnimationParameterName = "RelativeForwardSpeedNormalized";
		protected const string _relativeLateralSpeedNormalizedAnimationParameterName = "RelativeLateralSpeedNormalized";
		protected int _relativeForwardSpeedAnimationParameter;
		protected int _relativeLateralSpeedAnimationParameter;
		protected int _relativeForwardSpeedNormalizedAnimationParameter;
		protected int _relativeLateralSpeedNormalizedAnimationParameter;

        /// <summary>
        /// 在init上，如果需要，我们抓取模型
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			if (MovementRotatingModel == null)
			{
				MovementRotatingModel = _model;
			}

			_characterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();
			if (WeaponRotatingModel == null)
			{
				WeaponRotatingModel = _model;
			}
		}

        /// <summary>
        /// 每一帧我们都朝这个方向旋转
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			RotateToFaceMovementDirection();
			RotateToFaceWeaponDirection();
			RotateToFaceForcedRotation();
			RotateModel();
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

            // 如果旋转模式是即时的，我们只需旋转面对我们的方向

            float angle = Mathf.Atan2(_controller.CurrentDirection.y, _controller.CurrentDirection.x) * Mathf.Rad2Deg;

			if (MovementRotationSpeed == RotationSpeeds.Instant)
			{
				if (_controller.CurrentDirection != Vector3.zero)
				{
					_newMovementQuaternion = Quaternion.Euler(angle * Vector3.forward);
				}
			}

            // 如果旋转模式是平滑的，我们就朝着我们的方向旋转
            if (MovementRotationSpeed == RotationSpeeds.Smooth)
			{
				if (_controller.CurrentDirection != Vector3.zero)
				{
					_tmpRotation = Quaternion.Euler(angle * Vector3.forward);
					_newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceMovementDirectionSpeed);
				}
			}

            // 如果旋转模式是平滑的，即使输入已经释放，我们也会朝着我们的方向旋转
            if (MovementRotationSpeed == RotationSpeeds.SmoothAbsolute)
			{
				if (_controller.CurrentDirection.normalized.magnitude >= AbsoluteThresholdMovement)
				{
					_lastMovement = _controller.CurrentDirection;
				}
				if (_lastMovement != Vector3.zero)
				{
					float lastAngle = Mathf.Atan2(_lastMovement.y, _lastMovement.x) * Mathf.Rad2Deg;
					_tmpRotation = Quaternion.Euler(lastAngle * Vector3.forward);

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
			_shouldRotateTowardsWeapon = false;

            // 如果我们不应该面对我们的方向，我们什么也不做，然后退出
            if (!ShouldRotateToFaceWeaponDirection) { return; }
			if ((RotationMode != RotationModes.WeaponDirection) && (RotationMode != RotationModes.Both)) { return; }
			if (_characterHandleWeapon == null) { return; }
			if (_characterHandleWeapon.WeaponAimComponent == null) { return; }

			_shouldRotateTowardsWeapon = true;
			_rotationDirection = _characterHandleWeapon.WeaponAimComponent.CurrentAim.normalized;
			MMDebug.DebugDrawArrow(this.transform.position, _rotationDirection, Color.red);
            
			float angle = Mathf.Atan2(_rotationDirection.y, _rotationDirection.x) * Mathf.Rad2Deg;

            // 如果旋转模式是即时的，我们只需旋转面对我们的方向
            if (WeaponRotationSpeed == RotationSpeeds.Instant)
			{
				if (_rotationDirection != Vector3.zero)
				{
					_newWeaponQuaternion = Quaternion.Euler(angle * Vector3.forward);
				}
			}

            // 如果旋转模式是平滑的，我们就朝着我们的方向旋转
            if (WeaponRotationSpeed == RotationSpeeds.Smooth)
			{
				if (_rotationDirection != Vector3.zero)
				{
					_tmpRotation = Quaternion.Euler(angle * Vector3.forward);
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
					float lastAngle = Mathf.Atan2(_lastMovement.y, _lastMovement.x) * Mathf.Rad2Deg;
					_tmpRotation = Quaternion.Euler(lastAngle * Vector3.forward);
					_newWeaponQuaternion = Quaternion.Slerp(WeaponRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceWeaponDirectionSpeed);
				}
			}
		}

        /// <summary>
        /// 如果我们有一个强制旋转，我们应用它
        /// </summary>
        protected virtual void RotateToFaceForcedRotation()
		{
			if (ForcedRotation)
			{
				float angle = Mathf.Atan2(ForcedRotationDirection.y, ForcedRotationDirection.x) * Mathf.Rad2Deg;

				if (MovementRotationSpeed == RotationSpeeds.Instant)
				{
					if (ForcedRotationDirection != Vector3.zero)
					{
						_newMovementQuaternion = Quaternion.Euler(angle * Vector3.forward);
					}
				}

                // 如果旋转模式是平滑的，我们就朝着我们的方向旋转
                if (MovementRotationSpeed == RotationSpeeds.Smooth)
				{
					if (ForcedRotationDirection != Vector3.zero)
					{
						_tmpRotation = Quaternion.Euler(angle * Vector3.forward);
						_newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceMovementDirectionSpeed);
					}
				}

                // 如果旋转模式是平滑的，即使输入已经释放，我们也会朝着我们的方向旋转
                if (MovementRotationSpeed == RotationSpeeds.SmoothAbsolute)
				{
					if (ForcedRotationDirection.normalized.magnitude >= AbsoluteThresholdMovement)
					{
						_lastMovement = ForcedRotationDirection;
					}
					if (_lastMovement != Vector3.zero)
					{
						float lastAngle = Mathf.Atan2(_lastMovement.y, _lastMovement.x) * Mathf.Rad2Deg;
						_tmpRotation = Quaternion.Euler(lastAngle * Vector3.forward);

						_newMovementQuaternion = Quaternion.Slerp(MovementRotatingModel.transform.rotation, _tmpRotation, Time.deltaTime * RotateToFaceMovementDirectionSpeed);
					}
				}

				if (ForcedRotationImpactsWeapon)
				{
					_newWeaponQuaternion = _newMovementQuaternion;
				}

				ModelDirection = MovementRotatingModel.transform.forward.normalized;
				ModelAngles = MovementRotatingModel.transform.eulerAngles;
			}
		}

        /// <summary>
        /// 根据需要旋转模型
        /// </summary>
        protected virtual void RotateModel()
		{
			MovementRotatingModel.transform.rotation = _newMovementQuaternion;

			if (_shouldRotateTowardsWeapon)
			{
				WeaponRotatingModel.transform.rotation = _newWeaponQuaternion;
			}
		}

		protected Vector3 _positionLastFrame;
		protected Vector3 _newSpeed;

        /// <summary>
        /// 计算相对速度
        /// </summary>
        protected virtual void ComputeRelativeSpeeds()
		{
			if (Time.deltaTime != 0f)
			{
				_newSpeed = (this.transform.position - _positionLastFrame) / Time.deltaTime;
			}            

			if (_characterHandleWeapon == null)
			{
				_relativeSpeed = MovementRotatingModel.transform.InverseTransformVector(_newSpeed);
			}
			else
			{
				_relativeSpeed = WeaponRotatingModel.transform.InverseTransformVector(_newSpeed);
			}
			_relativeSpeedNormalized = _relativeSpeed.normalized;
			_positionLastFrame = this.transform.position;
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_relativeForwardSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeForwardSpeedAnimationParameter);
			RegisterAnimatorParameter(_relativeLateralSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeLateralSpeedAnimationParameter);
			RegisterAnimatorParameter(_relativeForwardSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeForwardSpeedNormalizedAnimationParameter);
			RegisterAnimatorParameter(_relativeLateralSpeedNormalizedAnimationParameterName, AnimatorControllerParameterType.Float, out _relativeLateralSpeedNormalizedAnimationParameter);
		}

        /// <summary>
        /// 将当前速度和Walking状态的当前值发送给动画器
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeForwardSpeedAnimationParameter, _relativeSpeed.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeLateralSpeedAnimationParameter, _relativeSpeed.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeForwardSpeedNormalizedAnimationParameter, _relativeSpeedNormalized.z, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _relativeLateralSpeedNormalizedAnimationParameter, _relativeSpeedNormalized.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}
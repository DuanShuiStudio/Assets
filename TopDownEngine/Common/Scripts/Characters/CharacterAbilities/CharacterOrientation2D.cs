using System;
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
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Orientation 2D")]
	public class CharacterOrientation2D : CharacterAbility
	{
        /// 可能的面对模式
        public enum FacingModes { None, MovementDirection, WeaponDirection, Both }
		public enum FacingBases { WeaponAngle, MousePositionX, SceneReticlePositionX }
        /// 该字符的面向模式
        public FacingModes FacingMode = FacingModes.None;

		[MMEnumCondition("FacingMode", (int)FacingModes.WeaponDirection, (int)FacingModes.Both)]
		public FacingBases FacingBase = FacingBases.WeaponAngle;
        
		[MMInformation("你还可以决定角色是否必须在后退时自动翻转。另外，如果你不使用精灵，你可以在这里定义角色模型的局部尺度如何受到翻转的影响。默认情况下，它在x轴上翻转，但您可以更改它以适应您的模型。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]

		[Header("Horizontal Flip水平翻转")]

		/// whether we should flip the model's scale when the character changes direction or not		
		[Tooltip("是否应该在角色改变方向时翻转模型的比例")]
		public bool ModelShouldFlip = false;
		/// the scale value to apply to the model when facing left
		[MMCondition("ModelShouldFlip", true)]
		[Tooltip("当面向左边时应用于模型的比例值")]
		public Vector3 ModelFlipValueLeft = new Vector3(-1, 1, 1);
		/// the scale value to apply to the model when facing east
		[MMCondition("ModelShouldFlip", true)]
		[Tooltip("当面向东方时，适用于模型的比例值")]
		public Vector3 ModelFlipValueRight = new Vector3(1, 1, 1);
		/// whether we should rotate the model on direction change or not		
		[Tooltip("是否应该在方向改变时旋转模型")]
		public bool ModelShouldRotate;
		/// the rotation to apply to the model when it changes direction		
		[MMCondition("ModelShouldRotate", true)]
		[Tooltip("当模型改变方向时应用于模型的旋转")]
		public Vector3 ModelRotationValueLeft = new Vector3(0f, 180f, 0f);
		/// the rotation to apply to the model when it changes direction		
		[MMCondition("ModelShouldRotate", true)]
		[Tooltip("当模型改变方向时应用于模型的旋转")]
		public Vector3 ModelRotationValueRight = new Vector3(0f, 0f, 0f);
		/// the speed at which to rotate the model when changing direction, 0f means instant rotation		
		[MMCondition("ModelShouldRotate", true)]
		[Tooltip("模型改变方向时的旋转速度，0f表示瞬间旋转")]
		public float ModelRotationSpeed = 0f;
        
		[Header("Direction方向")]

		/// true if the player is facing east
		[MMInformation("通常，把所有的角色都朝东是一个很好的做法。如果这个角色不是这种情况，请选择West。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		[Tooltip("如果玩家面向东方则为真")]
		public Character.FacingDirections InitialFacingDirection = Character.FacingDirections.East;
		/// the threshold at which movement is considered
		[Tooltip("移动被视为的阈值")]
		public float AbsoluteThresholdMovement = 0.5f;
		/// the threshold at which weapon gets considered
		[Tooltip("武器被视为的阈值")]
		public float AbsoluteThresholdWeapon = 0.5f;
		/// the direction this character is currently facing
		[MMReadOnly]
		[Tooltip("这个角色当前面对的方向")]
		public Character.FacingDirections CurrentFacingDirection = Character.FacingDirections.East;
		/// whether or not this character is facing east
		[MMReadOnly]
		[Tooltip("这个角色是否朝东")]
		public bool IsFacingRight = true;

		protected Vector3 _targetModelRotation;
		protected CharacterHandleWeapon _characterHandleWeapon;
		protected Vector3 _lastRegisteredVelocity;
		protected Vector3 _rotationDirection;
		protected Vector3 _lastMovement = Vector3.zero;
		protected Vector3 _lastAim = Vector3.zero;
		protected float _lastNonNullXMovement;        
		protected float _lastNonNullXInput;
		protected int _direction;
		protected int _directionLastFrame = 0;
		protected float _horizontalDirection;
		protected float _verticalDirection;

		protected const string _facingDirectionAnimationParameterName = "FacingDirection2D";
		protected const string _horizontalDirectionAnimationParameterName = "HorizontalDirection";
		protected const string _verticalDirectionAnimationParameterName = "VerticalDirection";
		protected int _horizontalDirectionAnimationParameter;
		protected int _verticalDirectionAnimationParameter;
		protected const string _horizontalSpeedAnimationParameterName = "HorizontalSpeed";
		protected const string _verticalSpeedAnimationParameterName = "VerticalSpeed";
		protected int _horizontalSpeedAnimationParameter;
		protected int _verticalSpeedAnimationParameter;
		protected int _facingDirectionAnimationParameter;
		protected float _lastDirectionX;
		protected float _lastDirectionY;
		protected bool _initialized = false;
		protected float _directionFloat;

        /// <summary>
        /// 在唤醒时，我们初始化面向方向并抓取组件
        /// </summary>
        protected override void Awake()
		{
			base.Awake();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<CharacterHandleWeapon>();
		}

        /// <summary>
        /// 在开始时，我们重置CurrentDirection
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			if (_controller == null)
			{
				_controller = this.gameObject.GetComponentInParent<TopDownController>();
			}
			_controller.CurrentDirection = Vector3.zero;
			_initialized = true;
			if (InitialFacingDirection == Character.FacingDirections.West)
			{
				IsFacingRight = false;
				_direction = -1;
			}
			else
			{
				IsFacingRight = true;
				_direction = 1;
			}
			Face(InitialFacingDirection);
			_directionLastFrame = 0;
			CurrentFacingDirection = InitialFacingDirection;
			switch(InitialFacingDirection)
			{
				case Character.FacingDirections.East:
					_lastDirectionX = 1f;
					_lastDirectionY = 0f;
					break;
				case Character.FacingDirections.West:
					_lastDirectionX = -1f;
					_lastDirectionY = 0f;
					break;
				case Character.FacingDirections.North:
					_lastDirectionX = 0f;
					_lastDirectionY = 1f;
					break;
				case Character.FacingDirections.South:
					_lastDirectionX = 0f;
					_lastDirectionY = -1f;
					break;
			}
		}

        /// <summary>
        /// 在处理能力上，我们要面向设定的方向翻转
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();

			if (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			{
				return;
			}

			if (!AbilityAuthorized)
			{
				return;
			}

			DetermineFacingDirection();
			FlipToFaceMovementDirection();
			FlipToFaceWeaponDirection();
			ApplyModelRotation();
			FlipAbilities();

			_directionLastFrame = _direction;
			_lastNonNullXMovement = (Mathf.Abs(_controller.CurrentDirection.x) > 0) ? _controller.CurrentDirection.x : _lastNonNullXMovement;
			if (_inputManager != null)
			{
				_lastNonNullXInput = (Mathf.Abs(_inputManager.PrimaryMovement.x) > _inputManager.Threshold.x) ? _inputManager.PrimaryMovement.x : _lastNonNullXInput;
			}
		}

		protected virtual void FixedUpdate()
		{
			ComputeRelativeSpeeds();
		}

		protected virtual void DetermineFacingDirection()
		{
			if (_controller.CurrentDirection == Vector3.zero)
			{
				ApplyCurrentDirection();
			}

			if (_controller.CurrentDirection.normalized.magnitude >= AbsoluteThresholdMovement)
			{
				if (Mathf.Abs(_controller.CurrentDirection.y) > Mathf.Abs(_controller.CurrentDirection.x))
				{
					CurrentFacingDirection = (_controller.CurrentDirection.y > 0) ? Character.FacingDirections.North : Character.FacingDirections.South;
				}
				else
				{
					CurrentFacingDirection = (_controller.CurrentDirection.x > 0) ? Character.FacingDirections.East : Character.FacingDirections.West;
				}
				_horizontalDirection = Mathf.Abs(_controller.CurrentDirection.x) >= AbsoluteThresholdMovement ? _controller.CurrentDirection.x : 0f;
				if (_character.CharacterDimension == Character.CharacterDimensions.Type2D)
				{
					_verticalDirection = Mathf.Abs(_controller.CurrentDirection.y) >= AbsoluteThresholdMovement ? _controller.CurrentDirection.y : 0f;	
				}
				else
				{
					_verticalDirection = Mathf.Abs(_controller.CurrentDirection.z) >= AbsoluteThresholdMovement ? _controller.CurrentDirection.z : 0f;
				}
			}
			else
			{
				_horizontalDirection = _lastDirectionX;
				_verticalDirection = _lastDirectionY;
			}
            
			switch (CurrentFacingDirection)
			{
				case Character.FacingDirections.West:
					_directionFloat = 0f;
					break;
				case Character.FacingDirections.North:
					_directionFloat = 1f;
					break;
				case Character.FacingDirections.East:
					_directionFloat = 2f;
					break;
				case Character.FacingDirections.South:
					_directionFloat = 3f;
					break;
			}
            
			_lastDirectionX = _horizontalDirection;
			_lastDirectionY = _verticalDirection;
		}

        /// <summary>
        /// 将当前方向应用于控制器
        /// </summary>
        protected virtual void ApplyCurrentDirection()
		{
			if (!_initialized)
			{
				Initialization();
			}
            
			switch (CurrentFacingDirection)
			{
				case Character.FacingDirections.East:
					_controller.CurrentDirection = Vector3.right;
					break;
				case Character.FacingDirections.West:
					_controller.CurrentDirection = Vector3.left;
					break;
				case Character.FacingDirections.North:
					_controller.CurrentDirection = Vector3.up;
					break;
				case Character.FacingDirections.South:
					_controller.CurrentDirection = Vector3.down;
					break;
			}
		}

        /// <summary>
        /// 如果模型应该旋转，我们修改它的旋转
        /// </summary>
        protected virtual void ApplyModelRotation()
		{
			if (!ModelShouldRotate)
			{
				return;
			}

			if (ModelRotationSpeed > 0f)
			{
				_character.CharacterModel.transform.localEulerAngles = Vector3.Lerp(_character.CharacterModel.transform.localEulerAngles, _targetModelRotation, Time.deltaTime * ModelRotationSpeed);
			}
			else
			{
				_character.CharacterModel.transform.localEulerAngles = _targetModelRotation;
			}
		}

        /// <summary>
        /// 将物体翻转到面向方向
        /// </summary>
        protected virtual void FlipToFaceMovementDirection()
		{
			// if we're not supposed to face our direction, we do nothing and exit
			if ((FacingMode != FacingModes.MovementDirection) && (FacingMode != FacingModes.Both)) { return; }
            
			if (_controller.CurrentDirection.normalized.magnitude >= AbsoluteThresholdMovement)
			{
				float checkedDirection = (Mathf.Abs(_controller.CurrentDirection.normalized.x) > 0) ? _controller.CurrentDirection.normalized.x : _lastNonNullXMovement;
                
				if (checkedDirection >= 0)
				{
					FaceDirection(1);
				}
				else
				{
					FaceDirection(-1);
				}
			}                
		}

        /// <summary>
        /// 翻转角色以面对当前武器方向
        /// </summary>
        protected virtual void FlipToFaceWeaponDirection()
		{
			if (_characterHandleWeapon == null)
			{
				return;
			}
            // 如果我们不应该面对我们的方向，我们什么也不做，然后退出
            if ((FacingMode != FacingModes.WeaponDirection) && (FacingMode != FacingModes.Both)) { return; }
            
			if (_characterHandleWeapon.WeaponAimComponent != null)
			{
				switch (FacingBase)
				{
					case FacingBases.WeaponAngle:
						float weaponAngle = _characterHandleWeapon.WeaponAimComponent.CurrentAngleAbsolute;
						if ((weaponAngle > 90) || (weaponAngle < -90))
						{
							FaceDirection(-1);
						}
						else if (weaponAngle != 90f && weaponAngle != -90f) 
						{
							FaceDirection(1);
						}	
						break;
					case FacingBases.MousePositionX:
						if (_characterHandleWeapon.WeaponAimComponent.GetMousePosition().x < this.transform.position.x)
						{
							FaceDirection(-1);
						}
						else
						{
							FaceDirection(1);
						}	
						break;
					case FacingBases.SceneReticlePositionX:
						if (_characterHandleWeapon.WeaponAimComponent.GetReticlePosition().x < this.transform.position.x) 
						{
							FaceDirection(-1);
						}
						else
						{
							FaceDirection(1);
						}	
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
				

				_horizontalDirection = _characterHandleWeapon.WeaponAimComponent.CurrentAimAbsolute.normalized.x;
				if (_character.CharacterDimension == Character.CharacterDimensions.Type2D)
				{
					_verticalDirection = _characterHandleWeapon.WeaponAimComponent.CurrentAimAbsolute.normalized.y;	
				}
				else
				{
					_verticalDirection = _characterHandleWeapon.WeaponAimComponent.CurrentAimAbsolute.normalized.z;
				}
			}            
		}

        /// <summary>
        /// 定义CurrentFacingDirection
        /// </summary>
        /// <param name="direction"></param>
        public virtual void Face(Character.FacingDirections direction)
		{
			CurrentFacingDirection = direction;
			ApplyCurrentDirection();
			if (direction == Character.FacingDirections.West)
			{
				FaceDirection(-1);
			}
			if (direction == Character.FacingDirections.East)
			{
				FaceDirection(1);
			}
		}

        /// <summary>
        /// 水平翻转角色及其依赖关系
        /// </summary>
        public virtual void FaceDirection(int direction)
		{
			if (ModelShouldFlip)
			{
				FlipModel(direction);
			}

			if (ModelShouldRotate)
			{
				RotateModel(direction);
			}

			_direction = direction;
			IsFacingRight = _direction == 1;
		}

        /// <summary>
        /// 按指定方向旋转模型
        /// </summary>
        /// <param name="direction"></param>
        protected virtual void RotateModel(int direction)
		{
			if (_character.CharacterModel != null)
			{
				_targetModelRotation = (direction == 1) ? ModelRotationValueRight : ModelRotationValueLeft;
				_targetModelRotation.x = _targetModelRotation.x % 360;
				_targetModelRotation.y = _targetModelRotation.y % 360;
				_targetModelRotation.z = _targetModelRotation.z % 360;
			}
		}

        /// <summary>
        /// 只翻转模型，对武器或附件没有影响
        /// </summary>
        public virtual void FlipModel(int direction)
		{
			if (_character.CharacterModel != null)
			{
				_character.CharacterModel.transform.localScale = (direction == 1) ? ModelFlipValueRight : ModelFlipValueLeft;
			}
			else
			{
				_spriteRenderer.flipX = (direction == -1);
			}
		}

        /// <summary>
        /// 对所有其他能力发送翻转事件
        /// </summary>
        protected virtual void FlipAbilities()
		{
			if ((_directionLastFrame != 0) && (_directionLastFrame != _direction))
			{
				_character.FlipAllAbilities();
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
			_positionLastFrame = this.transform.position;
		}

        /// <summary>
        /// 添加所需的动画器参数到动画器参数列表（如果存在的话）
        /// </summary>
        protected override void InitializeAnimatorParameters()
		{
			RegisterAnimatorParameter(_horizontalDirectionAnimationParameterName, AnimatorControllerParameterType.Float, out _horizontalDirectionAnimationParameter);
			RegisterAnimatorParameter(_verticalDirectionAnimationParameterName, AnimatorControllerParameterType.Float, out _verticalDirectionAnimationParameter);

			RegisterAnimatorParameter(_horizontalSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _horizontalSpeedAnimationParameter);
			RegisterAnimatorParameter(_verticalSpeedAnimationParameterName, AnimatorControllerParameterType.Float, out _verticalSpeedAnimationParameter);
			RegisterAnimatorParameter(_facingDirectionAnimationParameterName, AnimatorControllerParameterType.Float, out _facingDirectionAnimationParameter);
		}

        /// <summary>
        /// 在每个循环结束时，向角色的动画师发送跳跃状态
        /// </summary>
        public override void UpdateAnimator()
		{
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _horizontalDirectionAnimationParameter, _horizontalDirection, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _verticalDirectionAnimationParameter, _verticalDirection, _character._animatorParameters, _character.RunAnimatorSanityChecks);

			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _horizontalSpeedAnimationParameter, _newSpeed.x, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _verticalSpeedAnimationParameter, _newSpeed.y, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _facingDirectionAnimationParameter, _directionFloat, _character._animatorParameters);      
		}

	}
}
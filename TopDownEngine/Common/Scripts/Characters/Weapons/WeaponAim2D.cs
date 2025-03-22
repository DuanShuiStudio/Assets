using UnityEngine;
using MoreMountains.Tools;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.TopDownEngine
{
	[RequireComponent(typeof(Weapon))]
    /// <summary>
    /// 将此组件添加到一个武器上，你就能瞄准它了（意思是你会旋转它）
    /// 支持的控制模式有鼠标、主要移动（你瞄准角色指向的方向）和次要移动（使用与移动不同的次级轴）。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Aim 2D")]
	public class WeaponAim2D : WeaponAim
	{
		protected Vector2 _inputMovement;
		protected bool _hasOrientation2D = false;
		protected bool _facingRightLastFrame;

        /// <summary>
        /// 在初始化时，我们获取相机并初始化我们上一个非空的移动
        /// </summary>
        protected override void Initialization()
		{
			if (_initialized)
			{
				return;
			}
			base.Initialization();
            
			if (_weapon.Owner?.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterOrientation2D>() != null)
			{
				_hasOrientation2D = true;
				switch (_weapon.Owner?.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterOrientation2D>().CurrentFacingDirection)
				{
					case Character.FacingDirections.East:
						_lastNonNullMovement = Vector2.right;
						break;
					case Character.FacingDirections.North:
						_lastNonNullMovement = Vector2.up;
						break;
					case Character.FacingDirections.West:
						_lastNonNullMovement = Vector2.left;
						break;
					case Character.FacingDirections.South:
						_lastNonNullMovement = Vector2.down;
						break;
				}
			}
		}

        /// <summary>
        /// 经过调整以补偿角色当前方向的、武器当前瞄准的角度
        /// </summary>
        public override float CurrentAngleRelative
		{
			get
			{
				if (_weapon != null)
				{
					if (_weapon.Owner != null)
					{
						if (_hasOrientation2D)
						{
							if (_weapon.Owner.Orientation2D.IsFacingRight)
							{
								return CurrentAngle;
							}
							else
							{
								return -CurrentAngle;
							}                        
						}
					}
				}
				return 0;
			}
		}

        /// <summary>
        /// 计算当前瞄准方向
        /// </summary>
        protected override void GetCurrentAim()
		{
			if (!AimControlActive)
			{
				return;
			}
			
			if (_weapon.Owner == null)
			{
				return;
			}

			if ((_weapon.Owner.LinkedInputManager == null) && (_weapon.Owner.CharacterType == Character.CharacterTypes.Player))
			{
				return;
			}

			AutoDetectWeaponMode();

			switch (AimControl)
			{
				case AimControls.Off:
					if (_weapon.Owner == null) { return; }
					GetOffAim();
					break;

				case AimControls.Script:
					GetScriptAim();
					break;

				case AimControls.PrimaryMovement:
					if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
					{
						return;
					}

					if (_weapon.Owner.LinkedInputManager.PrimaryMovement.magnitude > MinimumMagnitude)
					{
						GetPrimaryMovementAim();
					}

					break;

				case AimControls.SecondaryMovement:
					if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
					{
						return;
					}

					if (_weapon.Owner.LinkedInputManager.SecondaryMovement.magnitude > MinimumMagnitude)
					{
						GetSecondaryMovementAim();
					}

					break;                    

				case AimControls.PrimaryThenSecondaryMovement:
					if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
					{
						return;
					}

					if (_weapon.Owner.LinkedInputManager.PrimaryMovement.magnitude > MinimumMagnitude)
					{
						GetPrimaryMovementAim();
					}
					else
					{
						GetSecondaryMovementAim();
					}
					break;

				case AimControls.SecondaryThenPrimaryMovement:
					if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
					{
						return;
					}

					if (_weapon.Owner.LinkedInputManager.SecondaryMovement.magnitude > MinimumMagnitude)
					{
						GetSecondaryMovementAim();
					}
					else
					{
						GetPrimaryMovementAim();
					}

					break;

				case AimControls.Mouse:
					if (_weapon.Owner == null)
					{
						return;
					}
					GetMouseAim();                    
					break;

				case AimControls.CharacterRotateCameraDirection:
					if (_weapon.Owner == null)
					{
						return;
					}
					_currentAim = _weapon.Owner.CameraDirection;
					_currentAimAbsolute = _weapon.Owner.CameraDirection;
					if (_hasOrientation2D)
					{
						_currentAim = (_weapon.Owner.Orientation2D.IsFacingRight) ? _currentAim : -_currentAim;    
					}
					_direction = -(transform.position - _currentAim);
					break;
			}
		}

        /// <summary>
        /// 将武器瞄准一个新的点
        /// </summary>
        /// <param name="newAim">New aim.</param>
        public override void SetCurrentAim(Vector3 newAim, bool setAimAsLastNonNullMovement = false)
		{
			if (!AimControlActive)
			{
				return;
			}
			
			base.SetCurrentAim(newAim, setAimAsLastNonNullMovement);
	        
			_lastNonNullMovement.x = newAim.x;
			_lastNonNullMovement.y = newAim.y;
		}

        /// <summary>
        /// 重置瞄准
        /// </summary>
        public virtual void GetOffAim()
		{
			_currentAim = Vector2.right;
			_currentAimAbsolute = Vector2.right;
			_direction = Vector2.right;
		}

        /// <summary>
        /// 根据脚本提供的当前瞄准计算当前的瞄准方向
        /// </summary>
        public virtual void GetScriptAim()
		{
			_currentAimAbsolute = _currentAim;
			if (_hasOrientation2D)
			{
				_currentAim = (_weapon.Owner.Orientation2D.IsFacingRight) ? _currentAim : -_currentAim;    
			}
			_direction = -(transform.position - _currentAim);
		}

        /// <summary>
        /// 获取主要输入（默认为左摇杆）移动
        /// </summary>
        public virtual void GetPrimaryMovementAim()
		{
			if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
			{
				return;
			}

			if (_lastNonNullMovement == Vector2.zero)
			{
				_lastNonNullMovement = _weapon.Owner.LinkedInputManager.LastNonNullPrimaryMovement;
			}

			_inputMovement = _weapon.Owner.LinkedInputManager.PrimaryMovement;

			TestLastMovement();


			_currentAimAbsolute = _inputMovement;

			if (_hasOrientation2D)
			{
				if (_weapon.Owner.Orientation2D.IsFacingRight)
				{
					_currentAim = _inputMovement;
					_direction = transform.position + _currentAim;
				}
				else
				{
					_currentAim = -_inputMovement;
					_direction = -(transform.position - _currentAim);
				}    
			}
			else
			{
				_currentAim = _inputMovement;
				_direction = transform.position + _currentAim;
			}

			StoreLastMovement();
		}

        /// <summary>
        /// 获取次要移动（默认为右摇杆）
        /// </summary>
        public virtual void GetSecondaryMovementAim()
		{
			if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
			{
				return;
			}

			if (_lastNonNullMovement == Vector2.zero)
			{
				_lastNonNullMovement = _weapon.Owner.LinkedInputManager.LastNonNullSecondaryMovement;
			}

			_inputMovement = _weapon.Owner.LinkedInputManager.SecondaryMovement;
			TestLastMovement();

			_currentAimAbsolute = _inputMovement;

			if (_hasOrientation2D)
			{
				if (_weapon.Owner.Orientation2D.IsFacingRight)
				{
					_currentAim = _inputMovement;
					_direction = transform.position + _currentAim;
				}
				else
				{
					_currentAim = -_inputMovement;
					_direction = -(transform.position - _currentAim);
				}    
			}
			else
			{
				_currentAim = _inputMovement;
				_direction = transform.position + _currentAim;
			}
            
			StoreLastMovement();
		}

        /// <summary>
        ///获取鼠标瞄准
        /// </summary>
        public virtual void GetMouseAim()
		{
			_mousePosition = InputManager.Instance.MousePosition;
			
			_mousePosition.z = 10;

			_direction = _mainCamera.ScreenToWorldPoint(_mousePosition);
			_direction.z = _weapon.Owner.transform.position.z;

			_reticlePosition = _direction;

			_currentAimAbsolute = _direction - _weapon.transform.position;

			_currentAim = _direction - _weapon.transform.position;
            
			if (_hasOrientation2D)
			{
				if (_weapon.Owner.Orientation2D.IsFacingRight)
				{
					_currentAim = _direction - _weapon.transform.position;
					_currentAimAbsolute = _currentAim;
				}
				else
				{
					_currentAim = _weapon.transform.position - _direction;
				}
			}            
		}

        /// <summary>
        /// 如有必要，获取存储的上一次移动值
        /// </summary>
        protected virtual void TestLastMovement()
		{
			if (RotationMode == RotationModes.Strict2Directions)
			{
				_inputMovement.x = Mathf.Abs(_inputMovement.x) > 0 ? _inputMovement.x : _lastNonNullMovement.x;
				_inputMovement.y = Mathf.Abs(_inputMovement.y) > 0 ? _inputMovement.y : _lastNonNullMovement.y;
			}            
			else
			{
				_inputMovement = _inputMovement.magnitude > 0 ? _inputMovement : _lastNonNullMovement;
			}
		}

        /// <summary>
        /// 存储移动值以供下一帧使用
        /// </summary>
        protected virtual void StoreLastMovement()
		{
			if (RotationMode == RotationModes.Strict2Directions)
			{
				_lastNonNullMovement.x = Mathf.Abs(_inputMovement.x) > 0 ? _inputMovement.x : _lastNonNullMovement.x;
				_lastNonNullMovement.y = Mathf.Abs(_inputMovement.y) > 0 ? _inputMovement.y : _lastNonNullMovement.y;
			}
			else
			{
				_lastNonNullMovement = _inputMovement.magnitude > 0 ? _inputMovement : _lastNonNullMovement;
			}
		}

        /// <summary>
        /// 每帧，我们计算瞄准方向并相应地旋转武器。
        /// </summary>
        protected override void Update()
		{
			HideMousePointer();
			HideReticle();
			if (GameManager.Instance.Paused)
			{
				return;
			}
			GetCurrentAim();
			DetermineWeaponRotation();
		}

        /// <summary>
        /// 在固定更新时，我们移动目标和准星
        /// </summary>
        protected virtual void FixedUpdate()
		{
			if (GameManager.Instance.Paused)
			{
				return;
			}
			MoveTarget();
			MoveReticle();
		}

        /// <summary>
        /// 根据当前的瞄准方向确定武器的旋转
        /// </summary>
        protected override void DetermineWeaponRotation()
		{
			if (_currentAim != Vector3.zero)
			{
				if (_direction != Vector3.zero)
				{
					CurrentAngle = Mathf.Atan2(_currentAim.y, _currentAim.x) * Mathf.Rad2Deg;
					CurrentAngleAbsolute = Mathf.Atan2(_currentAimAbsolute.y, _currentAimAbsolute.x) * Mathf.Rad2Deg;
					if (RotationMode == RotationModes.Strict4Directions || RotationMode == RotationModes.Strict8Directions)
					{
						CurrentAngle = MMMaths.RoundToClosest(CurrentAngle, _possibleAngleValues);
					}
					if (RotationMode == RotationModes.Strict2Directions)
					{
						CurrentAngle = 0f;
					}

                    // 我们添加额外的角度
                    CurrentAngle += _additionalAngle;

					bool flip = false;
                    // 我们将角度限制在检查器中设置的最小/最大值范围内
                    if (_hasOrientation2D)
					{
						if (_weapon.Owner.Orientation2D.IsFacingRight)
						{
							CurrentAngle = Mathf.Clamp(CurrentAngle, MinimumAngle, MaximumAngle);
						}
						else
						{
							CurrentAngle = Mathf.Clamp(CurrentAngle, -MaximumAngle, -MinimumAngle);
						}
						flip = _facingRightLastFrame != _weapon.Owner.Orientation2D.IsFacingRight;
						_facingRightLastFrame = _weapon.Owner.Orientation2D.IsFacingRight;
					}
					else
					{
						CurrentAngle = Mathf.Clamp(CurrentAngle, MinimumAngle, MaximumAngle);
					}
                    
					_lookRotation = Quaternion.Euler(CurrentAngle * Vector3.forward);
					RotateWeapon(_lookRotation, flip);
				}
			}
			else
			{
				CurrentAngle = 0f;
				RotateWeapon(_initialRotation);
			}
			MMDebug.DebugDrawArrow(this.transform.position, _currentAimAbsolute.normalized, Color.green);
		}

        /// <summary>
        /// 如果已经设置了准星，则实例化准星并将其定位
        /// </summary>
        protected override void InitializeReticle()
		{
			if (_weapon.Owner == null) { return; }
			if (Reticle == null) { return; }
			if (ReticleType == ReticleTypes.None) { return; }

			if (_reticle != null)
			{
				Destroy(_reticle);
			}

			if (ReticleType == ReticleTypes.Scene)
			{
				_reticle = (GameObject)Instantiate(Reticle);

				if (!ReticleAtMousePosition)
				{
					if (_weapon.Owner != null)
					{
						_reticle.transform.SetParent(_weapon.transform);
						_reticle.transform.localPosition = ReticleDistance * Vector3.right;
					}
				}                
			}

			if (ReticleType == ReticleTypes.UI)
			{
				_reticle = (GameObject)Instantiate(Reticle);
				_reticle.transform.SetParent(GUIManager.Instance.MainCanvas.transform);
				_reticle.transform.localScale = Vector3.one;
				if (_reticle.gameObject.MMGetComponentNoAlloc<MMUIFollowMouse>() != null)
				{
					_reticle.gameObject.MMGetComponentNoAlloc<MMUIFollowMouse>().TargetCanvas = GUIManager.Instance.MainCanvas;
				}
			}
		}

        /// <summary>
        /// 如果被告知要跟随指针，每帧移动准星
        /// </summary>
        protected override void MoveReticle()
		{
			if (ReticleType == ReticleTypes.None) { return; }
			if (_reticle == null) { return; }
			if (_weapon.Owner.ConditionState.CurrentState == CharacterStates.CharacterConditions.Paused) { return; }

			if (ReticleType == ReticleTypes.Scene)
			{
                // 如果我们不应该旋转准星，我们就强制其旋转，否则我们应用当前的瞄准旋转
                if (!RotateReticle)
				{
					_reticle.transform.rotation = Quaternion.identity;
				}
				else
				{
					if (ReticleAtMousePosition)
					{
						_reticle.transform.rotation = _lookRotation;
					}
				}

                // 如果我们处于跟随鼠标模式，且当前的控制方案是鼠标，我们将准星移动到鼠标的位置
                if (ReticleAtMousePosition && AimControl == AimControls.Mouse)
				{
					_reticle.transform.position = _reticlePosition;
				}                
			}
		}

        /// <summary>
        /// 移动相机目标
        /// </summary>
        protected override void MoveTarget()
		{
			if (_weapon.Owner == null)
			{
				return;
			}
            
			if (MoveCameraTargetTowardsReticle)
			{
				if (ReticleType != ReticleTypes.None)
				{
					_newCamTargetPosition = _reticlePosition;
					_newCamTargetDirection = _newCamTargetPosition - this.transform.position;
					if (_newCamTargetDirection.magnitude > CameraTargetMaxDistance)
					{
						_newCamTargetDirection = _newCamTargetDirection.normalized * CameraTargetMaxDistance;
					}

					_newCamTargetPosition = this.transform.position + _newCamTargetDirection;

					_newCamTargetPosition = Vector3.Lerp(_weapon.Owner.CameraTarget.transform.position,
						Vector3.Lerp(this.transform.position, _newCamTargetPosition, CameraTargetOffset),
						Time.deltaTime * CameraTargetSpeed);

					_weapon.Owner.CameraTarget.transform.position = _newCamTargetPosition;
				}
				else
				{
					_newCamTargetPosition = this.transform.position + _currentAimAbsolute.normalized * CameraTargetMaxDistance;
					_newCamTargetDirection = _newCamTargetPosition - this.transform.position;
		            
					_newCamTargetPosition = this.transform.position + _newCamTargetDirection;

					_newCamTargetPosition = Vector3.Lerp(_weapon.Owner.CameraTarget.transform.position, Vector3.Lerp(this.transform.position, _newCamTargetPosition, CameraTargetOffset), Time.deltaTime * CameraTargetSpeed);

					_weapon.Owner.CameraTarget.transform.position = _newCamTargetPosition;
                    
				}
			}
		}
	}
}
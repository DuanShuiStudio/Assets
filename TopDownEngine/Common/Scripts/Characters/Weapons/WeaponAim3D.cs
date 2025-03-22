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
    /// 支持的控制模式有鼠标、主要移动（你瞄准角色指向的方向）和次要移动（使用与移动不同的次级轴）
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Aim 3D")]
	public class WeaponAim3D : WeaponAim
	{
		public enum AimCenters { Owner, Weapon }
		
		[MMInspectorGroup("3D", true, 3)]
		/// if this is true, aim will be unrestricted to angles, and will aim freely in all 3 axis, useful when dealing with AI and elevation
		[Tooltip("如果这是真的，瞄准将不受角度限制，并将在所有三个轴上自由瞄准，这在处理AI和仰角时很有用")]
		public bool Unrestricted3DAim = false;
		/// whether aim direction should be computed from the owner, or from the weapon
		[Tooltip("无论是应该从所有者那里计算瞄准方向，还是从武器那里计算")]
		public AimCenters AimCenter = AimCenters.Owner;
	    
		[MMInspectorGroup("Reticle and slopes", true, 4)]
		/// whether or not the reticle should move vertically to stay above slopes
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("准星是否应该垂直移动以保持在斜坡上方")]
		public bool ReticleMovesWithSlopes = false;
		/// the layers the reticle should consider as obstacles to move on
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("准星应视为障碍物的层")]
		public LayerMask ReticleObstacleMask = LayerManager.ObstaclesLayerMask;
		/// the maximum slope elevation for the reticle
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("准星的最大斜坡仰角")]
		public float MaximumSlopeElevation = 50f;
		/// if this is true, the aim system will try to compensate when aim direction is null (for example when you haven't set any primary input yet)
		[Tooltip("如果这是真的，当瞄准方向为空时（例如，当你还没有设置任何主要输入时），瞄准系统将尝试进行补偿。")]
		public bool AvoidNullAim = true;

		protected Vector2 _inputMovement;
		protected Vector3 _slopeTargetPosition;
		protected Vector3 _weaponAimCurrentAim;

		protected override void Initialization()
		{
			if (_initialized)
			{
				return;
			}
			base.Initialization();
			_mainCamera = Camera.main;
		}

		protected virtual void Reset()
		{
			ReticleObstacleMask = LayerMask.NameToLayer("Ground");
		}

        /// <summary>
        /// 计算当前瞄准方向。
        /// </summary>
        protected override void GetCurrentAim()
		{
			if (!AimControlActive)
			{
				if (ReticleType == ReticleTypes.Scene)
				{
					ComputeReticlePosition();
				}
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

			if ((_weapon.Owner.ConditionState.CurrentState != CharacterStates.CharacterConditions.Normal) &&
			    (_weapon.Owner.ConditionState.CurrentState != CharacterStates.CharacterConditions.ControlledMovement))
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
					GetPrimaryMovementAim();
					break;

				case AimControls.SecondaryMovement:
					if ((_weapon.Owner == null) || (_weapon.Owner.LinkedInputManager == null))
					{
						return;
					}
					GetSecondaryMovementAim();
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

					if (_currentAim == Vector3.zero)
					{
						_currentAim = _weapon.Owner.transform.forward;
						_weaponAimCurrentAim = _currentAim;
						_direction = transform.position + _currentAim;
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
					_weaponAimCurrentAim = _currentAim;
					_direction = transform.position + _currentAim;
					break;
			}
			
			if (AvoidNullAim && (_currentAim == Vector3.zero))
			{
				GetOffAim();
			}
		}

		public virtual void GetOffAim()
		{
			_currentAim = Vector3.right;
			_weaponAimCurrentAim = _currentAim;
			_direction = Vector3.right;
		}

		public virtual void GetPrimaryMovementAim()
		{
			if (_lastNonNullMovement == Vector2.zero)
			{
				_lastNonNullMovement = _weapon.Owner.LinkedInputManager.LastNonNullPrimaryMovement;
			}

			_inputMovement = _weapon.Owner.LinkedInputManager.PrimaryMovement;
			_inputMovement = _inputMovement.magnitude > MinimumMagnitude ? _inputMovement : _lastNonNullMovement;

			_currentAim.x = _inputMovement.x;
			_currentAim.y = 0f;
			_currentAim.z = _inputMovement.y;
			_weaponAimCurrentAim = _currentAim;
			_direction = transform.position + _currentAim;

			_lastNonNullMovement = _inputMovement.magnitude > MinimumMagnitude ? _inputMovement : _lastNonNullMovement;
		}

		public virtual void GetSecondaryMovementAim()
		{
			if (_lastNonNullMovement == Vector2.zero)
			{
				_lastNonNullMovement = _weapon.Owner.LinkedInputManager.LastNonNullSecondaryMovement;
			}

			_inputMovement = _weapon.Owner.LinkedInputManager.SecondaryMovement;
			_inputMovement = _inputMovement.magnitude > MinimumMagnitude ? _inputMovement : _lastNonNullMovement;

			_currentAim.x = _inputMovement.x;
			_currentAim.y = 0f;
			_currentAim.z = _inputMovement.y;
			_weaponAimCurrentAim = _currentAim;
			_direction = transform.position + _currentAim;

			_lastNonNullMovement = _inputMovement.magnitude > MinimumMagnitude ? _inputMovement : _lastNonNullMovement;
		}

		public virtual void GetScriptAim()
		{
			_direction = -(transform.position - _currentAim);
			_weaponAimCurrentAim = _currentAim;
		}
		
		public virtual void GetMouseAim()
		{
			ComputeReticlePosition();

			if (Vector3.Distance(_direction, transform.position) < MouseDeadZoneRadius)
			{
				_direction = _lastMousePosition;
			}
			else
			{
				_lastMousePosition = _direction;
			}

			_direction.y = transform.position.y;
			_currentAim = _direction - _weapon.Owner.transform.position;

			if (AimCenter == AimCenters.Owner)
			{
				_weaponAimCurrentAim = _direction - _weapon.Owner.transform.position;
			}
			else
			{
				_weaponAimCurrentAim = _direction - _weapon.transform.position;
				if (_weapon.WeaponUseTransform)
				{
					_weaponAimCurrentAim = _direction - _weapon.WeaponUseTransform.position;
				}
			}
		}

		protected virtual void ComputeReticlePosition()
		{
			_mousePosition = InputManager.Instance.MousePosition;
			
			Ray ray = _mainCamera.ScreenPointToRay(_mousePosition);
			Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
			float distance;
			if (_playerPlane.Raycast(ray, out distance))
			{
				Vector3 target = ray.GetPoint(distance);
				_direction = target;
			}
            
			_reticlePosition = _direction;
		}

        /// <summary>
        /// 每帧，我们计算瞄准方向并相应地旋转武器
		/// </summary>
        protected override void Update()
		{
			HideMousePointer();
			HideReticle();
			if (GameManager.HasInstance && GameManager.Instance.Paused)
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
			UpdatePlane();
		}

		protected virtual void UpdatePlane()
		{
			_playerPlane.SetNormalAndPosition (Vector3.up, this.transform.position);
		}

        /// <summary>
        /// 根据当前的瞄准方向确定武器的旋转
        /// </summary>
        protected override void DetermineWeaponRotation()
		{
			if (ReticleMovesWithSlopes)
			{
				if (Vector3.Distance(_slopeTargetPosition, this.transform.position) < MouseDeadZoneRadius)
				{
					return;
				}
				AimAt(_slopeTargetPosition);

				if (_weaponAimCurrentAim != Vector3.zero)
				{
					if (_direction != Vector3.zero)
					{
						CurrentAngle = Mathf.Atan2 (_weaponAimCurrentAim.z, _weaponAimCurrentAim.x) * Mathf.Rad2Deg;
						CurrentAngleAbsolute = Mathf.Atan2(_weaponAimCurrentAim.y, _weaponAimCurrentAim.x) * Mathf.Rad2Deg;
						if (RotationMode == RotationModes.Strict4Directions || RotationMode == RotationModes.Strict8Directions)
						{
							CurrentAngle = MMMaths.RoundToClosest (CurrentAngle, _possibleAngleValues);
						}
						CurrentAngle += _additionalAngle;
						CurrentAngle = Mathf.Clamp (CurrentAngle, MinimumAngle, MaximumAngle);	
						CurrentAngle = -CurrentAngle + 90f;
						_lookRotation = Quaternion.Euler (CurrentAngle * Vector3.up);
					}
				}

				return;
			}

			if (Unrestricted3DAim)
			{
				AimAt(this.transform.position + _weaponAimCurrentAim);
				return;
			}

			if (_weaponAimCurrentAim != Vector3.zero)
			{
				if (_direction != Vector3.zero)
				{
					CurrentAngle = Mathf.Atan2 (_weaponAimCurrentAim.z, _weaponAimCurrentAim.x) * Mathf.Rad2Deg;
					CurrentAngleAbsolute = Mathf.Atan2(_weaponAimCurrentAim.y, _weaponAimCurrentAim.x) * Mathf.Rad2Deg;
					if (RotationMode == RotationModes.Strict4Directions || RotationMode == RotationModes.Strict8Directions)
					{
						CurrentAngle = MMMaths.RoundToClosest (CurrentAngle, _possibleAngleValues);
					}

                    // 我们添加额外的角度
                    CurrentAngle += _additionalAngle;

                    // 我们将角度限制在检查器中设置的最小/最大值范围内

                    CurrentAngle = Mathf.Clamp (CurrentAngle, MinimumAngle, MaximumAngle);	
					CurrentAngle = -CurrentAngle + 90f;

					_lookRotation = Quaternion.Euler (CurrentAngle * Vector3.up);
                    
					RotateWeapon(_lookRotation);
				}
			}
			else
			{
				CurrentAngle = 0f;
				RotateWeapon(_initialRotation);	
			}
		}

		protected override void AimAt(Vector3 target)
		{
			base.AimAt(target);

			_aimAtDirection = target - transform.position;
			_aimAtQuaternion = Quaternion.LookRotation(_aimAtDirection, Vector3.up);
			if (WeaponRotationSpeed == 0f)
			{
				transform.rotation = _aimAtQuaternion;
			}
			else
			{
				transform.rotation = Quaternion.Lerp(transform.rotation, _aimAtQuaternion, WeaponRotationSpeed * Time.deltaTime);	
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
			_lastNonNullMovement.y = newAim.z;
		}

        /// <summary>
        /// 根据检查器中定义的设置初始化准星
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
						_reticle.transform.localPosition = ReticleDistance * Vector3.forward;
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
					_reticle.transform.position = MMMaths.Lerp(_reticle.transform.position, _reticlePosition, 0.3f, Time.deltaTime);
				}
			}
			_reticlePosition = _reticle.transform.position;
            
			if (ReticleMovesWithSlopes)
			{
                // 我们从上方投射一条光线
                RaycastHit groundCheck = MMDebug.Raycast3D(_reticlePosition + Vector3.up * MaximumSlopeElevation / 2f, Vector3.down, MaximumSlopeElevation, ReticleObstacleMask, Color.cyan, true);
				if (groundCheck.collider != null)
				{
					_reticlePosition.y = groundCheck.point.y + ReticleHeight;
					_reticle.transform.position = _reticlePosition;

					_slopeTargetPosition = groundCheck.point + Vector3.up * ReticleHeight;
				}
				else
				{
					_slopeTargetPosition = _reticle.transform.position;
				}
			}
		}

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
					if (_newCamTargetDirection.sqrMagnitude > (CameraTargetMaxDistance*CameraTargetMaxDistance))
					{
						_newCamTargetDirection = _newCamTargetDirection.normalized * CameraTargetMaxDistance;
					}
					_newCamTargetPosition = this.transform.position + _newCamTargetDirection;

					_newCamTargetPosition = Vector3.Lerp(_weapon.Owner.CameraTarget.transform.position, Vector3.Lerp(this.transform.position, _newCamTargetPosition, CameraTargetOffset), Time.deltaTime * CameraTargetSpeed);

					_weapon.Owner.CameraTarget.transform.position = _newCamTargetPosition;
				}
				else
				{
					_newCamTargetPosition = this.transform.position + CurrentAim.normalized * CameraTargetMaxDistance;
					_newCamTargetDirection = _newCamTargetPosition - this.transform.position;
		            
					_newCamTargetPosition = this.transform.position + _newCamTargetDirection;

					_newCamTargetPosition = Vector3.Lerp(_weapon.Owner.CameraTarget.transform.position, Vector3.Lerp(this.transform.position, _newCamTargetPosition, CameraTargetOffset), Time.deltaTime * CameraTargetSpeed);

					_weapon.Owner.CameraTarget.transform.position = _newCamTargetPosition;
				}
			}
		}
	}
}
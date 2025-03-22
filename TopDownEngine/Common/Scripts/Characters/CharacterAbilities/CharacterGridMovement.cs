using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此能力添加到角色中，使其在网格上移动。
    /// 这将需要一个GridManager出现在你的场景中
    /// 不要在同一个角色上同时使用该组件和CharacterMovement组件。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Grid Movement")] 
	public class CharacterGridMovement : CharacterAbility 
	{
        /// 网格上可能的方向
        public enum GridDirections { None, Up, Down, Left, Right }
        /// 可能的输入模式
        public enum InputModes { InputManager, Script }
        /// 可能的模式
        public enum DimensionModes { TwoD, ThreeD }

		[Header("Movement移动")]

		/// the maximum speed of the character
		[Tooltip("角色的最大速度")]
		public float MaximumSpeed = 8;
		/// the acceleration of the character
		[Tooltip("角色的加速度")]
		public float Acceleration = 5;
		/// the current speed at which the character is going
		[MMReadOnly]
		[Tooltip("角色当前的速度")]
		public float CurrentSpeed;
		/// a multiplier to apply to the maximum speed
		[MMReadOnly]
		[Tooltip("应用于最大速度的乘数")]
		public float MaximumSpeedMultiplier = 1f;
		/// a multiplier to apply to the acceleration, letting you modify it safely from outside
		[MMReadOnly]
		[Tooltip("一个适用于加速度的乘数，让您从外部安全地修改它")]
		public float AccelerationMultiplier = 1f;

		[Header("Input Settings输入设置")]

		/// whether to use the input manager or a script to feed the inputs
		[Tooltip("是否使用输入管理器或脚本来提供输入")]
		public InputModes InputMode = InputModes.InputManager;
		/// whether or not input should be buffered (to anticipate the next turn)
		[Tooltip("是否应该缓冲输入（以预测下一个转弯）")]
		public bool UseInputBuffer = true;
		/// the size of the input buffer (in grid units)
		[Tooltip("输入缓冲区的大小（以网格为单位）")]
		public int BufferSize = 2;
		/// whether or not the agent can perform fast direction changes such as U turns
		[Tooltip("代理是否可以执行快速方向变化，如U型转弯")]
		public bool FastDirectionChanges = true;
		/// the speed threshold after which the character is not considered idle anymore
		[Tooltip("速度阈值，超过该阈值后角色不再被认为是空闲的")]
		public float IdleThreshold = 0.05f;
		/// if this is true, movement values will be normalized - prefer checking this when using mobile controls
		[Tooltip("如果这个条件为真，移动值将被归一化 - 在使用移动控制时优先检查这一点")]
		public bool NormalizedInput = false;

		[Header("Grid网格")]

		/// the offset to apply when detecting obstacles
		[Tooltip("检测障碍物时应用的偏移量")]
		public Vector3 ObstacleDetectionOffset = new Vector3(0f, 0.5f, 0f);
        /// 对象在网格上的位置
        public virtual Vector3Int CurrentGridPosition { get; protected set; }
        /// 物体到达下一个完美贴图时所处的位置
        public virtual Vector3Int TargetGridPosition { get; protected set; }
        /// 每当角色处于贴图的确切位置时，都是如此
        [MMReadOnly]
		[Tooltip("每当角色处于贴图的确切位置时，都是如此")]
		public bool PerfectTile;
		/// the coordinates of the cell this character currently occupies
		[MMReadOnly]
		[Tooltip("这个角色当前占据的格子的坐标")]
		public Vector3Int CurrentCellCoordinates;
		/// whether this character is in 2D or 3D. This gets automatically computed at start
		[MMReadOnly]
		[Tooltip("该角色是在2D还是3D中。这在开始时会自动计算")]
		public DimensionModes DimensionMode = DimensionModes.TwoD;

		[Header("Test测试")]
		[MMInspectorButton("Left")]
		public bool LeftButton;
		[MMInspectorButton("Right")]
		public bool RightButton;
		[MMInspectorButton("Up")]
		public bool UpButton;
		[MMInspectorButton("Down")]
		public bool DownButton;
		[MMInspectorButton("StopMovement")]
		public bool StopButton;
		[MMInspectorButton("LeftOneCell")]
		public bool LeftOneCellButton;
		[MMInspectorButton("RightOneCell")]
		public bool RightOneCellButton;
		[MMInspectorButton("UpOneCell")]
		public bool UpOneCellButton;
		[MMInspectorButton("DownOneCell")]
		public bool DownOneCellButton;

		protected GridDirections _inputDirection;
		protected GridDirections _currentDirection = GridDirections.Up;
		protected GridDirections _bufferedDirection;
		protected bool _movementInterruptionBuffered = false;
		protected bool _perfectTile = false;                
		protected Vector3 _inputMovement;
		protected Vector3 _endWorldPosition;
		protected bool _movingToNextGridUnit = false;
		protected bool _stopBuffered = false;        
		protected int _lastBufferInGridUnits;
		protected bool _agentMoving;
		protected GridDirections _newDirection;
		protected float _horizontalMovement;
		protected float _verticalMovement;
		protected Vector3Int _lastOccupiedCellCoordinates;
		protected const string _speedAnimationParameterName = "Speed";
		protected const string _walkingAnimationParameterName = "Walking";
		protected const string _idleAnimationParameterName = "Idle";
		protected int _speedAnimationParameter;
		protected int _walkingAnimationParameter;
		protected int _idleAnimationParameter;
		protected bool _firstPositionRegistered = false;
		protected Vector3Int _newCellCoordinates = Vector3Int.zero;
		protected Vector3 _lastCurrentDirection;

		protected bool _leftPressedLastFrame = false;
		protected bool _rightPressedLastFrame = false;
		protected bool _downPressedLastFrame = false;
		protected bool _upPressedLastFrame = false;

        /// <summary>
        /// 一个用于设置角色方向的公共方法（仅在Script InputMode中）
        /// </summary>
        /// <param name="newMovement"></param>
        public virtual void SetMovement(Vector2 newMovement)
		{
			_horizontalMovement = newMovement.x;
			_verticalMovement = newMovement.y;
		}

        /// <summary>
        /// 将角色向左移动一个单元格
        /// </summary>
        public virtual void LeftOneCell()
		{
			StartCoroutine(OneCell(Vector2.left));
		}

        /// <summary>
        /// 将角色向右移动一个单元格
        /// </summary>
        public virtual void RightOneCell()
		{
			StartCoroutine(OneCell(Vector2.right));
		}

        /// <summary>
        /// 将角色向上移动一个单元格
        /// </summary>
        public virtual void UpOneCell()
		{
			StartCoroutine(OneCell(Vector2.up));
		}

        /// <summary>
        /// 将角色向下移动一个单元格
        /// </summary>
        public virtual void DownOneCell()
		{
			StartCoroutine(OneCell(Vector2.down));
		}

        /// <summary>
        /// 一种内部协同程序，用于将字符沿指定方向移动一个单元格
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected virtual IEnumerator OneCell(Vector2 direction)
		{
			SetMovement(direction);
			yield return null;
			SetMovement(Vector2.zero);
		}

        /// <summary>
        /// 将脚本移动设置为向左
        /// </summary>
        public virtual void Left() { SetMovement(Vector2.left); }
        /// <summary>
        /// 设置脚本向右移动
        /// </summary>
        public virtual void Right() { SetMovement(Vector2.right); }
        /// <summary>
        ///将脚本移动设置为上
        /// </summary>
        public virtual void Up() { SetMovement(Vector2.up); }
        /// <summary>
        /// 将脚本移动设置为下
        /// </summary>
        public virtual void Down() { SetMovement(Vector2.down); }
		/// <summary>
		/// 停止脚本移动
		/// </summary>
		public virtual void StopMovement() { SetMovement(Vector2.zero); }

        /// <summary>
        /// 在初始化中，我们将移动速度设置为WalkSpeed。
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization ();
			DimensionMode = DimensionModes.ThreeD;
			if (_controller.gameObject.MMGetComponentNoAlloc<TopDownController2D>() != null)
			{
				DimensionMode = DimensionModes.TwoD;
				_controller.FreeMovement = false;
			}
			_controller.PerformCardinalObstacleRaycastDetection = true;
			_bufferedDirection = GridDirections.None;
		}

        /// <summary>
        /// 将第一个位置注册到GridManager
        /// </summary>
        protected virtual void RegisterFirstPosition()
		{
			if (!_firstPositionRegistered)
			{
				_endWorldPosition = this.transform.position;
				_lastOccupiedCellCoordinates = GridManager.Instance.WorldToCellCoordinates(_endWorldPosition);
				CurrentCellCoordinates = _lastOccupiedCellCoordinates;
				GridManager.Instance.OccupyCell(_lastOccupiedCellCoordinates);
				GridManager.Instance.SetLastPosition(this.gameObject, _lastOccupiedCellCoordinates);
				GridManager.Instance.SetNextPosition(this.gameObject, _lastOccupiedCellCoordinates);
				_firstPositionRegistered = true;
			}
		}
        
		public virtual void SetCurrentWorldPositionAsNewPosition()
		{
			_endWorldPosition = this.transform.position;
			GridManager.Instance.FreeCell(_lastOccupiedCellCoordinates);
			_lastOccupiedCellCoordinates = GridManager.Instance.WorldToCellCoordinates(_endWorldPosition);
			CurrentCellCoordinates = _lastOccupiedCellCoordinates;
			GridManager.Instance.OccupyCell(_lastOccupiedCellCoordinates);
			GridManager.Instance.SetLastPosition(this.gameObject, _lastOccupiedCellCoordinates);
			GridManager.Instance.SetNextPosition(this.gameObject, _lastOccupiedCellCoordinates);
			_firstPositionRegistered = true;
		}

        /// <summary>
        /// 3次传递中的第2次。可以把它看作Update（）
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();

			if ((_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen)
				|| (_character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Stunned))
			{
				return;
			}

			RegisterFirstPosition();

			_controller.DetectObstacles(GridManager.Instance.GridUnitSize, ObstacleDetectionOffset);
			DetermineInputDirection();
			ApplyAcceleration();
			HandleMovement();
			HandleState();
		}

        /// <summary>
        /// 获取水平和垂直输入并存储它们
        /// </summary>
        protected override void HandleInput()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			if (InputMode == InputModes.InputManager)
			{
				_horizontalMovement = NormalizedInput ? Mathf.Round(_horizontalInput) : _horizontalInput;
				_verticalMovement = NormalizedInput ? Mathf.Round(_verticalInput) : _verticalInput;
			}            
		}

        /// <summary>
        /// 根据按下次数，确定输入方向
        /// </summary>
        protected virtual void DetermineInputDirection()
		{
            // 如果我们没有按任何方向，我们就停止
            if ((Mathf.Abs(_horizontalMovement) <= IdleThreshold) && (Mathf.Abs(_verticalMovement) <= IdleThreshold))
			{
				Stop(_newDirection);
				_newDirection = GridDirections.None;
				_inputMovement = Vector3.zero;
			}

            // 如果我们第一次按下一个方向，它就变成了我们的新方向
            if ((_horizontalMovement < 0f) && !_leftPressedLastFrame) { _newDirection = GridDirections.Left; _inputMovement = Vector3.left; }
			if ((_horizontalMovement > 0f) && !_rightPressedLastFrame) { _newDirection = GridDirections.Right; _inputMovement = Vector3.right; }
			if ((_verticalMovement < 0f) && !_downPressedLastFrame) { _newDirection = GridDirections.Down; _inputMovement = Vector3.down; }
			if ((_verticalMovement > 0f) && !_upPressedLastFrame) { _newDirection = GridDirections.Up; _inputMovement = Vector3.up; }

            // 如果我们按了一个方向，而刚刚释放了它，我们就会寻找另一个方向
            if ((_horizontalMovement == 0f) && (_leftPressedLastFrame || _rightPressedLastFrame)) { _newDirection = GridDirections.None; }
			if ((_verticalMovement == 0f) && (_downPressedLastFrame || _upPressedLastFrame)) { _newDirection = GridDirections.None; }

            // 如果在这一点上我们没有方向，我们取任何按下的方向
            if (_newDirection == GridDirections.None)
			{
				if (_horizontalMovement < 0f) { _newDirection = GridDirections.Left; _inputMovement = Vector3.left; }
				if (_horizontalMovement > 0f) { _newDirection = GridDirections.Right; _inputMovement = Vector3.right;  }
				if (_verticalMovement < 0f) { _newDirection = GridDirections.Down; _inputMovement = Vector3.down; }
				if (_verticalMovement > 0f) { _newDirection = GridDirections.Up; _inputMovement = Vector3.up; }
			}
            
			_inputDirection = _newDirection;

            // 我们为下一帧保存我们的印刷机
            _leftPressedLastFrame = (_horizontalMovement < 0f);
			_rightPressedLastFrame = (_horizontalMovement > 0f);
			_downPressedLastFrame = (_verticalMovement < 0f);
			_upPressedLastFrame = (_verticalMovement > 0f);
		}

        /// <summary>
        /// 停止角色并使其朝向指定的方向
        /// </summary>
        /// <param name="direction"></param>
        public virtual void Stop(GridDirections direction)
		{
			if (direction == GridDirections.None)
			{
				return;
			}
			_bufferedDirection = direction;
			_stopBuffered = true;
		}

        /// <summary>
        /// 根据加速度修改当前速度
        /// </summary>
        protected virtual void ApplyAcceleration()
		{
			if ((_currentDirection != GridDirections.None) && (CurrentSpeed < MaximumSpeed * MaximumSpeedMultiplier))
			{
				CurrentSpeed = CurrentSpeed + Acceleration * AccelerationMultiplier * Time.deltaTime;
				CurrentSpeed = Mathf.Clamp(CurrentSpeed, 0f, MaximumSpeed * MaximumSpeedMultiplier);
			}
		}

        /// <summary>
        /// 在网格上移动角色
        /// </summary>
        protected virtual void HandleMovement()
		{
			_perfectTile = false;
			PerfectTile = false;
			ProcessBuffer();

            // 如果我们要做U型转弯，我们会在允许的情况下改变方向
            /*if (FastDirectionChanges && _agentMoving && !_stopBuffered && _movingToNextGridUnit)
			{
			    if ((_bufferedDirection != GridDirections.None) 
			        && (_bufferedDirection == GetInverseDirection(_currentDirection)))
			    {
			        _newCellCoordinates = CurrentCellCoordinates + ConvertDirectionToVector3Int(_currentDirection);
			        _endWorldPosition = GridManager.Instance.CellToWorldCoordinates(_newCellCoordinates);
			        _endWorldPosition = DimensionClamp(_endWorldPosition);
			        _currentDirection = _bufferedDirection;
			    }
			}*/

            // 如果我们不在格子间的话
            if (!_movingToNextGridUnit)
			{
				PerfectTile = true;

                // 如果我们有一个停止缓冲
                if (_movementInterruptionBuffered)
				{
					_perfectTile = true;
					_movementInterruptionBuffered = false;
					return;
				}

                // 如果我们不再有方向
                if (_bufferedDirection == GridDirections.None)
				{
					_currentDirection = GridDirections.None;
					_bufferedDirection = GridDirections.None;
					_agentMoving = false;
					CurrentSpeed = 0;

					GridManager.Instance.SetLastPosition(this.gameObject, GridManager.Instance.WorldToCellCoordinates(_endWorldPosition));
					GridManager.Instance.SetNextPosition(this.gameObject, GridManager.Instance.WorldToCellCoordinates(_endWorldPosition));

					return;
				}

                // 我们检查我们是否可以在选定的方向上移动
                if (((_currentDirection == GridDirections.Left) && (_controller.DetectedObstacleLeft != null))
				    || ((_currentDirection == GridDirections.Right) && (_controller.DetectedObstacleRight != null))
				    || ((_currentDirection == GridDirections.Up) && (_controller.DetectedObstacleUp != null))
				    || ((_currentDirection == GridDirections.Down) && (_controller.DetectedObstacleDown != null)))
				{
					_currentDirection = _bufferedDirection;
                    
					GridManager.Instance.SetLastPosition(this.gameObject, GridManager.Instance.WorldToCellCoordinates(_endWorldPosition));
					GridManager.Instance.SetNextPosition(this.gameObject, GridManager.Instance.WorldToCellCoordinates(_endWorldPosition));

					return;
				}

                // 我们检查我们是否可以在选定的方向上移动
                if (((_bufferedDirection == GridDirections.Left) && !(_controller.DetectedObstacleLeft != null))
				    || ((_bufferedDirection == GridDirections.Right) && !(_controller.DetectedObstacleRight != null))
				    || ((_bufferedDirection == GridDirections.Up) && !(_controller.DetectedObstacleUp != null))
				    || ((_bufferedDirection == GridDirections.Down) && !(_controller.DetectedObstacleDown != null)))
				{
					_currentDirection = _bufferedDirection;
				}

                // 我们计算了一下，然后朝新的目的地走去
                _movingToNextGridUnit = true;
				DetermineEndPosition();

                // 我们要确保目标单元是自由的
                if (GridManager.Instance.CellIsOccupied(TargetGridPosition))
				{
					_movingToNextGridUnit = false;
					_currentDirection = GridDirections.None;
					_bufferedDirection = GridDirections.None;
					_agentMoving = false;
					CurrentSpeed = 0;
				}
				else
				{
					GridManager.Instance.FreeCell(_lastOccupiedCellCoordinates);
					GridManager.Instance.SetLastPosition(this.gameObject, _lastOccupiedCellCoordinates);
					GridManager.Instance.SetNextPosition(this.gameObject, TargetGridPosition);
					GridManager.Instance.OccupyCell(TargetGridPosition);
					CurrentCellCoordinates = TargetGridPosition;
					_lastOccupiedCellCoordinates = TargetGridPosition;
				}
			}

            // 计算新的网格位置
            TargetGridPosition = GridManager.Instance.WorldToCellCoordinates(_endWorldPosition);

            // 将控制器移动到下一个位置
            Vector3 newPosition = Vector3.MoveTowards(transform.position, _endWorldPosition, Time.deltaTime * CurrentSpeed);

			_lastCurrentDirection = _endWorldPosition - this.transform.position;
			_lastCurrentDirection = _lastCurrentDirection.MMRound();
			if (_lastCurrentDirection != Vector3.zero)
			{
				_controller.CurrentDirection = _lastCurrentDirection;
			}
			_controller.MovePosition(newPosition, true);
		}

        /// <summary>
        /// 处理缓冲输入
        /// </summary>
        protected virtual void ProcessBuffer()
		{
            // 如果输入有一个方向，它就变成了新的缓冲方向
            if ((_inputDirection != GridDirections.None) && !_stopBuffered)
			{
				_bufferedDirection = _inputDirection;
				_lastBufferInGridUnits = BufferSize;
			}

            // 如果我们没有移动，得到一个输入，我们就开始移动
            if (!_agentMoving && _inputDirection != GridDirections.None)
			{
				_currentDirection = _inputDirection;
				_agentMoving = true;
			}

            // 如果我们到达下一个贴图，我们就不再移动了
            if (_movingToNextGridUnit && (transform.position == _endWorldPosition))
			{
				_movingToNextGridUnit = false;
				CurrentGridPosition = GridManager.Instance.WorldToCellCoordinates(_endWorldPosition);
			}

            // 我们处理缓冲区。如果我们有一个缓冲的方向，在一个完美的贴图上，没有输入
            if ((_bufferedDirection != GridDirections.None) && !_movingToNextGridUnit && (_inputDirection == GridDirections.None) && UseInputBuffer)
			{
                // 我们减少缓冲计数器
                _lastBufferInGridUnits--;
                // 如果缓冲区过期，则返回当前方向
                if ((_lastBufferInGridUnits < 0) && (_bufferedDirection != _currentDirection))
				{
					_bufferedDirection = _currentDirection;
				}
			}

            // 如果我们计划停下来，但没有移动，我们就停下来
            if ((_stopBuffered) && !_movingToNextGridUnit)
			{
				_bufferedDirection = GridDirections.None;
				_stopBuffered = false;
			}
		}

        /// <summary>
        /// 根据当前方向确定结束位置
        /// </summary>
        protected virtual void DetermineEndPosition()
		{
			TargetGridPosition = CurrentCellCoordinates + ConvertDirectionToVector3Int(_currentDirection);
			_endWorldPosition = GridManager.Instance.CellToWorldCoordinates(TargetGridPosition);
            // 我们保持z（2D）或y （3D）
            _endWorldPosition = DimensionClamp(_endWorldPosition);
		}

		protected Vector3 DimensionClamp(Vector3 newPosition)
		{
			if (DimensionMode == DimensionModes.TwoD)
			{
				newPosition.z = this.transform.position.z;
			}
			else
			{
				newPosition.y = this.transform.position.y;
			}

			return newPosition;
		}

        /// <summary>
        /// 将GridDirection转换为基于当前D的Vector3
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected virtual Vector3Int ConvertDirectionToVector3Int(GridDirections direction)
		{
			if (direction != GridDirections.None)
			{
				if (direction == GridDirections.Left) return Vector3Int.left;
				if (direction == GridDirections.Right) return Vector3Int.right;
                
				if (DimensionMode == DimensionModes.TwoD)
				{
					if (direction == GridDirections.Up) return Vector3Int.up;
					if (direction == GridDirections.Down) return Vector3Int.down;
				}
				else
				{
					if (direction == GridDirections.Up) return Vector3Int.RoundToInt(Vector3.forward);
					if (direction == GridDirections.Down) return Vector3Int.RoundToInt(Vector3.back);
				}                
			}
			return Vector3Int.zero;
		}

        /// <summary>
        /// 返回GridDirection的相反方向
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected virtual GridDirections GetInverseDirection(GridDirections direction)
		{
			if (direction != GridDirections.None)
			{
				if (direction == GridDirections.Left) return GridDirections.Right;
				if (direction == GridDirections.Right) return GridDirections.Left;
				if (direction == GridDirections.Up) return GridDirections.Down;
				if (direction == GridDirections.Down) return GridDirections.Up;
			}
			return GridDirections.None;
		}

		protected virtual void HandleState()
		{
			if (_movingToNextGridUnit)
			{
				if (_movement.CurrentState != CharacterStates.MovementStates.Walking)
				{
					_movement.ChangeState(CharacterStates.MovementStates.Walking);
					PlayAbilityStartFeedbacks();
				}
			}
			else
			{
				if (_movement.CurrentState != CharacterStates.MovementStates.Idle)
				{
					_movement.ChangeState(CharacterStates.MovementStates.Idle);
					if (_startFeedbackIsPlaying)
					{
						StopStartFeedbacks();
						PlayAbilityStopFeedbacks();
					}
				}
			}            
		}

        /// <summary>
        /// 死后，我们释放了最后一个被占的单元
        /// </summary>
        protected override void OnDeath()
		{
			base.OnDeath();
			GridManager.Instance.FreeCell(_lastOccupiedCellCoordinates);
		}

        /// <summary>
        /// 在Respawn中，我们强制重新初始化
        /// </summary>
        protected override void OnRespawn()
		{
			base.OnRespawn();
			Initialization();
			_firstPositionRegistered = false;
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
			MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _speedAnimationParameter, CurrentSpeed, _character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _walkingAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Walking),_character._animatorParameters, _character.RunAnimatorSanityChecks);
			MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter, (_movement.CurrentState == CharacterStates.MovementStates.Idle),_character._animatorParameters, _character.RunAnimatorSanityChecks);
		}
	}
}
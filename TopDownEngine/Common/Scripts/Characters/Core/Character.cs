using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// This class will pilot the TopDownController component of your character.
    /// 这个类将试点你的角色的TopDownController组件。
    /// This is where you'll implement all of your character's game rules, like jump, dash, shoot, stuff like that.
	/// 在这里你将执行所有角色的游戏规则，如跳跃，冲刺，射击等等。
    /// Animator parameters : Grounded (bool), xSpeed (float), ySpeed (float), 
	/// 动画器参数：接地（bool）， xSpeed (float), ySpeed (float)，
    /// CollidingLeft (bool), CollidingRight (bool), CollidingBelow (bool), CollidingAbove (bool), Idle (bool)
    /// Random : a random float between 0 and 1, updated every frame, useful to add variance to your state entry transitions for example
	/// 随机：一个介于0和1之间的随机浮点数，每帧更新一次，例如，在你的状态输入转换中添加变化是很有用的
    /// RandomConstant : a random int (between 0 and 1000), generated at Start and that'll remain constant for the entire lifetime of this animator, useful to have different characters of the same type 
	/// RandomConstant：一个随机整数（介于0和1000之间），在开始时生成，并在动画器的整个生命周期中保持不变，这对于具有相同类型的不同角色很有用
    /// </summary>
    [SelectionBase]
	[AddComponentMenu("TopDown Engine/Character/Core/Character")] 
	public class Character : TopDownMonoBehaviour
	{
		/// 你的角色可能的初始面向方向
		public enum FacingDirections { West, East, North, South }

		public enum CharacterDimensions { Type2D, Type3D }
		[MMReadOnly]
		public CharacterDimensions CharacterDimension;

        /// the possible character types : player controller or AI (controlled by the computer) <summary>
        /// 可能的角色类型：玩家控制器或AI（由电脑控制）
        /// </summary>
        public enum CharacterTypes { Player, AI }

		[MMInformation("角色脚本是所有角色技能的必备基础。你的角色可以是由AI控制的非玩家角色，也可以是由玩家控制的玩家角色。在这种情况下，您需要指定一个PlayerID，它必须与InputManager中指定的PlayerID匹配。通常是“Player1”、“Player2”等。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// Is the character player-controlled or controlled by an AI ?
		[Tooltip("角色是由玩家控制还是由AI控制 ?")]
		public CharacterTypes CharacterType = CharacterTypes.AI;
		/// Only used if the character is player-controlled. The PlayerID must match an input manager's PlayerID. It's also used to match Unity's input settings. So you'll be safe if you keep to Player1, Player2, Player3 or Player4
		[Tooltip("只有当角色由玩家控制时才会使用。PlayerID必须匹配输入管理器的PlayerID。它也用于匹配Unity的输入设置。所以如果你一直使用Player1， Player2， Player3或Player4，你就会很安全")]
		public string PlayerID = "";

        /// the various states of the character  
        [Tooltip("性格的不同状态")]
        public virtual CharacterStates CharacterState { get; protected set; }

		[Header("Animator")]
		[MMInformation("引擎将尝试为这个角色找到一个动画器。如果它在相同的游戏对象上，它应该找到它。如果它嵌套在某个地方，则需要在下面绑定它。你也可以决定完全摆脱它，在这种情况下，只要取消勾选“use mecanim”。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the character animator
		[Tooltip("角色动画器，这个类和所有能力应该更新参数")]
		public Animator CharacterAnimator;
		/// Set this to false if you want to implement your own animation system
		[Tooltip("如果你想实现你自己的动画系统，将此设置为false")]
		public bool UseDefaultMecanim = true;
		/// If this is true, sanity checks will be performed to make sure animator parameters exist before updating them. Turning this to false will increase performance but will throw errors if you're trying to update non existing parameters. Make sure your animator has the required parameters.
		[Tooltip("如果这是真的，在更新动画器参数之前，将执行完整性检查以确保它们存在。将此设置为false将提高性能，但如果您试图更新不存在的参数，则会抛出错误。确保你的动画器有必要的参数")]
		public bool RunAnimatorSanityChecks = false;
		/// if this is true, animator logs for the associated animator will be turned off to avoid potential spam
		[Tooltip("如果这是真的，相关动画器的动画器日志将被关闭，以避免潜在的垃圾邮件")]
		public bool DisableAnimatorLogs = true;

		[Header("Bindings绑定")]
		[MMInformation("如果这是一个常规的，基于精灵的角色，并且如果SpriteRenderer和角色位于相同的GameObject上，则不绑定此选项。如果没有，您将需要将实际模型作为Character对象的父对象，并在下面绑定它。看看3D演示角色的例子。这背后的想法是，模型可能会移动，翻转，但对撞机将保持不变。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// the 'model' (can be any gameobject) used to manipulate the character. Ideally it's separated (and nested) from the collider/TopDown controller/abilities, to avoid messing with collisions.
		[Tooltip("用于操纵角色的‘模型’（可以是任何游戏对象）。理想情况下，它与碰撞器/俯视控制器/能力分开（并嵌套在一起），以避免干扰碰撞。")]
		public GameObject CharacterModel;
		/// the Health script associated to this Character, will be grabbed automatically if left empty
		[Tooltip("与此角色关联的生命值脚本，如果为空，将自动抓取")]
		public Health CharacterHealth;
        
		[Header("Events事件")]
		[MMInformation("在这里，你可以定义是否要在改变状态时让角色触发事件。有关更多信息，请参阅MMTools的State Machine文档。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// If this is true, the Character's state machine will emit events when entering/exiting a state
		[Tooltip("如果这是真的，角色的状态机在进入/退出状态时会发出事件")]
		public bool SendStateChangeEvents = true;
        
		[Header("Abilities能力")]
		/// A list of gameobjects (usually nested under the Character) under which to search for additional abilities
		[Tooltip("一个游戏对象列表（通常嵌套在角色下），用于搜索额外的能力")]
		public List<GameObject> AdditionalAbilityNodes;
        
		[Header("AI")]
		/// The brain currently associated with this character, if it's an Advanced AI. By default the engine will pick the one on this object, but you can attach another one if you'd like
		[Tooltip("如果这是一个高级AI，当前与这个角色关联的大脑。默认情况下，引擎会选择这个对象上的大脑，但你也可以根据需要附加另一个")]
		public AIBrain CharacterBrain;

		/// Whether to optimize this character for mobile. Will disable its cone of vision on mobile
		[Tooltip("是否为移动设备优化此角色。在移动设备上将禁用其视野锥")]
		public bool OptimizeForMobile = true;

		/// State Machines
		public MMStateMachine<CharacterStates.MovementStates> MovementState;
		public MMStateMachine<CharacterStates.CharacterConditions> ConditionState;


        /// associated camera and input manager
        [Tooltip("关联的相机和输入管理器")]
        public virtual InputManager LinkedInputManager { get; protected set; }
        /// the animator associated to this character 
        [Tooltip("与这个角色相关的动画器")]
        public virtual Animator _animator { get; protected set; }
        /// a list of animator parameters 
        [Tooltip("动画器参数列表")]
        public virtual HashSet<int> _animatorParameters { get; set; }
        /// this character's orientation 2D ability
        [Tooltip("这个角色的二维方向能力")]
        public virtual CharacterOrientation2D Orientation2D { get; protected set; }
        /// this character's orientation 3D ability
        [Tooltip("这个角色的三维方向能力")]
        public virtual CharacterOrientation3D Orientation3D { get; protected set; }
        /// an object to use as the camera's point of focus and follow target
        [Tooltip("一个作为摄像机焦点和跟随目标的对象")]
        public virtual GameObject CameraTarget { get; set; }
        /// the direction of the camera associated to this character 
        [Tooltip("与这个角色相关的摄像机方向")]
        public virtual Vector3 CameraDirection { get; protected set; }

		protected CharacterAbility[] _characterAbilities;
		protected bool _abilitiesCachedOnce = false;
		protected TopDownController _controller;
		protected float _animatorRandomNumber;
		protected bool _spawnDirectionForced = false;

		protected const string _groundedAnimationParameterName = "Grounded";
		protected const string _aliveAnimationParameterName = "Alive";
		protected const string _currentSpeedAnimationParameterName = "CurrentSpeed";
		protected const string _xSpeedAnimationParameterName = "xSpeed";
		protected const string _ySpeedAnimationParameterName = "ySpeed";
		protected const string _zSpeedAnimationParameterName = "zSpeed";
		protected const string _xVelocityAnimationParameterName = "xVelocity";
		protected const string _yVelocityAnimationParameterName = "yVelocity";
		protected const string _zVelocityAnimationParameterName = "zVelocity";
		protected const string _idleAnimationParameterName = "Idle";
		protected const string _randomAnimationParameterName = "Random";
		protected const string _randomConstantAnimationParameterName = "RandomConstant";
		protected const string _transformVelocityXAnimationParameterName = "TransformVelocityX";
		protected const string _transformVelocityYAnimationParameterName = "TransformVelocityY";
		protected const string _transformVelocityZAnimationParameterName = "TransformVelocityZ";
		
		protected int _groundedAnimationParameter;
		protected int _aliveAnimationParameter;
		protected int _currentSpeedAnimationParameter;
		protected int _xSpeedAnimationParameter;
		protected int _ySpeedAnimationParameter;
		protected int _zSpeedAnimationParameter;
		protected int _xVelocityAnimationParameter;
		protected int _yVelocityAnimationParameter;
		protected int _zVelocityAnimationParameter;
		protected int _transformVelocityXAnimationParameter;
		protected int _transformVelocityYAnimationParameter;
		protected int _transformVelocityZAnimationParameter;
		
		protected int _idleAnimationParameter;
		protected int _randomAnimationParameter;
		protected int _randomConstantAnimationParameter;
		protected bool _animatorInitialized = false;
		protected CharacterPersistence _characterPersistence;
		protected bool _onReviveRegistered;
		protected Coroutine _conditionChangeCoroutine;
		protected CharacterStates.CharacterConditions _lastState;
		protected Vector3 _transformVelocity;
		protected Vector3 _thisPositionLastFrame;


        /// <summary>
        /// 初始化字符的实例
        /// </summary>
        protected virtual void Awake()
		{		
			Initialization();
		}

        /// <summary>
        /// 获取并存储输入管理器、摄像头和组件
        /// </summary>
        protected virtual void Initialization()
		{            
			if (this.gameObject.MMGetComponentNoAlloc<TopDownController2D>() != null)
			{
				CharacterDimension = CharacterDimensions.Type2D;
			}
			if (this.gameObject.MMGetComponentNoAlloc<TopDownController3D>() != null)
			{
				CharacterDimension = CharacterDimensions.Type3D;
			}

            // 我们初始化状态机
            MovementState = new MMStateMachine<CharacterStates.MovementStates>(gameObject,SendStateChangeEvents);
			ConditionState = new MMStateMachine<CharacterStates.CharacterConditions>(gameObject,SendStateChangeEvents);

            // 我们得到当前的输入管理器
            SetInputManager();
            // 我们存储组件以供以后使用
            CharacterState = new CharacterStates();
			_controller = this.gameObject.GetComponent<TopDownController> ();
			if (CharacterHealth == null)
			{
				CharacterHealth = this.gameObject.GetComponent<Health> ();	
			}

			CacheAbilitiesAtInit();
			if (CharacterBrain == null)
			{
				CharacterBrain = this.gameObject.GetComponent<AIBrain>(); 
			}

			if (CharacterBrain != null)
			{
				CharacterBrain.Owner = this.gameObject;
			}

			Orientation2D = FindAbility<CharacterOrientation2D>();
			Orientation3D = FindAbility<CharacterOrientation3D>();
			_characterPersistence = FindAbility<CharacterPersistence>();
			_thisPositionLastFrame = this.transform.position;

			AssignAnimator();

            // 实例化相机目标
            if (CameraTarget == null)
			{
				CameraTarget = new GameObject();
			}            
			CameraTarget.transform.SetParent(this.transform);
			CameraTarget.transform.localPosition = Vector3.zero;
			CameraTarget.name = "CameraTarget";

			if (LinkedInputManager != null)
			{
				if (OptimizeForMobile && LinkedInputManager.IsMobile)
				{
					if (this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>() != null)
					{
						this.gameObject.MMGetComponentNoAlloc<MMConeOfVision2D>().enabled = false;
					}
				}
			}            
		}

        /// <summary>
        /// 必要时缓存能力
        /// </summary>
        protected virtual void CacheAbilitiesAtInit()
		{
			if (_abilitiesCachedOnce)
			{
				return;
			}
			CacheAbilities();
		}

        /// <summary>
        /// 获取技能并将其缓存以备将来使用
        /// 确保在运行时添加能力时调用这个函数
        /// 理想情况下，你会想要避免在运行时添加组件，这是昂贵的，
        /// 最好是激活/禁用组件。
        /// 但如果你需要，调用这个方法。
        /// </summary>
        public virtual void CacheAbilities()
		{
            // 我们掌握了我们这个水平的所有能力
            _characterAbilities = this.gameObject.GetComponents<CharacterAbility>();

            // 如果用户指定了更多节点
            if ((AdditionalAbilityNodes != null) && (AdditionalAbilityNodes.Count > 0))
			{
                // 我们创建一个临时列表
                List<CharacterAbility> tempAbilityList = new List<CharacterAbility>();

                // 我们把所有已经发现的能力都列在清单上
                for (int i = 0; i < _characterAbilities.Length; i++)
				{
					tempAbilityList.Add(_characterAbilities[i]);
				}

                // 我们把节点上的1加起来
                for (int j = 0; j < AdditionalAbilityNodes.Count; j++)
				{
					CharacterAbility[] tempArray = AdditionalAbilityNodes[j].GetComponentsInChildren<CharacterAbility>();
					foreach(CharacterAbility ability in tempArray)
					{
						tempAbilityList.Add(ability);
					}
				}

				_characterAbilities = tempAbilityList.ToArray();
			}
			_abilitiesCachedOnce = true;
		}

        /// <summary>
        /// 强制（重新）初始化角色的能力
        /// </summary>
        public virtual void ForceAbilitiesInitialization()
		{
			for (int i = 0; i < _characterAbilities.Length; i++)
			{
				_characterAbilities[i].ForceInitialization();
			}
			for (int j = 0; j < AdditionalAbilityNodes.Count; j++)
			{
				CharacterAbility[] tempArray = AdditionalAbilityNodes[j].GetComponentsInChildren<CharacterAbility>();
				foreach(CharacterAbility ability in tempArray)
				{
					ability.ForceInitialization();
				}
			}
		}

        /// <summary>
        /// 一种检查角色是否具有某种能力的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FindAbility<T>() where T:CharacterAbility
		{
			CacheAbilitiesAtInit();

			Type searchedAbilityType = typeof(T);
            
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability is T characterAbility)
				{
					return characterAbility;
				}
			}

			return null;
		}

        /// <summary>
        /// 一种检查角色是否具有某种能力的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public CharacterAbility FindAbilityByString(string abilityName)
		{
			CacheAbilitiesAtInit();
            
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.GetType().Name == abilityName)
				{
					return ability;
				}
			}

			return null;
		}

        /// <summary>
        /// 一种检查角色是否具有某种能力的方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> FindAbilities<T>() where T:CharacterAbility
		{
			CacheAbilitiesAtInit();

			List<T> resultList = new List<T>();
			Type searchedAbilityType = typeof(T);

			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability is T characterAbility)
				{
					resultList.Add(characterAbility);
				}
			}

			return resultList;
		}

        /// <summary>
        /// 将动画师绑定到此角色
        /// </summary>
        public virtual void AssignAnimator(bool forceAssignation = false)
		{
			if (_animatorInitialized && !forceAssignation)
			{
				return;
			}
            
			_animatorParameters = new HashSet<int>();

			if (CharacterAnimator != null)
			{
				_animator = CharacterAnimator;
			}
			else
			{
				_animator = this.gameObject.GetComponent<Animator>();
			}

			if (_animator != null)
			{
				if (DisableAnimatorLogs)
				{
					_animator.logWarnings = false;
				}
				InitializeAnimatorParameters();
			}

			_animatorInitialized = true;
		}

        /// <summary>
        /// 初始化动画器参数。
        /// </summary>
        protected virtual void InitializeAnimatorParameters()
		{
			if (_animator == null) { return; }
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _groundedAnimationParameterName, out _groundedAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _currentSpeedAnimationParameterName, out _currentSpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _xSpeedAnimationParameterName, out _xSpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _ySpeedAnimationParameterName, out _ySpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _zSpeedAnimationParameterName, out _zSpeedAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _idleAnimationParameterName, out _idleAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _aliveAnimationParameterName, out _aliveAnimationParameter, AnimatorControllerParameterType.Bool, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _randomAnimationParameterName, out _randomAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _randomConstantAnimationParameterName, out _randomConstantAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _xVelocityAnimationParameterName, out _xVelocityAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _yVelocityAnimationParameterName, out _yVelocityAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _zVelocityAnimationParameterName, out _zVelocityAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _transformVelocityXAnimationParameterName, out _transformVelocityXAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _transformVelocityYAnimationParameterName, out _transformVelocityYAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);
			MMAnimatorExtensions.AddAnimatorParameterIfExists(_animator, _transformVelocityZAnimationParameterName, out _transformVelocityZAnimationParameter, AnimatorControllerParameterType.Float, _animatorParameters);

            // 我们更新常量浮动动画参数
            int randomConstant = UnityEngine.Random.Range(0, 1000);
			MMAnimatorExtensions.UpdateAnimatorInteger(_animator, _randomConstantAnimationParameter, randomConstant, _animatorParameters, RunAnimatorSanityChecks);
		}

        /// <summary>
        /// 获取（如果存在的话）与角色的玩家ID匹配的InputManager
        /// </summary>
        public virtual void SetInputManager()
		{
			if (CharacterType == CharacterTypes.AI)
			{
				LinkedInputManager = null;
				UpdateInputManagersInAbilities();
				return;
			}

            // 我们得到相应的输入管理器
            if (!string.IsNullOrEmpty(PlayerID))
			{
				LinkedInputManager = null;
				InputManager[] foundInputManagers = FindObjectsOfType(typeof(InputManager)) as InputManager[];
				foreach (InputManager foundInputManager in foundInputManagers)
				{
					if (foundInputManager.PlayerID == PlayerID)
					{
						LinkedInputManager = foundInputManager;
					}
				}
			}
			UpdateInputManagersInAbilities();
		}

        /// <summary>
        /// 为这个角色及其所有能力设置一个新的输入管理器
        /// </summary>
        /// <param name="inputManager"></param>
        public virtual void SetInputManager(InputManager inputManager)
		{
			LinkedInputManager = inputManager;
			UpdateInputManagersInAbilities();
		}

        /// <summary>
        /// 更新所有能力的链接输入管理器
        /// </summary>
        protected virtual void UpdateInputManagersInAbilities()
		{
			if (_characterAbilities == null)
			{
				return;
			}
			for (int i = 0; i < _characterAbilities.Length; i++)
			{
				_characterAbilities[i].SetInputManager(LinkedInputManager);
			}
		}

		protected void FixedUpdate()
		{
			_transformVelocity = (this.transform.position - _thisPositionLastFrame) / Time.deltaTime;
			_thisPositionLastFrame = this.transform.position;
		}

        /// <summary>
        /// 重置所有技能的输入
        /// </summary>
        public virtual void ResetInput()
		{
			if (_characterAbilities == null)
			{
				return;
			}
			foreach (CharacterAbility ability in _characterAbilities)
			{
				ability.ResetInput();
			}
		}

        /// <summary>
        /// 设置玩家ID
        /// </summary>
        /// <param name="newPlayerID">New player ID.</param>
        public virtual void SetPlayerID(string newPlayerID)
		{
			PlayerID = newPlayerID;
			SetInputManager();
		}

        /// <summary>
        /// 这叫做每一帧。
        /// </summary>
        protected virtual void Update()
		{		
			EveryFrame();
				
		}

        /// <summary>
        /// 我们每一帧都这么做。这与Update是分开的，以获得更大的灵活性。
        /// </summary>
        protected virtual void EveryFrame()
		{
            // 我们处理我们的能力
            EarlyProcessAbilities();
			ProcessAbilities();
			LateProcessAbilities();

            // 我们将各种状态发送给动画器。	 
            UpdateAnimators();
		}

        /// <summary>
        /// 调用所有注册能力的早期过程方法
        /// </summary>
        protected virtual void EarlyProcessAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.EarlyProcessAbility();
				}
			}
		}

        /// <summary>
        /// 调用所有注册能力的Process方法
        /// </summary>
        protected virtual void ProcessAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.ProcessAbility();
				}
			}
		}

        /// <summary>
        /// 调用所有注册能力的后期处理方法
        /// </summary>
        protected virtual void LateProcessAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled && ability.AbilityInitialized)
				{
					ability.LateProcessAbility();
				}
			}
		}


        /// <summary>
        /// 这在Update（）中调用，并将每个动画器参数设置为相应的State值
        /// </summary>
        protected virtual void UpdateAnimators()
		{
			UpdateAnimationRandomNumber();

			if ((UseDefaultMecanim) && (_animator!= null))
			{
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _groundedAnimationParameter, _controller.Grounded,_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _aliveAnimationParameter, (ConditionState.CurrentState != CharacterStates.CharacterConditions.Dead),_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _currentSpeedAnimationParameter, _controller.CurrentMovement.magnitude, _animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _xSpeedAnimationParameter, _controller.CurrentMovement.x,_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _ySpeedAnimationParameter, _controller.CurrentMovement.y,_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _zSpeedAnimationParameter, _controller.CurrentMovement.z,_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorBool(_animator, _idleAnimationParameter,(MovementState.CurrentState == CharacterStates.MovementStates.Idle),_animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _randomAnimationParameter, _animatorRandomNumber, _animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _xVelocityAnimationParameter, _controller.Velocity.x, _animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _yVelocityAnimationParameter, _controller.Velocity.y, _animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _zVelocityAnimationParameter, _controller.Velocity.z, _animatorParameters, RunAnimatorSanityChecks);
				
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _transformVelocityXAnimationParameter, _transformVelocity.x, _animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _transformVelocityYAnimationParameter, _transformVelocity.y, _animatorParameters, RunAnimatorSanityChecks);
				MMAnimatorExtensions.UpdateAnimatorFloat(_animator, _transformVelocityZAnimationParameter, _transformVelocity.z, _animatorParameters, RunAnimatorSanityChecks);


				foreach (CharacterAbility ability in _characterAbilities)
				{
					if (ability.enabled && ability.AbilityInitialized)
					{	
						ability.UpdateAnimator();
					}
				}
			}
		}
		
		public virtual void RespawnAt(Vector3 spawnPosition, FacingDirections facingDirection)
		{
			transform.position = spawnPosition;
			
			if (!gameObject.activeInHierarchy)
			{
				gameObject.SetActive(true);
                //调试。LogError（“刷出：你的角色的游戏对象是不活动的”）；
            }

            // 我们让它起死回生（如果它已经死了）
            ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
            // 我们重新启用它的2D碰撞器
            if (this.gameObject.MMGetComponentNoAlloc<Collider2D>() != null)
			{
				this.gameObject.MMGetComponentNoAlloc<Collider2D>().enabled = true;
			}
            // 我们重新启用它的3D碰撞器
            if (this.gameObject.MMGetComponentNoAlloc<Collider>() != null)
			{
				this.gameObject.MMGetComponentNoAlloc<Collider>().enabled = true;
			}

            // 我们让它再次处理碰撞
            _controller.enabled = true;
			_controller.CollisionsOn();
			_controller.Reset();
			
			Reset();
			UnFreeze();

			if (CharacterHealth != null)
			{
				CharacterHealth.StoreInitialPosition();
				if (_characterPersistence != null)
				{
					if (_characterPersistence.Initialized)
					{
						if (CharacterHealth != null)
						{
							CharacterHealth.UpdateHealthBar(false);
						}
						return;
					}
				}
				CharacterHealth.ResetHealthToMaxHealth();
				CharacterHealth.Revive();
			}

			if (CharacterBrain != null)
			{
				CharacterBrain.enabled = true;
			}

            // 面对方向
            if (FindAbility<CharacterOrientation2D>() != null)
			{
				FindAbility<CharacterOrientation2D>().InitialFacingDirection = facingDirection;
				FindAbility<CharacterOrientation2D>().Face(facingDirection);
			}
            // 面对方向
            if (FindAbility<CharacterOrientation3D>() != null)
			{
				FindAbility<CharacterOrientation3D>().Face(facingDirection); 
			}
		}

        /// <summary>
        /// 让玩家在传入参数的位置重生
        /// </summary>
        /// <param name="spawnPoint">The location of the respawn.</param>
        public virtual void RespawnAt(Transform spawnPoint, FacingDirections facingDirection)
		{
			RespawnAt(spawnPoint.position, facingDirection);
		}

        /// <summary>
        /// 召唤开启所有能力
        /// </summary>
        public virtual void FlipAllAbilities()
		{
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled)
				{
					ability.Flip();
				}
			}
		}

        /// <summary>
        /// 生成一个随机数发送给动画师
        /// </summary>
        protected virtual void UpdateAnimationRandomNumber()
		{
			_animatorRandomNumber = Random.Range(0f, 1f);
		}

        /// <summary>
        /// 使用此方法可以在指定的持续时间内改变角色的状态，然后重置它。
        /// 你也可以用它来禁用重力一段时间，并有选择地重置力。
        /// </summary>
        /// <param name="newCondition"></param>
        /// <param name="duration"></param>
        /// <param name="resetControllerForces"></param>
        /// <param name="disableGravity"></param>
        public virtual void ChangeCharacterConditionTemporarily(CharacterStates.CharacterConditions newCondition,
			float duration, bool resetControllerForces, bool disableGravity)
		{
			if (_conditionChangeCoroutine != null)
			{
				StopCoroutine(_conditionChangeCoroutine);
			}
			_conditionChangeCoroutine = StartCoroutine(ChangeCharacterConditionTemporarilyCo(newCondition, duration, resetControllerForces, disableGravity));
		}

        /// <summary>
        /// 处理临时更改由changecharacterconditiontemporary指定的条件的协程
        /// </summary>
        /// <param name="newCondition"></param>
        /// <param name="duration"></param>
        /// <param name="resetControllerForces"></param>
        /// <param name="disableGravity"></param>
        /// <returns></returns>
        protected virtual IEnumerator ChangeCharacterConditionTemporarilyCo(
			CharacterStates.CharacterConditions newCondition,
			float duration, bool resetControllerForces, bool disableGravity)
		{
			if (_lastState != newCondition) if ((_lastState != newCondition) && (this.ConditionState.CurrentState != newCondition))
			{
				_lastState = this.ConditionState.CurrentState;
			}
			
			this.ConditionState.ChangeState(newCondition);
			if (resetControllerForces) { _controller?.SetMovement(Vector2.zero); }
			if (disableGravity && (_controller != null)) { _controller.GravityActive = false; }
			yield return MMCoroutine.WaitFor(duration);
			this.ConditionState.ChangeState(_lastState);
			if (disableGravity && (_controller != null)) { _controller.GravityActive = true; }
		}

        /// <summary>
        /// 存储相关的摄像机方向
        /// </summary>
        public virtual void SetCameraDirection(Vector3 direction)
		{
			CameraDirection = direction;
		}

        /// <summary>
        /// 冻结这个角色。
        /// </summary>
        public virtual void Freeze()
		{
			_controller.SetGravityActive(false);
			_controller.SetMovement(Vector2.zero);
			ConditionState.ChangeState(CharacterStates.CharacterConditions.Frozen);
		}

        /// <summary>
        /// 解冻这个角色
        /// </summary>
        public virtual void UnFreeze()
		{
			if (ConditionState.CurrentState == CharacterStates.CharacterConditions.Frozen)
			{
				_controller.SetGravityActive(true);
				ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);
			}
		}

        /// <summary>
        /// 调用来禁用玩家（例如在关卡结束时）。
        /// 在此之后，它将不再移动和响应输入。
        /// </summary>
        public virtual void Disable()
		{
			this.enabled = false;
			_controller.enabled = false;			
		}

        /// <summary>
        /// 当角色死亡时调用。
        /// 调用每个技能的Reset（）方法，这样你就可以在需要时将设置恢复到原始值
        /// </summary>
        public virtual void Reset()
		{
			_spawnDirectionForced = false;
			if (_characterAbilities == null)
			{
				return;
			}
			if (_characterAbilities.Length == 0)
			{
				return;
			}
			foreach (CharacterAbility ability in _characterAbilities)
			{
				if (ability.enabled)
				{
					ability.ResetAbility();
				}
			}
		}

        /// <summary>
        /// 在复活时，我们强制刷出方向
        /// </summary>
        protected virtual void OnRevive()
		{
			if (CharacterBrain != null)
			{
				CharacterBrain.enabled = true;
				CharacterBrain.ResetBrain();
			}
		}

		protected virtual void OnDeath()
		{
			if (CharacterBrain != null)
			{
				CharacterBrain.TransitionToState("");
				CharacterBrain.enabled = false;
			}
			if (MovementState.CurrentState != CharacterStates.MovementStates.FallingDownHole)
			{
				MovementState.ChangeState(CharacterStates.MovementStates.Idle);
			}            
		}

		protected virtual void OnHit()
		{

		}

        /// <summary>
        /// OnEnable，我们注册OnRevive事件
        /// </summary>
        protected virtual void OnEnable ()
		{
			if (CharacterHealth != null)
			{
				if (!_onReviveRegistered)
				{
					CharacterHealth.OnRevive += OnRevive;
					_onReviveRegistered = true;
				}
				CharacterHealth.OnDeath += OnDeath;
				CharacterHealth.OnHit += OnHit;
			}
		}

        /// <summary>
        /// OnDisable，我们注销OnRevive事件
        /// </summary>
        protected virtual void OnDisable()
		{
			if (CharacterHealth != null)
			{
				CharacterHealth.OnDeath -= OnDeath;
				CharacterHealth.OnHit -= OnHit;
			}			
		}
	}
}
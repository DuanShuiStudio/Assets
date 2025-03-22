using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.Feedbacks;
using System.Collections.Generic;
using UnityEngine.Events;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 扩展这个类，以便在特定区域内按下按钮时激活某些功能。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Button Activated")]
	public class ButtonActivated : TopDownMonoBehaviour 
	{
		public enum ButtonActivatedRequirements { Character, ButtonActivator, Either, None }
		public enum InputTypes { Default, Button, Key }

		[MMInspectorGroup("Requirements", true, 10)]
		[MMInformation("在这里您可以指定与该区域互动所需的条件。它是否要求具备“按钮激活”这一角色能力？是否只有玩家才能与其互动？", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// if this is true, objects with a ButtonActivator class will be able to interact with this zone
		[Tooltip("如果这是真的，那么带有“按钮激活器”（ButtonActivator）类的对象将能够与这个区域互动。")]
		public ButtonActivatedRequirements ButtonActivatedRequirement = ButtonActivatedRequirements.Either;
		/// if this is true, this can only be activated by player Characters
		[Tooltip("如果这是真的，那么这个（功能、装置等，具体需根据上下文确定）只能由玩家角色来激活。")]
		public bool RequiresPlayerType = true;
		/// if this is true, this zone can only be activated if the character has the required ability
		[Tooltip("如果这是真的，那么这个区域只有在角色具备所需能力的情况下才能被激活。\r\n\r\n")]
		public bool RequiresButtonActivationAbility = true;
        
		[MMInspectorGroup("Activation Conditions", true, 11)]

		[MMInformation("在这里您可以具体指定与该区域互动的方式。您可以使其自动激活，只有当角色处于地面状态时才激活，或者完全阻止其激活。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// if this is false, the zone won't be activable 
		[Tooltip("如果这是假的，那么这个区域将无法被激活 ")]
		public bool Activable = true;
		/// if true, the zone will activate whether the button is pressed or not
		[Tooltip("如果为真，无论是否按下按钮，该区域都将激活")]
		public bool AutoActivation = false;
		/// the delay, in seconds, during which the character has to be within the zone to activate it
		[MMCondition("AutoActivation", true)]
		[Tooltip("角色必须在这个区域内停留的延迟时间（以秒为单位），以便激活该区域")]
		public float AutoActivationDelay = 0f;
		/// if this is true, exiting the zone will reset the auto activation delay
		[MMCondition("AutoActivation", true)]
		[Tooltip("如果为真，离开该区域将重置自动激活的延迟时间")]
		public bool AutoActivationDelayResetsOnExit = true;
		/// if this is set to false, the zone won't be activable while not grounded
		[Tooltip("如果将其设置为假，那么在未着地的情况下，该区域将无法被激活")]
		public bool CanOnlyActivateIfGrounded = false;
		
		
		/// Set this to true if you want the CharacterBehaviorState to be notified of the player's entry into the zone.
		[Tooltip("如果希望角色行为状态能够通知玩家进入该区域，请将其设置为真")]
		public bool ShouldUpdateState = true;
		/// if this is true, enter won't be retriggered if another object enters, and exit will only be triggered when the last object exits
		[Tooltip("如果为真，当另一个对象进入时，“进入”事件将不会被重新触发，而“退出”事件只有在最后一个对象离开时才会被触发")]
		public bool OnlyOneActivationAtOnce = true;
		/// a layermask with all the layers that can interact with this specific button activated zone
		[Tooltip("一个图层蒙版，包含所有能与这个特定按钮激活区域互动的图层。")]
		public LayerMask TargetLayerMask = ~0;

		[MMInspectorGroup("Number of Activations", true, 12)]

		[MMInformation("您可以决定让这个区域永久可互动，或者只允许有限次数的互动，并且可以指定每次使用之间的延迟时间（以秒为单位）。", MoreMountains.Tools.MMInformationAttribute.InformationType.Info,false)]
		/// if this is set to false, your number of activations will be MaxNumberOfActivations
		[Tooltip("如果设置为假，那么您的激活次数将等于MaxNumberOfActivations")]
		public bool UnlimitedActivations = true;
		/// the number of times the zone can be interacted with
		[Tooltip("该区域可以被互动的次数")]
		public int MaxNumberOfActivations = 0;
		/// the amount of remaining activations on this zone
		[Tooltip("这个区域剩余的激活次数")]
		[MMReadOnly]
		public int NumberOfActivationsLeft;
		/// the delay (in seconds) after an activation during which the zone can't be activated
		[Tooltip("激活后的延迟时间（以秒为单位），在这个时间内该区域无法被再次激活")]
		public float DelayBetweenUses = 0f;
		/// if this is true, the zone will disable itself (forever or until you manually reactivate it) after its last use
		[Tooltip("如果为真，在其最后一次使用后，该区域将自行禁用（永久禁用或直到您手动重新激活为止）")]
		public bool DisableAfterUse = false;

		[MMInspectorGroup("Input", true, 13)]

		/// the selected input type (default, button or key)
		[Tooltip("选择的输入类型（默认、按钮或按键）")]
		public InputTypes InputType = InputTypes.Default;
		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			/// the input action to use for this button activated object
			public InputActionProperty InputSystemAction = new InputActionProperty(
				new InputAction(
					name: "ButtonActivatedAction",
					type: InputActionType.Button, 
					binding: "Keyboard/space", 
					interactions: "Press(behavior=2)"));
		#else
			/// the selected button string used to activate this zone
			[MMEnumCondition("InputType", (int)InputTypes.Button)]
			[Tooltip("用于激活该区域的选定按钮字符串")]
			public string InputButton = "Interact";
			/// the key used to activate this zone
			[MMEnumCondition("InputType", (int)InputTypes.Key)]
			[Tooltip("用于激活该区域的按键")]
			public KeyCode InputKey = KeyCode.Space;
		#endif

		[MMInspectorGroup("Animation", true, 14)]

		/// an (absolutely optional) animation parameter that can be triggered on the character when activating the zone	
		[Tooltip("一个（绝对可选的）动画参数，可以在角色激活该区域时触发	")]
		public string AnimationTriggerParameterName;

		[MMInspectorGroup("Visual Prompt", true, 15)]

		[MMInformation("您可以让这个区域显示一个视觉提示，以向玩家表明它是可互动的", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// if this is true, a prompt will be shown if setup properly
		[Tooltip("如果为真，并且设置正确，将显示一个提示")]
		public bool UseVisualPrompt = true;
		/// the gameobject to instantiate to present the prompt
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("用于实例化以显示提示的游戏对象")]
		public ButtonPrompt ButtonPromptPrefab;
		/// the text to display in the button prompt
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("要在按钮提示中显示的文本")]
		public string ButtonPromptText = "A";
		/// the text to display in the button prompt
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("按钮提示中显示的文本")]
		public Color ButtonPromptColor = MMColors.LawnGreen;
		/// the color for the prompt's text
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("提示文本的颜色")]
		public Color ButtonPromptTextColor = MMColors.White;
		/// If true, the "buttonA" prompt will always be shown, whether the player is in the zone or not.
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("如果为真，无论玩家是否在区域内，都会始终显示“buttonA”提示")]
		public bool AlwaysShowPrompt = true;
		/// If true, the "buttonA" prompt will be shown when a player is colliding with the zone
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("如果为真，当玩家与该区域发生碰撞时，将显示“buttonA”提示")]
		public bool ShowPromptWhenColliding = true;
		/// If true, the prompt will hide after use
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("如果为真，使用后提示将隐藏")]
		public bool HidePromptAfterUse = false;
		/// the position of the actual buttonA prompt relative to the object's center
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("实际的buttonA提示相对于物体中心的位置。")]
		public Vector3 PromptRelativePosition = Vector3.zero;
		/// the rotation of the actual buttonA prompt 
		[MMCondition("UseVisualPrompt", true)]
		[Tooltip("实际的buttonA提示的旋转")]
		public Vector3 PromptRotation = Vector3.zero;

		[MMInspectorGroup("Feedbacks", true, 16)]
		
		/// a feedback to play when the zone gets activated
		[Tooltip("当该区域被激活时要播放的反馈")]
		public MMFeedbacks ActivationFeedback;
		/// a feedback to play when the zone tries to get activated but can't
		[Tooltip("当该区域试图被激活但却无法激活时要播放的反馈")]
		public MMFeedbacks DeniedFeedback;
		/// a feedback to play when the zone gets entered	
		[Tooltip("当进入该区域时要播放的反馈")]
		public MMFeedbacks EnterFeedback;
		/// a feedback to play when the zone gets exited	
		[Tooltip("当离开该区域时要播放的反馈")]
		public MMFeedbacks ExitFeedback;

		[MMInspectorGroup("Actions", true, 17)]
		
		/// a UnityEvent to trigger when this zone gets activated
		[Tooltip("当该区域被激活时要触发的UnityEvent")]
		public UnityEvent OnActivation;
		/// a UnityEvent to trigger when this zone gets exited
		[Tooltip("当该区域被退出时要触发的UnityEvent")]
		public UnityEvent OnExit;
		/// a UnityEvent to trigger when a character is within the zone
		[Tooltip("当角色在区域内时要触发的UnityEvent")]
		public UnityEvent OnStay;

		protected Animator _buttonPromptAnimator;
		protected ButtonPrompt _buttonPrompt;
		protected Collider _collider;
		protected Collider2D _collider2D;
		protected bool _promptHiddenForever = false;
		protected CharacterButtonActivation _characterButtonActivation;
		protected float _lastActivationTimestamp;
		protected List<GameObject> _collidingObjects;
		protected Character _currentCharacter;
		protected bool _staying = false;
		protected Coroutine _autoActivationCoroutine;
        
		public virtual bool AutoActivationInProgress { get; set; }
		public virtual float AutoActivationStartedAt { get; set; }
		public bool InputActionPerformed
		{
			get
			{
				#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
					return InputSystemAction.action.WasPressedThisFrame();
				#else
					return false;
				#endif
			}
		}

        /// <summary>
        /// 在启用时，我们初始化ButtonActivated区域
        /// </summary>
        protected virtual void OnEnable()
		{
			Initialization ();
		}

        /// <summary>
        /// Grabs components and shows prompt if needed.
        /// </summary>
        public virtual void Initialization()
		{
			_collider = this.gameObject.GetComponent<Collider>();
			_collider2D = this.gameObject.GetComponent<Collider2D>();
			NumberOfActivationsLeft = MaxNumberOfActivations;
			_collidingObjects = new List<GameObject>();

			ActivationFeedback?.Initialization(this.gameObject);
			DeniedFeedback?.Initialization(this.gameObject);
			EnterFeedback?.Initialization(this.gameObject);
			ExitFeedback?.Initialization(this.gameObject);

			if (AlwaysShowPrompt)
			{
				ShowPrompt();
			}
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				InputSystemAction.action.Enable();
			#endif
		}

        /// <summary>
        /// 在禁用时，如果需要，我们会禁用输入操作
        /// </summary>
        protected virtual void OnDisable()
		{
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				InputSystemAction.action.Disable();
			#endif
		}

		protected virtual IEnumerator TriggerButtonActionCo()
		{
			if (AutoActivationDelay <= 0f)
			{
				TriggerButtonAction();
				yield break;
			}
			else
			{
				AutoActivationInProgress = true;
				AutoActivationStartedAt = Time.time;
				yield return MMCoroutine.WaitFor(AutoActivationDelay);
				AutoActivationInProgress = false;
				TriggerButtonAction();
				yield break;
			}
		}

        /// <summary>
        /// 当按下输入按钮时，我们会检查该区域是否可以被激活，如果可以，则触发ZoneActivated。
        /// </summary>
        public virtual void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				PromptError();
				return;
			}

			_staying = true;
			ActivateZone();
		}

		public virtual void TriggerExitAction(GameObject collider)
		{
			_staying = false;
			if (OnExit != null)
			{
				OnExit.Invoke();
			}
		}

        /// <summary>
        ///使该区域可激活
        /// </summary>
        public virtual void MakeActivable()
		{
			Activable = true;
		}

        /// <summary>
        /// 使该区域不可激活
        /// </summary>
        public virtual void MakeUnactivable()
		{
			Activable = false;
		}

        /// <summary>
        /// 如果该区域尚未激活，则将其激活；如果该区域已激活，则将其停用。
        /// </summary>
        public virtual void ToggleActivable()
		{
			Activable = !Activable;
		}

		protected virtual void Update()
		{
			if (_staying && (OnStay != null))
			{
				OnStay.Invoke();
			}
		}

        /// <summary>
        /// 激活该区域
        /// </summary>
        protected virtual void ActivateZone()
		{
			if (OnActivation != null)
			{
				OnActivation.Invoke();
			}

			_lastActivationTimestamp = Time.time;

			ActivationFeedback?.PlayFeedbacks(this.transform.position);

			if (HidePromptAfterUse)
			{
				_promptHiddenForever = true;
				HidePrompt();	
			}	
			NumberOfActivationsLeft--;

			if (DisableAfterUse && (NumberOfActivationsLeft <= 0))
			{
				DisableZone();
			}
		}

        /// <summary>
        /// 触发一个错误
        /// </summary>
        public virtual void PromptError()
		{
			if (_buttonPromptAnimator != null)
			{
				_buttonPromptAnimator.SetTrigger("Error");
			}
			DeniedFeedback?.PlayFeedbacks(this.transform.position);
		}

        /// <summary>
        /// 显示按钮A的提示
        /// </summary>
        public virtual void ShowPrompt()
		{
			if (!UseVisualPrompt || _promptHiddenForever || (ButtonPromptPrefab == null))
			{
				return;
			}

            // 我们在该区域的顶部添加了一个闪烁的A提示
            if (_buttonPrompt == null)
			{
				_buttonPrompt = (ButtonPrompt)Instantiate(ButtonPromptPrefab);
				_buttonPrompt.Initialization();
				_buttonPromptAnimator = _buttonPrompt.gameObject.MMGetComponentNoAlloc<Animator>();
			}
			
			if (_collider != null)
			{
				_buttonPrompt.transform.position = _collider.bounds.center + PromptRelativePosition;
			}
			if (_collider2D != null)
			{
				_buttonPrompt.transform.position = _collider2D.bounds.center + PromptRelativePosition;
			}

			if (_buttonPrompt != null)
			{
				_buttonPrompt.transform.parent = transform;
				_buttonPrompt.transform.localEulerAngles = PromptRotation;
				_buttonPrompt.SetText(ButtonPromptText);
				_buttonPrompt.SetBackgroundColor(ButtonPromptColor);
				_buttonPrompt.SetTextColor(ButtonPromptTextColor);
				_buttonPrompt.Show();
			}
		}

        /// <summary>
        /// 隐藏按钮A的提示
        /// </summary>
        public virtual void HidePrompt()
		{
			if (_buttonPrompt != null)
			{
				_buttonPrompt.Hide();
			}
		}

        /// <summary>
        /// 禁用已激活按钮的区域
        /// </summary>
        public virtual void DisableZone()
		{
			Activable = false;
            
			if (_collider != null)
			{
				_collider.enabled = false;
			}

			if (_collider2D != null)
			{
				_collider2D.enabled = false;
			}
	            
			if (ShouldUpdateState && (_characterButtonActivation != null))
			{
				_characterButtonActivation.InButtonActivatedZone = false;
				_characterButtonActivation.ButtonActivatedZone = null;
			}
		}

        /// <summary>
        /// 启用已激活按钮的区域
        /// </summary>
        public virtual void EnableZone()
		{
			Activable = true;
            
			if (_collider != null)
			{
				_collider.enabled = true;
			}

			if (_collider2D != null)
			{
				_collider2D.enabled = true;
			}
		}

        /// <summary>
        /// 处理与2D触发器的进入碰撞
        /// </summary>
        /// <param name="collidingObject">Colliding object.</param>
        protected virtual void OnTriggerEnter2D (Collider2D collidingObject)
		{
			TriggerEnter (collidingObject.gameObject);
		}
        /// <summary>
        /// 处理与2D触发器的离开碰撞
        /// </summary>
        /// <param name="collidingObject">Colliding object.</param>
        protected virtual void OnTriggerExit2D (Collider2D collidingObject)
		{
			TriggerExit (collidingObject.gameObject);
		}
        /// <summary>
        /// 处理与触发器的进入碰撞
        /// </summary>
        /// <param name="collidingObject">Colliding object.</param>
        protected virtual void OnTriggerEnter (Collider collidingObject)
		{
			TriggerEnter (collidingObject.gameObject);
		}
        /// <summary>
        /// 处理与触发器的离开碰撞
        /// </summary>
        /// <param name="collidingObject">Colliding object.</param>
        protected virtual void OnTriggerExit (Collider collidingObject)
		{
			TriggerExit (collidingObject.gameObject);
		}

        /// <summary>
        /// 当某物与按钮激活区域碰撞时触发
        /// </summary>
        /// <param name="collider">Something colliding with the water.</param>
        protected virtual void TriggerEnter(GameObject collider)
		{            
			if (!CheckConditions(collider))
			{
				return;
			}

            // 如果我们只能在着地时激活此区域，我们检查是否有控制器以及它是否未着地。
            // 我们什么都不做并退出。
            if (CanOnlyActivateIfGrounded)
			{
				if (collider != null)
				{
					TopDownController controller = collider.gameObject.MMGetComponentNoAlloc<TopDownController>();
					if (controller != null)
					{
						if (!controller.Grounded)
						{
							return;
						}
					}
				}
			}

            // 此时该物体正在碰撞且已获得授权，我们将其添加到我们的列表中。
            _collidingObjects.Add(collider.gameObject);
			if (!TestForLastObject(collider))
			{
				return;
			}
            
			EnterFeedback?.PlayFeedbacks(this.transform.position);

			if (ShouldUpdateState)
			{
				_characterButtonActivation = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterButtonActivation>();
				if (_characterButtonActivation != null)
				{
					_characterButtonActivation.InButtonActivatedZone = true;
					_characterButtonActivation.ButtonActivatedZone = this;
					_characterButtonActivation.InButtonAutoActivatedZone = AutoActivation;
				}
			}

			if (AutoActivation)
			{
				_autoActivationCoroutine = StartCoroutine(TriggerButtonActionCo());
			}

            // 如果我们尚未显示提示，并且该区域可以被激活，我们就显示它
            if (ShowPromptWhenColliding)
			{
				ShowPrompt();	
			}
		}

        /// <summary>
        /// 当某物离开水时触发
        /// </summary>
        /// <param name="collider">Something colliding with the dialogue zone.</param>
        protected virtual void TriggerExit(GameObject collider)
		{
			if (!CheckConditions(collider))
			{
				return;
			}

			_collidingObjects.Remove(collider.gameObject);
			if (!TestForLastObject(collider))
			{
				return;
			}
            
			AutoActivationInProgress = false;
			if (_autoActivationCoroutine != null)
			{
				StopCoroutine(_autoActivationCoroutine);
			}

			if (ShouldUpdateState)
			{
				_characterButtonActivation = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterButtonActivation>();
				if (_characterButtonActivation != null)
				{
					_characterButtonActivation.InButtonActivatedZone=false;
					_characterButtonActivation.ButtonActivatedZone=null;		
				}
			}

			ExitFeedback?.PlayFeedbacks(this.transform.position);

			if ((_buttonPrompt!=null) && !AlwaysShowPrompt)
			{
				HidePrompt();	
			}

			TriggerExitAction(collider);
		}

        /// <summary>
        /// 测试离开我们区域的对象是否是最后一个剩余的对象。
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected virtual bool TestForLastObject(GameObject collider)
		{
			if (OnlyOneActivationAtOnce)
			{
				if (_collidingObjects.Count > 0)
				{
					bool lastObject = true;
					foreach (GameObject obj in _collidingObjects)
					{
						if ((obj != null) && (obj != collider))
						{
							lastObject = false;
						}
					}
					return lastObject;
				}                    
			}
			return true;            
		}

        /// <summary>
        /// 检查剩余的使用次数以及每次使用之间的最终延迟，如果该区域可以被激活则返回真
        /// </summary>
        /// <returns><c>true</c>, if number of uses was checked, <c>false</c> otherwise.</returns>
        public virtual bool CheckNumberOfUses()
		{
			if (!Activable)
			{
				return false;
			}

			if (Time.time - _lastActivationTimestamp < DelayBetweenUses)
			{
				return false;
			}

			if (UnlimitedActivations)
			{
				return true;
			}

			if (NumberOfActivationsLeft == 0)
			{
				return false;
			}

			if (NumberOfActivationsLeft > 0)
			{
				return true;
			}
			return false;
		}

        /// <summary>
        /// 决定是否应该激活这个区域
        /// </summary>
        /// <returns><c>true</c>, if conditions was checked, <c>false</c> otherwise.</returns>
        /// <param name="character">Character.</param>
        /// <param name="characterButtonActivation">Character button activation.</param>
        protected virtual bool CheckConditions(GameObject collider)
		{
			if (!MMLayers.LayerInLayerMask(collider.layer, TargetLayerMask))
			{
				return false;
			}
			
			Character character = collider.gameObject.MMGetComponentNoAlloc<Character>();

			switch (ButtonActivatedRequirement)
			{
				case ButtonActivatedRequirements.Character:
					if (character == null)
					{
						return false;
					}
					break;

				case ButtonActivatedRequirements.ButtonActivator:
					if (collider.gameObject.MMGetComponentNoAlloc<ButtonActivator>() == null)
					{
						return false;
					}
					break;

				case ButtonActivatedRequirements.Either:
					if ((character == null) && (collider.gameObject.MMGetComponentNoAlloc<ButtonActivator>() == null))
					{
						return false;
					}
					break;
			}

			if (RequiresPlayerType)
			{
				if (character == null)
				{
					return false;
				}
				if (character.CharacterType != Character.CharacterTypes.Player)
				{
					return false;
				}
			}

			if (RequiresButtonActivationAbility)
			{
				CharacterButtonActivation characterButtonActivation = collider.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterButtonActivation>();
                // 我们检查与水碰撞的物体实际上是一个TopDown控制器和一个角色
                if (characterButtonActivation == null)
				{
					return false;	
				}
				else
				{
					if (!characterButtonActivation.AbilityAuthorized)
					{
						return false;
					}
				}
			}

			return true;
		}
	}
}
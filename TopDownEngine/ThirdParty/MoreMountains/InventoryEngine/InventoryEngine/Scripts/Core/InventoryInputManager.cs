using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
	using UnityEngine.InputSystem;
#endif

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 调用游戏中的库存示例 
    /// 不过，我建议您让输入和GUI管理类来处理这些
    /// </summary>
    public class InventoryInputManager : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[Header("Targets目标")]
		[MMInformation("在这里绑定您的库存容器（打开/关闭库存时您想打开/关闭的CanvasGroup）、主要的InventoryDisplay，以及在打开时将在InventoryDisplay下显示的覆盖层。", MMInformationAttribute.InformationType.Info, false)]
		/// The CanvasGroup containing all the elements you want to show/hide when pressing the open/close inventory button
		[Tooltip("包含所有按下打开/关闭库存按钮时显示/隐藏的元素的CanvasGroup")]
		public CanvasGroup TargetInventoryContainer;
		/// The main inventory display
		[Tooltip("主要的库存显示")] 
		public InventoryDisplay TargetInventoryDisplay;
		/// The Fader that will be used under it when opening/closing the inventory
		[Tooltip("在打开/关闭库存时将用于其下的淡入淡出效果")]
		public CanvasGroup Overlay;

		[Header("Overlay覆盖层")] 
		/// the opacity of the overlay when active
		[Tooltip("激活时覆盖层的不透明度")]
		public float OverlayActiveOpacity = 0.85f;
		/// the opacity of the overlay when inactive
		[Tooltip("未激活时覆盖层的不透明度")]
		public float OverlayInactiveOpacity = 0f;

		[Header("开始行为")]
		[MMInformation("如果将 HideContainerOnStart 设置为 true，则在开始时，即使您在场景视图中将其可见，上面定义的 TargetInventoryContainer 也会自动隐藏。这在设置时很有用", MMInformationAttribute.InformationType.Info, false)]
		/// if this is true, the inventory container will be hidden automatically on start
		[Tooltip("如果这是真的，库存容器将在开始时自动隐藏")]
		public bool HideContainerOnStart = true;

		[Header("Permissions权限")]
		[MMInformation("在这里，您可以决定是仅在打开时捕获输入，还是始终捕获输入", MMInformationAttribute.InformationType.Info, false)]
		/// if this is true, the inventory container will be hidden automatically on start
		[Tooltip("如果这是真的，库存容器将在开始时自动隐藏")]
		public bool InputOnlyWhenOpen = true;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		
			[Header("Input System Key Mapping输入系统按键映射")] 

			/// 用于打开/关闭库存的按键
			public InputActionProperty ToggleInventoryKey = new InputActionProperty(
				new InputAction(
					name: "IM_Toggle",
					type: InputActionType.Button, 
					binding: "Keyboard/I", 
					interactions: "Press(behavior=2)"));
			
			/// 用于打开库存的按键
			public InputActionProperty OpenInventoryKey = new InputActionProperty(
				new InputAction(
					name: "IM_Open",
					type: InputActionType.Button, 
					interactions: "Press(behavior=2)"));
			
			/// 用于关闭库存的按键
			public InputActionProperty CloseInventoryKey = new InputActionProperty(
				new InputAction(
					name: "IM_Close",
					type: InputActionType.Button, 
					interactions: "Press(behavior=2)"));

			/// 用于打开/关闭库存的Alt键
			public InputActionProperty CancelKey = new InputActionProperty(
				new InputAction(
					name: "IM_Cancel", 
					type: InputActionType.Button, 
					binding: "Keyboard/escape", 
					interactions: "Press(behavior=2)"));
			
			/// 用于移动物品的按键
			public InputActionProperty MoveKey = new InputActionProperty(
				new InputAction(
					name: "IM_Move", 
					type: InputActionType.Button, 
					binding: "Keyboard/insert", 
					interactions: "Press(behavior=2)"));
			
			/// 用于装备物品的按键
			public InputActionProperty EquipKey = new InputActionProperty(
				new InputAction(
					name: "IM_Equip", 
					type: InputActionType.Button, 
					binding: "Keyboard/home",
					interactions: "Press(behavior=2)"));
			
			/// 用于使用物品的按键
			public InputActionProperty UseKey = new InputActionProperty(
				new InputAction(
					name: "IM_Use", 
					type: InputActionType.Button, 
					binding: "Keyboard/end",
					interactions: "Press(behavior=2)"));
			
			/// 用于装备或使用物品的按键
			public InputActionProperty EquipOrUseKey = new InputActionProperty(
				new InputAction(
					name: "IM_EquipOrUse", 
					type: InputActionType.Button, 
					binding: "Keyboard/space", 
					interactions: "Press(behavior=2)"));
			
			/// 用于丢弃物品的按键
			public InputActionProperty DropKey = new InputActionProperty(
				new InputAction(
					name: "IM_Drop", 
					type: InputActionType.Button,
					binding: "Keyboard/delete",		
					interactions: "Press(behavior=2)"));
			
			/// 用于转到下一个库存的按键
			public InputActionProperty NextInvKey = new InputActionProperty(
				new InputAction(
					name: "IM_NextInv", 
					type: InputActionType.Button, 
					binding: "Keyboard/pageDown", 
					interactions: "Press(behavior=2)"));
			
			/// 用于转到上一个库存的按键
			public InputActionProperty PrevInvKey = new InputActionProperty(
				new InputAction(
					name: "IM_PrevInv", 
					type: InputActionType.Button, 
					binding: "Keyboard/pageUp", 
					interactions: "Press(behavior=2)"));
#else
        [Header("Key Mapping按键映射")]
		[MMInformation("在这里，您需要设置各种按键绑定。默认情况下有一些，但您可以自由更改它们", MMInformationAttribute.InformationType.Info, false)]
        /// 打开/关闭库存的钥匙
        public KeyCode ToggleInventoryKey = KeyCode.I;
        /// 用于打开/关闭库存的Alt键
        public KeyCode ToggleInventoryAltKey = KeyCode.Joystick1Button6;
        /// 用于打开库存的键
        public KeyCode OpenInventoryKey;
        /// 用于关闭库存的键
        public KeyCode CloseInventoryKey;
        /// 用来打开/关闭库存的Alt键
        public KeyCode CancelKey = KeyCode.Escape;
        /// 用来打开/关闭库存的Alt键
        public KeyCode CancelKeyAlt = KeyCode.Joystick1Button7;
        /// 用于移动物品的键
        public string MoveKey = "insert";
        /// 用于移动物品的Alt键
        public string MoveAltKey = "joystick button 2";
        /// 用于装备物品的键
        public string EquipKey = "home";
        /// 用于装备物品的Alt键
        public string EquipAltKey = "home";
        /// 使用物品的键
        public string UseKey = "end";
        /// 使用物品的Alt键
        public string UseAltKey = "end";
        /// 用于装备或使用物品的键
        public string EquipOrUseKey = "space";
        /// 用于装备或使用物品的Alt键
        public string EquipOrUseAltKey = "joystick button 0";
        /// 丢弃物品的键
        public string DropKey = "delete";
        /// 丢弃物品的Alt键
        public string DropAltKey = "joystick button 1";
        /// 用于切换到下一个库存的键
        public string NextInvKey = "page down";
        /// 切换到下一个库存的Alt键
        public string NextInvAltKey = "joystick button 4";
        /// 用于切换到上一个库存的键
        public string PrevInvKey = "page up";
        /// 切换到上一个库存的Alt键
        public string PrevInvAltKey = "joystick button 5";
		#endif

		[Header("Close Bindings关闭绑定")]
        /// 当这个库存打开时，应该强制关闭的其他库存列表
        public List<string> CloseList;

		public enum ManageButtonsModes { Interactable, SetActive }
        
		[Header("Buttons按钮")]
        /// 如果这是真的，那么当目前选中的槽位不同时，InputManager将根据当前选中的槽位来更改库存控制按钮的可交互状态
        public bool ManageButtons = false;
        /// 所选模式将启用带有（可交互）的按钮，这会改变按钮的可交互状态；SetActive将启用/禁用按钮的游戏对象
        [MMCondition("ManageButtons", true)] 
		public ManageButtonsModes ManageButtonsMode = ManageButtonsModes.SetActive;
        /// 用于装备或使用物品的按钮
        [MMCondition("ManageButtons", true)]
		public Button EquipUseButton;
        /// 用于移动物品的按钮
        [MMCondition("ManageButtons", true)]
		public Button MoveButton;
        /// 用于丢弃物品的按钮
        [MMCondition("ManageButtons", true)]
		public Button DropButton;
        /// 用于装备物品的按钮
        [MMCondition("ManageButtons", true)]
		public Button EquipButton;
        /// 使用物品的按钮
        [MMCondition("ManageButtons", true)]
		public Button UseButton;
        /// 用于卸下装备的按钮
        [MMCondition("ManageButtons", true)]
		public Button UnEquipButton;

        /// 返回活动槽位
        public virtual InventorySlot CurrentlySelectedInventorySlot { get; set; }

		[Header("State状态")]
        /// 如果这是真的，那么关联的库存是打开的；否则就是关闭的
        [MMReadOnly]
		public bool InventoryIsOpen;

		protected CanvasGroup _canvasGroup;
		protected GameObject _currentSelection;
		protected InventorySlot _currentInventorySlot;
		protected List<InventoryHotbar> _targetInventoryHotbars;
		protected InventoryDisplay _currentInventoryDisplay;
		private bool _isEquipUseButtonNotNull;
		private bool _isEquipButtonNotNull;
		private bool _isUseButtonNotNull;
		private bool _isUnEquipButtonNotNull;
		private bool _isMoveButtonNotNull;
		private bool _isDropButtonNotNull;
		
		protected bool _toggleInventoryKeyPressed;
		protected bool _openInventoryKeyPressed;
		protected bool _closeInventoryKeyPressed;
		protected bool _cancelKeyPressed;
		protected bool _prevInvKeyPressed;
		protected bool _nextInvKeyPressed;
		protected bool _moveKeyPressed;
		protected bool _equipOrUseKeyPressed;
		protected bool _equipKeyPressed;
		protected bool _useKeyPressed;
		protected bool _dropKeyPressed;
		protected bool _hotbarInputPressed = false;

        /// <summary>
        /// 在开始时，我们获取引用并准备我们的热键列表
        /// </summary>
        protected virtual void Start()
		{
			_isDropButtonNotNull = DropButton != null;
			_isMoveButtonNotNull = MoveButton != null;
			_isUnEquipButtonNotNull = UnEquipButton != null;
			_isUseButtonNotNull = UseButton != null;
			_isEquipButtonNotNull = EquipButton != null;
			_isEquipUseButtonNotNull = EquipUseButton != null;
			_currentInventoryDisplay = TargetInventoryDisplay;
			InventoryIsOpen = false;
			_targetInventoryHotbars = new List<InventoryHotbar>();
			_canvasGroup = GetComponent<CanvasGroup>();
			foreach (InventoryHotbar go in FindObjectsOfType(typeof(InventoryHotbar)) as InventoryHotbar[])
			{
				_targetInventoryHotbars.Add(go);
			}
			if (HideContainerOnStart)
			{
				if (TargetInventoryContainer != null) { TargetInventoryContainer.alpha = 0; }
				if (Overlay != null) { Overlay.alpha = OverlayInactiveOpacity; }
				EventSystem.current.sendNavigationEvents = false;
				if (_canvasGroup != null)
				{
					_canvasGroup.blocksRaycasts = false;
				}
			}
		}

        /// <summary>
        /// 每一帧，我们检查库存、热键的输入，并检查当前的选择。
        /// </summary>
        protected virtual void Update()
		{
			HandleInventoryInput();
			HandleHotbarsInput();
			CheckCurrentlySelectedSlot();
			HandleButtons();
		}

        /// <summary>
        /// 每一帧，我们检查并存储当前选中的对象
        /// </summary>
        protected virtual void CheckCurrentlySelectedSlot()
		{
			_currentSelection = EventSystem.current.currentSelectedGameObject;
			if (_currentSelection == null)
			{
				return;
			}
			_currentInventorySlot = _currentSelection.gameObject.MMGetComponentNoAlloc<InventorySlot>();
			if (_currentInventorySlot != null)
			{
				CurrentlySelectedInventorySlot = _currentInventorySlot;
			}
		}

        /// <summary>
        /// 如果ManageButtons设置为true，将根据当前选中的槽位来决定是否让库存控件可交互
        /// </summary>
        protected virtual void HandleButtons()
		{
			if (!ManageButtons)
			{
				return;
			}
            
			if (CurrentlySelectedInventorySlot != null)
			{
				if (_isUseButtonNotNull)
				{
					SetButtonState(UseButton, CurrentlySelectedInventorySlot.Usable() && CurrentlySelectedInventorySlot.UseButtonShouldShow());
				}

				if (_isEquipButtonNotNull)
				{
					SetButtonState(EquipButton, CurrentlySelectedInventorySlot.Equippable() && CurrentlySelectedInventorySlot.EquipButtonShouldShow());
				}

				if (_isEquipUseButtonNotNull)
				{
					SetButtonState(EquipUseButton, (CurrentlySelectedInventorySlot.Usable() ||
					                                CurrentlySelectedInventorySlot.Equippable()) && CurrentlySelectedInventorySlot.EquipUseButtonShouldShow());
				}

				if (_isUnEquipButtonNotNull)
				{
					SetButtonState(UnEquipButton, CurrentlySelectedInventorySlot.Unequippable() && CurrentlySelectedInventorySlot.UnequipButtonShouldShow());
				}

				if (_isMoveButtonNotNull)
				{
					SetButtonState(MoveButton, CurrentlySelectedInventorySlot.Movable() && CurrentlySelectedInventorySlot.MoveButtonShouldShow());
				}

				if (_isDropButtonNotNull)
				{
					SetButtonState(DropButton, CurrentlySelectedInventorySlot.Droppable() && CurrentlySelectedInventorySlot.DropButtonShouldShow());
				}
			}
			else
			{
				SetButtonState(UseButton, false);
				SetButtonState(EquipButton, false);
				SetButtonState(EquipUseButton, false);
				SetButtonState(DropButton, false);
				SetButtonState(MoveButton, false);
				SetButtonState(UnEquipButton, false);
			}
		}

        /// <summary>
        /// 一种用于打开或关闭按钮的内部方法
        /// </summary>
        /// <param name="targetButton"></param>
        /// <param name="state"></param>
        protected virtual void SetButtonState(Button targetButton, bool state)
		{
			if (ManageButtonsMode == ManageButtonsModes.Interactable)
			{
				targetButton.interactable = state;
			}
			else
			{
				targetButton.gameObject.SetActive(state);
			}
		}

        /// <summary>
        /// 根据其当前状态打开或关闭库存面板
        /// </summary>
        public virtual void ToggleInventory()
		{
			if (InventoryIsOpen)
			{
				CloseInventory();
			}
			else
			{
				OpenInventory();
			}
		}

        /// <summary>
        /// 打开库存面板
        /// </summary>
        public virtual void OpenInventory()
		{
			if (CloseList.Count > 0)
			{
				foreach (string playerID in CloseList)
				{
					MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloseRequest, null, "", null, 0, 0, playerID);
				}
			}
            
			if (_canvasGroup != null)
			{
				_canvasGroup.blocksRaycasts = true;
			}

            // 我们打开我们的库存
            MMInventoryEvent.Trigger(MMInventoryEventType.InventoryOpens, null, TargetInventoryDisplay.TargetInventoryName, TargetInventoryDisplay.TargetInventory.Content[0], 0, 0, TargetInventoryDisplay.PlayerID);
			MMGameEvent.Trigger("inventoryOpens");
			InventoryIsOpen = true;

			StartCoroutine(MMFade.FadeCanvasGroup(TargetInventoryContainer, 0.2f, 1f));
			StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, OverlayActiveOpacity));
		}

        /// <summary>
        /// 关闭库存面板
        /// </summary>
        public virtual void CloseInventory()
		{
			if (_canvasGroup != null)
			{
				_canvasGroup.blocksRaycasts = false;
			}
            // 我们关闭我们的库存
            MMInventoryEvent.Trigger(MMInventoryEventType.InventoryCloses, null, TargetInventoryDisplay.TargetInventoryName, null, 0, 0, TargetInventoryDisplay.PlayerID);
			MMGameEvent.Trigger("inventoryCloses");
			InventoryIsOpen = false;

			StartCoroutine(MMFade.FadeCanvasGroup(TargetInventoryContainer, 0.2f, 0f));
			StartCoroutine(MMFade.FadeCanvasGroup(Overlay, 0.2f, OverlayInactiveOpacity));
		}

        /// <summary>
        /// 处理与库存相关的输入并对其采取行动
        /// </summary>
        protected virtual void HandleInventoryInput()
		{
            // 如果我们没有当前的库存显示，我们就不进行任何操作并退出
            if (_currentInventoryDisplay == null)
			{
				return;
			}
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				_toggleInventoryKeyPressed = ToggleInventoryKey.action.WasPressedThisFrame();
				_openInventoryKeyPressed = OpenInventoryKey.action.WasPressedThisFrame();
				_closeInventoryKeyPressed = CloseInventoryKey.action.WasPressedThisFrame();
				_cancelKeyPressed = CancelKey.action.WasPressedThisFrame();
				_prevInvKeyPressed = PrevInvKey.action.WasPressedThisFrame();
				_nextInvKeyPressed = NextInvKey.action.WasPressedThisFrame();
				_moveKeyPressed = MoveKey.action.WasPressedThisFrame();
				_equipOrUseKeyPressed = EquipOrUseKey.action.WasPressedThisFrame();
				_equipKeyPressed = EquipKey.action.WasPressedThisFrame();
				_useKeyPressed = UseKey.action.WasPressedThisFrame();
				_dropKeyPressed = DropKey.action.WasPressedThisFrame();
			#else
				_toggleInventoryKeyPressed = Input.GetKeyDown(ToggleInventoryKey) || Input.GetKeyDown(ToggleInventoryAltKey);
				_openInventoryKeyPressed = Input.GetKeyDown(OpenInventoryKey);
				_closeInventoryKeyPressed = Input.GetKeyDown(CloseInventoryKey);
				_cancelKeyPressed = (Input.GetKeyDown(CancelKey)) || (Input.GetKeyDown(CancelKeyAlt));
				_prevInvKeyPressed = Input.GetKeyDown(PrevInvKey) || Input.GetKeyDown(PrevInvAltKey);
				_nextInvKeyPressed = Input.GetKeyDown(NextInvKey) || Input.GetKeyDown(NextInvAltKey);
				_moveKeyPressed = (Input.GetKeyDown(MoveKey) || Input.GetKeyDown(MoveAltKey));
				_equipOrUseKeyPressed = Input.GetKeyDown(EquipOrUseKey) || Input.GetKeyDown(EquipOrUseAltKey);
				_equipKeyPressed = Input.GetKeyDown(EquipKey) || Input.GetKeyDown(EquipAltKey);
				_useKeyPressed = Input.GetKeyDown(UseKey) || Input.GetKeyDown(UseAltKey);
				_dropKeyPressed = Input.GetKeyDown(DropKey) || Input.GetKeyDown(DropAltKey);
#endif

            // 如果用户按下了“切换库存”键
            if (_toggleInventoryKeyPressed)
			{
				ToggleInventory();
			}

			if (_openInventoryKeyPressed)
			{
				OpenInventory();
			}

			if (_closeInventoryKeyPressed)
			{
				CloseInventory();
			}

			if (_cancelKeyPressed)
			{
				if (InventoryIsOpen)
				{
					CloseInventory();
				}
			}

            // 如果仅在打开时授权输入，并且库存当前是关闭的，那么我们就不进行任何操作并退出
            if (InputOnlyWhenOpen && !InventoryIsOpen)
			{
				return;
			}

            // 之前的库存面板
            if (_prevInvKeyPressed)
			{
				if (_currentInventoryDisplay.GoToInventory(-1) != null)
				{
					_currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(-1);
				}
			}

            // 下一个库存面板
            if (_nextInvKeyPressed)
			{
				if (_currentInventoryDisplay.GoToInventory(1) != null)
				{
					_currentInventoryDisplay = _currentInventoryDisplay.GoToInventory(1);
				}
			}

			// 移动
			if (_moveKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					if (CurrentlySelectedInventorySlot.CurrentItem != null && CurrentlySelectedInventorySlot.CurrentItem.DisplayProperties.AllowMoveShortcut)
					{
						CurrentlySelectedInventorySlot.Move();
					}
				}
			}

            // 装备或使用
            if (_equipOrUseKeyPressed)
			{
				if (CurrentlySelectedInventorySlot.CurrentItem != null && CurrentlySelectedInventorySlot.CurrentItem.DisplayProperties.AllowEquipUseShortcut)
				{
					EquipOrUse();
				}
			}

			// 装备
			if (_equipKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					if (CurrentlySelectedInventorySlot.CurrentItem != null && CurrentlySelectedInventorySlot.CurrentItem.DisplayProperties.AllowEquipShortcut)
					{
						CurrentlySelectedInventorySlot.Equip();
					}
				}
			}

			// 使用
			if (_useKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					if (CurrentlySelectedInventorySlot.CurrentItem != null && CurrentlySelectedInventorySlot.CurrentItem.DisplayProperties.AllowUseShortcut)
					{
						CurrentlySelectedInventorySlot.Use();
					}
				}
			}

			// 丢弃
			if (_dropKeyPressed)
			{
				if (CurrentlySelectedInventorySlot != null)
				{
					if (CurrentlySelectedInventorySlot.CurrentItem != null && CurrentlySelectedInventorySlot.CurrentItem.DisplayProperties.AllowDropShortcut)
					{
						CurrentlySelectedInventorySlot.Drop();
					}
				}
			}
		}

        /// <summary>
        /// 检查热键输入并对其采取行动
        /// </summary>
        protected virtual void HandleHotbarsInput()
		{
			if (!InventoryIsOpen)
			{
				foreach (InventoryHotbar hotbar in _targetInventoryHotbars)
				{
					if (hotbar != null)
					{
						#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
						_hotbarInputPressed = hotbar.HotbarInputAction.action.WasPressedThisFrame();
						#else
						_hotbarInputPressed = Input.GetKeyDown(hotbar.HotbarKey) || Input.GetKeyDown(hotbar.HotbarAltKey);
						#endif
						
						if (_hotbarInputPressed)
						{
							hotbar.Action();
						}
					}
				}
			}
		}

        /// <summary>
        /// 按下装备/使用按钮时，我们确定要调用这两种方法中的哪一种
        /// </summary>
        public virtual void EquipOrUse()
		{
			if (CurrentlySelectedInventorySlot.Equippable())
			{
				CurrentlySelectedInventorySlot.Equip();
			}
			if (CurrentlySelectedInventorySlot.Usable())
			{
				CurrentlySelectedInventorySlot.Use();
			}
		}

		public virtual void Equip()
		{
			CurrentlySelectedInventorySlot.Equip();
		}

		public virtual void Use()
		{
			CurrentlySelectedInventorySlot.Use();
		}

		public virtual void UnEquip()
		{
			CurrentlySelectedInventorySlot.UnEquip();
		}

        /// <summary>
        /// 触发选中槽位的移动方法
        /// </summary>
        public virtual void Move()
		{
			CurrentlySelectedInventorySlot.Move();
		}

        /// <summary>
        /// 触发选中槽位的丢弃方法
        /// </summary>
        public virtual void Drop()
		{
			CurrentlySelectedInventorySlot.Drop();
		}

        /// <summary>
        /// 捕获MMInventoryEvents（可能是鼠标、键盘或其他与库存相关的事件）并对其采取行动
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.PlayerID != TargetInventoryDisplay.PlayerID)
			{
				return;
			}
            
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryCloseRequest)
			{
				CloseInventory();
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听MMInventoryEvents
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				ToggleInventoryKey.action.Enable();
				OpenInventoryKey.action.Enable();
				CloseInventoryKey.action.Enable();
				CancelKey.action.Enable();
				MoveKey.action.Enable();
				EquipKey.action.Enable();
				UseKey.action.Enable();
				EquipOrUseKey.action.Enable();
				DropKey.action.Enable();
				NextInvKey.action.Enable();
				PrevInvKey.action.Enable();
			#endif
		}

        /// <summary>
        /// 在禁用时，我们停止监听MMInventoryEvents
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
				ToggleInventoryKey.action.Disable();
				OpenInventoryKey.action.Disable();
				CloseInventoryKey.action.Disable();
				CancelKey.action.Disable();
				MoveKey.action.Disable();
				EquipKey.action.Disable();
				UseKey.action.Disable();
				EquipOrUseKey.action.Disable();
				DropKey.action.Disable();
				NextInvKey.action.Disable();
				PrevInvKey.action.Disable();
			#endif
		}
	}
}
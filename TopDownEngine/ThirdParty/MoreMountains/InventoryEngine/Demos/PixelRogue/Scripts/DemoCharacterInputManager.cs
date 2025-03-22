using UnityEngine;
using MoreMountains.Tools;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 一个非常简单的输入管理器，用于处理演示角色的输入并使其移动
    /// </summary>
    public class DemoCharacterInputManager : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
        /// 将在关卡中移动的角色
        [MMInformation("demo-这个组件是一个非常简单的输入管理器，它处理演示角色的输入并使其移动。如果你从场景中移除它，你的角色将无法再移动。", MMInformationAttribute.InformationType.Info,false)]
		public InventoryDemoCharacter DemoCharacter ;
		
		[Header("demo-Input输入")]
		
		#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
		
		public InputActionProperty LeftInputAction = new InputActionProperty(
			new InputAction(
				name: "IM_Demo_LeftKey",
				type: InputActionType.Button, 
				binding: "Keyboard/leftArrow", 
				interactions: "Press(behavior=2)"));
		
		public InputActionProperty RightInputAction = new InputActionProperty(
			new InputAction(
				name: "IM_Demo_RightKey",
				type: InputActionType.Button, 
				binding: "Keyboard/rightArrow", 
				interactions: "Press(behavior=2)"));
		
		public InputActionProperty UpInputAction = new InputActionProperty(
			new InputAction(
				name: "IM_Demo_UpKey",
				type: InputActionType.Button, 
				binding: "Keyboard/upArrow", 
				interactions: "Press(behavior=2)"));
		
		public InputActionProperty DownInputAction = new InputActionProperty(
			new InputAction(
				name: "IM_Demo_DownKey",
				type: InputActionType.Button, 
				binding: "Keyboard/downArrow", 
				interactions: "Press(behavior=2)"));
		
		#else

		public KeyCode LeftKey = KeyCode.LeftArrow;
		public KeyCode LeftKeyAlt = KeyCode.None;
		public KeyCode RightKey = KeyCode.RightArrow;
		public KeyCode RightKeyAlt = KeyCode.None;
		public KeyCode DownKey = KeyCode.DownArrow;
		public KeyCode DownKeyAlt = KeyCode.None;
		public KeyCode UpKey = KeyCode.UpArrow;
		public KeyCode UpKeyAlt = KeyCode.None;
	    
		#endif
	        
		public string VerticalAxisName = "Vertical";

		protected bool _pause = false;

        /// <summary>
        /// 每一帧，我们都会检查物品栏、热键栏和角色的输入
        /// </summary>
        protected virtual void Update ()
		{
			HandleDemoCharacterInput();
		}

        /// <summary>
        /// 处理演示角色的移动输入。
        /// </summary>
        protected virtual void HandleDemoCharacterInput()
		{
			if (_pause)
			{
				DemoCharacter.SetMovement(0,0);
				return;
			}

			float horizontalMovement = 0f;
			float verticalMovement = 0f;
			
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			
			if (LeftInputAction.action.IsPressed()) { horizontalMovement = -1f; }
			if (RightInputAction.action.IsPressed()) { horizontalMovement = 1f; }
			if (DownInputAction.action.IsPressed()) { verticalMovement = -1f; }
			if (UpInputAction.action.IsPressed()) { verticalMovement = 1f; }
			
			#else
				if ( (Input.GetKey(LeftKey)) || (Input.GetKey(LeftKeyAlt)) ) { horizontalMovement = -1f; }
				if ( (Input.GetKey(RightKey)) || (Input.GetKey(RightKeyAlt)) ) { horizontalMovement = 1f; }
				if ( (Input.GetKey(DownKey)) || (Input.GetKey(DownKeyAlt)) ) { verticalMovement = -1f; }
				if ( (Input.GetKey(UpKey)) || (Input.GetKey(UpKeyAlt)) ) { verticalMovement = 1f; }
			#endif
			
			
			
				
			
			DemoCharacter.SetMovement(horizontalMovement,verticalMovement);
		}

        /// <summary>
        /// 捕捉MMInventoryEvents以检测暂停
        /// </summary>
        /// <param name="inventoryEvent">Inventory event.</param>
        public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryOpens)
			{
				_pause = true;
			}
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryCloses)
			{
				_pause = false;
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听MMInventoryEvents
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			UpInputAction.action.Enable();
			DownInputAction.action.Enable();
			LeftInputAction.action.Enable();
			RightInputAction.action.Enable();
			#endif
		}

        /// <summary>
        /// 在禁用时，我们停止监听MMInventoryEvents
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
			#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			UpInputAction.action.Disable();
			DownInputAction.action.Disable();
			LeftInputAction.action.Disable();
			RightInputAction.action.Disable();
			#endif
		}
	}
}
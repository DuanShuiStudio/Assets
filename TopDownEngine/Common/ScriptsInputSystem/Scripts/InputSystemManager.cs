using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果您更喜欢使用Unity的InputSystem而不是传统的输入系统，这是一个替换用的InputManager
    /// 请注意，它目前不是引擎中的默认解决方案，因为Unity的旧版本不支持它
    /// 而且大多数人仍然不使用它
    /// 你可以在MinimalScene3D_InputSystem演示场景中看到一个如何设置的示例
    /// </summary>
    public class InputSystemManager : InputManager
    {
        /// 一组用于读取输入的输入操作
        public TopDownEngineInputActions InputActions;
        /// 鼠标的位置
        public override Vector2 MousePosition => Mouse.current.position.ReadValue();

        protected Vector2 _primaryMovementInput;
        protected Vector2 _secondaryMovementInput;

        protected override void Awake()
        {
            base.Awake();
            InputActions = new TopDownEngineInputActions();
        }

        /// <summary>
        /// 在初始化时，我们注册所有的操作
        /// </summary>
        protected override void Initialization()
        {
            base.Initialization();

            InputActions.PlayerControls.PrimaryMovement.performed += context =>
            {
                _primaryMovementInput = context.ReadValue<Vector2>();
                TestForceDesktop();
            };
            InputActions.PlayerControls.SecondaryMovement.performed += context => _secondaryMovementInput = context.ReadValue<Vector2>();
            InputActions.PlayerControls.CameraRotation.performed += context => _cameraRotationInput = context.ReadValue<float>();

            InputActions.PlayerControls.Jump.performed += context => { BindButton(context, JumpButton); };
            InputActions.PlayerControls.Run.performed += context => { BindButton(context, RunButton); };
            InputActions.PlayerControls.Dash.performed += context => { BindButton(context, DashButton); };
            InputActions.PlayerControls.Crouch.performed += context => { BindButton(context, CrouchButton); };
            InputActions.PlayerControls.Shoot.performed += context => { BindButton(context, ShootButton); };
            InputActions.PlayerControls.SecondaryShoot.performed += context => { BindButton(context, SecondaryShootButton); };
            InputActions.PlayerControls.Interact.performed += context => { BindButton(context, InteractButton); };
            InputActions.PlayerControls.Reload.performed += context => { BindButton(context, ReloadButton); };
            InputActions.PlayerControls.Pause.performed += context => { BindButton(context, PauseButton); };
            InputActions.PlayerControls.SwitchWeapon.performed += context => { BindButton(context, SwitchWeaponButton); };
            InputActions.PlayerControls.SwitchCharacter.performed += context => { BindButton(context, SwitchCharacterButton); };
            InputActions.PlayerControls.TimeControl.performed += context => { BindButton(context, TimeControlButton); };
        }

        protected virtual void TestForceDesktop()
        {
            if ((Mathf.Abs(_primaryMovement.x) > Threshold.x) ||
             (Mathf.Abs(_primaryMovement.y) > Threshold.y))
            {
                _primaryAxisActiveTimestamp = Time.unscaledTime;
                
                if (IsMobile && ForceDesktopIfPrimaryAxisActive)
                {
                    IsMobile = false;
                    IsPrimaryAxisActive = true;
                    if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(false); }
                }
            }
            
        }

        protected override void Update()
        {
            TestAutoRevert();
            _primaryMovement = ApplyCameraRotation(_primaryMovementInput);
            _secondaryMovement = ApplyCameraRotation(_secondaryMovementInput);
        }

        protected virtual void TestAutoRevert()
        {
            if (!IsMobile && ForceDesktopIfPrimaryAxisActive && AutoRevertToMobileIfPrimaryAxisInactive)
            {
                if (Time.unscaledTime - _primaryAxisActiveTimestamp > AutoRevertToMobileIfPrimaryAxisInactiveDuration)
                {
                    if (GUIManager.HasInstance) { GUIManager.Instance.SetMobileControlsActive(true, MovementControl); }
                    IsMobile = true;
                    IsPrimaryAxisActive = false;
                }
            }
        }

        /// <summary>
        /// 根据输入值改变按钮的状态
        /// </summary>
        /// <param name="context"></param>
        /// <param name="imButton"></param>
        protected virtual void BindButton(InputAction.CallbackContext context, MMInput.IMButton imButton)
        {
            if (!InputDetectionActive)
            {
                return;
            }
        
            var control = context.control;

            if (control is ButtonControl button)
            {
                if (button.wasPressedThisFrame)
                {
                    imButton.State.ChangeState(MMInput.ButtonStates.ButtonDown);
                }
                if ( button.wasReleasedThisFrame 
                    || (imButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed && !button.isPressed) )
                {
                    imButton.State.ChangeState(MMInput.ButtonStates.ButtonUp);
                }
            }
        }

        protected override void GetInputButtons()
        {
            // 现在没用了
        }

        public override void SetMovement()
        {
            //什么都不做
        }

        public override void SetSecondaryMovement()
        {
            //什么都不做
        }

        protected override void SetShootAxis()
        {
            //什么都不做
        }

        protected override void SetCameraRotationAxis()
        {
            // 什么都不做
        }

        protected override void TestPrimaryAxis()
        {
            // 什么都不做
        }

        /// <summary>
        /// 在启用时，我们启用输入操作
        /// </summary>
        protected virtual void OnEnable()
        {
            InputActions.Enable();
        }

        /// <summary>
        /// 在禁用时，我们禁用输入操作
        /// </summary>
        protected virtual void OnDisable()
        {
            InputActions.Disable();
        }
    }
}
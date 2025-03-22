using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Users;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果您在多人游戏环境中更喜欢使用Unity的InputSystem而不是传统的输入系统，这是一个替换用的InputManager
    /// 请注意，它目前不是引擎中的默认解决方案，因为Unity的旧版本不支持它
    /// 而且大多数人仍然不使用它
    /// 你可以在MinimalScene3D_InputSystem_Multiplayer演示场景中看到一个如何设置的示例。
    /// </summary>
    public class InputSystemManagerEventsBased : InputManager
    {
        /// 鼠标的位置
        public override Vector2 MousePosition => Mouse.current.position.ReadValue();
        
        public void OnJump(InputAction.CallbackContext context) { BindButton(context, JumpButton); }
        public void OnPrimaryMovement(InputAction.CallbackContext context) { _primaryMovement = ApplyCameraRotation(context.ReadValue<Vector2>());  }
        public void OnSecondaryMovement(InputAction.CallbackContext context) { _secondaryMovement = ApplyCameraRotation(context.ReadValue<Vector2>()); }
        public void OnRun(InputAction.CallbackContext context) { BindButton(context, RunButton); }
        public void OnDash(InputAction.CallbackContext context) { BindButton(context, DashButton); }
        public void OnCrouch(InputAction.CallbackContext context) { BindButton(context, CrouchButton); }
        public void OnShoot(InputAction.CallbackContext context) { BindButton(context, ShootButton); }
        public void OnSecondaryShoot(InputAction.CallbackContext context) { BindButton(context, SecondaryShootButton); }
        public void OnInteract(InputAction.CallbackContext context) { BindButton(context, InteractButton); }
        public void OnReload(InputAction.CallbackContext context) { BindButton(context, ReloadButton); }
        public void OnPause(InputAction.CallbackContext context) { BindButton(context, PauseButton); }
        public void OnSwitchWeapon(InputAction.CallbackContext context) { BindButton(context, SwitchWeaponButton); }
        public void OnSwitchCharacter(InputAction.CallbackContext context) { BindButton(context, SwitchCharacterButton); }
        public void OnTimeControl(InputAction.CallbackContext context) { BindButton(context, TimeControlButton); }

        /// <summary>
        /// 根据输入值改变按钮的状态
        /// </summary>
        /// <param name="context"></param>
        /// <param name="imButton"></param>
        protected virtual void BindButton(InputAction.CallbackContext context, MMInput.IMButton imButton)
        {
            if (!context.performed)
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
                if (button.wasReleasedThisFrame)
                {
                    imButton.State.ChangeState(MMInput.ButtonStates.ButtonUp);
                }
            }
        }
        
        protected override void TestPrimaryAxis()
        {
            // 什么也不做
        }

        protected override void GetInputButtons()
        {
            // 现在已经弃用
        }

        public override void SetMovement()
        {
            //什么也不做
        }

        public override void SetSecondaryMovement()
        {
            //什么也不做
        }

        protected override void SetCameraRotationAxis()
        {
            // 什么也不做
        }

        protected override void SetShootAxis()
        {
            //什么也不做
        }
    }
}



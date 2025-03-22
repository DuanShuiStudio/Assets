using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// InputSystemManager的一个版本，让你定义一组要读取的输入操作
    /// </summary>
    public class InputSystemManagerBase<T> : InputManager where T : IInputActionCollection, new()
    {
        /// 一组用于读取输入的输入操作
        public T InputActions;

        /// 鼠标的位置
        public override Vector2 MousePosition => Mouse.current.position.ReadValue();

        protected override void Awake()
        {
            base.Awake();
            InputActions = new T();
        }

        /// <summary>
        /// 根据输入值改变按钮的状态
        /// </summary>
        /// <param name="context"></param>
        /// <param name="imButton"></param>
        protected virtual void BindButton(InputAction.CallbackContext context, MMInput.IMButton imButton)
        {
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

        protected override void Update()
        {
            // base.Update();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

namespace MoreMountains.TopDownEngine
{
	/// <summary>
	/// Add this component to a WeaponAim, and it'll automatically handle switching its weapon aim control mode to mouse if mouse becomes active.
	/// If you then touch any of the gamepad axis again, it'll switch back aim control to it.
	/// The WeaponAim control mode needs to be initially set to a gamepad control mode
	/// </summary>
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Aim Mouse Override")]
	public class WeaponAimMouseOverride : MonoBehaviour
	{
		[Header("Behavior")]
		[MMInformation("将此组件添加到一个WeaponAim，如果鼠标变为活动状态，它将自动处理将其武器瞄准控制模式切换到鼠标。" +
                       "如果您再次触摸游戏手柄的任何轴，它将切换回该轴的瞄准控制 " +
                       "WeaponAim的控制模式需要最初设置为游戏手柄控制模式", 
						MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		
		/// if this is true, mouse position will be evaluated, and if it differs from the one last frame, we'll switch to mouse control mode
		[Tooltip("如果这是真的，将评估鼠标位置，并且如果它与上一帧的不同，我们将切换到鼠标控制模式")]
		public bool CheckMouse = true;
		/// if this is true, the primary axis will be evaluated, and if it differs from the one last frame, we'll switch back to the initial control mode
		[Tooltip("如果这是真的，将评估主轴，并且如果它与上一帧的不同，我们将切换回初始控制模式")]
		public bool CheckPrimaryAxis = true;
		/// if this is true, the secondary axis will be evaluated, and if it differs from the one last frame, we'll switch back to the initial control mode
		[Tooltip("如果这是真的，将评估次级轴，并且如果它与上一帧的不同，我们将切换回初始控制模式")]
		public bool CheckSecondaryAxis = true;
		
		protected WeaponAim _weaponAim;
		protected Vector2 _primaryAxisInput;
		protected Vector2 _primaryAxisInputLastFrame;
		protected Vector2 _secondaryAxisInput;
		protected Vector2 _secondaryAxisInputLastFrame;
		protected Vector2 _mouseInput;
		protected Vector2 _mouseInputLastFrame;
		protected WeaponAim.AimControls _initialAimControl;

        /// <summary>
        /// 在Awake时，我们存储我们的WeaponAim组件并获取我们的初始瞄准控制模式。
        /// </summary>
        protected virtual void Awake()
		{
			_weaponAim = this.gameObject.GetComponent<WeaponAim>();
			GetInitialAimControl();
		}

        /// <summary>
        /// 将当前的瞄准控制模式设置为初始模式，这是组件从鼠标模式返回时将切换回的模式。
        /// </summary>
        public virtual void GetInitialAimControl()
		{
			_initialAimControl = _weaponAim.AimControl;
			if (_weaponAim.AimControl == WeaponAim.AimControls.Mouse)
			{
				Debug.LogWarning(this.gameObject + " : this component requires that you set its associated WeaponAim to a control mode other than Mouse.");
			}
		}

        /// <summary>
        /// 在更新时，检查鼠标和轴，并存储上一帧的数据。
        /// </summary>
        protected virtual void Update()
		{
			CheckMouseInput();
			CheckAxisInput();
			StoreLastFrameData();
		}

        /// <summary>
        /// 我们存储当前的输入数据，以便在下一帧进行比较。
        /// </summary>
        protected virtual void StoreLastFrameData()
		{
			_mouseInputLastFrame = _mouseInput;
			_primaryAxisInputLastFrame = _primaryAxisInput;
			_secondaryAxisInputLastFrame = _secondaryAxisInput;
		}

        /// <summary>
        /// 检查鼠标输入是否已更改，如果是，则切换到鼠标控制。
        /// </summary>
        protected virtual void CheckMouseInput()
		{
			if (!CheckMouse)
			{
				return;
			}

			_mouseInput = _weaponAim.TargetWeapon.Owner.LinkedInputManager.MousePosition;
			if (_mouseInput != _mouseInputLastFrame)
			{
				SwitchToMouse();
			}
		}

        /// <summary>
        /// 检查轴输入是否已更改，如果是，则切换回初始控制模式
        /// </summary>
        protected virtual void CheckAxisInput()
		{
			if (CheckPrimaryAxis)
			{
				_primaryAxisInput = _weaponAim.TargetWeapon.Owner.LinkedInputManager.PrimaryMovement;
				if (_primaryAxisInput != _primaryAxisInputLastFrame)
				{
					SwitchToInitialControlMode();
				}
			}

			if (CheckSecondaryAxis)
			{
				_secondaryAxisInput = _weaponAim.TargetWeapon.Owner.LinkedInputManager.SecondaryMovement;
				if (_secondaryAxisInput != _secondaryAxisInputLastFrame)
				{
					SwitchToInitialControlMode();
				}
			}
		}

        /// <summary>
        /// 将瞄准控制模式更改为鼠标
        /// </summary>
        public virtual void SwitchToMouse()
		{
			_weaponAim.AimControl = WeaponAim.AimControls.Mouse;
		}

        /// <summary>
        /// 将瞄准控制从鼠标模式更改为初始模式
        /// </summary>
        public virtual void SwitchToInitialControlMode()
		{
			_weaponAim.AimControl = _initialAimControl;
		}
	}	
}


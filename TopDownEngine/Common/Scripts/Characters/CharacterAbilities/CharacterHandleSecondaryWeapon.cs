using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此职业添加到角色中，使其可以使用武器
    /// 注意，该组件将触发动画（如果它们的参数存在于Animator中），基于
    /// 当前武器的动画
    /// 动画器参数：从武器的检查器中定义
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Handle Secondary Weapon")]
	public class CharacterHandleSecondaryWeapon : CharacterHandleWeapon
	{
        /// 这个CharacterHandleWeapon的ID /索引。这将被用来决定什么处理武器能力应该装备武器。
        ///  如果你创造了更多的“处理武器”技能，请确保覆盖并增加它
        public override int HandleWeaponID { get { return 2; } }

        /// <summary>
        /// 获取输入并根据按下的内容触发方法
        /// </summary>
        protected override void HandleInput()
		{
			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal)
			    || (CurrentWeapon == null))
			{
				return;
			}

			bool inputAuthorized = true;
			if (CurrentWeapon != null)
			{
				inputAuthorized = CurrentWeapon.InputAuthorized;
			}
			
			if (inputAuthorized && ((_inputManager.SecondaryShootButton.State.CurrentState == MMInput.ButtonStates.ButtonDown) || (_inputManager.SecondaryShootAxis == MMInput.ButtonStates.ButtonDown)))
			{
				ShootStart();
			}
			
			bool buttonPressed =
				(_inputManager.SecondaryShootButton.State.CurrentState == MMInput.ButtonStates.ButtonPressed) ||
				(_inputManager.SecondaryShootAxis == MMInput.ButtonStates.ButtonPressed); 
            
			if (inputAuthorized && ContinuousPress && (CurrentWeapon.TriggerMode == Weapon.TriggerModes.Auto) && buttonPressed)
			{
				ShootStart();
			}

			if (_inputManager.ReloadButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				Reload();
			}

			if (inputAuthorized && ((_inputManager.SecondaryShootButton.State.CurrentState == MMInput.ButtonStates.ButtonUp) || (_inputManager.SecondaryShootAxis == MMInput.ButtonStates.ButtonUp)))
			{
				ShootStop();
				CurrentWeapon.WeaponInputReleased();
			}
			
			if ((CurrentWeapon.WeaponState.CurrentState == Weapon.WeaponStates.WeaponDelayBetweenUses)
			    && ((_inputManager.SecondaryShootAxis == MMInput.ButtonStates.Off) && (_inputManager.SecondaryShootButton.State.CurrentState == MMInput.ButtonStates.Off))
			    && !(UseSecondaryAxisThresholdToShoot && (_inputManager.SecondaryMovement.magnitude > _inputManager.Threshold.magnitude)))
			{
				CurrentWeapon.WeaponInputStop();
			}

			if (inputAuthorized && UseSecondaryAxisThresholdToShoot && (_inputManager.SecondaryMovement.magnitude > _inputManager.Threshold.magnitude))
			{
				ShootStart();
			}
		}
	}
}
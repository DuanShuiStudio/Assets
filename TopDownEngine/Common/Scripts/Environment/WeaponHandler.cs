using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个简单的组件，您可以用它来控制武器，并按需启动和停止，而无需角色来处理它
    /// 您可以在KoalaHealth演示场景中看到它的实际操作，它为那个演示中的加农炮提供了动力
    /// </summary>
    public class WeaponHandler : TopDownMonoBehaviour
    {
        [Header("Weapon武器")]
        /// the weapon you want this component to pilot
        [Tooltip("您希望此组件控制的武器")]
        public Weapon TargetWeapon;

        [Header("Debug调试")] 
        [MMInspectorButton("StartShooting")]
        public bool StartShootingButton;
        [MMInspectorButton("StopShooting")]
        public bool StopShootingButton;

        /// <summary>
        /// 使相关武器开始射击
        /// </summary>
        public virtual void StartShooting()
        {
            if (TargetWeapon == null)
            {
                return;
            }
            TargetWeapon.WeaponInputStart();
        }

        /// <summary>
        /// 使相关相关武器停止射击
        /// </summary>
        public virtual void StopShooting()
        {
            if (TargetWeapon == null)
            {
                return;
            }
            TargetWeapon.WeaponInputStop();
        }
    }
}


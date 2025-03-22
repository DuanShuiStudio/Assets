using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个类将自动绘制一个圆形，以匹配自动瞄准武器的半径（如果有的话）
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
	[AddComponentMenu("TopDown Engine/Weapons/Weapon Auto Aim Radius Circle")]
	public class WeaponAutoAimRadiusCircle : MMLineRendererCircle
	{
		[Header("Weapon Radius")]
		public CharacterHandleWeapon TargetHandleWeaponAbility;

        /// <summary>
        /// 在初始化时，将它自己挂接到武器变化上
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_line = gameObject.GetComponent<LineRenderer>();
			_line.enabled = false;
            
			if (TargetHandleWeaponAbility != null)
			{
				TargetHandleWeaponAbility.OnWeaponChange += OnWeaponChange;
			}
		}

        /// <summary>
        /// 当武器变化时，如果它有自动瞄准功能，就在它周围绘制一个圆形
        /// </summary>
        void OnWeaponChange()
		{
			if (TargetHandleWeaponAbility.CurrentWeapon == null)
			{
				return;
			}
			WeaponAutoAim autoAim = TargetHandleWeaponAbility.CurrentWeapon.GetComponent<WeaponAutoAim>();
			_line.enabled = (autoAim != null);
            
			if (autoAim != null)
			{
				HorizontalRadius = autoAim.ScanRadius;
				VerticalRadius = autoAim.ScanRadius;
			}
			DrawCircle();
		}

        /// <summary>
        /// 在禁用时，我们从代理中解钩
        /// </summary>
        void OnDisable()
		{
			if (TargetHandleWeaponAbility != null)
			{
				TargetHandleWeaponAbility.OnWeaponChange -= OnWeaponChange;
			}
		}
	}
}
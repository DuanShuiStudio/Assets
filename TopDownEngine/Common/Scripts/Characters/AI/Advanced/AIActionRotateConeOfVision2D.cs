using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个AIAction会将AI的ConeOfVision2D旋转到AI的移动方向或武器瞄准方向
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Rotate Cone Of Vision 2D")]
	public class AIActionRotateConeOfVision2D : AIAction
	{
        /// 我们可以将圆锥体瞄准的可能方向
        public enum Modes { Movement, WeaponAim }
		
		[Header("Bindings绑定")]
		/// the cone of vision 2D to rotate
		[Tooltip("要旋转的2D视锥")]
		public MMConeOfVision2D TargetConeOfVision2D;
        
		[Header("Aim瞄准")] 
		/// whether to aim at the AI's movement direction or the weapon aim direction
		[Tooltip("是否瞄准AI的移动方向或武器的瞄准方向")]
		public Modes Mode = Modes.Movement;

		[Header("Interpolation插值")] 
		/// whether or not to interpolate the rotation
		[Tooltip("是否进行旋转插值")]
		public bool Interpolate = false;
		/// the rate at which to interpolate the rotation
		[Tooltip("旋转插值的速率")]
		[MMCondition("Interpolate", true)] 
		public float InterpolateRate = 5f;
        
		protected CharacterHandleWeapon _characterHandleWeapon;
		protected WeaponAim _weaponAim;
		protected TopDownController _controller;
		protected Vector3 _newAim;
		protected float _angle;
		protected Vector3 _eulerAngles = Vector3.zero;

        /// <summary>
        /// 在init中，我们获取组件
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
			_controller = this.gameObject.GetComponentInParent<TopDownController>();
			if (TargetConeOfVision2D == null)
			{
				TargetConeOfVision2D = this.gameObject.GetComponent<MMConeOfVision2D>();	
			}
		}

		public override void PerformAction()
		{
			AimCone();
		}

        /// <summary>
        /// 如果可能的话，将圆锥体瞄准运动或武器目标
        /// </summary>
        protected virtual void AimCone()
		{
			if (TargetConeOfVision2D == null)
			{
				return;
			}
            
			switch (Mode )
			{
				case Modes.Movement:
					AimAt(_controller.CurrentDirection.normalized);
					break;
				case Modes.WeaponAim:
					if (_weaponAim == null)
					{
						GrabWeaponAim();
					}
					else
					{
						AimAt(_weaponAim.CurrentAim.normalized);    
					}
					break;
			}
		}

        /// <summary>
        /// 旋转圆锥体，如果需要，插入旋转
        /// </summary>
        /// <param name="direction"></param>
        protected virtual void AimAt(Vector3 direction)
		{
			if (Interpolate)
			{
				_newAim = MMMaths.Lerp(_newAim, direction, InterpolateRate, Time.deltaTime);
			}
			else
			{
				_newAim = direction;
			}

			_angle = MMMaths.AngleBetween(this.transform.right, _newAim);
			_eulerAngles.y = -_angle;
            
			TargetConeOfVision2D.SetDirectionAndAngles(_newAim, _eulerAngles);
		}

        /// <summary>
        /// 缓存武器瞄准器
        /// </summary>
        protected virtual void GrabWeaponAim()
		{
			if ((_characterHandleWeapon != null) && (_characterHandleWeapon.CurrentWeapon != null))
			{
				_weaponAim = _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim>();
			}            
		}

        /// <summary>
        /// 一进门我们就拿起武器瞄准并藏起来
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			GrabWeaponAim();
		}
	}
}
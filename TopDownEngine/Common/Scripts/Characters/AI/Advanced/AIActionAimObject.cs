using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一种用于瞄准AI移动或瞄准方向的AIACtion
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Aim Object")]
    public class AIActionAimObject : AIAction
    {
        /// 我们可以瞄准目标物体的可能方向
        public enum Modes { Movement, WeaponAim }
        /// 瞄准运动或武器瞄准方向的轴
        public enum PossibleAxis { Right, Forward }

        [Header("Aim Object瞄准对象")]
        /// an object to aim
        [Tooltip("瞄准的对象")]
        public GameObject GameObjectToAim;
        /// whether to aim at the AI's movement direction or the weapon aim direction
        [Tooltip("是否瞄准AI的移动方向或武器的瞄准方向")]
        public Modes Mode = Modes.Movement;
        /// the axis to aim at the moment or weapon aim direction (usually right for 2D, forward for 3D)
        [Tooltip("瞄准的轴向或武器瞄准方向（通常2D为右，3D为前）")]
        public PossibleAxis Axis = PossibleAxis.Right;

        [Header("Interpolation插入")]
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

        /// <summary>
        /// 在init中，我们获取组件
        /// </summary>
        public override void Initialization()
        {
            if (!ShouldInitialize) return;
            base.Initialization();
            _characterHandleWeapon = this.gameObject.GetComponentInParent<Character>()?.FindAbility<CharacterHandleWeapon>();
            _controller = this.gameObject.GetComponentInParent<TopDownController>();
        }

		public override void PerformAction()
		{
			AimObject();
		}

        /// <summary>
        /// 如果可能的话，将目标瞄准运动或武器目标
        /// </summary>
        protected virtual void AimObject()
        {
            if (GameObjectToAim == null)
            {
                return;
            }

            switch (Mode)
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
        /// 旋转目标对象，在需要时插入旋转
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

            switch (Axis)
            {
                case PossibleAxis.Forward:
                    GameObjectToAim.transform.forward = _newAim;
                    break;
                case PossibleAxis.Right:
                    GameObjectToAim.transform.right = _newAim;
                    break;
            }
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
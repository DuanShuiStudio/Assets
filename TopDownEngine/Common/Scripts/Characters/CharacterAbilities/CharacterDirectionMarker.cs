using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个技能可以让你根据角色的移动方向或目标方向来定位物体
    /// 这个对象可以是任何你想要的东西（一个精灵，一个模型，一条线等）。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Direction Marker")]
	public class CharacterDirectionMarker : CharacterAbility
	{
		/// 可被基于的旋转模式
		public enum Modes { MovementDirection, AimDirection, None }
        
		[Header("Direction Marker方向标记")] 
		/// the object to rotate
		[Tooltip("要旋转的对象")]
		public Transform DirectionMarker;
		/// a unique ID used to reference the marker ability
		[Tooltip("用于引用标记能力的唯一ID")]
		public int DirectionMarkerID;
		/// the selected mode to pick direction on
		[Tooltip("选择方向的选定模式")]
		public Modes Mode = Modes.MovementDirection;
        
		[Header("Position位置")]
		/// the offset to apply as the center of rotation
		[Tooltip("应用于旋转中心的偏移量")]
		public Vector3 RotationCenterOffset = Vector3.zero;
		/// the axis to consider as up when aiming
		[Tooltip("瞄准时要考虑的向上轴")]
		public Vector3 UpVector = Vector3.up;
		/// the axis to consider as forward when aiming
		[Tooltip("瞄准时要考虑的向前轴")]
		public Vector3 ForwardVector = Vector3.forward;
		/// if this is true, the marker won't be able to rotate on its X axis
		[Tooltip("如果为真，则标记将无法在其X轴上旋转")]
		public bool PreventXRotation = false;
		/// if this is true, the marker won't be able to rotate on its Y axis
		[Tooltip("如果为真，则标记将无法在其Y轴上旋转")]
		public bool PreventYRotation = false;
		/// if this is true, the marker won't be able to rotate on its Z axis
		[Tooltip("如果为真，则标记将无法在其Z轴上旋转")]
		public bool PreventZRotation = false;

		[Header("Offset along magnitude沿幅值的偏移")] 
		/// whether or not to offset the position along the direction's magnitude (for example, moving faster could move the marker further away from the character)
		[Tooltip("是否沿方向的幅值偏移位置（例如，移动得更快可能会使标记离角色更远）")]
		public bool OffsetAlongMagnitude = false;
		/// the minimum bounds of the velocity's magnitude
		[Tooltip("速度幅值的最小界限")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float MinimumVelocity = 0f;
		/// the maximum bounds of the velocity's magnitude
		[Tooltip("速度幅值的最大界限")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float MaximumVelocity = 7f;
		/// the distance at which to position the marker when at the lowest velocity
		[Tooltip("当速度最低时，用于定位标记的距离")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float OffsetRemapMin = 0f;
		/// the distance at which to position the marker when at the highest velocity
		[Tooltip("当速度最高时，用于定位标记的距离")]
		[MMCondition("OffsetAlongMagnitude", true)]
		public float OffsetRemapMax = 1f;
        
		[Header("Auto Disable自动禁用")]
		/// whether or not to disable the marker when the movement magnitude is under a certain threshold
		[Tooltip("是否在运动幅值低于某个阈值时禁用标记")]
		public bool DisableBelowThreshold = false;
		/// the threshold below which to disable the marker
		[Tooltip("禁用标记的阈值下限")]
		[MMCondition("DisableBelowThreshold", true)]
		public float DisableThreshold = 0.1f;
        
		[Header("Interpolation插值")] 
		/// whether or not to interpolate the rotation
		[Tooltip("是否插值旋转")]
		public bool Interpolate = false;
		/// the rate at which to interpolate the rotation
		[Tooltip("插值旋转的速率")]
		[MMCondition("Interpolate", true)] 
		public float InterpolateRate = 5f;
        
		[Header("Interpolation插值")] 
		protected CharacterHandleWeapon _characterHandleWeapon;
		protected WeaponAim _weaponAim;
		protected Vector3 _direction;
		protected Quaternion _newRotation;
		protected Vector3 _newPosition;
		protected Vector3 _newRotationVector;

        /// <summary>
        /// 在init中，我们存储了CharacterHandleWeapon
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_characterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();
		}

        /// <summary>
        /// 在Process中，我们瞄准我们的对象
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			AimMarker();
		}

        /// <summary>
        /// 旋转对象以匹配所选方向
        /// </summary>
        protected virtual void AimMarker()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
            
			if (DirectionMarker == null)
			{
				return;
			}
            
			switch (Mode )
			{
				case Modes.MovementDirection:
					AimAt(_controller.CurrentDirection.normalized);
					ApplyOffset(_controller.Velocity.magnitude);
					break;
				case Modes.AimDirection:
					if (_weaponAim == null)
					{
						GrabWeaponAim();
					}
					else
					{
						AimAt(_weaponAim.CurrentAim.normalized);    
						ApplyOffset(_weaponAim.CurrentAim.magnitude);
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
				_direction = MMMaths.Lerp(_direction, direction, InterpolateRate, Time.deltaTime);
			}
			else
			{
				_direction = direction;
			}

			if (_direction == Vector3.zero)
			{
				return;
			}
            
			_newRotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_direction, UpVector), InterpolateRate * Time.time);

			_newRotationVector.x = PreventXRotation ? 0f : _newRotation.eulerAngles.x;
			_newRotationVector.y = PreventYRotation ? 0f : _newRotation.eulerAngles.y;
			_newRotationVector.z = PreventZRotation ? 0f : _newRotation.eulerAngles.z;
			_newRotation.eulerAngles = _newRotationVector;
            
			DirectionMarker.transform.rotation = _newRotation;
		}

        /// <summary>
        /// 如果需要，应用偏移量
        /// </summary>
        /// <param name="rawValue"></param>
        protected virtual void ApplyOffset(float rawValue)
		{
			_newPosition = RotationCenterOffset; 

			if (OffsetAlongMagnitude)
			{
				float remappedValue = MMMaths.Remap(rawValue, MinimumVelocity, MaximumVelocity, OffsetRemapMin, OffsetRemapMax);

				_newPosition += ForwardVector * remappedValue; 
				_newPosition = _newRotation * _newPosition;
			}

			if (Interpolate)
			{
				_newPosition = MMMaths.Lerp(DirectionMarker.transform.localPosition, _newPosition, InterpolateRate, Time.deltaTime);
			}

			DirectionMarker.transform.localPosition = _newPosition;

			if (DisableBelowThreshold)
			{
				DirectionMarker.gameObject.SetActive(rawValue > DisableThreshold);    
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
	}    
}
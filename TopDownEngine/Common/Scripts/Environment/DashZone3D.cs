using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此区域添加到触发碰撞体，当3D角色进入时它会自动触发冲刺
    /// </summary>
    [RequireComponent(typeof(Collider))]
	[AddComponentMenu("TopDown Engine/Environment/Dash Zone 3D")]
	public class DashZone3D : TopDownMonoBehaviour
	{
		[Header("Bindings绑定")]

		/// the collider of the obstacle you want to dash over
		[Tooltip("您想要冲刺越过的障碍物的碰撞体")]
		public Collider CoverObstacleCollider;
		/// the (optional) exit dash zone on the other side of the collider
		[Tooltip("碰撞体另一侧的（可选）出口冲刺区域")]
		public List<DashZone3D> ExitDashZones;

		[Header("DashSettings冲刺设置")]

		/// the distance of the dash triggered when entering the zone
		[Tooltip("进入区域时触发的冲刺距离")]
		public float DashDistance = 3f;
		/// the duration of the dash
		[Tooltip("冲刺的持续时间")]
		public float DashDuration;
		/// the curve to apply to the dash
		[Tooltip("应用于冲刺的曲线")]
		public AnimationCurve DashCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

		[Header("Settings设置")]

		/// the max angle at which the character should approach the obstacle for the dash to happen
		[Tooltip("角色接近障碍物以触发冲刺的最大角度")]
		public float MaxFacingAngle = 90f;
		/// the duration in seconds before re-enabling all triggers in the zone
		[Tooltip("在重新启用区域内所有触发器前的持续时间（以秒为单位）")]
		public float TriggerResetDuration = 1f;
		/// if this is false, the dash won't happen
		[Tooltip("如果这是假的，冲刺将不会发生")]
		public bool DashAuthorized = true;

		protected CharacterDash3D _characterDash3D;
		protected CharacterHandleWeapon _characterHandleWeapon;
		protected WeaponAim3D _weaponAim3D;
		protected CharacterOrientation3D _characterOrientation3D;
		protected CharacterOrientation3D.RotationModes _rotationMode;
		protected WeaponAim.AimControls _weaponAimControl;
		protected Character _character;
		protected Collider _collider;
		protected WaitForSeconds _dashWaitForSeconds;
		protected WaitForSeconds _triggerResetForSeconds;
		protected Vector3 _direction1;
		protected Vector3 _direction2;
		protected bool _dashInProgress = false;

        /// <summary>
        /// 在开始时，我们初始化我们的区域。
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 抓取碰撞体并读取等待秒数。
        /// </summary>
        protected virtual void Initialization()
		{
			_collider = this.gameObject.GetComponent<Collider>();
			_collider.isTrigger = true;
			_dashWaitForSeconds = new WaitForSeconds(DashDuration);
			_triggerResetForSeconds = new WaitForSeconds(TriggerResetDuration);
		}

        /// <summary>
        /// 在触发进入时，我们准备好冲刺。
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter(Collider collider)
		{
			TestForDash(collider);
		}

        /// <summary>
        /// 在触发停留时，我们准备好冲刺
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerStay(Collider collider)
		{
			TestForDash(collider);
		}

        /// <summary>
        /// 确保碰撞体符合我们的规格，并且角度正确
        /// 如果符合条件，触发冲刺序列
        /// </summary>
        protected virtual void TestForDash(Collider collider)
		{
            // 如果已经有一个冲刺在进行中，我们什么也不做并退出
            if ((_dashInProgress == true) || !DashAuthorized)
			{
				return;
			}

            // 我们确保它是正确类型的字符
            _character = collider.gameObject.MMGetComponentNoAlloc<Character>();
			_characterDash3D = _character?.FindAbility<CharacterDash3D>();
			if (_characterDash3D == null)
			{
				return;
			}

            // 我们确保角度是正确的
            _characterOrientation3D = _character?.FindAbility<CharacterOrientation3D>();

			_direction1 = (_character.CharacterModel.transform.forward ).normalized;
			_direction1.y = 0f;
			_direction2 = (this.transform.forward).normalized;
			_direction2.y = 0f;
            
			float angle = Vector3.Angle(_direction1, _direction2);
			if (angle > MaxFacingAngle)
			{
				return;
			}

            //我们触发冲刺
            _characterHandleWeapon = _character?.FindAbility<CharacterHandleWeapon>();

            // 我们开始队列
            StartCoroutine(DashSequence());
		}

        /// <summary>
        /// 设置冲刺属性，触发冲刺，然后重置一切
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator DashSequence()
		{
			_dashInProgress = true;

            // 我们设置冲刺的属性
            _characterDash3D.DashDistance = DashDistance;
			_characterDash3D.DashCurve = DashCurve;
			_characterDash3D.DashDuration = DashDuration;
			_characterDash3D.DashDirection = this.transform.forward;

            // 我们将关闭输入检测
            _character.LinkedInputManager.InputDetectionActive = false;

            // 我们对模型强制旋转
            if (_characterOrientation3D != null)
			{
				_rotationMode = _characterOrientation3D.RotationMode;
				_characterOrientation3D.RotationMode = CharacterOrientation3D.RotationModes.MovementDirection;
				_characterOrientation3D.ForcedRotation = true;
				_characterOrientation3D.ForcedRotationDirection = ((this.transform.position + this.transform.forward * 10) - this.transform.position).normalized;
			}

            // 我们对武器强制旋转
            if (_characterHandleWeapon != null)
			{
				if (_characterHandleWeapon.CurrentWeapon != null)
				{
					_weaponAim3D = _characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim3D>();
					if (_weaponAim3D != null)
					{
						_weaponAimControl = _weaponAim3D.AimControl;
						_weaponAim3D.AimControl = WeaponAim.AimControls.Script;
						_weaponAim3D.SetCurrentAim(((this.transform.position + this.transform.forward * 10) - this.transform.position).normalized);
					}
				}
			}

            // 我们禁用障碍碰撞
            CoverObstacleCollider.enabled = false;

            // 我们关闭触发器
            SetColliderTrigger(false);
			foreach(DashZone3D dashZone in ExitDashZones)
			{
				dashZone.SetColliderTrigger(false);
			}

            // 我们开始冲刺
            _characterDash3D.DashStart();

            // 我们等待冲刺的持续时间
            yield return _dashWaitForSeconds;

            // 我们将一切恢复原状
            _character.LinkedInputManager.InputDetectionActive = true;
			if (_characterOrientation3D != null)
			{
				_characterOrientation3D.ForcedRotation = false;
				_characterOrientation3D.RotationMode = _rotationMode;
			}            
			if (_weaponAim3D != null)
			{
				_weaponAim3D.AimControl = _weaponAimControl;
			}            
			CoverObstacleCollider.enabled = true;

            // 我们等待再次开启触发器
            yield return _triggerResetForSeconds;

            // 我们将触发器重新开启
            SetColliderTrigger(true);
			foreach (DashZone3D dashZone in ExitDashZones)
			{
				dashZone.SetColliderTrigger(true);
			}

			_dashInProgress = false;
		}

        /// <summary>
        /// 设置这个碰撞体的触发器为关闭或开启
        /// </summary>
        /// <param name="status"></param>
        public virtual void SetColliderTrigger(bool status)
		{
			_collider.enabled = status;
		}
        
	}
}
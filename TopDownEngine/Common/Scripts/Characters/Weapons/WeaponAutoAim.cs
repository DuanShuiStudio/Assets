using System;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个抽象类，旨在被扩展为二维和三维的具体情况，处理自动瞄准的基础功能
    /// 被扩展的组件应该放在一个带有瞄准组件的武器上。
    /// </summary>
    [RequireComponent(typeof(Weapon))]
	public abstract class WeaponAutoAim : TopDownMonoBehaviour
	{
		[Header("Layer Masks图层蒙版")]
		/// the layermask on which to look for aim targets
		[Tooltip("用于寻找瞄准目标的图层蒙版")]
		public LayerMask TargetsMask;
		/// the layermask on which to look for obstacles
		[Tooltip("用于寻找障碍物的图层蒙版")]
		public LayerMask ObstacleMask = LayerManager.ObstaclesLayerMask;

		[Header("Scan for Targets搜索目标")]
		/// the radius (in units) around the character within which to search for targets
		[Tooltip("围绕角色搜索目标的半径（以单位计）")]
		public float ScanRadius = 15f;
		/// the size of the boxcast that will be performed to verify line of fire
		[Tooltip("将执行的用于验证火力线的箱体投射的大小")]
		public Vector2 LineOfFireBoxcastSize = new Vector2(0.1f, 0.1f);
		/// the duration (in seconds) between 2 scans for targets
		[Tooltip("两次目标扫描之间的时间间隔（以秒计）")]
		public float DurationBetweenScans = 1f;
		/// an offset to apply to the weapon's position for scan 
		[Tooltip("为了扫描而应用于武器位置的偏移量 ")]
		public Vector3 DetectionOriginOffset = Vector3.zero;
		/// if this is true, auto aim scan will only acquire new targets if the owner is in the idle state 
		[Tooltip("如果为真，则只有当拥有者处于空闲状态时，自动瞄准扫描才会获取新目标")]
		public bool OnlyAcquireTargetsIfOwnerIsIdle = false;
		
		[Header("Weapon Rotation武器旋转")]
		/// the rotation mode to apply when a target is found
		[Tooltip("当发现目标时要应用的旋转模式")]
		public WeaponAim.RotationModes RotationMode;
		/// if this is true, the auto aim direction will also be passed as the last non null direction, so the weapon will keep aiming in that direction should the target be lost
		[Tooltip("如果为真，则自动瞄准方向也会作为最后一个非空方向传递，这样即使目标丢失，武器也会保持朝那个方向瞄准")]
		public bool ApplyAutoAimAsLastDirection = true;
        
		[Header("Camera Target相机目标")]
		/// whether or not this component should take control of the camera target when a camera is found
		[Tooltip("当发现相机时，这个组件是否应该控制相机目标")]
		public bool MoveCameraTarget = true;
		/// the normalized distance (between 0 and 1) at which the camera target should be, on a line going from the weapon owner (0) to the auto aim target (1) 
		[Tooltip("在从武器拥有者（0）到自动瞄准目标（1）的直线上，相机目标应该处于的标准化距离（介于0和1之间）")]
		[Range(0f, 1f)]
		public float CameraTargetDistance = 0.5f;
		/// the maximum distance from the weapon owner at which the camera target can be
		[Tooltip("相机目标距离武器拥有者的最大距离")]
		[MMCondition("MoveCameraTarget", true)]
		public float CameraTargetMaxDistance = 10f;
		/// the speed at which to move the camera target
		[Tooltip("移动相机目标的速度")]
		[MMCondition("MoveCameraTarget", true)]
		public float CameraTargetSpeed = 5f;
		/// if this is true, the camera target will move back to the character if no target is found
		[Tooltip("如果为真，且没有找到目标，相机目标将移回角色")]
		[MMCondition("MoveCameraTarget", true)]
		public bool MoveCameraToCharacterIfNoTarget = false;

		[Header("Aim Marker瞄准标记")]
		/// An AimMarker prefab to use to show where this auto aim weapon is aiming
		[Tooltip("一个AimMarker预制件，用于显示这个自动瞄准武器的瞄准位置")]
		public AimMarker AimMarkerPrefab;
		/// if this is true, the aim marker will be removed when the weapon gets destroyed
		[Tooltip("如果为真，当武器被摧毁时，瞄准标记将被移除。")]
		public bool DestroyAimMarkerOnWeaponDestroy = true;

		[Header("Feedback反馈")]
		/// A feedback to play when a target is found and we didn't have one already
		[Tooltip("当找到一个目标且我们之前没有时播放的反馈")]
		public MMFeedbacks FirstTargetFoundFeedback;
		/// a feedback to play when we already had a target and just found a new one
		[Tooltip("当我们已经有目标并且刚刚发现了一个新目标时要播放的反馈")]
		public MMFeedbacks NewTargetFoundFeedback;
		/// a feedback to play when no more targets are found, and we just lost our last target
		[Tooltip("当没有更多的目标被找到，并且我们刚刚失去了最后一个目标时要播放的反馈")]
		public MMFeedbacks NoMoreTargetsFeedback;

		[Header("Debug调试")]
		/// the current target of the auto aim module
		[Tooltip("自动瞄准模块的当前目标")]
		[MMReadOnly]
		public Transform Target;
		/// whether or not to draw a debug sphere around the weapon to show its aim radius
		[Tooltip("是否在武器周围绘制一个调试球体，以显示其瞄准半径")]
		public bool DrawDebugRadius = true;
        
		protected float _lastScanTimestamp = 0f;
		protected WeaponAim _weaponAim;
		protected WeaponAim.AimControls _originalAimControl;
		protected WeaponAim.RotationModes _originalRotationMode;
		protected Vector3 _raycastOrigin;
		protected Weapon _weapon;
		protected bool _originalMoveCameraTarget;
		protected Transform _targetLastFrame;
		protected AimMarker _aimMarker;

        /// <summary>
        /// 在唤醒时，我们初始化我们的组件
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 在初始化时，我们获取我们的WeaponAim
        /// </summary>
        protected virtual void Initialization()
		{
			_weaponAim = this.gameObject.GetComponent<WeaponAim>();
			_weapon = this.gameObject.GetComponent<Weapon>();
			_isOwnerNull = _weapon.Owner == null;
			if (_weaponAim == null)
			{
				Debug.LogWarning(this.name + " : the WeaponAutoAim on this object requires that you add either a WeaponAim2D or WeaponAim3D component to your weapon.");
				return;
			}

			_originalAimControl = _weaponAim.AimControl;
			_originalRotationMode = _weaponAim.RotationMode;
			_originalMoveCameraTarget = _weaponAim.MoveCameraTargetTowardsReticle;

			FirstTargetFoundFeedback?.Initialization(this.gameObject);
			NewTargetFoundFeedback?.Initialization(this.gameObject);
			NoMoreTargetsFeedback?.Initialization(this.gameObject);

			if (AimMarkerPrefab != null)
			{
				_aimMarker = Instantiate(AimMarkerPrefab);
				_aimMarker.name = this.gameObject.name + "_AimMarker";
				_aimMarker.Disable();
			}
		}

        /// <summary>
        /// 在更新时，我们设置射线原点，周期性扫描，并在需要时设置瞄准
        /// </summary>
        protected virtual void Update()
		{
			if (_weaponAim == null)
			{
				return;
			}

			DetermineRaycastOrigin();
			ScanIfNeeded();
			HandleTarget();
			HandleMoveCameraTarget();
			HandleTargetChange();
			_targetLastFrame = Target;
		}

        /// <summary>
        /// 一种用于计算检测投射物原点的方法。
        /// </summary>
        protected abstract void DetermineRaycastOrigin();

        /// <summary>
        /// 这个方法应该定义如何执行目标扫描
        /// </summary>
        /// <returns></returns>
        protected abstract bool ScanForTargets();

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public virtual bool CanAcquireNewTargets() 
		{
			if (OnlyAcquireTargetsIfOwnerIsIdle && !_isOwnerNull)
			{
				if (_weapon.Owner.MovementState.CurrentState != CharacterStates.MovementStates.Idle)
				{
					return false;
				}
			}

			return true;
		}

        /// <summary>
        /// 将瞄准坐标发送给武器的瞄准组件
        /// </summary>
        protected abstract void SetAim();

        /// <summary>
        /// 如果需要，将相机目标向自动瞄准目标移动
        /// </summary>
        protected Vector3 _newCamTargetPosition;
		protected Vector3 _newCamTargetDirection;
		protected bool _isOwnerNull;

        /// <summary>
        /// 检查目标变化，并在需要时触发相应的方法
        /// </summary>
        protected virtual void HandleTargetChange()
		{
			if (Target == _targetLastFrame)
			{
				return;
			}

			if (_aimMarker != null)
			{
				_aimMarker.SetTarget(Target);
			}

			if (Target == null)
			{
				NoMoreTargets();
				return;
			}

			if (_targetLastFrame == null)
			{
				FirstTargetFound();
				return;
			}

			if ((_targetLastFrame != null) && (Target != null))
			{
				NewTargetFound();
			}
		}

        /// <summary>
        /// 当没有更多的目标被找到，并且我们刚刚失去了一个目标时，我们会播放一个专门的反馈
        /// </summary>
        protected virtual void NoMoreTargets()
		{
			NoMoreTargetsFeedback?.PlayFeedbacks();
		}

        /// <summary>
        /// 当找到一个新目标并且我们之前没有时，我们会播放一个专门的反馈
        /// </summary>
        protected virtual void FirstTargetFound()
		{
			FirstTargetFoundFeedback?.PlayFeedbacks();
		}

        /// <summary>
        /// 当找到一个新目标，并且我们之前有另一个目标时，我们会播放一个专门的反馈
        /// </summary>
        protected virtual void NewTargetFound()
		{
			NewTargetFoundFeedback?.PlayFeedbacks();
		}

        /// <summary>
        /// 如果需要，移动相机目标
        /// </summary>
        protected virtual void HandleMoveCameraTarget()
		{
			bool targetIsNull = (Target == null);
			
			if (!MoveCameraTarget || (_isOwnerNull))
			{
				return;
			}

			if (!MoveCameraToCharacterIfNoTarget && targetIsNull)
			{
				return;
			}

			if (targetIsNull)
			{
				_newCamTargetPosition = _weapon.Owner.transform.position;
			}
			else
			{
				_newCamTargetPosition = Vector3.Lerp(_weapon.Owner.transform.position, Target.transform.position, CameraTargetDistance);	
			}
			
			_newCamTargetDirection = _newCamTargetPosition - this.transform.position;
            
			if (_newCamTargetDirection.magnitude > CameraTargetMaxDistance)
			{
				_newCamTargetDirection = _newCamTargetDirection.normalized * CameraTargetMaxDistance;
			}

			_newCamTargetPosition = this.transform.position + _newCamTargetDirection;

			_newCamTargetPosition = Vector3.Lerp(_weapon.Owner.CameraTarget.transform.position,
				_newCamTargetPosition,
				Time.deltaTime * CameraTargetSpeed);

			_weapon.Owner.CameraTarget.transform.position = _newCamTargetPosition;
		}

        /// <summary>
        /// 执行周期性扫描
        /// </summary>
        protected virtual void ScanIfNeeded()
		{
			if (Time.time - _lastScanTimestamp > DurationBetweenScans)
			{
				ScanForTargets();
				_lastScanTimestamp = Time.time;
			}
		}

        /// <summary>
        ///如果需要，设置瞄准；否则，恢复到之前的瞄准控制模式
        /// </summary>
        protected virtual void HandleTarget()
		{
			if (Target == null)
			{
				_weaponAim.AimControl = _originalAimControl;
				_weaponAim.RotationMode = _originalRotationMode;
				_weaponAim.MoveCameraTargetTowardsReticle = _originalMoveCameraTarget;
			}
			else
			{
				_weaponAim.AimControl = WeaponAim.AimControls.Script;
				_weaponAim.RotationMode = RotationMode;
				if (MoveCameraTarget)
				{
					_weaponAim.MoveCameraTargetTowardsReticle = false;
				}
				SetAim();
			}
		}

        /// <summary>
        /// 在武器周围绘制一个球体，以显示其自动瞄准半径
        /// </summary>
        protected virtual void OnDrawGizmos()
		{
			if (DrawDebugRadius)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(_raycastOrigin, ScanRadius);
			}
		}

        /// <summary>
        /// 在禁用时，如果需要，我们隐藏瞄准标记
        /// </summary>
        protected virtual void OnDisable()
		{
			if (_aimMarker != null)
			{
				_aimMarker.Disable();
			}
		}

		protected void OnDestroy()
		{
			if (DestroyAimMarkerOnWeaponDestroy && (_aimMarker != null))
			{
				Destroy(_aimMarker.gameObject);
			}
		}
	}
}
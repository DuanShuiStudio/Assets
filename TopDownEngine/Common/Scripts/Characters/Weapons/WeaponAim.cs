using System;
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.UI;

namespace MoreMountains.TopDownEngine
{
	[RequireComponent(typeof(Weapon))]
	public abstract class WeaponAim : TopDownMonoBehaviour, MMEventListener<TopDownEngineEvent>
	{
		/// the list of possible control modes
		public enum AimControls { Off, PrimaryMovement, SecondaryMovement, Mouse, Script, SecondaryThenPrimaryMovement, PrimaryThenSecondaryMovement, CharacterRotateCameraDirection }
		/// the list of possible rotation modes
		public enum RotationModes { Free, Strict2Directions, Strict4Directions, Strict8Directions }
		/// the possible types of reticles
		public enum ReticleTypes { None, Scene, UI }

		[MMInspectorGroup("Control Mode", true, 5)]
		[MMInformation("将此组件添加到武器上，您就能够瞄准（旋转）它。它支持三种不同的控制模式：鼠标（武器会朝向指针方向）、主要移动（您将朝向当前输入方向瞄准），或次要移动（朝向第二个输入轴瞄准，比如双摇杆射击游戏）", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// the aim control mode
		[Tooltip("当前选定的瞄准控制模式")]
		public AimControls AimControl = AimControls.SecondaryMovement;
		/// if this is true, this script will be able to read input from its specified AimControl mode
		[Tooltip("如果此条件为真，则此脚本将能够从其指定的AimControl模式读取输入")]
		public bool AimControlActive = true;

		[MMInspectorGroup("Weapon Rotation", true, 10)]
		[MMInformation("在这里你可以定义旋转是否是自由的，严格的在4个方向（上，下，左，右）或者8个方向（同样的 + 对角线）。你也可以设置一个旋转的速度和最小与最大角度。例如，如果你不想你的角色能够向后瞄准，把最小角度设置为 -90 度，最大角度设置为90度.", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// the rotation mode
		[Tooltip("旋转模式")]
		public RotationModes RotationMode = RotationModes.Free;
		/// the the speed at which the weapon reaches its new position. Set it to zero if you want movement to directly follow input
		[Tooltip("武器到达新位置的速度。如果你想让移动直接跟随输入，就把它设置为0")]
		public float WeaponRotationSpeed = 1f;
		/// the minimum angle at which the weapon's rotation will be clamped
		[Range(-180, 180)]
		[Tooltip("武器旋转将被限制的最小角度")]
		public float MinimumAngle = -180f;
		/// the maximum angle at which the weapon's rotation will be clamped
		[Range(-180, 180)]
		[Tooltip("武器旋转将被限制的最大角度")]
		public float MaximumAngle = 180f;
		/// the minimum threshold at which the weapon's rotation magnitude will be considered 
		[Tooltip("武器旋转量将被视为的最小阈值 ")]
		public float MinimumMagnitude = 0.2f;

		[MMInspectorGroup("Reticle", true, 11)]
		[MMInformation("你也可以在屏幕上显示一个准星来检查你瞄准的位置。如果你不想使用它，可以留空。如果你将准星距离设置为0，它将跟随光标移动，否则它将位于武器中心的圆圈上。你也可以让它跟随鼠标移动，甚至替换鼠标指针。你还可以决定指针是否应该旋转以反映瞄准角度，或者保持稳定", MoreMountains.Tools.MMInformationAttribute.InformationType.Info, false)]
		/// Defines whether the reticle is placed in the scene or in the UI
		[Tooltip("定义准星是放置在场景中还是用户界面（UI）中")]
		public ReticleTypes ReticleType = ReticleTypes.None;
		/// the gameobject to display as the aim's reticle/crosshair. Leave it blank if you don't want a reticle
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("要显示为瞄准器准星/十字准线的游戏对象。如果不需要准星，请留空")]
		public GameObject Reticle;
		/// the distance at which the reticle will be from the weapon
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene)]
		[Tooltip("准星与武器的距离")]
		public float ReticleDistance;
		/// the height at which the reticle should position itself above the ground, when in Scene mode
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("在场景模式下，准星应该位于地面以上的高度")]
		public float ReticleHeight;
		/// if set to true, the reticle will be placed at the mouse's position (like a pointer)
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene)]
		[Tooltip("如果设置为true，准星将放置在鼠标的位置（就像一个指针）")]
		public bool ReticleAtMousePosition;
		/// if set to true, the reticle will rotate on itself to reflect the weapon's rotation. If not it'll remain stable.
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene)]
		[Tooltip("如果设置为true，准星将随着武器的旋转而自身旋转。如果不设置，它将保持稳定不动")]
		public bool RotateReticle = false;
		/// if set to true, the reticle will replace the mouse pointer
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("如果设置为true，准星将取代鼠标指针")]
		public bool ReplaceMousePointer = true;
		/// the radius around the weapon rotation centre where the mouse will be ignored, to avoid glitches
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("以武器旋转中心为圆心，鼠标将被忽略的半径范围，以避免故障")]
		public float MouseDeadZoneRadius = 0.5f;
		/// if set to false, the reticle won't be added and displayed
		[MMEnumCondition("ReticleType", (int)ReticleTypes.Scene, (int)ReticleTypes.UI)]
		[Tooltip("如果设置为false，将不会添加和显示准星")]
		public bool DisplayReticle = true;

		[MMInspectorGroup("Camera Target", true, 12)]
		/// whether the camera target should be moved towards the reticle to provide a better vision of the possible target. If you don't have a reticle, it'll be moved towards your aim direction.
		[Tooltip("是否应该将相机目标移向准星，以便更好地观察可能的目标。如果没有准星，它将移向你的瞄准方向")]
		public bool MoveCameraTargetTowardsReticle = false;
		/// the offset to apply to the camera target along the transform / reticle line
		[Range(0f, 1f)]
		[Tooltip("沿着转换/准星线应用到相机目标的偏移量")]
		public float CameraTargetOffset = 0.3f;
		/// the maximum distance at which to move the camera target
		[MMCondition("MoveCameraTargetTowardsReticle", true)]
		[Tooltip("相机目标从武器移动的最大距离")]
		public float CameraTargetMaxDistance = 10f;
		/// the speed at which the camera target should be moved
		[MMCondition("MoveCameraTargetTowardsReticle", true)]
		[Tooltip("相机目标移动的速度")]
		public float CameraTargetSpeed = 5f;

		public virtual float CurrentAngleAbsolute { get; protected set; }
        /// 武器当前的旋转角度
        public virtual Quaternion CurrentRotation { get { return transform.rotation; } }
        /// 武器当前的指向方向
        public virtual Vector3 CurrentAim { get { return _currentAim; } }
        /// 武器当前的绝对方向（与翻转无关）
        public virtual Vector3 CurrentAimAbsolute { get { return _currentAimAbsolute; } }
        /// 武器当前瞄准的角度
        public virtual float CurrentAngle { get; protected set; }
        /// 经过调整以补偿角色当前方向的、武器当前瞄准的角度
        public virtual float CurrentAngleRelative
		{
			get
			{
				if (_weapon != null)
				{
					if (_weapon.Owner != null)
					{
						return CurrentAngle;
					}
				}
				return 0;
			}
		}

		public virtual Weapon TargetWeapon => _weapon;
        
		protected Camera _mainCamera;
		protected Vector2 _lastNonNullMovement;
		protected Weapon _weapon;
		protected Vector3 _currentAim = Vector3.zero;
		protected Vector3 _currentAimAbsolute = Vector3.zero;
		protected Quaternion _lookRotation;
		protected Vector3 _direction;
		protected float[] _possibleAngleValues;
		protected Vector3 _mousePosition;
		protected Vector3 _lastMousePosition;
		protected float _additionalAngle;
		protected Quaternion _initialRotation;
		protected Plane _playerPlane;
		protected GameObject _reticle;
		protected Vector3 _reticlePosition;
		protected Vector3 _newCamTargetPosition;
		protected Vector3 _newCamTargetDirection;
		protected bool _initialized = false;

        /// <summary>
        /// 在Start()方法中，我们触发初始化
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 获取武器组件，初始化角度值
        /// </summary>
        protected virtual void Initialization()
		{
			_weapon = GetComponent<Weapon>();
			_mainCamera = Camera.main;

			if (RotationMode == RotationModes.Strict4Directions)
			{
				_possibleAngleValues = new float[5];
				_possibleAngleValues[0] = -180f;
				_possibleAngleValues[1] = -90f;
				_possibleAngleValues[2] = 0f;
				_possibleAngleValues[3] = 90f;
				_possibleAngleValues[4] = 180f;
			}
			if (RotationMode == RotationModes.Strict8Directions)
			{
				_possibleAngleValues = new float[9];
				_possibleAngleValues[0] = -180f;
				_possibleAngleValues[1] = -135f;
				_possibleAngleValues[2] = -90f;
				_possibleAngleValues[3] = -45f;
				_possibleAngleValues[4] = 0f;
				_possibleAngleValues[5] = 45f;
				_possibleAngleValues[6] = 90f;
				_possibleAngleValues[7] = 135f;
				_possibleAngleValues[8] = 180f;
			}
			_initialRotation = transform.rotation;
			InitializeReticle();
			_playerPlane = new Plane(Vector3.up, Vector3.zero);
			_initialized = true;
		}
        
		public virtual void ApplyAim()
		{
			Initialization();
			GetCurrentAim();
			DetermineWeaponRotation();
		}

        /// <summary>
        /// 将武器瞄准一个新的点
        /// </summary>
        /// <param name="newAim">New aim.</param>
        public virtual void SetCurrentAim(Vector3 newAim, bool setAimAsLastNonNullMovement = false)
		{
			_currentAim = newAim;
		}

		protected virtual void GetCurrentAim()
		{

		}

        /// <summary>
        /// 每帧，我们计算瞄准方向并相应地旋转武器
        /// </summary>
        protected virtual void Update()
		{

		}

        /// <summary>
        /// 在LateUpdate中，重置任何额外的角度
        /// </summary>
        protected virtual void LateUpdate()
		{
			ResetAdditionalAngle();
		}

        /// <summary>
        /// 确定武器的旋转角度
        /// </summary>
        protected virtual void DetermineWeaponRotation()
		{

		}

        /// <summary>
        /// 移动武器的准星
        /// </summary>
        protected virtual void MoveReticle()
		{

		}

        /// <summary>
        ///返回准星的位置
        /// </summary>
        /// <returns></returns>
        public virtual Vector3 GetReticlePosition()
		{
			return _reticle.transform.position;
		}

        /// <summary>
        /// 返回当前的鼠标位置
        /// </summary>
        public virtual Vector3 GetMousePosition()
		{
			return _mainCamera.ScreenToWorldPoint(_mousePosition);
		}

        /// <summary>
        /// 旋转武器，可选择是否对其应用偏移
        /// </summary>
        /// <param name="newRotation">New rotation.</param>
        protected virtual void RotateWeapon(Quaternion newRotation, bool forceInstant = false)
		{
			if (GameManager.Instance.Paused)
			{
				return;
			}
            // 如果旋转速度等于0，我们实现即时旋转
            if ((WeaponRotationSpeed == 0f) || forceInstant)
			{
				transform.rotation = newRotation;
			}
            // 否则我们对旋转应用偏移
            else
            {
				transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, WeaponRotationSpeed * Time.deltaTime);
			}
		}

		protected Vector3 _aimAtDirection;
		protected Quaternion _aimAtQuaternion;
        
		protected virtual void AimAt(Vector3 target)
		{
		}

        /// <summary>
        /// 如果已经设置了准星，则实例化准星并将其定位
        /// </summary>
        protected virtual void InitializeReticle()
		{
           
		}

        /// <summary>
        /// 此方法定义了角色相机目标的移动方式
        /// </summary>
        protected virtual void MoveTarget()
		{

		}

        /// <summary>
        /// 移除任何剩余的准星
        /// </summary>
        public virtual void RemoveReticle()
		{
			if (_reticle != null)
			{
				Destroy(_reticle.gameObject);
			}
		}

        /// <summary>
        /// 根据DisplayReticle设置隐藏（或显示）准星
        /// </summary>
        protected virtual void HideReticle()
		{
			if (_reticle != null)
			{
				if (GameManager.Instance.Paused)
				{
					_reticle.gameObject.SetActive(false);
					return;
				}
				_reticle.gameObject.SetActive(DisplayReticle);
			}
		}

        /// <summary>
        /// 根据设置隐藏或显示鼠标指针
        /// </summary>
        protected virtual void HideMousePointer()
		{
			if (AimControl != AimControls.Mouse)
			{
				return;
			}
			if (GameManager.Instance.Paused)
			{
				Cursor.visible = true;
				return;
			}
			if (ReplaceMousePointer)
			{
				Cursor.visible = false;
			}
			else
			{
				Cursor.visible = true;
			}
		}

        /// <summary>
        /// 在销毁时，如果需要的话，我们重新初始化光标
        /// </summary>
        protected void OnDestroy()
		{
			if (ReplaceMousePointer)
			{
				Cursor.visible = true;
			}
		}


        /// <summary>
        /// 为武器的旋转添加额外的角度
        /// </summary>
        /// <param name="addedAngle"></param>
        public virtual void AddAdditionalAngle(float addedAngle)
		{
			_additionalAngle += addedAngle;
		}

        /// <summary>
        /// 重置额外的角度
        /// </summary>
        protected virtual void ResetAdditionalAngle()
		{
			_additionalAngle = 0;
		}

		protected virtual void AutoDetectWeaponMode()
		{
			if (_weapon.Owner.LinkedInputManager != null)
			{
				if ((_weapon.Owner.LinkedInputManager.ForceWeaponMode) && (AimControl != AimControls.Off))
				{
					AimControl = _weapon.Owner.LinkedInputManager.WeaponForcedMode;
				}

				if ((!_weapon.Owner.LinkedInputManager.ForceWeaponMode) && (_weapon.Owner.LinkedInputManager.IsMobile) && (AimControl == AimControls.Mouse))
				{
					AimControl = AimControls.PrimaryMovement;
				}
			}
		}

		public void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.LevelStart:
					_initialized = false;
					Initialization();
					break;
			}
		}

        /// <summary>
        /// 启用时，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

        /// <summary>
        ///禁用时，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}
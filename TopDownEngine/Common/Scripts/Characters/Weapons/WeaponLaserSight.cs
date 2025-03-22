using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这个类添加到武器上，它会向武器面对的方向投射一条激光射线
    /// </summary>
    [AddComponentMenu("TopDown Engine/Weapons/Weapon Laser Sight")]
	public class WeaponLaserSight : TopDownMonoBehaviour 
	{
        /// 这个武器激光瞄准器可以运行的可能模式，默认为3D
        public enum Modes { TwoD, ThreeD }

		[Header("General Settings常规设置")]

		/// whether this laser should work in 2D or 3D
		[Tooltip("这个激光是否应该在2D或3D中工作")]
		public Modes Mode = Modes.ThreeD;
		/// if this is false, raycasts won't be computed for this laser sight
		[Tooltip("如果此选项为假，则不会为这个激光瞄准器计算光线投射")]
		public bool PerformRaycast = true;
		/// if this is false, the laser won't be drawn
		[MMCondition("PerformRaycast")]
		[Tooltip("如果此选项为假，则不会绘制激光")]
		public bool DrawLaser = true;

		[Header("Raycast Settings光线投射设置")]

		/// the origin of the raycast used to detect obstacles
		[Tooltip("用于检测障碍物的光线投射的起点")]
		public Vector3 RaycastOriginOffset;
		/// the origin of the visible laser
		[Tooltip("可见激光的起点")]
		public Vector3 LaserOriginOffset;
		/// the maximum distance to which we should draw the laser
		[Tooltip("我们应该绘制激光的最大距离")]
		public float LaserMaxDistance = 50;
		/// the collision mask containing all layers that should stop the laser
		[Tooltip("包含所有应该阻止激光的层的碰撞遮罩")]
		public LayerMask LaserCollisionMask;

		[Header("Laser激光")]

		/// the width of the laser
		[Tooltip("激光的宽度")]
		public Vector2 LaserWidth = new Vector2(0.05f, 0.05f);
		/// the material used to render the laser
		[Tooltip("用于渲染激光的材质")]
		public Material LaserMaterial;

		public virtual LineRenderer _line { get; protected set; }
		public virtual RaycastHit _hit { get; protected set; }
		public RaycastHit2D _hit2D;
		public virtual Vector3 _origin { get; protected set; }
		public virtual Vector3 _raycastOrigin { get; protected set; }

		protected Vector3 _destination;
		protected Vector3 _laserOffset;
		protected Weapon _weapon;
		protected Vector3 _direction;

		protected Vector3 _weaponPosition, _thisPosition, _thisForward;
		protected Quaternion _weaponRotation, _thisRotation;
		protected int _initFrame;

		/// <summary>
		/// 初始化
		/// </summary>
		protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 在初始化时，如果需要，我们创建我们的线条
        /// </summary>
        protected virtual void Initialization()
		{
			if (DrawLaser)
			{
				_line = gameObject.AddComponent<LineRenderer>();
				_line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				_line.receiveShadows = true;
				_line.startWidth = LaserWidth.x;
				_line.endWidth = LaserWidth.y;
				_line.material = LaserMaterial;
				_line.SetPosition(0, this.transform.position);
				_line.SetPosition(1, this.transform.position);
			}
			_weapon = GetComponent<Weapon>();
			if (_weapon == null)
			{
				Debug.LogWarning("这个WeaponLaserSight没有与武器相关联。请将其添加到带有Weapon组件的游戏对象上");
			}

			_initFrame = Time.frameCount;
		}

        /// <summary>
        /// 每帧我们绘制我们的激光
        /// </summary>
        protected virtual void LateUpdate()
		{
			ShootLaser();
		}

        /// <summary>
        /// 绘制实际的激光
        /// </summary>
        public virtual void ShootLaser()
		{
			if (!PerformRaycast)
			{
				return;
			}

			_laserOffset = LaserOriginOffset;
			_weaponPosition = _weapon.transform.position;
			_weaponRotation = _weapon.transform.rotation;
			_thisPosition = this.transform.position;
			_thisRotation = this.transform.rotation;
			_thisForward = this.transform.forward;
            
			if (Mode == Modes.ThreeD)
			{
                // 我们的激光将从武器的激光原点射出
                _origin = MMMaths.RotatePointAroundPivot(_thisPosition + _laserOffset, _thisPosition, _thisRotation);
				_raycastOrigin = MMMaths.RotatePointAroundPivot(_thisPosition + RaycastOriginOffset, _thisPosition, _thisRotation);

                // 我们在武器前方投射一条光线以检测障碍物
                _hit = MMDebug.Raycast3D(_raycastOrigin, _thisForward, LaserMaxDistance, LaserCollisionMask, Color.red, true);

                // 如果我们击中了某物，我们的目标就是光线投射的命中点
                if (_hit.transform != null)
				{
					_destination = _hit.point;
				}
                // 否则，我们就只在武器前方绘制我们的激光
                else
                {
					_destination = _origin + _thisForward * LaserMaxDistance;
				}
			}
			else
			{
				_direction = _weapon.Flipped ? Vector3.left : Vector3.right;
				if (_direction == Vector3.left)
				{
					_laserOffset.x = -LaserOriginOffset.x;
				}

				_raycastOrigin = MMMaths.RotatePointAroundPivot(_weaponPosition + _laserOffset, _weaponPosition, _weaponRotation);
				_origin = _raycastOrigin;

                // 我们在武器前方投射一条光线以检测障碍物
                _hit2D = MMDebug.RayCast(_raycastOrigin, _weaponRotation * _direction, LaserMaxDistance, LaserCollisionMask, Color.red, true);
				if (_hit2D)
				{
					_destination = _hit2D.point;
				}
                // 否则，我们就只在武器前方绘制我们的激光 
                else
                {
					_destination = _origin;
					_destination.x = _destination.x + LaserMaxDistance * _direction.x;
					_destination = MMMaths.RotatePointAroundPivot(_destination, _weaponPosition, _weaponRotation);
				}
			}

			if (Time.frameCount <= _initFrame + 1)
			{
				return;
			}

            // 我们设置激光线的起点和终点坐标
            if (DrawLaser)
			{
				_line.SetPosition(0, _origin);
				_line.SetPosition(1, _destination);
			}			
		}

        /// <summary>
        /// 根据传入参数中的状态打开或关闭激光
        /// </summary>
        /// <param name="status">If set to <c>true</c> status.</param>
        public virtual void LaserActive(bool status)
		{
			_line.enabled = status;
		}

	}
}
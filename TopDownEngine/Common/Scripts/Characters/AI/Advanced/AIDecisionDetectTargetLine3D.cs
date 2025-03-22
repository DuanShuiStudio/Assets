using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果在指定方向的射线或箱形投射中找到目标，则此Decision将返回true
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Detect Target Line 3D")]
	public class AIDecisionDetectTargetLine3D : AIDecision
	{
        /// 可能的检测方法
        public enum DetectMethods { Ray, WideRay }
        /// the detection method
        [Tooltip("所选择的检测方法：射线是单射线，宽射线更昂贵但也更准确")]
		public DetectMethods DetectMethod = DetectMethods.Ray;
		/// the width of the ray to cast (if we're in WideRay mode only
		[Tooltip("投射的射线宽度（仅当我们处于WideRay模式时")]
		public float RayWidth = 1f;
		/// the distance up to which we'll cast our rays
		[Tooltip("我们将投射射线的距离")]
		public float DetectionDistance = 10f;
		/// the offset to apply to the ray(s)
		[Tooltip("要应用于射线的偏移量")]
		public Vector3 DetectionOriginOffset = new Vector3(0,0,0);
		/// the layer(s) on which we want to search a target on
		[Tooltip("我们要在其上搜索目标的层（多个层）")]
		public LayerMask TargetLayer;
		/// the layer(s) on which obstacles are set. Obstacles will block the ray
		[Tooltip("设置障碍物的层（多个层）。障碍物将阻挡射线")]
		public LayerMask ObstaclesLayer = LayerManager.ObstaclesLayerMask;
		/// a transform to use as the rotation reference for detection raycasts. If you have a rotating model for example, you'll want to set it as your reference transform here.
		[Tooltip("一个用作检测射线投射旋转参考的变换。例如，如果你有一个旋转的模型，你会想要在这里将其设置为你的参考变换")]
		public Transform ReferenceTransform;
		/// if this is true, this decision will force the weapon to aim in the detection direction
		[Tooltip("如果这个条件为真，那么这个决策将强制武器瞄准检测方向")]
		public bool ForceAimToDetectionDirection = false;
		/// if this is true, this decision will set the AI Brain's Target to null if no target is found
		[Tooltip("如果这个条件为真，那么当AI大脑没有找到目标时，这个决策会将AI大脑的目标设置为null")]
		public bool SetTargetToNullIfNoneIsFound = true;

		protected Vector3 _direction;
		protected float _distanceToTarget;
		protected Vector3 _raycastOrigin;
		protected Character _character;
		protected Color _gizmosColor = Color.yellow;
		protected Vector3 _gizmoCenter;
		protected Vector3 _gizmoSize;
		protected bool _init = false;
		protected CharacterHandleWeapon _characterHandleWeapon;

        /// <summary>
        /// 在Init中，我们获取角色
        /// </summary>
        public override void Initialization()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			_characterHandleWeapon = _character.FindAbility<CharacterHandleWeapon>();
			_gizmosColor.a = 0.25f;
			_init = true;
			if (ReferenceTransform == null)
			{
				ReferenceTransform = this.transform;
			}
		}

        /// <summary>
        /// 决定我们找一个目标
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return DetectTarget();
		}

        /// <summary>
        /// 如果射线找到目标，则返回true
        /// </summary>
        /// <returns></returns>
        protected virtual bool DetectTarget()
		{
			bool hit = false;
			_distanceToTarget = 0;
			Transform target = null;
			RaycastHit raycast;

			_direction = ReferenceTransform.forward;

            // 我们向代理的左边投射一束光线来检查玩家
            _raycastOrigin = ReferenceTransform.position + DetectionOriginOffset ;

			if (DetectMethod == DetectMethods.Ray)
			{
				raycast = MMDebug.Raycast3D(_raycastOrigin, _direction, DetectionDistance, TargetLayer, MMColors.Gold, true);
                
			}
			else
			{
				hit = Physics.BoxCast(_raycastOrigin, Vector3.one * (RayWidth * 0.5f), _direction, out raycast, ReferenceTransform.rotation, DetectionDistance, TargetLayer);
			}
                
			if (raycast.collider != null)
			{
				hit = true;
				_distanceToTarget = Vector3.Distance(_raycastOrigin, raycast.point);
				target = raycast.collider.gameObject.transform;
			}

			if (hit)
			{
                // 我们要确保中间没有障碍
                float distance = Vector3.Distance(target.transform.position, _raycastOrigin);
				RaycastHit raycastObstacle = MMDebug.Raycast3D(_raycastOrigin, (target.transform.position - _raycastOrigin).normalized, distance, ObstaclesLayer, Color.gray, true);
                
				if ((raycastObstacle.collider != null) && (_distanceToTarget > raycastObstacle.distance))
				{
					if (SetTargetToNullIfNoneIsFound) { _brain.Target = null; }
					return false;
				}
				else
				{
                    // 如果没有障碍物，我们存储目标并返回true
                    _brain.Target = target;
					return true;
				}
			}

			ForceDirection();
            
			if (SetTargetToNullIfNoneIsFound) { _brain.Target = null; }
			return false;           
		}

        /// <summary>
        /// 如果需要，强制武器瞄准选定的方向
        /// </summary>
        protected virtual void ForceDirection()
		{
			if (!ForceAimToDetectionDirection)
			{
				return;
			}
			if (_characterHandleWeapon == null)
			{
				return;
			}
			if (_characterHandleWeapon.CurrentWeapon == null)
			{
				return;
			}
			_characterHandleWeapon.CurrentWeapon.gameObject.MMGetComponentNoAlloc<WeaponAim3D>()?.SetCurrentAim(ReferenceTransform.forward);
		}

        /// <summary>
        /// 绘制射线小玩意儿
        /// </summary>
        protected virtual void OnDrawGizmos()
		{
			if (DetectMethod != DetectMethods.WideRay)
			{
				return;
			}

			Gizmos.color = _gizmosColor;
            

            
			_gizmoCenter = DetectionOriginOffset + Vector3.forward * DetectionDistance / 2f;
			_gizmoSize.x = RayWidth;
			_gizmoSize.y = RayWidth;
			_gizmoSize.z = DetectionDistance;
            
			Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
			if (ReferenceTransform != null)
			{
				Gizmos.matrix = ReferenceTransform.localToWorldMatrix;
			}
            
            
			Gizmos.DrawCube(_gizmoCenter, _gizmoSize);
            
            
		}
        
        
	}
}
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果TargetLayer layermask上的任何对象进入它的视线，这个Decision将返回true。
	/// 它还会将Brain的Target设置为该对象。你可以选择让它在光线模式下，在这种情况下，它的视线将是一条实际的线（光线投射），或者让它更宽（在这种情况下，它将使用球体投射）。
	/// 你也可以为光线的原点指定一个偏移量，以及一个阻挡它的障碍层遮罩。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Detect Target Line 2D")]
	//[RequireComponent(typeof(Character))]
	//[RequireComponent(typeof(CharacterOrientation2D))]
	public class AIDecisionDetectTargetLine2D : AIDecision
	{
        /// 可能的检测方法
        public enum DetectMethods { Ray, WideRay }
        /// 可能的检测方向
        public enum DetectionDirections { Front, Back, Both }
        /// 检测方法
        [Tooltip("所选择的检测方法：射线是单射线，宽射线更昂贵但也更准确")]
		public DetectMethods DetectMethod = DetectMethods.Ray;
		/// the detection direction
		[Tooltip("检测方向：前，后，或两者")]
		public DetectionDirections DetectionDirection = DetectionDirections.Front;
		/// the width of the ray to cast (if we're in WideRay mode only
		[Tooltip("投射的射线宽度（仅当我们处于WideRay模式时）")]
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
		/// if this is true, this decision will set the AI Brain's Target to null if no target is found
		[Tooltip("如果这个条件为真，那么当AI大脑没有找到目标时，这个决策会将AI大脑的目标设置为null")]
		public bool SetTargetToNullIfNoneIsFound = true;

		protected Vector2 _direction;
		protected Vector2 _facingDirection;
		protected float _distanceToTarget;
		protected Vector2 _raycastOrigin;
		protected Character _character;
		protected CharacterOrientation2D _orientation2D;
		protected bool _drawLeftGizmo = false;
		protected bool _drawRightGizmo = false;
		protected Color _gizmosColor = Color.yellow;
		protected Vector3 _gizmoCenter;
		protected Vector3 _gizmoSize;
		protected bool _init = false;
		protected Vector2 _boxcastSize = Vector2.zero;
		protected bool _isFacingRight = true;

		protected virtual Vector2 _transformRight { get { return ReferenceTransform.right; } }
		protected virtual Vector2 _transformLeft { get { return -ReferenceTransform.right; } }
		protected virtual Vector2 _transformUp { get { return ReferenceTransform.up; } }
		protected virtual Vector2 _transformDown { get { return -ReferenceTransform.up; } }

        /// <summary>
        /// 在Init中，我们获取角色
        /// </summary>
        public override void Initialization()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
			_orientation2D = _character?.FindAbility<CharacterOrientation2D>();
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
			RaycastHit2D raycast;
			_drawLeftGizmo = false;
			_drawRightGizmo = false;

			_boxcastSize.x = DetectionDistance / 5f;
			_boxcastSize.y = RayWidth;

			if (_orientation2D == null)
			{
				_facingDirection = _transformRight;
				_isFacingRight = true;
			}
			else
			{
				_isFacingRight = _orientation2D.IsFacingRight;
				_facingDirection = _orientation2D.IsFacingRight ? _transformRight : _transformLeft;    
			}

            // 我们向代理的左边投射一束光线来检查玩家
            _raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
			_raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;

            // 我们将它向左转换
            if ((DetectionDirection == DetectionDirections.Both)
			    || ((DetectionDirection == DetectionDirections.Front) && (!_isFacingRight))
			    || ((DetectionDirection == DetectionDirections.Back) && (_isFacingRight)))
			{
				if (DetectMethod == DetectMethods.Ray)
				{
					raycast = MMDebug.RayCast(_raycastOrigin, _transformLeft, DetectionDistance, TargetLayer, MMColors.Gold, true);
				}
				else
				{
					raycast = Physics2D.BoxCast(_raycastOrigin - Vector2.right * _boxcastSize.x / 2f, _boxcastSize, 0f, Vector2.left, DetectionDistance - _boxcastSize.x, TargetLayer);
					MMDebug.RayCast(_raycastOrigin + _transformUp * RayWidth/2f, _transformLeft, DetectionDistance, TargetLayer, MMColors.Gold, true);
					MMDebug.RayCast(_raycastOrigin - _transformUp * RayWidth / 2f, _transformLeft, DetectionDistance, TargetLayer, MMColors.Gold, true);
					MMDebug.RayCast(_raycastOrigin - _transformUp * RayWidth / 2f + _transformLeft * DetectionDistance, _transformUp, RayWidth, TargetLayer, MMColors.Gold, true);
					_drawLeftGizmo = true;
				}

                // 如果我们看到一个玩家
                if (raycast)
				{
					hit = true;
					_direction = Vector2.left;
					_distanceToTarget = Vector2.Distance(_raycastOrigin, raycast.point);
					target = raycast.collider.gameObject.transform;
				}
			}

            // 	我们向代理的右侧投射一束光线来检查玩家
            if ((DetectionDirection == DetectionDirections.Both)
			    || ((DetectionDirection == DetectionDirections.Front) && (_isFacingRight))
			    || ((DetectionDirection == DetectionDirections.Back) && (!_isFacingRight)))
			{
				if (DetectMethod == DetectMethods.Ray)
				{
					raycast = MMDebug.RayCast(_raycastOrigin, _transformRight, DetectionDistance, TargetLayer, MMColors.DarkOrange, true);
				}
				else
				{
					raycast = Physics2D.BoxCast(_raycastOrigin + Vector2.right * _boxcastSize.x / 2f, _boxcastSize, 0f, Vector2.right, DetectionDistance - _boxcastSize.x, TargetLayer);
					MMDebug.RayCast(_raycastOrigin + _transformUp * RayWidth / 2f, _transformRight, DetectionDistance, TargetLayer, MMColors.DarkOrange, true);
					MMDebug.RayCast(_raycastOrigin - _transformUp * RayWidth / 2f, _transformRight, DetectionDistance, TargetLayer, MMColors.DarkOrange, true);
					MMDebug.RayCast(_raycastOrigin - _transformUp * RayWidth / 2f + _transformRight * DetectionDistance, _transformUp, RayWidth, TargetLayer, MMColors.DarkOrange, true);
					_drawLeftGizmo = true;
				}
                
				if (raycast)
				{
					hit = true;
					_direction = Vector2.right;
					_distanceToTarget = Vector2.Distance(_raycastOrigin, raycast.point);
					target = raycast.collider.gameObject.transform;
				}
			}

			if (hit)
			{
                // 我们要确保中间没有障碍
                float distance = Vector2.Distance((Vector2)target.transform.position, _raycastOrigin);
				RaycastHit2D raycastObstacle = MMDebug.RayCast(_raycastOrigin, ((Vector2)target.transform.position - _raycastOrigin).normalized, distance, ObstaclesLayer, Color.gray, true);
                
				if (raycastObstacle && _distanceToTarget > raycastObstacle.distance)
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
			if (SetTargetToNullIfNoneIsFound) { _brain.Target = null; }
			return false;           
		}

        /// <summary>
        /// 绘制射线小玩意儿
        /// </summary>
        protected virtual void OnDrawGizmos()
		{
			if ((DetectMethod != DetectMethods.WideRay) || !_init)
			{
				return;
			}

			Gizmos.color = _gizmosColor;

			_raycastOrigin.x = transform.position.x + _facingDirection.x * DetectionOriginOffset.x / 2;
			_raycastOrigin.y = transform.position.y + DetectionOriginOffset.y;

			if ((DetectionDirection == DetectionDirections.Both)
			    || ((DetectionDirection == DetectionDirections.Front) && (!_isFacingRight))
			    || ((DetectionDirection == DetectionDirections.Back) && (_isFacingRight)))
			{
				_gizmoCenter = (Vector3)_raycastOrigin + Vector3.left * DetectionDistance / 2f;
				_gizmoSize.x = DetectionDistance;
				_gizmoSize.y = RayWidth;
				_gizmoSize.z = 1f;
				Gizmos.DrawCube(_gizmoCenter, _gizmoSize);
			}

			if ((DetectionDirection == DetectionDirections.Both)
			    || ((DetectionDirection == DetectionDirections.Front) && (_isFacingRight))
			    || ((DetectionDirection == DetectionDirections.Back) && (!_isFacingRight)))
			{
				_gizmoCenter = (Vector3)_raycastOrigin + Vector3.right * DetectionDistance / 2f;
				_gizmoSize.x = DetectionDistance;
				_gizmoSize.y = RayWidth;
				_gizmoSize.z = 1f;
				Gizmos.DrawCube(_gizmoCenter, _gizmoSize);
			}
		}
	}
}
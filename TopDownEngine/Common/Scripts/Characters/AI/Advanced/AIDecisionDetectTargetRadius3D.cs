using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果TargetLayer layermask上的对象在其指定的半径内，此决定将返回true，否则返回false。它还会将Brain的Target设置为该对象。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Detect Target Radius 3D")]
	//[RequireComponent(typeof(Character))]
	public class AIDecisionDetectTargetRadius3D : AIDecision
	{
		/// the radius to search our target in
		[Tooltip("搜索目标的半径")]
		public float Radius = 3f;
        /// 要应用的偏移量（从碰撞器的中心开始）
        [Tooltip("要应用的偏移量（从碰撞器的中心开始）")]
		public Vector3 DetectionOriginOffset = new Vector3(0, 0, 0);
		/// the layer(s) to search our target on
		[Tooltip("搜索目标的层（多个层）")]
		public LayerMask TargetLayerMask;
		/// the layer(s) to block the sight
		[Tooltip("阻挡视线的层（多个层）")]
		public LayerMask ObstacleMask = LayerManager.ObstaclesLayerMask;
		/// the frequency (in seconds) at which to check for obstacles
		[Tooltip("检查障碍物的频率（以秒为单位）")]
		public float TargetCheckFrequency = 1f;
		/// if this is true, this AI will be able to consider itself (or its children) a target
		[Tooltip("如果这个条件为真，那么这个AI将能够将自己（或其子对象）视为目标")] 
		public bool CanTargetSelf = false;
		/// the maximum amount of targets the overlap detection can acquire
		[Tooltip("重叠检测能够获取的最大目标数量")]
		public int OverlapMaximum = 10;

		protected Collider _collider;
		protected Vector3 _raycastOrigin;
		protected Character _character;
		protected Color _gizmoColor = Color.yellow;
		protected bool _init = false;
		protected Vector3 _raycastDirection;
		protected Collider[] _hits;
		protected float _lastTargetCheckTimestamp = 0f;
		protected bool _lastReturnValue = false;
		protected List<Transform> _potentialTargets;

        /// <summary>
        /// 在init中，我们获取字符组件
        /// </summary>
        public override void Initialization()
		{
			_lastTargetCheckTimestamp = 0f;
			_potentialTargets = new List<Transform>();
			_character = this.gameObject.GetComponentInParent<Character>();
			_collider = this.gameObject.GetComponentInParent<Collider>();
			_gizmoColor.a = 0.25f;
			_init = true;
			_lastReturnValue = false;
			_hits = new Collider[OverlapMaximum];
		}

        /// <summary>
        /// 决定，我们检查目标
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return DetectTarget();
		}

        /// <summary>
        /// 如果在圆内找到目标则返回true
        /// </summary>
        /// <returns></returns>
        protected virtual bool DetectTarget()
		{
            // 我们检查是否需要探测一个新目标
            if (Time.time - _lastTargetCheckTimestamp < TargetCheckFrequency)
			{
				return _lastReturnValue;
			}
			_potentialTargets.Clear();

			_lastTargetCheckTimestamp = Time.time;
			_raycastOrigin = _collider.bounds.center + DetectionOriginOffset / 2;
			int numberOfCollidersFound = Physics.OverlapSphereNonAlloc(_raycastOrigin, Radius, _hits, TargetLayerMask);

            // 如果周围没有目标，我们就撤退
            if (numberOfCollidersFound == 0)
			{
				_lastReturnValue = false;
				return false;
			}

            // 我们遍历找到的每个对撞机
            int min = Mathf.Min(OverlapMaximum, numberOfCollidersFound);
			for (int i = 0; i < min; i++)
			{
				if (_hits[i] == null)
				{
					continue;
				}
                
				if (!CanTargetSelf)
				{
					if ((_hits[i].gameObject == _brain.Owner) || (_hits[i].transform.IsChildOf(this.transform)))
					{
						continue;
					}    
				}
                
				_potentialTargets.Add(_hits[i].gameObject.transform);
			}

            // 我们按距离对目标进行分类
            _potentialTargets.Sort(delegate(Transform a, Transform b)
			{return Vector3.Distance(this.transform.position,a.transform.position)
				.CompareTo(
					Vector3.Distance(this.transform.position,b.transform.position) );
			});

            // 我们返回第一个未隐蔽的目标
            foreach (Transform t in _potentialTargets)
			{
				_raycastDirection = t.position - _raycastOrigin;
				RaycastHit hit = MMDebug.Raycast3D(_raycastOrigin, _raycastDirection, _raycastDirection.magnitude, ObstacleMask.value, Color.yellow, true);
				if (hit.collider == null)
				{
					_brain.Target = t;
					_lastReturnValue = true;
					return true;
				}
			}

			_lastReturnValue = false;
			return false;
		}

        /// <summary>
        /// 绘制检测圆的小部件
        /// </summary>
        protected virtual void OnDrawGizmosSelected()
		{
			_raycastOrigin = transform.position + DetectionOriginOffset / 2;

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(_raycastOrigin, Radius);
			if (_init)
			{
				Gizmos.color = _gizmoColor;
				Gizmos.DrawSphere(_raycastOrigin, Radius);
			}            
		}
	}
}
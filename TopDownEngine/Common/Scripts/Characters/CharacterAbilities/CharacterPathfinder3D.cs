using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.AI;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这个类添加到一个3D角色，它将能够导航一个导航网格（如果在场景中当然有一个）
    /// </summary>
    [MMHiddenProperties("AbilityStartFeedbacks", "AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Pathfinder 3D")]
	public class CharacterPathfinder3D : CharacterAbility
	{
		public enum PathRefreshModes { None, TimeBased, SpeedThresholdBased }
		
		[Header("PathfindingTarget寻径目标")]

		/// the target the character should pathfind to
		[Tooltip("角色应该路径寻找的目标")]
		public Transform Target;
		
		/// specifies which area mask is passable by this agent
		[Tooltip("指定此代理可通行的哪个区域遮罩")]
		[MMNavMeshAreaMask]
		public int AreaMask = ~0;
		/// the distance to waypoint at which the movement is considered complete
		[Tooltip("到路径点的距离，在这个距离上运动被认为是完成的")]
		public float DistanceToWaypointThreshold = 1f;
		/// if the target point can't be reached, the distance threshold around that point in which to look for an alternative end point
		[Tooltip("如果无法到达目标点，则在该点周围寻找替代终点的距离阈值")]
		public float ClosestPointThreshold = 3f;
		/// a minimum delay (in seconds) between two navmesh requests - longer delay means better performance but less accuracy
		[Tooltip("两个导航网格请求之间的最小延迟（以秒为单位）-更长的延迟意味着更好的性能，但准确性较低")]
		public float MinimumDelayBeforePollingNavmesh = 0.1f;

		[Header("Path Refresh路径刷新")]
		/// the chosen mode in which to refresh the path (none : nothing will happen and path will only refresh on set new destination,
		/// time based : path will refresh every x seconds, speed threshold based : path will refresh every x seconds if the character's speed is below a certain threshold
		[Tooltip(" 所选择的刷新路径的模式(none：不发生任何事情，只在设置新目的地时刷新路径）；" +
                 "基于时间：路径将每x秒刷新一次，基于速度阈值：如果角色的速度低于某个阈值，路径将每x秒刷新一次")]
		public PathRefreshModes PathRefreshMode = PathRefreshModes.None;
		/// the speed under which the path should be recomputed, usually if the character blocks against an obstacle
		[Tooltip("当角色遇到障碍物时，需要重新计算路径的速度")]
		[MMEnumCondition("PathRefreshMode", (int)PathRefreshModes.SpeedThresholdBased)]
		public float RefreshSpeedThreshold = 1f;
		/// the interval at which to refresh the path, in seconds
		[Tooltip("刷新路径的时间间隔，单位为秒")]
		[MMEnumCondition("PathRefreshMode", (int)PathRefreshModes.TimeBased, (int)PathRefreshModes.SpeedThresholdBased)]
		public float RefreshInterval = 2f;

		[Header("Debug调试")]
		/// whether or not we should draw a debug line to show the current path of the character
		[Tooltip("我们是否应该画一条调试线来显示角色的当前路径")]
		public bool DebugDrawPath;

		/// the current path
		[MMReadOnly]
		[Tooltip("当前路径")]
		public NavMeshPath AgentPath;
		/// a list of waypoints the character will go through
		[MMReadOnly]
		[Tooltip("角色将经过的路点列表")]
		public Vector3[] Waypoints;
		/// the index of the next waypoint
		[MMReadOnly]
		[Tooltip("下一个路径点的索引")]
		public int NextWaypointIndex;
		/// the direction of the next waypoint
		[MMReadOnly]
		[Tooltip("下一个路径点的方向")]
		public Vector3 NextWaypointDirection;
		/// the distance to the next waypoint
		[MMReadOnly]
		[Tooltip("到下一个路径点的距离")]
		public float DistanceToNextWaypoint;

		public event System.Action<int, int, float> OnPathProgress;

		public virtual void InvokeOnPathProgress(int waypointIndex, int waypointsLength, float distance)
		{
			OnPathProgress?.Invoke(waypointIndex, waypointsLength, distance);
		}

		protected int _waypoints;
		protected Vector3 _direction;
		protected Vector2 _newMovement;
		protected Vector3 _lastValidTargetPosition;
		protected Vector3 _closestStartNavmeshPosition;
		protected Vector3 _closestTargetNavmeshPosition;
		protected NavMeshHit _navMeshHit;
		protected bool _pathFound;
		protected float _lastRequestAt = -Single.MaxValue;
		protected bool _initialized = false;

		protected override void Initialization()
		{
			base.Initialization();
			AgentPath = new NavMeshPath();
			_lastValidTargetPosition = this.transform.position;
			Array.Resize(ref Waypoints, 5);
			_initialized = true;
		}

        /// <summary>
        /// 设置角色路径查找的新目的地
        /// </summary>
        /// <param name="destinationTransform"></param>
        public virtual void SetNewDestination(Transform destinationTransform)
		{
			if (destinationTransform == null)
			{
				Target = null;
				return;
			}
			Target = destinationTransform;
			DeterminePath(this.transform.position, Target.position);
		}

        /// <summary>
        /// 在Update中，如果需要，我们绘制路径，确定下一个路径点，并在需要时移动到它
        /// </summary>
        public override void ProcessAbility()
		{
			if (Target == null)
			{
				return;
			}

			if (!AbilityAuthorized
			    || (_condition.CurrentState != CharacterStates.CharacterConditions.Normal))
			{
				return;
			}

			PerformRefresh();
			DrawDebugPath();
			DetermineNextWaypoint();
			DetermineDistanceToNextWaypoint();
			MoveController();
		}

        /// <summary>
        /// 将控制器移动到下一个点
        /// </summary>
        protected virtual void MoveController()
		{
			if ((Target == null) || (NextWaypointIndex <= 0))
			{
				_characterMovement.SetMovement(Vector2.zero);
				return;
			}
			else
			{
				_direction = (Waypoints[NextWaypointIndex] - this.transform.position).normalized;
				_newMovement.x = _direction.x;
				_newMovement.y = _direction.z;
				_characterMovement.SetMovement(_newMovement);
			}
		}

		protected virtual void PerformRefresh()
		{
			if (PathRefreshMode == PathRefreshModes.None)
			{
				return;
			}
			
			if (NextWaypointIndex <= 0)
			{
				return;
			}

			bool refreshNeeded = false;

			if (Time.time - _lastRequestAt > RefreshInterval)
			{
				refreshNeeded = true;
				_lastRequestAt = Time.time;
			}

			if (PathRefreshMode == PathRefreshModes.SpeedThresholdBased)
			{
				if (_controller.Speed.magnitude > RefreshSpeedThreshold)
				{
					refreshNeeded = false;
				}
			}

			if (refreshNeeded)
			{
				DeterminePath(this.transform.position, Target.position, true);
			}
		}

        /// <summary>
        /// 如果两点之间存在路径，则返回true
        /// </summary>
        /// <param name="startingPosition"></param>
        /// <param name="targetPosition"></param>
        /// <returns></returns>
        public virtual bool PathExists(Vector3 startingPosition, Vector3 targetPosition)
		{
			NavMesh.CalculatePath(startingPosition, targetPosition, AreaMask, AgentPath);
			return AgentPath.status == NavMeshPathStatus.PathComplete;
		}

        /// <summary>
        /// 返回导航网格上最接近指定位置的位置
        /// </summary>
        /// <param name="somePosition"></param>
        /// <returns></returns>
        protected virtual Vector3 FindClosestPositionOnNavmesh(Vector3 somePosition)
		{
			Vector3 newPosition = somePosition;
			if (NavMesh.SamplePosition(somePosition, out _navMeshHit, ClosestPointThreshold, AreaMask))
			{
				newPosition = _navMeshHit.position;
			}
			return newPosition;
		}

        /// <summary>
        /// 确定代理的下一个路径位置。如果找不到路径，NextPosition将为零
        /// </summary>
        /// <param name="startingPos"></param>
        /// <param name="targetPos"></param>
        /// <returns></returns>        
        protected virtual void DeterminePath(Vector3 startingPosition, Vector3 targetPosition, bool ignoreDelay = false)
		{
			if (!ignoreDelay && (Time.time - _lastRequestAt < MinimumDelayBeforePollingNavmesh))
			{
				return;
			}
			
			_lastRequestAt = Time.time;
			
			NextWaypointIndex = 0;
			
			_closestStartNavmeshPosition = FindClosestPositionOnNavmesh(startingPosition);
			_closestTargetNavmeshPosition = FindClosestPositionOnNavmesh(targetPosition);

			_pathFound = NavMesh.CalculatePath(_closestStartNavmeshPosition, _closestTargetNavmeshPosition, AreaMask, AgentPath);
			if (_pathFound)
			{
				_lastValidTargetPosition = _closestTargetNavmeshPosition;
			}
			else
			{
				NavMesh.CalculatePath(startingPosition, _lastValidTargetPosition, AreaMask, AgentPath);
			}

			_waypoints = AgentPath.GetCornersNonAlloc(Waypoints);
			if (_waypoints >= Waypoints.Length)
			{
				Array.Resize(ref Waypoints, _waypoints +5);
				_waypoints = AgentPath.GetCornersNonAlloc(Waypoints);
			}
			if (_waypoints >= 2)
			{
				NextWaypointIndex = 1;
			}

			InvokeOnPathProgress(NextWaypointIndex, Waypoints.Length, Vector3.Distance(this.transform.position, Waypoints[NextWaypointIndex]));
		}

        /// <summary>
        /// 根据到下一个航路点的距离确定它
        /// </summary>
        protected virtual void DetermineNextWaypoint()
		{
			if (_waypoints <= 0)
			{
				return;
			}
			if (NextWaypointIndex < 0)
			{
				return;
			}

			var distance = Vector3.Distance(this.transform.position, Waypoints[NextWaypointIndex]);
			if (distance <= DistanceToWaypointThreshold)
			{
				if (NextWaypointIndex + 1 < _waypoints)
				{
					NextWaypointIndex++;
				}
				else
				{
					NextWaypointIndex = -1;
				}
				InvokeOnPathProgress(NextWaypointIndex, _waypoints, distance);
			}
		}

        /// <summary>
        /// 确定到下一个航路点的距离
        /// </summary>
        protected virtual void DetermineDistanceToNextWaypoint()
		{
			if (NextWaypointIndex <= 0)
			{
				DistanceToNextWaypoint = 0;
			}
			else
			{
				DistanceToNextWaypoint = Vector3.Distance(this.transform.position, Waypoints[NextWaypointIndex]);
			}
		}

        /// <summary>
        /// 绘制调试线以显示当前路径
        /// </summary>
        protected virtual void DrawDebugPath()
		{
			if (DebugDrawPath)
			{
				if (_waypoints <= 0)
				{
					if (Target != null)
					{
						DeterminePath(transform.position, Target.position);
					}
				}
				for (int i = 0; i < _waypoints - 1; i++)
				{
					Debug.DrawLine(Waypoints[i], Waypoints[i + 1], Color.red);
				}
			}
		}
	}
}
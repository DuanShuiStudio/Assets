using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
#if MM_CINEMACHINE || MM_CINEMACHINE3
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个CinemachineExtension允许你在一个或多个轴上锁定一个Cinemachine
    /// </summary>
    [ExecuteInEditMode]
	[SaveDuringPlay]
	[AddComponentMenu("")]
	public class CinemachineAxisLocker : CinemachineExtension
	{
        /// 锁定轴的可能方法
        public enum Methods { ForcedPosition, InitialPosition, ColliderBoundsCenter, Collider2DBoundsCenter }
		/// whether or not axis should be locked on X
		[Tooltip("是否应该在X轴上锁定")]
		public bool LockXAxis = false;
		/// whether or not axis should be locked on Y
		[Tooltip("是否应该在Y轴上锁定")]
		public bool LockYAxis = false;
		/// whether or not axis should be locked on Z
		[Tooltip("是否应该在Z轴上锁定")]
		public bool LockZAxis = false;
		/// the selected method to lock axis on 
		[Tooltip("选择的方法来锁定轴")]
		public Methods Method = Methods.InitialPosition;
		/// the position to lock axis based on
		[MMEnumCondition("Method", (int)Methods.ForcedPosition)]
		[Tooltip("基于此位置锁定轴")]
		public Vector3 ForcedPosition;
		/// the collider to lock axis on
		[MMEnumCondition("Method", (int)Methods.ColliderBoundsCenter)]
		[Tooltip("锁定轴的碰撞器")]
		public Collider TargetCollider;
		/// the 2D collider to lock axis on
		[MMEnumCondition("Method", (int)Methods.Collider2DBoundsCenter)]
		[Tooltip("锁定轴的2D碰撞器")]
		public Collider2D TargetCollider2D;

		protected Vector3 _forcedPosition;

        /// <summary>
        /// 在开始时，我们根据选定的选择初始化强制位置
        /// </summary>
        protected virtual void Start()
		{
			switch (Method)
			{
				case Methods.ForcedPosition:
					_forcedPosition = ForcedPosition;
					break;

				case Methods.InitialPosition:
					_forcedPosition = this.transform.position;
					break;

				case Methods.ColliderBoundsCenter:
					_forcedPosition = TargetCollider.bounds.center;
					break;

				case Methods.Collider2DBoundsCenter:
					_forcedPosition = TargetCollider2D.bounds.center + (Vector3)TargetCollider2D.offset;
					break;
			}
		}

        /// <summary>
        /// 锁定位置
        /// </summary>
        /// <param name="vcam"></param>
        /// <param name="stage"></param>
        /// <param name="state"></param>
        /// <param name="deltaTime"></param>
        protected override void PostPipelineStageCallback(
			CinemachineVirtualCameraBase vcam,
			CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
		{
			if (enabled && stage == CinemachineCore.Stage.Body)
			{
				var pos = state.RawPosition;
				if (LockXAxis)
				{
					pos.x = _forcedPosition.x;
				}
				if (LockYAxis)
				{
					pos.y = _forcedPosition.y;
				}
				if (LockZAxis)
				{
					pos.z = _forcedPosition.z;
				}
				state.RawPosition = pos;
			}
		}
	}
}
#endif
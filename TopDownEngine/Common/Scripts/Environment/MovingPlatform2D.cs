using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个处理沿一组节点在2D中移动的平台的类
    /// </summary>
    [AddComponentMenu("TopDown Engine/Environment/Moving Platform 2D")]
	public class MovingPlatform2D : MMPathMovement
	{
		[Header("Safe DistanceSafe Distance")]
		/// whether or not to use Safe Distance mode, to force the character to move onto the platform 
		[Tooltip("是否使用安全距离模式，强制角色移动到平台上 ")]
		public bool UseSafeDistance = false;
		/// the distance to move the character at in safe distance mode
		[MMCondition("UseSafeDistance", true)]
		[Tooltip("在安全距离模式下移动角色的距离。")]
		public float ForcedSafeDistance = 1f;

		protected TopDownController2D _topdDownController2D;
		protected Vector3 _translationVector;
        
		protected virtual void AttachCharacterToMovingPlatform(Collider2D collider)
		{
			_topdDownController2D = collider.gameObject.MMGetComponentNoAlloc<TopDownController2D>();
			if (_topdDownController2D != null)
			{
				_topdDownController2D.SetMovingPlatform(this);
			}
			// 
            
			if (UseSafeDistance)
			{
				float distance = Vector3.Distance(collider.transform.position, this.transform.position);
				if (distance > ForcedSafeDistance)
				{
					_translationVector = (this.transform.position - collider.transform.position).normalized * Mathf.Min(distance, ForcedSafeDistance);
					collider.transform.Translate(_translationVector);
				}                    
			}
		}

		protected virtual void DetachCharacterFromPlatform(Collider2D collider)
		{
			_topdDownController2D = collider.gameObject.MMGetComponentNoAlloc<TopDownController2D>();
			if (_topdDownController2D != null)
			{
				_topdDownController2D.SetMovingPlatform(null);
			}
		}

        /// <summary>
        /// 当某个物体发生碰撞时，如果是顶视角控制器，我们就会将这个平台分配给它。
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			AttachCharacterToMovingPlatform(collider);
		}

        /// <summary>
        /// 当物体停止碰撞时，如果它是顶视角控制器，我们就取消把这个平台分配给它。
        /// </summary>
        /// <param name="collider"></param>
        protected virtual void OnTriggerExit2D(Collider2D collider)
		{
			DetachCharacterFromPlatform(collider);
		}
	}
}
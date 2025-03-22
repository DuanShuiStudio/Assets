using UnityEngine;
using System.Collections;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这个类添加到boxcollider中，以表示关卡的边界
    /// </summary>
    [AddComponentMenu("TopDown Engine/Camera/Level Limits")]
	public class LevelLimits : TopDownMonoBehaviour
	{
		/// left x coordinate
		[Tooltip("左x坐标")]
		public float LeftLimit;
		/// right x coordinate
		[Tooltip("右x坐标")]
		public float RightLimit;
		/// bottom y coordinate 
		[Tooltip("底部y坐标 ")]
		public float BottomLimit;
		/// top y coordinate
		[Tooltip("顶部y坐标")]
		public float TopLimit;

		protected BoxCollider2D _collider;

        /// <summary>
        /// 在awake状态下，用关卡限制填充公共变量
        /// </summary>
        protected virtual void Awake()
		{
			_collider = GetComponent<BoxCollider2D>();

			LeftLimit = _collider.bounds.min.x;
			RightLimit = _collider.bounds.max.x;
			BottomLimit = _collider.bounds.min.y;
			TopLimit = _collider.bounds.max.y;
		}
	}
}
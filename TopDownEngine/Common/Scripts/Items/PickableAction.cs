using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此类添加到对象上，并在拾取时触发指定的动作
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Pickable Action")]
	public class PickableAction : PickableItem
	{
        /// 在被拾取时触发的动作
        [Tooltip("the action(s) to trigger when picked")]
		public UnityEvent PickEvent;

        /// <summary>
        /// 当有东西与该对象碰撞时触发
        /// </summary>
        /// <param name="collider">Other.</param>
        protected override void Pick(GameObject picker)
		{
			base.Pick(picker);
			if (PickEvent != null)
			{
				PickEvent.Invoke();
			}
		}
	}
}
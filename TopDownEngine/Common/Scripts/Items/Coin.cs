using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 硬币管理器
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Coin")]
	public class Coin : PickableItem
	{
		/// The amount of points to add when collected
		[Tooltip("收集时要添加的分数")]
		public int PointsToAdd = 10;

        /// <summary>
        /// 当某物与硬币碰撞时触发
        /// </summary>
        /// <param name="collider">Other.</param>
        protected override void Pick(GameObject picker) 
		{
            // 我们为GameManager发送一个新的分数事件（以及其他可能也在监听它的类）
            TopDownEnginePointEvent.Trigger(PointsMethods.Add, PointsToAdd);
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个抽象类，用于定义武器的额外条件以防止其发射。
    /// </summary>
    public abstract class WeaponPreventShooting : TopDownMonoBehaviour
	{
        /// <summary>
        /// 重写此方法以定义射击条件
        /// </summary>
        /// <returns></returns>
        public abstract bool ShootingAllowed();
	}
}
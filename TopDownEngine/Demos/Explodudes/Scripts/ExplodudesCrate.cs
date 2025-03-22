using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个修改《爆炸演示场景》中板条（crates，可能是某种物体或道具）y轴缩放比例的类
    /// </summary>
    public class ExplodudesCrate : TopDownMonoBehaviour
	{
		protected const float MinHeight = 0.8f;
		protected const float MaxHeight = 1.1f;
		protected Vector3 _newScale = Vector3.one;

        /// <summary>
        /// 一开始我们随机化y轴缩放比例，这只是为了美观考虑
        /// </summary>
        protected virtual void Start()
		{
			_newScale.y = Random.Range(MinHeight, MaxHeight);
			this.transform.localScale = _newScale;
		}
	}
}
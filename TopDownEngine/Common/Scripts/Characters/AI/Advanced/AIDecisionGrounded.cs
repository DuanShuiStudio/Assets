using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果字符是接地的，这个决定将返回true，否则返回false。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Grounded")]
	public class AIDecisionGrounded : AIDecision
	{
		/// The duration, in seconds, after entering the state this Decision is in during which we'll ignore being grounded
		[Tooltip("进入此决策所在状态后，我们将忽略被禁足的持续时间（以秒为单位）")]
		public float GroundedBufferDelay = 0.2f;

		protected TopDownController _topDownController;
		protected float _startTime = 0f;

        /// <summary>
        /// 在init中，我们获取TopDownController组件
        /// </summary>
        public override void Initialization()
		{
			_topDownController = this.gameObject.GetComponentInParent<TopDownController>();
		}

        /// <summary>
        /// 在决定我们检查一下我们是否被禁足了
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return EvaluateGrounded();
		}

        /// <summary>
        /// 检查角色是否接地
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateGrounded()
		{
			if (Time.time - _startTime < GroundedBufferDelay)
			{
				return false;
			}
			return (_topDownController.Grounded);
		}

        /// <summary>
        /// 在进入状态时，我们重置开始时间
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			_startTime = Time.time;
		}
	}
}
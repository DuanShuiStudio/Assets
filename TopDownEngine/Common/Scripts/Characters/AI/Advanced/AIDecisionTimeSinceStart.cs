using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个决定将在关卡加载后指定的持续时间（以秒为单位）过去后返回true。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Time Since Start")]
	public class AIDecisionTimeSinceStart : AIDecision
	{
		/// The duration (in seconds) after which to return true
		[Tooltip("持续时间（以秒为单位），之后返回true")]
		public float AfterTime;

		protected float _startTime;

        /// <summary>
        /// 在init上，我们存储当前时间
        /// </summary>
        public override void Initialization()
		{
			_startTime = Time.time;
		}

        /// <summary>
        /// 在判定后，我们评估关卡开始后的时间
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return EvaluateTime();
		}

        /// <summary>
        /// 如果从级别开始的时间超过了我们的要求，则返回true
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateTime()
		{
			return (Time.time - _startTime >= AfterTime);
		}
	}
}
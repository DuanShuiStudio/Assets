using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个决定将在指定的持续时间之后返回true，在最小值和最大值之间随机选择（以秒为单位），因为大脑已经处于该决定所处的状态。在做完其他事情后用它做X秒的事情。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Time In State")]
	public class AIDecisionTimeInState : AIDecision
	{
		/// The minimum duration, in seconds, after which to return true
		[Tooltip("最小持续时间，以秒为单位，之后返回true")]
		public float AfterTimeMin = 2f;
		/// The maximum duration, in seconds, after which to return true
		[Tooltip("最大持续时间，以秒为单位，之后返回true")]
		public float AfterTimeMax = 2f;

		protected float _randomTime;

        /// <summary>
        /// 决定我们评估我们的时间
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return EvaluateTime();
		}

        /// <summary>
        /// 如果进入当前状态已经过了足够的时间，则返回true
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateTime()
		{
			if (_brain == null) { return false; }
			return (_brain.TimeInThisState >= _randomTime);
		}

        /// <summary>
        /// 在init中，我们随机化下一个延迟
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();
			RandomizeTime();
		}

        /// <summary>
        /// 在进入状态时，我们随机化下一个延迟
        /// </summary>
        public override void OnEnterState()
		{
			base.OnEnterState();
			RandomizeTime();
		}

        /// <summary>
        /// 在随机化时间中，我们随机化下一个延迟
        /// </summary>
        protected virtual void RandomizeTime()
		{
			_randomTime = Random.Range(AfterTimeMin, AfterTimeMax);
		}
	}
}
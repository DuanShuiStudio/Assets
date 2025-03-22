using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果当前大脑的目标在指定范围内，这个决定将返回true，否则返回false。
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Distance To Target")]
	public class AIDecisionDistanceToTarget : AIDecision
	{
        /// 可能的比较模式
        public enum ComparisonModes { StrictlyLowerThan, LowerThan, Equals, GreaterThan, StrictlyGreaterThan }
		/// the comparison mode
		[Tooltip("比较模式")]
		public ComparisonModes ComparisonMode = ComparisonModes.GreaterThan;
		/// the distance to compare with
		[Tooltip("用于比较的距离")]
		public float Distance;

        /// <summary>
        /// 在决定，我们检查我们的距离目标
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return EvaluateDistance();
		}

        /// <summary>
        /// 如果满足距离条件则返回true
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateDistance()
		{
			if (_brain.Target == null)
			{
				return false;
			}

			float distance = Vector3.Distance(this.transform.position, _brain.Target.position);

			if (ComparisonMode == ComparisonModes.StrictlyLowerThan)
			{
				return (distance < Distance);
			}
			if (ComparisonMode == ComparisonModes.LowerThan)
			{
				return (distance <= Distance);
			}
			if (ComparisonMode == ComparisonModes.Equals)
			{
				return (distance == Distance);
			}
			if (ComparisonMode == ComparisonModes.GreaterThan)
			{
				return (distance >= Distance);
			}
			if (ComparisonMode == ComparisonModes.StrictlyGreaterThan)
			{
				return (distance > Distance);
			}
			return false;
		}
	}
}
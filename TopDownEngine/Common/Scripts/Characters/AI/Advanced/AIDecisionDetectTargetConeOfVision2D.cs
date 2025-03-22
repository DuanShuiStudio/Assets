using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果MMConeOfVision检测到至少一个目标，这个Decision将返回true，并将其设置为大脑的目标
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Detect Target Cone Of Vision 2D")]
	public class AIDecisionDetectTargetConeOfVision2D : AIDecision
	{
		/// if this is true, this decision will set the AI Brain's Target to null if no target is found
		[Tooltip("如果这个值为真，则没有找到目标时，此决策将使AI大脑的目标设置为null")]
		public bool SetTargetToNullIfNoneIsFound = true;

		[Header("Bindings绑定")]
		/// the cone of vision 2D to rotate
		[Tooltip("要旋转的2D视锥")]
		public MMConeOfVision2D TargetConeOfVision2D;

        /// <summary>
        /// 在Init上，我们抓取MMConeOfVision
        /// </summary>
        public override void Initialization()
		{
			base.Initialization();
			if (TargetConeOfVision2D == null)
			{
				TargetConeOfVision2D = this.gameObject.GetComponent<MMConeOfVision2D>(); 
			}
		}

        /// <summary>
        /// 决定我们找一个目标
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return DetectTarget();
		}

        /// <summary>
        /// 如果MMConeOfVision至少有一个目标，它就成为我们新的大脑目标，这个决定为真，否则为假。
        /// </summary>
        /// <returns></returns>
        protected virtual bool DetectTarget()
		{
			if (TargetConeOfVision2D.VisibleTargets.Count == 0)
			{
				if (SetTargetToNullIfNoneIsFound)
				{
					_brain.Target = null;
				}

				return false;
			}
			else
			{
				_brain.Target = TargetConeOfVision2D.VisibleTargets[0];
				return true;
			}
		}
	}
}
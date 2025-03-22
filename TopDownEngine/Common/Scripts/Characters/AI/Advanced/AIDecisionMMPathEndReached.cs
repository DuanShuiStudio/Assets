using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 当目标路径到达它的终点时，这个决策将返回true（这要求它处于OnlyOnce循环模式）
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision MM Path End Reached")]
    public class AIDecisionMMPathEndReached : AIDecision
    {
        public MMPath TargetPath;

        /// <summary>
        /// 我们在决定时返回true
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
        {
            return TargetPath.EndReached;
        }
    }
}

using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 该决策将掷骰子，如果结果低于或等于Odds值，则返回true
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Random")]
	public class AIDecisionRandom : AIDecision
	{
		[Header("Random随机数")]
		/// the total number to consider (in "5 out of 10", this would be 10)
		[Tooltip("要考虑的总数（在“5 / 10”中，这将是10）")]
		public int TotalChance = 10;
		/// when rolling our dice, if the result is below the Odds, this decision will be true. In "5 out of 10", this would be 5.
		[Tooltip("在掷骰子时，如果结果低于Odds，这个决策将为真。在“5 out of 10”中，这将是5")]
		public int Odds = 4;

		protected Character _targetCharacter;

        /// <summary>
        /// 在判定结果中，我们检查一下胜率是否对我们有利
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return EvaluateOdds();
		}

        /// <summary>
        /// 如果大脑的目标正对着我们返回true（这需要目标有一个Character组件）
        /// </summary>
        /// <returns></returns>
        protected virtual bool EvaluateOdds()
		{
			int dice = MMMaths.RollADice(TotalChance);
			bool result = (dice <= Odds);
			return result;
		}
	}
}
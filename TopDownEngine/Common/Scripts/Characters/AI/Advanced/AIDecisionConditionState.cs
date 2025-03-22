using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 如果字符处于指定的条件状态，此决策将返回true
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Decisions/AI Decision Condition State")]
	public class AIDecisionConditionState : AIDecision
	{
		public CharacterStates.CharacterConditions ConditionState = CharacterStates.CharacterConditions.Stunned;
		protected Character _character;

        /// <summary>
        /// 在init中，我们获取字符组件
        /// </summary>
        public override void Initialization()
		{
			_character = this.gameObject.GetComponentInParent<Character>();
		}

        /// <summary>
        /// 在决定，我们检查我们在什么状态
        /// </summary>
        /// <returns></returns>
        public override bool Decide()
		{
			return (_character.ConditionState.CurrentState == ConditionState);
		}
	}
}
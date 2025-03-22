using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个简单的动作让你可以在运行时将AI的大脑换成一个新的大脑，在检查器中指定
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Actions/AI Action Swap Brain")]
	public class AIActionSwapBrain : AIAction
	{
		/// the brain to replace the Character's one with
		[Tooltip("要替换角色大脑的新的脑")]
		public AIBrain NewAIBrain;

		protected Character _character;

        /// <summary>
        /// 在init中，我们获取并存储字符
        /// </summary>
        public override void Initialization()
		{
			if(!ShouldInitialize) return;
			base.Initialization();
			_character = this.gameObject.GetComponentInParent<Character>();
		}

        /// <summary>
        /// 在PerformAction上，我们交换了我们的大脑
        /// </summary>
        public override void PerformAction()
		{
			SwapBrain();
		}

        /// <summary>
        /// 让旧的大脑失效，用新的大脑交换，然后激活它
        /// </summary>
        protected virtual void SwapBrain()
		{
			if (NewAIBrain == null) return;

            // 我们使“老”的大脑失去功能
            _character.CharacterBrain.gameObject.SetActive(false);
			_character.CharacterBrain.enabled = false;
            // 我们把它换成新的
            _character.CharacterBrain = NewAIBrain;
            // 我们启用新的并重置它
            NewAIBrain.gameObject.SetActive(true);
			NewAIBrain.enabled = true;
			NewAIBrain.Owner = _character.gameObject;
			NewAIBrain.ResetBrain();
		}
	}
}
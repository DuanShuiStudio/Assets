using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 该技能允许你将整个技能节点替换为参数中设置的节点
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Ability NodeSwap")]
	public class CharacterAbilityNodeSwap : CharacterAbility
	{
		[Header("Ability Node Swap能力节点交换")]
        
		/// a list of GameObjects that will replace this Character's set of ability nodes when the ability executes
		[Tooltip("一个游戏对象列表，当能力执行时，这些游戏对象将替换此角色的能力节点集")]
		public List<GameObject> AdditionalAbilityNodes;

        /// <summary>
        /// 如果玩家按下切换角色按钮，我们就交换能力。
        /// 此功能重用SwitchCharacter输入以避免增加输入项，但可以随意重写此方法以添加专用的方法
        /// </summary>
        protected override void HandleInput()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
            
			if (_inputManager.SwitchCharacterButton.State.CurrentState == MMInput.ButtonStates.ButtonDown)
			{
				SwapAbilityNodes();
			}
		}

        /// <summary>
        /// 禁用旧的能力节点，与新的能力节点交换，并启用它们
        /// </summary>
        public virtual void SwapAbilityNodes()
		{
			foreach (GameObject node in _character.AdditionalAbilityNodes)
			{
				node.gameObject.SetActive(false);
			}
            
			_character.AdditionalAbilityNodes = AdditionalAbilityNodes;

			foreach (GameObject node in _character.AdditionalAbilityNodes)
			{
				node.gameObject.SetActive(true);
			}

			_character.CacheAbilities();
		}
	}
}
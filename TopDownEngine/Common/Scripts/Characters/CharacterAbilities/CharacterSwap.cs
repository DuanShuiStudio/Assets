using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此能力添加到角色中，它将成为场景中可交换的角色池的一部分。
    /// 你需要一个CharacterSwapManager在你的场景中工作。
    /// </summary>
    [MMHiddenProperties("AbilityStopFeedbacks")]
	[AddComponentMenu("TopDown Engine/Character/Abilities/Character Swap")]
	public class CharacterSwap : CharacterAbility
	{
		[Header("Character Swap角色交换")]
		/// the order in which this character should be picked 
		[Tooltip("这个角色被选中的顺序")]
		public int Order = 0;
		/// the playerID to put back in the Character class once this character gets swapped
		[Tooltip("一旦这个角色被交换，就要把它放回到Character类中的玩家ID")]
		public string PlayerID = "Player1";

		[Header("AI")] 
		/// if this is true, the AI Brain (if there's one on this character) will reset on swap
		[Tooltip("如果这个条件为真，那么在交换时AI大脑（如果这个角色上有的话）将会重置")]
		public bool ResetAIBrainOnSwap = true;

		protected string _savedPlayerID;
		protected Character.CharacterTypes _savedCharacterType;

        /// <summary>
        /// 在init中，我们获取角色类型和playerID并存储它们以供以后使用
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_savedCharacterType = _character.CharacterType;
			_savedPlayerID = _character.PlayerID;
		}

        /// <summary>
        /// 由CharacterSwapManager调用，更改该角色的类型并设置其输入管理器
        /// </summary>
        public virtual void SwapToThisCharacter()
		{
			if (!AbilityAuthorized)
			{
				return;
			}
			PlayAbilityStartFeedbacks();
			_character.PlayerID = PlayerID;
			_character.CharacterType = Character.CharacterTypes.Player;
			_character.SetInputManager();
			if (_character.CharacterBrain != null)
			{
				_character.CharacterBrain.BrainActive = false;
			}
		}

        /// <summary>
        /// 当另一个角色取代这个角色成为活动角色时调用，重置它的类型和玩家ID并杀死它的输入
        /// </summary>
        public virtual void ResetCharacterSwap()
		{
			_character.CharacterType = Character.CharacterTypes.AI;
			_character.PlayerID = _savedPlayerID;
			_character.SetInputManager(null);
			_characterMovement.SetHorizontalMovement(0f);
			_characterMovement.SetVerticalMovement(0f);
			_character.ResetInput();
			if (_character.CharacterBrain != null)
			{
				_character.CharacterBrain.BrainActive = true;
				if (ResetAIBrainOnSwap)
				{
					_character.CharacterBrain.ResetBrain();    
				}
			}
		}

        /// <summary>
        /// 如果此角色是当前活跃的交换角色，则返回true
        /// </summary>
        /// <returns></returns>
        public virtual bool Current()
		{
			return (_character.CharacterType == Character.CharacterTypes.Player);
		}
	}
}
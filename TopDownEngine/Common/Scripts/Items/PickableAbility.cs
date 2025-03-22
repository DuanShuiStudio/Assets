using MoreMountains.Tools;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此类添加到具有触发器2D碰撞箱的对象上，它将变成一个可选对象，能够允许或禁止角色上的能力
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Pickable Ability")]
	public class PickableAbility : PickableItem
	{
		public enum Methods
		{
			Permit,
			Forbid
		}

		[Header("Pickable Ability可选能力")] 
		/// whether this object should permit or forbid an ability when picked
		[Tooltip("这个对象在被选择时应该允许还是禁止某种能力")]
		public Methods Method = Methods.Permit;
		/// whether or not only characters of Player type should be able to pick this 
		[Tooltip("是否只有玩家类型的角色才能选择这个")]
		public bool OnlyPickableByPlayerCharacters = true;

		[HideInInspector] public string AbilityTypeAsString;

        /// <summary>
        /// 检查对象是否是可选的
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        protected override bool CheckIfPickable()
		{
			_character = _collidingObject.GetComponent<Character>();

            // 如果与硬币碰撞的不是角色行为，我们不进行任何操作并退出
            if (_character == null)
			{
				return false;
			}

			if (OnlyPickableByPlayerCharacters && (_character.CharacterType != Character.CharacterTypes.Player))
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// 在拾取时，我们允许或禁止目标能力
		/// </summary>
		protected override void Pick(GameObject picker)
		{
			if (_character == null)
			{
				return;
			}
			bool newState = (Method == Methods.Permit);
			CharacterAbility ability = _character.FindAbilityByString(AbilityTypeAsString);
			if (ability != null)
			{
				ability.PermitAbility(newState);
			}
		}
	}
}
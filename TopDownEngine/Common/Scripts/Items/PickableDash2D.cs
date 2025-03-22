using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace  MoreMountains.TopDownEngine
{
    /// <summary>
    /// 此类是你如何创建可选能力的一个示例，这些物品在被拾取时启用一项能力
    /// 在拾取角色上。这个示例将允许CharacterDash2D能力
    /// 当然，你可以为任何其他能力创建更多这样的物品
    ///
    /// 为了尝试一下，例如在KoalaDungeon演示场景中，创建一个新的空游戏对象，为其添加一个2D盒子碰撞器，并确保它是触发器
    /// 然后为其添加此组件。选择Koala预制件，取消勾选其Dash 2D能力的“允许能力”选项。现在播放场景
    /// 尝试冲刺（你不能），抓取这个对象，现在你就可以冲刺了
    /// </summary>
    public class PickableDash2D : PickableItem
	{
        /// <summary>
        /// 为了确保这可以被拾取，我们确保我们处理的是一个玩家角色
        /// </summary>
        /// <returns></returns>
        protected override bool CheckIfPickable()
		{
			_character = _collidingObject.GetComponent<Character>();

			if (_character == null)
			{
				return false;
			}
			if (_character.CharacterType != Character.CharacterTypes.Player)
			{
				return false;
			}
			return true;
		}

        /// <summary>
        /// 在拾取时，如果找到了，我们允许该能力
        /// </summary>
        /// <param name="picker"></param>
        protected override void Pick(GameObject picker)
		{
			_character.gameObject.MMGetComponentNoAlloc<Character>()?.FindAbility<CharacterDash2D>()?.PermitAbility(true);
		}
	}
}
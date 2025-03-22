using UnityEngine;

namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 在多人游戏环境中工作时，将此类添加到可以拾取物品的角色上，ItemPickers将自动将物品发送给正确的PlayerID
    /// </summary>
    public class InventoryCharacterIdentifier : MonoBehaviour
	{
        /// 玩家的唯一ID
        public string PlayerID = "Player1";
	}    
}
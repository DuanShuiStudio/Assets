using System;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 这个类展示了一个简单存储箱的示例，当玩家角色站在上面时会显示目标物品栏，当角色离开区域时会关闭它
    /// </summary>
    public class InventoryDemoStorageBox : MonoBehaviour
	{
		public CanvasGroup TargetCanvasGroup;

		public virtual void OpenStorage(string playerID)
		{
			TargetCanvasGroup.alpha = 1;
		}

		public virtual void CloseStorage(string playerID)
		{
			TargetCanvasGroup.alpha = 0;
		}
		
		public virtual void OnTriggerEnter(Collider collider)
		{
			OnEnter(collider.gameObject);
		}

		public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			OnEnter(collider.gameObject);
		}

		protected virtual void OnEnter(GameObject collider)
		{
            // 如果与拾取器碰撞的不是玩家，我们什么也不做并退出。
            if (!collider.CompareTag("Player"))
			{
				return;
			}

			string playerID = "Player1";
			InventoryCharacterIdentifier identifier = collider.GetComponent<InventoryCharacterIdentifier>();
			if (identifier != null)
			{
				playerID = identifier.PlayerID;
			}

			OpenStorage(playerID);
		}

		public void OnTriggerExit(Collider collider)
		{
			OnExit(collider.gameObject);
		}

		public virtual void OnTriggerExit2D (Collider2D collider) 
		{
			OnExit(collider.gameObject);
		}
		
		protected virtual void OnExit(GameObject collider)
		{
            // 如果与拾取器碰撞的不是玩家，我们什么也不做并退出
            if (!collider.CompareTag("Player"))
			{
				return;
			}

			string playerID = "Player1";
			InventoryCharacterIdentifier identifier = collider.GetComponent<InventoryCharacterIdentifier>();
			if (identifier != null)
			{
				playerID = identifier.PlayerID;
			}

			CloseStorage(playerID);
		}
	}	
}
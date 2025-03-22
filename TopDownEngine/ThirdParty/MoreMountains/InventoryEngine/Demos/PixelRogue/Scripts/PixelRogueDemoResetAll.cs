using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 一个非常小的类，用于在PixelRogue演示中重置物品栏和持久化数据。
    /// </summary>
    public class PixelRogueDemoResetAll : MonoBehaviour
	{
		const string _inventorySaveFolderName = "InventoryEngine"; 
		
		public virtual void ResetAll()
		{
            // 我们删除用于物品栏的保存文件夹
            MMSaveLoadManager.DeleteSaveFolder (_inventorySaveFolderName);
            // 我们删除我们的持久化数据
            MMPersistenceManager.Instance.ResetPersistence();
            // 我们重新加载场景
            SceneManager.LoadScene("PixelRogueRoom1");
		}
	}	
}


using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.InventoryEngine
{
    /// <summary>
    /// 一个演示类，用于从一级跳转到另一级
    /// </summary>
    public class ChangeLevel : MonoBehaviour 
	{
		/// The exact name of the scene to go to when entering the ChangeLevel zone
		[MMInformation("demo-这个演示组件，当添加到BoxCollider2D时，会在角色进入碰撞器时将场景更改为下面字段中指定的场景", MMInformationAttribute.InformationType.Info,false)]
		[Tooltip("demo-进入ChangeLevel区域时要跳转到的场景的确切名称")]
		public string Destination;

        /// <summary>
        /// 当角色进入ChangeLevel区域时，我们会触发一次通用保存，然后加载目标场景
        /// </summary>
        /// <param name="collider">Collider.</param>
        public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			if ((Destination != null) && (collider.gameObject.GetComponent<InventoryDemoCharacter>() != null))
			{
				MMGameEvent.Trigger("Save");
				SceneManager.LoadScene(Destination);
			}
		}
	}
}
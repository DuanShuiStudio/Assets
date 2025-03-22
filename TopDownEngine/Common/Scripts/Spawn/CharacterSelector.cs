using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将此组件添加到一个按钮（例如），以便能够存储一个选定的角色，并且可选地转到另一个场景
    /// 在DeadlineCharacterSelection演示场景中可以看到它的使用示例。
    /// </summary>
    public class CharacterSelector : TopDownMonoBehaviour 
	{
		/// The name of the scene to go to when calling LoadNextScene()
		[Tooltip("调用LoadNextScene()时要去的场景的名称")]
		public string DestinationSceneName;
		/// The character prefab to store in the GameManager
		[Tooltip("要存储在GameManager中的角色预制件")]
		public Character CharacterPrefab;

        /// <summary>
        /// 将选定的角色预制件存储在GameManager中
        /// </summary>
        public virtual void StoreCharacterSelection()
		{
			GameManager.Instance.StoreSelectedCharacter (CharacterPrefab);
		}

        /// <summary>
        /// 在GameManager中存储了选定的角色后，加载下一个场景
        /// </summary>
        public virtual void LoadNextScene()
		{
			StoreCharacterSelection ();
			MMSceneLoadingManager.LoadScene(DestinationSceneName);
		}
	}
}
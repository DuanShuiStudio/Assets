using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 此组件允许定义一个级别，然后可以访问和加载该级别。主要用于关卡地图场景
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/Level Selector")]
	public class LevelSelector : TopDownMonoBehaviour
	{
		/// the exact name of the target level
		[Tooltip("目标关卡的确切名称")]
		public string LevelName;

		/// if this is true, GoToLevel will ignore the LevelManager and do a direct call
		[Tooltip("如果为真，GoToLevel将忽略LevelManager并直接调用")]
		public bool DoNotUseLevelManager = false;

		/// if this is true, any persistent character will be destroyed when loading the new level
		[Tooltip("如果为真，加载新关卡时任何持久角色都会被销毁")]
		public bool DestroyPersistentCharacter = false;

        /// <summary>
        /// 加载检查器中指定的关卡
        /// </summary>
        public virtual void GoToLevel()
		{
			LoadScene(LevelName);
		}

        /// <summary>
        /// 加载一个新场景，无论是通过LevelManager还是不通过
        /// </summary>
        /// <param name="newSceneName"></param>
        protected virtual void LoadScene(string newSceneName)
		{
			if (DestroyPersistentCharacter)
			{
				GameManager.Instance.DestroyPersistentCharacter();
			}
			
			if (GameManager.Instance.Paused)
			{
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
			}
				
			if (DoNotUseLevelManager)
			{
				MMAdditiveSceneLoadingManager.LoadScene(newSceneName);    
			}
			else
			{
				LevelManager.Instance.GotoLevel(newSceneName);   
			}
		}

        /// <summary>
        /// 开始当前关卡，而无需重新加载整个场景
        /// </summary>
        public virtual void RestartLevel()
		{
			if (GameManager.Instance.Paused)
			{
				TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
			}            
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.RespawnStarted, null);
		}

        /// <summary>
        /// 加载当前关卡
        /// </summary>
        public virtual void ReloadLevel()
		{
            // 我们为GameManager（以及可能的其他类）触发一个unPause事件
            TopDownEngineEvent.Trigger(TopDownEngineEventTypes.UnPause, null);
			LoadScene(SceneManager.GetActiveScene().name);
		}
		
	}
}
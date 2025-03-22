using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个可拾取的星星，如果被拾取会触发TopDownEngineStarEvent
    /// 由你来负责实现处理该事件的相关内容。
    /// 你可以查看DeadlineStar（死亡之星）和DeadlineProgressManager来获取相关示例
    /// </summary>
    [AddComponentMenu("TopDown Engine/Items/Star")]
	public class Star : PickableItem
	{
		/// the ID of this star, used by the progress manager to know which one got unlocked
		[Tooltip("该星星的ID，进度管理器通过它来知晓哪个星星被解锁了")]
		public int StarID;

        /// <summary>
        /// 当有物体与星星发生碰撞时触发
        /// </summary>
        /// <param name="collider">Other.</param>
        protected override void Pick(GameObject picker) 
		{
            // 我们发送一个新的星星事件，任何人都可以捕获
            TopDownEngineStarEvent.Trigger(SceneManager.GetActiveScene().name, StarID);
		}
	}

	public struct TopDownEngineStarEvent
	{
		public string SceneName;
		public int StarID;

		public TopDownEngineStarEvent(string sceneName, int starID)
		{
			SceneName = sceneName;
			StarID = starID;
		}

		static TopDownEngineStarEvent e;
		public static void Trigger(string sceneName, int starID)
		{
			e.SceneName = sceneName;
			e.StarID = starID;
			MMEventManager.TriggerEvent(e);
		}
	}
}
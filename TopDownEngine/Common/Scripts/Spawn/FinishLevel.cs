using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 将这个类添加到一个触发器上，它将会把玩家传送到下一个关卡
    /// </summary>
    [AddComponentMenu("TopDown Engine/Spawn/Finish Level")]
	public class FinishLevel : ButtonActivated
	{
		[Header("Finish Level完成关卡")]
		/// the exact name of the level to transition to 
		[Tooltip("要转换到的关卡的确切名称")]
		public string LevelName;

        /// <summary>
        /// 当按钮被按下时，我们开始对话
        /// </summary>
        public override void TriggerButtonAction()
		{
			if (!CheckNumberOfUses())
			{
				return;
			}
			base.TriggerButtonAction ();
			GoToNextLevel();
		}

        /// <summary>
        /// 加载下一个关卡
        /// </summary>
        public virtual void GoToNextLevel()
		{
			if (LevelManager.HasInstance)
			{
				LevelManager.Instance.GotoLevel(LevelName);
			}
			else
			{
				MMSceneLoadingManager.LoadScene(LevelName);
			}
		}
	}
}
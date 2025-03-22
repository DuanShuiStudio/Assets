using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个类，用于从一个关卡转到下一个关卡，同时指定目标关卡的入口点
    /// 入口点在每个关卡的LevelManager组件中定义。它们只是列表中的Transforms
    /// 列表中的索引是入口点的标识符
    /// </summary>
    [AddComponentMenu("TopDown Engine/Spawn/Go To Level Entry Point")]
	public class GoToLevelEntryPoint : FinishLevel 
	{
		[Space(10)]
		[Header("Points of Entry入口点")]

		/// Whether or not to use entry points. If you don't, you'll simply move on to the next level
		[Tooltip("是否使用入口点。如果你不使用，你将简单地移动到下一个关卡")]
		public bool UseEntryPoints = false;
		/// The index of the point of entry to move to in the next level
		[Tooltip("在下一个关卡中要移动到的入口点的索引")]
		public int PointOfEntryIndex;
		/// The direction to face when moving to the next level
		[Tooltip("移动到下一个关卡时要面对的方向")]
		public Character.FacingDirections FacingDirection;

        /// <summary>
        /// 加载下一个关卡并将目标入口点索引存储在游戏管理器中
        /// </summary>
        public override void GoToNextLevel()
		{
			if (UseEntryPoints)
			{
				GameManager.Instance.StorePointsOfEntry(LevelName, PointOfEntryIndex, FacingDirection);
			}
			
			base.GoToNextLevel ();
		}
	}
}
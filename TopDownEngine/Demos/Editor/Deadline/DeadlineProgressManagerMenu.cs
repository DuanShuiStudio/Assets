using UnityEditor;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{	
	public static class DeadlineProgressManagerMenu 
	{
		[MenuItem("Tools/More Mountains/Reset all Deadline progress",false,21)]
        /// <summary>
        /// 在Deadline演示中添加一个菜单项以重置所有进度
        /// </summary>
        private static void ResetProgress()
		{
			DeadlineProgressManager.Instance.ResetProgress ();
		}
	}
}
using MoreMountains.Tools;
/*
 *demo
 *demo
 *demo
 *demo
 */
namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个用于Deadline演示的专用类，用于加载下一关卡
    /// </summary>
    public class DeadlineFinishLevel : FinishLevel 
	{
        /// <summary>
        /// 加载下一关卡
        /// </summary>
        public override void GoToNextLevel()
		{
			TopDownEngineEvent.Trigger(TopDownEngineEventTypes.LevelComplete, null);
			MMGameEvent.Trigger("Save");
			LevelManager.Instance.GotoLevel (LevelName);
		}	
	}
}
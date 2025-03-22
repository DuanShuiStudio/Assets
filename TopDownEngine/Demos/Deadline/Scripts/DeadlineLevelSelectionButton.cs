using UnityEngine;
using UnityEngine.UI;
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
    /// 一个用于处理Deadline演示关卡选择器中关卡显示的类
    /// </summary>
    public class DeadlineLevelSelectionButton : TopDownMonoBehaviour 
	{
		/// the name of the scene to bind to this element
		[Tooltip("demo-要绑定到这个元素的场景名称")]
		public string SceneName;
		/// the icon showing whether or not the level is locked
		[Tooltip("demo-显示关卡是否锁定的图标")]
		public Image LockedIcon;
		/// the icon showing whether or not the level has been completed
		[Tooltip("demo-显示关卡是否已完成的图标")]
		public Image CompletedIcon;

		[Header("demo-Stars星星")]
		/// the stars to display in the level element
		[Tooltip("demo-在关卡元素中显示的星星")]
		public Image[] Stars;
		/// the color to apply to stars when they're locked
		[Tooltip("demo-当星星被锁定时要应用的颜色")]
		public Color StarOffColor;
		/// the color to apply to stars once they've been unlocked
		[Tooltip("demo-一旦星星被解锁后要应用的颜色")]
		public Color StarOnColor;

		protected Button _button;

        /// <summary>
        /// 在检查器中指定要进入的关卡的方法
        /// </summary>
        public virtual void GoToLevel()
		{
			MMSceneLoadingManager.LoadScene(SceneName);
		}

        /// <summary>
        /// 在开始时，我们初始化我们的设置
        /// </summary>
        protected virtual void Start()
		{
			InitialSetup ();
		}

        /// <summary>
        /// 根据当前保存的数据设置各种元素（星星、锁定图标）
        /// </summary>
        protected virtual void InitialSetup()
		{
			_button = this.gameObject.GetComponent<Button>();
			
			foreach (DeadlineScene scene in DeadlineProgressManager.Instance.Scenes)
			{
				if (scene.SceneName == SceneName)
				{
					CompletedIcon.gameObject.SetActive(scene.LevelComplete);
					LockedIcon.gameObject.SetActive(!scene.LevelUnlocked);
					
					if (!scene.LevelUnlocked)
					{
						_button.interactable = false;
					}
					
					for (int i=0; i<Stars.Length; i++)
					{
						Stars [i].color = (scene.CollectedStars [i]) ? StarOnColor : StarOffColor;							
					}
				}
			}
		}
	}
}
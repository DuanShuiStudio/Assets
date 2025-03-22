using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback lets you unload a scene by name or build index
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈功能允许你通过场景名称或构建索引来卸载场景。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Scene/Unload Scene")]
	public class MMF_UnloadScene : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
		public enum ColorModes { Instant, Gradient, Interpolate }

        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SceneColor; } }

		public override bool EvaluateRequiresSetup()
		{
			if (Method == Methods.BuildIndex)
			{
				return false;
                
			}
			else if (Method == Methods.SceneName)
			{
				return ((SceneName == null) || (SceneName == ""));
			}
			return false;
		}
		public override string RequiredTargetText { get { return SceneName;  } }
		public override string RequiresSetupText { get { return "此反馈功能要求你在下方指定一个场景名称。同时，请确保你已将目标场景添加到项目的构建设置中。 "; } }
		#endif
        
		public enum Methods { BuildIndex, SceneName }

		[MMFInspectorGroup("Unload Scene", true, 43, false)]
        
		/// whether to unload a scene by build index or by name
		[Tooltip("是要通过构建索引还是通过名称来卸载场景。")]
		public Methods Method = Methods.SceneName;

		/// the build ID of the scene to unload, find it in your Build Settings
		[Tooltip("要卸载的场景的构建 ID，你可以在构建设置中找到它。")]
		[MMFEnumCondition("Method", (int)Methods.BuildIndex)]
		public int BuildIndex = 0;

		/// the name of the scene to unload
		[Tooltip("要卸载的场景的名称。")]
		[MMFEnumCondition("Method", (int)Methods.SceneName)]
		public string SceneName = "";

        
		/// whether or not to output warnings if the scene doesn't exist or can't be loaded
		[Tooltip("如果场景不存在或无法加载，是否输出警告信息。")]
		public bool OutputWarningsIfNeeded = true;
        
		protected Scene _sceneToUnload;

        /// <summary>
        /// 播放时，我们会更改目标 TextMesh Pro 文本组件（TMPText）的文本内容。 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			if (Method == Methods.BuildIndex)
			{
				_sceneToUnload = SceneManager.GetSceneByBuildIndex(BuildIndex);
			}
			else
			{
				_sceneToUnload = SceneManager.GetSceneByName(SceneName);
			}

			if ((_sceneToUnload != null) && (_sceneToUnload.isLoaded))
			{
				SceneManager.UnloadSceneAsync(_sceneToUnload);    
			}
			else
			{
				if (OutputWarningsIfNeeded)
				{
					Debug.LogWarning("卸载场景反馈：你正在尝试卸载一个尚未加载的场景。");    
				}
			}
		}
	}
}
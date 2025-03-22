using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will request the load of a new scene, using the method of your choice
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈将使用你选择的方法请求加载一个新场景。 ")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks.MMTools")]
	[FeedbackPath("Scene/Load Scene")]
	public class MMF_LoadScene : MMF_Feedback
	{
		/// 一个用于一次性禁用所有此类反馈的静态布尔值。
		public static bool FeedbackTypeAuthorized = true;
		/// 设置此反馈在检查器中的颜色。
		#if UNITY_EDITOR
		public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.SceneColor; } }
		public override bool EvaluateRequiresSetup() { return (DestinationSceneName == ""); }
		public override string RequiredTargetText { get { return DestinationSceneName;  } }
		public override string RequiresSetupText { get { return "This feedback requires that you specify a DestinationSceneName below. Make sure you also add that destination scene to your Build Settings."; } }
		#endif

		/// 加载新场景的可能方式：
		/// - direct直接：使用Unity的场景管理器（SceneManager）应用程序编程接口（API） 
		/// - direct additive直接叠加：使用Unity的场景管理器（SceneManager）API，但采用叠加模式（即在当前场景之上加载新场景） 
		/// - MMSceneLoadingManager场景加载管理器：这是一种简单、原汁原味的MM加载场景的方式 
		/// - MMAdditiveSceneLoadingManager叠加场景加载管理器：一种更高级的场景加载方式，拥有（多得多的）更多选项。 
		public enum LoadingModes { Direct, MMSceneLoadingManager, MMAdditiveSceneLoadingManager, DirectAdditive }

		[MMFInspectorGroup("Scene Loading", true, 57, true)]
		/// the name of the loading screen scene to use
		[Tooltip("要使用的加载屏幕场景的名称 - 必须将其添加到你的构建设置中。 ")]
		public string LoadingSceneName = "MMAdditiveLoadingScreen";
		/// the name of the destination scene
		[Tooltip("目标场景的名称 - 必须将其添加到你的构建设置中。 ")]
		public string DestinationSceneName = "";

		[Header("Mode模式")] 
		/// the loading mode to use
		[Tooltip("用于加载目标场景的加载模式： " +
		         "- direct直接：使用Unity的场景管理器（SceneManager）应用程序编程接口（API） " +
		         "- MMSceneLoadingManage MM场景加载管理器：这是一种简单的、最初的MM加载场景的方式。 " +
		         "- MMAdditiveSceneLoadingManager MM叠加场景加载管理器：一种更高级的场景加载方式，拥有（多得多的）更多选项。")]
		public LoadingModes LoadingMode = LoadingModes.MMAdditiveSceneLoadingManager;
        
		[Header("Loading Scene Manager加载场景管理器")]
		/// the priority to use when loading the new scenes
		[Tooltip("加载新场景时要使用的优先级")]
		public ThreadPriority Priority = ThreadPriority.High;
		/// whether or not to perform extra checks to make sure the loading screen and destination scene are in the build settings
		[Tooltip("是否执行额外的检查，以确保加载屏幕场景和目标场景已添加到构建设置中 。 ")]
		public bool SecureLoad = true;
		/// the chosen way to unload scenes (none, only the active scene, all loaded scenes)
		[Tooltip("所选择的卸载场景的方式（不卸载、仅卸载当前活动场景、卸载所有已加载的场景） ")]
		[MMFEnumCondition("LoadingMode", (int)LoadingModes.MMAdditiveSceneLoadingManager)]
		public MMAdditiveSceneLoadingManagerSettings.UnloadMethods UnloadMethod =
			MMAdditiveSceneLoadingManagerSettings.UnloadMethods.AllScenes;
		/// the name of the anti spill scene to use when loading additively.
		/// If left empty, that scene will be automatically created, but you can specify any scene to use for that. Usually you'll want your own anti spill scene to be just an empty scene, but you can customize its lighting settings for example.
		[Tooltip("在以叠加方式加载时要使用的防溢出场景的名称。 " +
		         "如果留空，该场景将自动创建，但你可以指定任何场景来使用。通常你会希望自己的防溢出场景只是一个空场景，但例如你也可以自定义其光照设置。 ")]
		[MMFEnumCondition("LoadingMode", (int)LoadingModes.MMAdditiveSceneLoadingManager)]
		public string AntiSpillSceneName = "";
		
		[MMFInspectorGroup("Loading Scene Delays", true, 58)] 
		/// a delay (in seconds) to apply before the first fade plays
		[Tooltip("在首次渐变效果开始之前要应用的延迟时间（以秒为单位） ")]
		public float BeforeEntryFadeDelay = 0f;
		/// the duration (in seconds) of the entry fade
		[Tooltip("进入时渐变效果的持续时间（以秒为单位） ")]
		public float EntryFadeDuration = 0.2f;
		/// a delay (in seconds) to apply after the first fade plays
		[Tooltip("在首次渐变效果播放之后要应用的延迟时间（以秒为单位） ")]
		public float AfterEntryFadeDelay = 0f;
		/// a delay (in seconds) to apply before the exit fade plays
		[Tooltip("在退出渐变效果播放之前要应用的延迟时间（以秒为单位） ")]
		public float BeforeExitFadeDelay = 0f;
		/// the duration (in seconds) of the exit fade
		[Tooltip("退出时渐变效果的持续时间（以秒为单位）")]
		public float ExitFadeDuration = 0.2f;
		
		[MMFInspectorGroup("Speed", true, 59)] 
		/// whether or not to interpolate progress (slower, but usually looks better and smoother)
		[Tooltip("是否对进度进行插值处理（这样速度会更慢，但通常看起来效果更好、更流畅） ")]
		public bool InterpolateProgress = true;
		/// the speed at which the progress bar should move if interpolated
		[Tooltip("如果进行了插值处理，进度条移动所应达到的速度 ")]
		public float ProgressInterpolationSpeed = 5f;
		/// a list of progress intervals (values should be between 0 and 1) and their associated speeds, letting you have the bar progress less linearly
		[Tooltip("一个进度区间列表（数值应介于0和1之间）以及它们对应的速度，这使你能够让进度条的推进不那么线性化。 ")]
		public List<MMSceneLoadingSpeedInterval> SpeedIntervals;
        
		[MMFInspectorGroup("Transitions", true, 59)]
		/// the order in which to play fades (really depends on the type of fader you have in your loading screen
		[Tooltip("渐变效果的播放顺序（这实际上取决于你在加载屏幕中所使用的渐变器类型） ")]
		public MMAdditiveSceneLoadingManager.FadeModes FadeMode = MMAdditiveSceneLoadingManager.FadeModes.FadeInThenOut;
		/// the tween to use on the entry fade
		[Tooltip("在进入渐变效果时要使用的补间动画 ")]
		public MMTweenType EntryFadeTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
		/// the tween to use on the exit fade
		[Tooltip("在退出渐变效果时要使用的补间动画 ")]
		public MMTweenType ExitFadeTween = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));

		/// <summary>
		/// 在播放时，我们使用指定的方法请求加载目标场景。
		/// </summary>
		/// <param name="position"></param>
		/// <param name="feedbacksIntensity"></param>
		protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}
			switch (LoadingMode)
			{
				case LoadingModes.Direct:
					SceneManager.LoadScene(DestinationSceneName);
					break;
				case LoadingModes.DirectAdditive:
					SceneManager.LoadScene(DestinationSceneName, LoadSceneMode.Additive);
					break;
				case LoadingModes.MMSceneLoadingManager:
					MMSceneLoadingManager.LoadScene(DestinationSceneName, LoadingSceneName);
					break;
				case LoadingModes.MMAdditiveSceneLoadingManager:
					MMAdditiveSceneLoadingManager.LoadScene(DestinationSceneName, LoadingSceneName, 
						Priority, SecureLoad, InterpolateProgress, 
						BeforeEntryFadeDelay, EntryFadeDuration,
						AfterEntryFadeDelay,
						BeforeExitFadeDelay, ExitFadeDuration,
						EntryFadeTween, ExitFadeTween,
						ProgressInterpolationSpeed, FadeMode, UnloadMethod, AntiSpillSceneName,
						SpeedIntervals);
					break;
			}
		}
	}
}
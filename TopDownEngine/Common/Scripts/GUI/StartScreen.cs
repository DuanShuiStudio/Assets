using UnityEngine;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using MoreMountains.MMInterface;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 简单启动界面类
    /// </summary>
    [AddComponentMenu("TopDown Engine/GUI/Start Screen")]
	public class StartScreen : TopDownMonoBehaviour
	{
		/// the level to load after the start screen
		[Tooltip("启动界面后要加载的关卡")]
		public string NextLevel;
		public enum LoadingSceneModes { Regular, Additive}
		/// whether to load the scene normally or additively
		[Tooltip("是否以正常方式或附加方式加载场景")]
		public LoadingSceneModes LoadingSceneMode = LoadingSceneModes.Regular;
		/// the name of the MMSceneLoadingManager scene you want to use
		[Tooltip("你想使用的MMSceneLoadingManager场景的名称")]
		public string LoadingSceneName = "";
		/// the delay after which the level should auto skip (if less than 1s, won't autoskip)
		[Tooltip("关卡应自动跳过的延迟（如果小于1秒，则不会自动跳过）")]
		public float AutoSkipDelay = 0f;

		[Header("Fades淡出")]
		/// the duration of the fade from black at the start of the level
		[Tooltip("关卡开始时从黑色淡出的时间")]
		public float FadeInDuration = 1f;
		/// the duration of the fade to black at the end of the level
		[Tooltip("关卡结束时淡入黑色的时间")]
		public float FadeOutDuration = 1f;
		/// the tween type to use to fade the startscreen in and out 
		[Tooltip("用于淡入和淡出启动界面的过渡类型")]
		public MMTweenType Tween = new MMTweenType(MMTween.MMTweenCurve.EaseInOutCubic);

		[Header("Sound Settings Bindings声音设置绑定")]
		/// the switch used to turn the music on or off
		[Tooltip("用于打开或关闭音乐的开关")]
		public MMSwitch MusicSwitch;
		/// the switch used to turn the SFX on or off
		[Tooltip("用于打开或关闭SFX音效的开关")]
		public MMSwitch SfxSwitch;

		/// <summary>
		/// 初始化
		/// </summary>
		protected virtual void Awake()
		{	
			GUIManager.Instance.SetHUDActive (false);
			MMFadeOutEvent.Trigger(FadeInDuration, Tween);
			Cursor.visible = true;
			if (AutoSkipDelay > 1f)
			{
				FadeOutDuration = AutoSkipDelay;
				StartCoroutine (LoadFirstLevel ());
			}
		}

        /// <summary>
        /// 开始时，初始化音乐和SFX音效开关
        /// </summary>
        protected async void Start()
		{
			await Task.Delay(1);
			
			if (MusicSwitch != null)
			{
				MusicSwitch.CurrentSwitchState = MMSoundManager.Instance.settingsSo.Settings.MusicOn ? MMSwitch.SwitchStates.Right : MMSwitch.SwitchStates.Left;
				MusicSwitch.InitializeState ();
			}

			if (SfxSwitch != null)
			{
				SfxSwitch.CurrentSwitchState = MMSoundManager.Instance.settingsSo.Settings.SfxOn ? MMSwitch.SwitchStates.Right : MMSwitch.SwitchStates.Left;
				SfxSwitch.InitializeState ();
			}
		}

        /// <summary>
        /// 在更新期间，我们只需等待用户按下“跳转”按钮
        /// </summary>
        protected virtual void Update()
		{
			if (!Input.GetButtonDown ("Player1_Jump"))
				return;
			
			ButtonPressed ();
		}

        /// <summary>
        /// 当主按钮被按下时会发生什么？
        /// </summary>
        public virtual void ButtonPressed()
		{
			MMFadeInEvent.Trigger(FadeOutDuration, Tween);
            // 如果用户按下“跳转”按钮，我们开始第一关
            StartCoroutine(LoadFirstLevel ());
		}

        /// <summary>
        /// 加载下一关
        /// </summary>
        /// <returns>The first level.</returns>
        protected virtual IEnumerator LoadFirstLevel()
		{
			yield return new WaitForSeconds (FadeOutDuration);
			if (LoadingSceneName == "")
			{
				MMSceneLoadingManager.LoadScene (NextLevel);	
			}
			else
			{
				if (LoadingSceneMode == LoadingSceneModes.Additive)
				{
					MMAdditiveSceneLoadingManagerSettings settings = new MMAdditiveSceneLoadingManagerSettings();
					settings.LoadingSceneName = LoadingSceneName;
					MMAdditiveSceneLoadingManager.LoadScene(NextLevel, settings);	
				}
				else
				{
					MMSceneLoadingManager.LoadScene (NextLevel, LoadingSceneName);
				}
			}
			
		}
	}
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace MoreMountains.Feedbacks
{
	/// <summary>
	/// This feedback will let you change the scene's skybox on play, replacing it with another one, either a specific one, or one picked at random among multiple skyboxes.
	/// </summary>
	[AddComponentMenu("")]
	[FeedbackHelp("此反馈功能可让你在播放时更改场景的天空盒，将其替换为另一个天空盒，既可以是指定的某个天空盒，也可以是从多个天空盒中随机选取的一个。")]
	[MovedFrom(false, null, "MoreMountains.Feedbacks")]
	[FeedbackPath("Renderer/Skybox")]
	public class MMF_Skybox : MMF_Feedback
	{
        /// 一个静态布尔值，用于一次性禁用此类所有反馈。
        public static bool FeedbackTypeAuthorized = true;
        /// 设置此反馈的检查器颜色。
#if UNITY_EDITOR
        public override Color FeedbackColor { get { return MMFeedbacksInspectorColors.RendererColor; } }
#endif

        /// 天空盒是否是随机选择的。
        public enum Modes { Single, Random }

		[MMFInspectorGroup("Skybox", true, 65)]
        /// 所选的模式。
        public Modes Mode = Modes.Single;
        /// 在单一天空盒模式下要指定的天空盒。
        public Material SingleSkybox;
        /// 在随机模式下可供选择的天空盒。
        public Material[] RandomSkyboxes;

		protected Material _initialSkybox;

        /// <summary>
        /// 播放时，我们将场景的天空盒设置为一个新的天空盒。
        /// </summary>
        /// <param name="position"></param>
        /// <param name="feedbacksIntensity"></param>
        protected override void CustomPlayFeedback(Vector3 position, float feedbacksIntensity = 1.0f)
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			_initialSkybox = RenderSettings.skybox;
            
			if (Mode == Modes.Single)
			{
				RenderSettings.skybox = SingleSkybox;
			}
			else if (Mode == Modes.Random)
			{
				RenderSettings.skybox = RandomSkyboxes[Random.Range(0, RandomSkyboxes.Length)];
			}
		}

        /// <summary>
        /// 在恢复操作时，我们会将物体放回其初始位置。
        /// </summary>
        protected override void CustomRestoreInitialValues()
		{
			if (!Active || !FeedbackTypeAuthorized)
			{
				return;
			}

			RenderSettings.skybox = _initialSkybox;
		}
	}    
}
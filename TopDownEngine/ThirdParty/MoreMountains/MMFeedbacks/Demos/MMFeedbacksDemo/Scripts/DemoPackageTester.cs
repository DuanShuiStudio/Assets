using MoreMountains.Tools;
using UnityEngine;
using UnityEditor;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    ///  这个类用于MMFeedbacks演示，将检查需求并在必要时输出错误消息。
    /// </summary>
    [AddComponentMenu("")]
	public class DemoPackageTester : MonoBehaviour
	{
		[MMFInformation("这个组件仅用于在该演示的依赖项未安装时在控制台中显示错误。如果你愿意，可以安全地移除它，通常你不会想在自己的游戏中保留它", MMFInformationAttribute.InformationType.Warning, false)]
        /// 场景是否需要安装后期处理？
        public bool RequiresPostProcessing;
        /// 场景是否需要安装TextMesh Pro？
        public bool RequiresTMP;
        /// 场景是否需要安装Cinemachine？
        public bool RequiresCinemachine;

        /// <summary>
        /// 在唤醒时，我们测试依赖项
        /// </summary>
        protected virtual void Awake()
		{
			#if  UNITY_EDITOR
			if (Application.isPlaying)
			{
				TestForDependencies();    
			}
			#endif
		}

        /// <summary>
        /// 检查依赖项是否已正确安装
        /// </summary>
        protected virtual void TestForDependencies()
		{
			bool missingDependencies = false;
			string missingString = "";
			bool cinemachineFound = false;
			bool tmpFound = false;
			bool postProcessingFound = false;
            
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			cinemachineFound = true;
			#endif
                        
			#if (MM_TEXTMESHPRO || MM_UGUI2)
			tmpFound = true;
			#endif
                        
			#if MM_POSTPROCESSING
			postProcessingFound = true;
			#endif

			if (missingDependencies)
			{
                // 我们什么都不做，但没有它我们会收到一个恼人的警告，所以这里我们提供了相关处理。
            }

            if (RequiresCinemachine && !cinemachineFound)
			{
				missingDependencies = true;
				missingString += "Cinemachine";
			}

			if (RequiresTMP && !tmpFound)
			{
				missingDependencies = true;
				if (missingString != "") { missingString += ", "; }
				missingString += "TextMeshPro";
			}
            
			if (RequiresPostProcessing && !postProcessingFound)
			{
				missingDependencies = true;
				if (missingString != "") { missingString += ", "; }
				missingString += "PostProcessing";
			}
            
			#if UNITY_EDITOR
			if (missingDependencies)
			{
				Debug.LogError("[DemoPackageTester] 看起来你缺少这个演示所需的一些依赖项 (" + missingString+")." +
                               " 你必须安装它们才能运行这个演示。你可以在文档中了解更多关于如何安装它们的信息, 在 http://feel-docs.moremountains.com/how-to-install.html" +
				               "\n\n");
                
				if (EditorUtility.DisplayDialog("缺少依赖项!",
                        "这个演示首先需要安装一些依赖项（Cinemachine、TextMesh Pro、PostProcessing）。\n\n" +
                        "当然，你可以不用它们就能使用Feel，但是这个演示需要它们才能正常工作（查看文档以了解更多信息！）。\n\n" +
                        "你愿意自动安装它们吗？“是的，安装依赖项”", "不"))
				{
					MMFDependencyInstaller.InstallFromPlay();
				}
			}
			#endif
		}
	}    
}
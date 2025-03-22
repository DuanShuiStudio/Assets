using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一种用于存储副本信息以及全局反馈设置的资产
    /// 它要求创建一个（且仅创建一个）MMFeedbacksConfiguration资源，并存储在资源文件夹中
    /// 在安装MMFeedbacks时已经完成了这一步
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/MMFeedbacks/Configuration", fileName = "MMFeedbacksConfiguration")]
	public class MMFeedbacksConfiguration : ScriptableObject
	{
		private static MMFeedbacksConfiguration _instance;
		private static bool _instantiated;

        /// <summary>
        /// 单例模式
        /// </summary>
        public static MMFeedbacksConfiguration Instance
		{
			get
			{
				if (_instantiated)
				{
					return _instance;
				}
                
				string assetName = typeof(MMFeedbacksConfiguration).Name;
                
				MMFeedbacksConfiguration loadedAsset = Resources.Load<MMFeedbacksConfiguration>("MMFeedbacksConfiguration");
				_instantiated = true;
				_instance = loadedAsset;
                
				return _instance;
			}
		}

		[Header("Debug调试")]
        /// 复制/粘贴的存储
        public MMFeedbacks _mmFeedbacks;
        
		[Header("Help settings帮助设置")]
        /// 如果这是真的，那么MMFeedbacks的检查器提示将会显示
        public bool ShowInspectorTips = true;
        
		private void OnDestroy(){ _instantiated = false; }
	}    
}
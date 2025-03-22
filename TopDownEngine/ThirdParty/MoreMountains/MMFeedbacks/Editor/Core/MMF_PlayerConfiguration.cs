using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreMountains.Feedbacks
{
    /// <summary>
    /// 一个用于存储复制信息以及全局反馈设置的资源
    /// 它要求创建一个（且仅创建一个）MMFeedbacksConfiguration资源，并存储在Resources文件夹中
    /// 在安装MMFeedbacks时已经完成了这一步骤
    /// </summary>
    [CreateAssetMenu(menuName = "MoreMountains/MMFeedbacks/Configuration", fileName = "MMFeedbacksConfiguration")]
	public class MMF_PlayerConfiguration : ScriptableObject
	{
		private static MMF_PlayerConfiguration _instance;
		private static bool _instantiated;

        /// <summary>
        /// 单例模式
        /// </summary>
        public static MMF_PlayerConfiguration Instance
		{
			get
			{
				if (_instantiated)
				{
					return _instance;
				}
                
				string assetName = typeof(MMF_PlayerConfiguration).Name;
                
				MMF_PlayerConfiguration loadedAsset = Resources.Load<MMF_PlayerConfiguration>("MMF_PlayerConfiguration");
				_instance = loadedAsset;    
				_instantiated = true;
                
				return _instance;
			}
		}
        
		[Header("Help settings帮助设置")]
        /// 如果这是真的，那么将会为MMFeedbacks显示检查器提示
        public bool ShowInspectorTips = true;
        /// 如果这是真的，当KeepPlaymodeChanges处于活动状态时退出播放模式，它会自动关闭，否则它将保持开启。
        public bool AutoDisableKeepPlaymodeChanges = true;
        /// 如果这是真的，当KeepPlaymodeChanges处于活动状态时退出播放模式，它会自动关闭，否则它将保持开启
        public bool InspectorGroupsExpandedByDefault = true;


        
		private void OnDestroy(){ _instantiated = false; }
	}    
}
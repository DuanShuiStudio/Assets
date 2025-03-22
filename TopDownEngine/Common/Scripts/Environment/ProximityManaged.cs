using System;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个可以添加到场景中任何对象上的类，以将其标记为由邻域管理器管理
    /// </summary>
    public class ProximityManaged : TopDownMonoBehaviour
	{
		[Header("Thresholds阈值")]
		/// the distance from the proximity center (the player) under which the object should be enabled
		[Tooltip("物体应当启用时与邻域中心（玩家）之间的距离")]
		public float EnableDistance = 35f;
		/// the distance from the proximity center (the player) after which the object should be disabled
		[Tooltip("物体应当禁用时与邻域中心（玩家）之间的距离")]
		public float DisableDistance = 45f;

		/// whether or not this object was disabled by the ProximityManager
		[MMReadOnly]
		[Tooltip("这个对象是否被邻域管理器禁用")]
		public bool DisabledByManager;

		[Header("Debug调试")] 
		/// a debug manager to add this object to, only used for debug
		[Tooltip("一个调试管理器，仅用于调试时添加此对象。")]
		public ProximityManager DebugProximityManager;
        /// 一个调试按钮，用于将此对象添加到调试管理器
        [MMInspectorButton("DebugAddObject")]
		public bool AddButton;

		public virtual ProximityManager Manager { get; set; }

        /// <summary>
        /// 用于将此对象添加到邻域管理器的调试方法
        /// </summary>
        public virtual void DebugAddObject()
		{
			DebugProximityManager.AddControlledObject(this);
		}

        /// <summary>
        /// 启用时，我们向管理器注册。
        /// </summary>
        private void OnEnable()
		{
			if (Manager != null)
			{
				return;
			}
			
			ProximityManager proximityManager = ProximityManager.Current;
			if (proximityManager != null) 
			{
				var targets = proximityManager.ControlledObjects;
				if (targets != null && targets.Count > 0) 
				{
					if (Manager == null)
					{
						Manager = proximityManager;
						proximityManager.ControlledObjects.Add(this);
					}
				}
			}
		}

        /// <summary>
        /// 销毁时，我们让管理器知道我们已经不存在了
        /// </summary>
        private void OnDestroy()
		{
			if (Manager != null && Manager.ControlledObjects != null)
			{
				Manager.ControlledObjects.Remove(this);
			}
		}
	}
}
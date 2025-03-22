using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个类处理X个玩家的分屏设置
    /// </summary>
    public class MultiplayerSplitCameraRig : TopDownMonoBehaviour, MMEventListener<MMGameEvent>
	{
		[Header("Multiplayer Split Camera Rig")]
		/// the list of camera controllers to bind to level manager players on load
		[Tooltip("在加载时绑定到LevelManager玩家的相机控制器列表")]
		public List<CinemachineCameraController> CameraControllers;

        /// <summary>
        /// 将每个相机控制器绑定到它的目标
        /// </summary>
        protected virtual void BindCameras()
		{
			int i = 0;
			foreach (Character character in LevelManager.Instance.Players)
			{
				CameraControllers[i].TargetCharacter = character;
				CameraControllers[i].FollowsAPlayer = true;
				CameraControllers[i].StartFollowing();
				i++;
			}
		}

        /// <summary>
        /// 当相机准备好被绑定时（通常由LevelManager告知我们），我们就绑定它们
        /// </summary>
        /// <param name="gameEvent"></param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			if (gameEvent.EventName == "CameraBound")
			{
				BindCameras();
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听游戏事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听游戏事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent>();
		}
	}
}
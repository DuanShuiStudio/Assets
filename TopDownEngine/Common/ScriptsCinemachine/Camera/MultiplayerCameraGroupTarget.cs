using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 自动获取一个Cinemachine相机组，并在加载时分配LevelManager的玩家，使一个Cinemachine虚拟相机跟随该目标
    /// </summary>
    public class MultiplayerCameraGroupTarget : TopDownMonoBehaviour, MMEventListener<MMGameEvent>, MMEventListener<TopDownEngineEvent>
	{
#if MM_CINEMACHINE
		[Header("Multiplayer Camera Group Target多人相机组目标")]
		/// the virtual camera that will follow the group target
		[Tooltip("将跟随组目标的虚拟相机")]
		public CinemachineVirtualCamera TargetCamera;
		protected CinemachineTargetGroup _targetGroup;
#elif MM_CINEMACHINE3
        [Header("Multiplayer Camera Group Target多人相机组目标")]
		/// the virtual camera that will follow the group target
		[Tooltip("将跟随组目标的虚拟相机")]
		public CinemachineCamera TargetCamera;
		protected CinemachineTargetGroup _targetGroup;
#endif

        /// <summary>
        /// 在唤醒时，我们获取我们的目标组组件
        /// </summary>
        protected virtual void Awake()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			_targetGroup = this.gameObject.GetComponent<CinemachineTargetGroup>();
			#endif
		}

        /// <summary>
        /// 在加载时，我们将角色绑定到目标组，并让虚拟相机跟随该目标组
        /// </summary>
        /// <param name="gameEvent"></param>
        public virtual void OnMMEvent(MMGameEvent gameEvent)
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (gameEvent.EventName == "Load")
			{
				if (_targetGroup == null)
				{
					return;
				}

				int i = 0;
				#if MM_CINEMACHINE
				_targetGroup.m_Targets = new CinemachineTargetGroup.Target[LevelManager.Instance.Players.Count];
				#elif MM_CINEMACHINE3
				_targetGroup.Targets = new List<CinemachineTargetGroup.Target>(new CinemachineTargetGroup.Target[LevelManager.Instance.Players.Count]);
				#endif

				foreach (Character character in LevelManager.Instance.Players)
				{
					CinemachineTargetGroup.Target target = new CinemachineTargetGroup.Target();
					#if MM_CINEMACHINE
					target.weight = 1;
					target.radius = 0;
					target.target = character.transform;
					_targetGroup.m_Targets[i] = target;
					#elif MM_CINEMACHINE3
					target.Weight = 1;
					target.Radius = 0;
					target.Object = character.transform;
					_targetGroup.Targets[i] = target;
					#endif

					i++;
				}

				TargetCamera.Follow = this.transform;
			}
			#endif
		}

		public virtual void OnMMEvent(TopDownEngineEvent tdEvent)
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (tdEvent.EventType == TopDownEngineEventTypes.PlayerDeath)
			{
				int i = 0;
				foreach (Character character in LevelManager.Instance.Players)
				{
					if (character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Dead)
					{
						#if MM_CINEMACHINE
						_targetGroup.m_Targets[i].weight = 0f;
						#elif MM_CINEMACHINE3
						_targetGroup.Targets[i].Weight = 0f;
						#endif
					}
					i++;
				}
			}
			#endif
		}

        /// <summary>
        /// 开始监听游戏事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMGameEvent>();
			this.MMEventStartListening<TopDownEngineEvent>();
		}

        /// <summary>
        /// 停止监听游戏事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMGameEvent>();
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}
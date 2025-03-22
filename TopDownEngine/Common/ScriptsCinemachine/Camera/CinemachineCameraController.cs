using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 一个处理Cinemachine驱动的相机跟随的类
    /// </summary>
    public class CinemachineCameraController : TopDownMonoBehaviour, MMEventListener<MMCameraEvent>, MMEventListener<TopDownEngineEvent>
	{
        /// 如果相机应该跟随玩家，则返回真
        public virtual bool FollowsPlayer { get; set; }
		/// Whether or not this camera should follow a player
		[Tooltip("无论这台相机是否应该跟随玩家")]
		public bool FollowsAPlayer = true;
		/// Whether to confine this camera to the level bounds, as defined in the LevelManager
		[Tooltip("是否将此相机限制在LevelManager中定义的关卡边界内")]
		public bool ConfineCameraToLevelBounds = true;
		/// If this is true, this confiner will listen to set confiner events
		[Tooltip("如果这是真的，这个限制器将会监听设置限制器事件")]
		public bool ListenToSetConfinerEvents = true;
		[MMReadOnly]
		/// the target character this camera should follow
		[Tooltip("这个相机应该跟随的目标角色")]
		public Character TargetCharacter;

		#if MM_CINEMACHINE
		protected CinemachineVirtualCamera _virtualCamera;
		protected CinemachineConfiner _confiner;
		#elif MM_CINEMACHINE3
		protected CinemachineCamera _virtualCamera;
		protected CinemachineConfiner3D _confiner;
		protected CinemachineConfiner2D _confiner2D;
		#endif

		protected int _lastStopFollow = -1;

        /// <summary>
        /// 在唤醒时，我们获取我们的组件
        /// </summary>
        protected virtual void Awake()
		{
			#if MM_CINEMACHINE
			_virtualCamera = GetComponent<CinemachineVirtualCamera>();
			_confiner = GetComponent<CinemachineConfiner>();
			#elif MM_CINEMACHINE3
			_virtualCamera = GetComponent<CinemachineCamera>();
			_confiner = GetComponent<CinemachineConfiner3D>();
			_confiner2D = GetComponent<CinemachineConfiner2D>();
			#endif
		}

        /// <summary>
        /// 在开始时，我们分配我们的边界体积
        /// </summary>
        protected virtual void Start()
		{
			#if MM_CINEMACHINE
			if ((_confiner != null) && ConfineCameraToLevelBounds && LevelManager.HasInstance)
			{
				_confiner.m_BoundingVolume = LevelManager.Instance.BoundsCollider;
			}
			#elif MM_CINEMACHINE3
			if (ConfineCameraToLevelBounds && LevelManager.HasInstance)
			{
				if (_confiner != null)
				{
					_confiner.BoundingVolume = LevelManager.Instance.BoundsCollider;	
				}
				if (_confiner2D != null)
				{
					_confiner2D.BoundingShape2D = LevelManager.Instance.BoundsCollider2D;	
				}
			}
			#endif
		}

		public virtual void SetTarget(Character character)
		{
			TargetCharacter = character;
		}

        /// <summary>
        /// 开始跟随LevelManager的主玩家
        /// </summary>
        public virtual void StartFollowing()
		{
			StartCoroutine(StartFollowingCo());
		}

		protected virtual IEnumerator StartFollowingCo()
		{
			if (_lastStopFollow > 0 && _lastStopFollow == Time.frameCount)
			{
				yield return null;
			}
			if (!FollowsAPlayer) { yield break; }
			FollowsPlayer = true;
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			_virtualCamera.Follow = TargetCharacter.CameraTarget.transform;
			_virtualCamera.enabled = true;
			#endif
			
			
			
		}

        /// <summary>
        /// 停止跟随任何目标
        /// </summary>
        public virtual void StopFollowing()
		{
			if (!FollowsAPlayer) { return; }
			FollowsPlayer = false;
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			_virtualCamera.Follow = null;
			_virtualCamera.enabled = false;
			#endif
			_lastStopFollow = Time.frameCount;
		}

		public virtual void OnMMEvent(MMCameraEvent cameraEvent)
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			switch (cameraEvent.EventType)
			{
				case MMCameraEventTypes.SetTargetCharacter:
					SetTarget(cameraEvent.TargetCharacter);
					break;

				case MMCameraEventTypes.SetConfiner:                    
					if (ListenToSetConfinerEvents)
					{
						#if MM_CINEMACHINE
						if (_confiner != null)
						{
							_confiner.m_BoundingVolume = cameraEvent.Bounds;
						}
						#elif MM_CINEMACHINE3
						if (_confiner != null)
						{
							_confiner.BoundingVolume = cameraEvent.Bounds;	
						}
						if (_confiner2D != null)
						{
							_confiner2D.BoundingShape2D = cameraEvent.Bounds2D;	
						}
						
						#endif
					}
					break;

				case MMCameraEventTypes.StartFollowing:
					if (cameraEvent.TargetCharacter != null)
					{
						if (cameraEvent.TargetCharacter != TargetCharacter)
						{
							return;
						}
					}
					StartFollowing();
					break;

				case MMCameraEventTypes.StopFollowing:
					if (cameraEvent.TargetCharacter != null)
					{
						if (cameraEvent.TargetCharacter != TargetCharacter)
						{
							return;
						}
					}
					StopFollowing();
					break;

				case MMCameraEventTypes.RefreshPosition:
					StartCoroutine(RefreshPosition());
					break;

				case MMCameraEventTypes.ResetPriorities:
					_virtualCamera.Priority = 0;
					break;
			}
			#endif
		}

		protected virtual IEnumerator RefreshPosition()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			_virtualCamera.enabled = false;
			#endif
			yield return null;
			StartFollowing();
		}

		public virtual void OnMMEvent(TopDownEngineEvent topdownEngineEvent)
		{
			if (topdownEngineEvent.EventType == TopDownEngineEventTypes.CharacterSwitch)
			{
				SetTarget(LevelManager.Instance.Players[0]);
				StartFollowing();
			}

			if (topdownEngineEvent.EventType == TopDownEngineEventTypes.CharacterSwap)
			{
				SetTarget(LevelManager.Instance.Players[0]);
				MMCameraEvent.Trigger(MMCameraEventTypes.RefreshPosition);
			}
		}

		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMCameraEvent>();
			this.MMEventStartListening<TopDownEngineEvent>();
		}

		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMCameraEvent>();
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}
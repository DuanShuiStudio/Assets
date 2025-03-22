using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using UnityEngine.Events;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 这个类让你定义关卡中房间的边界。
    /// 如果你想把关卡切割成几部分（想想超级马里奥或空洞骑士等例子），房间就很有用。
    /// 这些房间将需要它们自己的虚拟相机，以及一个定义它们大小的边界
    /// 注意，边界与定义房间的碰撞器不同
    /// 你可以在KoalaRooms演示场景中看到房间的示例。
    /// </summary>
    public class Room : TopDownMonoBehaviour, MMEventListener<TopDownEngineEvent>
	{
		public enum Modes { TwoD, ThreeD }

        /// 这个房间的碰撞器。
        public Vector3 RoomColliderCenter
		{
			get
			{
				if (_roomCollider2D != null)
				{
					return _roomCollider2D.bounds.center;
				}
				else
				{
					return _roomCollider.bounds.center;
				}
			}
		}
        
		public Vector3 RoomColliderSize
		{
			get
			{
				if (_roomCollider2D != null)
				{
					return _roomCollider2D.bounds.size;
				}
				else
				{
					return _roomCollider.bounds.size;
				}
			}
		}

		public Bounds RoomBounds
		{
			get
			{
				if (_roomCollider2D != null)
				{
					return _roomCollider2D.bounds;
				}
				else
				{
					return _roomCollider.bounds;
				}
			}
		}

		[Header("Mode模式")]
		/// whether this room is intended to work in 2D or 3D mode
		[Tooltip("这个房间是打算在2D模式还是3D模式下工作")]
		public Modes Mode = Modes.TwoD;

#if MM_CINEMACHINE
		[Header("Camera相机")]
		/// the virtual camera associated to this room
		[Tooltip("与这个房间相关联的虚拟相机")]
		public CinemachineVirtualCamera VirtualCamera;
		/// the confiner for this room, that will constrain the virtual camera, usually placed on a child object of the Room
		[Tooltip("这个房间的边界2D，它将约束虚拟相机，通常被放置在Room的子对象上")]
		public BoxCollider Confiner;
		/// the confiner component of the virtual camera
		[Tooltip("这个房间的边界3D，它将约束虚拟相机，通常被放置在Room的子对象上")]
		public CinemachineConfiner CinemachineCameraConfiner;
#elif MM_CINEMACHINE3
        [Header("Camera相机")]
		/// the virtual camera associated to this room
		[Tooltip("与这个房间相关联的虚拟相机")]
		public CinemachineCamera VirtualCamera;
		/// the confiner for this room, that will constrain the virtual camera, usually placed on a child object of the Room
		[Tooltip("这个房间的边界2D，它将约束虚拟相机，通常被放置在Room的子对象上")]
		public BoxCollider2D Confiner2D;
		/// the confiner for this room, that will constrain the virtual camera, usually placed on a child object of the Room
		[Tooltip("这个房间的边界3D，它将约束虚拟相机，通常被放置在Room的子对象上")]
		public BoxCollider Confiner3D;
		/// the confiner component of the virtual camera
		[Tooltip("虚拟相机的2D边界组件")]
		public CinemachineConfiner2D CinemachineCameraConfiner2D;
		/// the confiner component of the virtual camera
		[Tooltip("虚拟相机的3D边界组件")]
		public CinemachineConfiner3D CinemachineCameraConfiner3D;
		#endif
		/// whether or not the confiner should be auto resized on start to match the camera's size and ratio
		[Tooltip("是否应该在开始时自动调整边界的大小以匹配相机的大小和比例")]
		public bool ResizeConfinerAutomatically = true;
		/// whether or not this Room should look at the level's start position and declare itself the current room on start or not
		[Tooltip("这个房间是否应该在开始时查看关卡的起始位置，并声明自己是当前房间")]
		public bool AutoDetectFirstRoomOnStart = true;
		/// the depth of the room (used to resize the z value of the confiner
		[MMEnumCondition("Mode", (int)Modes.TwoD)]
		[Tooltip("房间的深度（用于调整边界的z值大小）")]
		public float RoomDepth = 100f;

		[Header("State状态")]
		/// whether this room is the current room or not
		[Tooltip("这个房间是否是当前房间")]
		public bool CurrentRoom = false;
		/// whether this room has already been visited or not
		[Tooltip("这个房间是否已经被访问过")]
		public bool RoomVisited = false;

		[Header("Actions操作")]
		/// the event to trigger when the player enters the room for the first time
		[Tooltip("当玩家第一次进入房间时触发的事件")]
		public UnityEvent OnPlayerEntersRoomForTheFirstTime;
		/// the event to trigger everytime the player enters the room
		[Tooltip("每次玩家进入房间时触发的事件")]
		public UnityEvent OnPlayerEntersRoom;
		/// the event to trigger everytime the player exits the room
		[Tooltip("每次玩家退出房间时触发的事件")]
		public UnityEvent OnPlayerExitsRoom;

		[Header("Activation激活")]
		/// a list of gameobjects to enable when entering the room, and disable when exiting it
		[Tooltip("进入房间时启用的游戏对象列表，退出房间时禁用")]
		public List<GameObject> ActivationList;

		protected Collider _roomCollider;
		protected Collider2D _roomCollider2D;
		protected Camera _mainCamera;
		protected Vector2 _cameraSize;
		protected bool _initialized = false;

        /// <summary>
        /// 在唤醒时，我们重置相机的优先级
        /// </summary>
        protected virtual void Awake()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (VirtualCamera != null)
			{
				VirtualCamera.Priority = 0;	
			}
			#endif
		}

        /// <summary>
        /// 在开始时，我们初始化房间
        /// </summary>
        protected virtual void Start()
		{
			Initialization();
		}

        /// <summary>
        /// 获取我们的房间碰撞器、主相机，并开始调整边界大小
        /// </summary>
        protected virtual void Initialization()
		{
			if (_initialized)
			{
				return;
			}
			_roomCollider = this.gameObject.GetComponent<Collider>();
			_roomCollider2D = this.gameObject.GetComponent<Collider2D>();
			_mainCamera = Camera.main;          
			StartCoroutine(ResizeConfiner());
			_initialized = true;
		}

        /// <summary>
        /// 调整边界大小
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator ResizeConfiner()
		{
#if MM_CINEMACHINE
			if ((VirtualCamera == null) || (Confiner == null) || !ResizeConfinerAutomatically)
			{
				yield break;
			}

			// 我们再等待两帧，以便Unity的像素完美相机组件准备好，因为显然发送事件不是一件事。
			yield return null;
			yield return null;

			Confiner.transform.position = RoomColliderCenter;
			Vector3 size = RoomColliderSize;

			switch (Mode)
			{
				case Modes.TwoD:
					size.z = RoomDepth;
					Confiner.size = size;
					_cameraSize.y = 2 * _mainCamera.orthographicSize;
					_cameraSize.x = _cameraSize.y * _mainCamera.aspect;

					Vector3 newSize = Confiner.size;

					if (Confiner.size.x < _cameraSize.x)
					{
						newSize.x = _cameraSize.x;
					}
					if (Confiner.size.y < _cameraSize.y)
					{
						newSize.y = _cameraSize.y;
					}

					Confiner.size = newSize;
					break;
				case Modes.ThreeD:
					Confiner.size = size;
					break;
			}
            
			CinemachineCameraConfiner.InvalidatePathCache();
#elif MM_CINEMACHINE3
            if ((VirtualCamera == null) || ((Confiner2D == null) && (Confiner3D == null)) || !ResizeConfinerAutomatically)
			{
				yield break;
			}

            // 我们再等待两帧，以便Unity的像素完美相机组件准备好，因为显然发送事件不是一件事。
            yield return null;
			yield return null;

			if (Confiner2D != null)
			{
				Confiner2D.transform.position = RoomColliderCenter;	
			}

			if (Confiner3D != null)
			{
				Confiner3D.transform.position = RoomColliderCenter;	
			}
			
			Vector3 size = RoomColliderSize;

			switch (Mode)
			{
				case Modes.TwoD:
					size.z = RoomDepth;
					Confiner2D.size = size;
					_cameraSize.y = 2 * _mainCamera.orthographicSize;
					_cameraSize.x = _cameraSize.y * _mainCamera.aspect;

					Vector3 newSize = Confiner2D.size;

					if (Confiner2D.size.x < _cameraSize.x)
					{
						newSize.x = _cameraSize.x;
					}
					if (Confiner2D.size.y < _cameraSize.y)
					{
						newSize.y = _cameraSize.y;
					}

					Confiner2D.size = newSize;
					break;
				case Modes.ThreeD:
					Confiner3D.size = size;
					break;
			}

			CinemachineCameraConfiner2D.InvalidateBoundingShapeCache();
			#else
			yield return null;
			#endif
		}

        /// <summary>
        /// 寻找关卡的起始位置，如果它在房间内，则将此房间设为当前房间
        /// </summary>
        protected virtual void HandleLevelStartDetection()
		{
			#if MM_CINEMACHINE || MM_CINEMACHINE3	
			if (!_initialized)
			{
				Initialization();
			}

			if (AutoDetectFirstRoomOnStart)
			{
				if (LevelManager.HasInstance)
				{
					if (RoomBounds.Contains(LevelManager.Instance.Players[0].transform.position))
					{
						MMCameraEvent.Trigger(MMCameraEventTypes.ResetPriorities);
						MMCinemachineBrainEvent.Trigger(MMCinemachineBrainEventTypes.ChangeBlendDuration, 0f);

						MMSpriteMaskEvent.Trigger(MMSpriteMaskEvent.MMSpriteMaskEventTypes.MoveToNewPosition,
							RoomColliderCenter,
							RoomColliderSize,
							0f, MMTween.MMTweenCurve.LinearTween);

						PlayerEntersRoom();
						if (VirtualCamera != null)
						{
							VirtualCamera.Priority = 10;
							VirtualCamera.enabled = true;	
						}
					}
					else
					{
						if (VirtualCamera != null)
						{
							VirtualCamera.Priority = 0;
							VirtualCamera.enabled = false;	
						}
					}
				}
			}
			#endif
		}

        /// <summary>
        /// 调用此方法让房间知道玩家进入了
        /// </summary>
        public virtual void PlayerEntersRoom()
		{
			CurrentRoom = true;
			if (RoomVisited)
			{
				OnPlayerEntersRoom?.Invoke();
			}
			else
			{
				RoomVisited = true;
				OnPlayerEntersRoomForTheFirstTime?.Invoke();
			}  
			foreach(GameObject go in ActivationList)
			{
				go.SetActive(true);
			}
		}

        /// <summary>
        /// 调用此方法让这个房间知道玩家退出了
        /// </summary>
        public virtual void PlayerExitsRoom()
		{
			CurrentRoom = false;
			OnPlayerExitsRoom?.Invoke();
			foreach (GameObject go in ActivationList)
			{
				go.SetActive(false);
			}
		}

        /// <summary>
        /// 当我们收到重生事件时，我们请求重新定位相机
        /// </summary>
        /// <param name="topDownEngineEvent"></param>
        public virtual void OnMMEvent(TopDownEngineEvent topDownEngineEvent)
		{
			if ((topDownEngineEvent.EventType == TopDownEngineEventTypes.RespawnComplete)
			    || (topDownEngineEvent.EventType == TopDownEngineEventTypes.LevelStart))
			{
				HandleLevelStartDetection();
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听事件
        /// </summary>
        protected virtual void OnEnable()
		{
			this.MMEventStartListening<TopDownEngineEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听事件
        /// </summary>
        protected virtual void OnDisable()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}
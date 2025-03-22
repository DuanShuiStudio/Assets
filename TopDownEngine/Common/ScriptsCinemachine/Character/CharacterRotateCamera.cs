using System.Collections;
using UnityEngine;
#if MM_CINEMACHINE
using Cinemachine;
#elif MM_CINEMACHINE3
using Unity.Cinemachine;
#endif
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    ///一个功能，让角色使用PlayerID_CameraRotationAxis输入轴来旋转其关联的相机
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/Abilities/Character Rotate Camera")]
	public class CharacterRotateCamera : CharacterAbility, MMEventListener<TopDownEngineEvent>
	{
        /// 此方法仅用于在能力的检查器开始时显示帮助框文本
        public override string HelpBoxText() { return "一个功能，让角色使用PlayerID_CameraRotationAxis输入轴来旋转其关联的相机"; }

		[Header("Rotation axis旋转轴")]
		/// the space in which to rotate the camera (usually world)
		[Tooltip("旋转相机的空间（通常是世界空间）")]
		public Space RotationSpace = Space.World;
		/// the camera's forward vector, usually 0,0,1
		[Tooltip("相机的向前向量，通常是0,0,1")]
		public Vector3 RotationForward = Vector3.forward;
		/// the axis on which to rotate the camera (usually 0,1,0 in 3D, 0,0,1 in 2D)
		[Tooltip("旋转相机的轴（通常在3D中是0,1,0，在2D中是0,0,1）")]
		public Vector3 RotationAxis = Vector3.up;

		[Header("Camera Speed相机速度")]
		/// the speed at which the camera should rotate
		[Tooltip("相机应该旋转的速度")]
		public float CameraRotationSpeed = 1f;
		/// the speed at which the camera should interpolate towards its target position
		[Tooltip("相机应该插值到其目标位置的速度")]
		public float CameraInterpolationSpeed = 0.2f;

		[Header("Input Manager输入管理器")] 
		/// if this is false, this ability won't read input
		[Tooltip("如果这是假的，这个能力将不会读取输入")]
		public bool InputAuthorized = true;
		/// whether or not this ability should make changes on the InputManager to set it in camera driven input mode
		[Tooltip("这个能力是否应该在InputManager上做出改变，以将其设置为相机驱动的输入模式")]
		public bool AutoSetupInputManager = true;

		protected float _requestedCameraAngle = 0f;
		protected Camera _mainCamera;
		#if MM_CINEMACHINE
		protected CinemachineBrain _brain;
		protected CinemachineVirtualCamera _virtualCamera;
		#elif MM_CINEMACHINE3
		protected CinemachineBrain _brain;
		protected CinemachineCamera _virtualCamera;
		#endif
		protected float _targetRotationAngle;
		protected Vector3 _cameraDirection;
		protected float _cameraDirectionAngle;

        /// <summary>
        /// 在初始化时，我们获取我们的相机并在需要时设置我们的输入管理器
        /// </summary>
        protected override void Initialization()
		{
			base.Initialization();
			_mainCamera = Camera.main;
			StartCoroutine(DelayedInitialization());
			if (AutoSetupInputManager)
			{
				_inputManager.RotateInputBasedOnCameraDirection = true;
				bool camera3D = (_character.CharacterDimension == Character.CharacterDimensions.Type3D);
				_inputManager.SetCamera(_mainCamera, camera3D);
			}
		}

        /// <summary>
        /// 因为Cinemachine只在LateUpdate时初始化，并且不提供事件来知道它何时准备就绪，我们稍微等待一下让它完成
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator DelayedInitialization()
		{
			yield return MMCoroutine.WaitForFrames(2);
			GetCurrentCamera();
		}

        /// <summary>
        /// 存储当前相机
        /// </summary>
        protected virtual void GetCurrentCamera()
		{
			#if MM_CINEMACHINE
			_brain = _mainCamera.GetComponent<CinemachineBrain>();
			if (_brain != null)
			{
				_virtualCamera = _brain.ActiveVirtualCamera as CinemachineVirtualCamera;
			}
			#elif MM_CINEMACHINE3
			_brain = _mainCamera.GetComponent<CinemachineBrain>();
			if (_brain != null)
			{
				_virtualCamera = _brain.ActiveVirtualCamera as CinemachineCamera;
			}
			#endif
		}

        /// <summary>
        /// 如果InputAuthorized为假，你可以使用此方法从另一个脚本强制设置相机角度
        /// </summary>
        /// <param name="newAngle"></param>
        public virtual void SetCameraAngle(float newAngle)
		{
			_requestedCameraAngle = newAngle;
		}

        /// <summary>
        /// 读取输入以了解相机的请求旋转角度
        /// </summary>
        protected override void HandleInput()
		{
			base.HandleInput();
			if (!InputAuthorized)
			{
				return;
			}
			_requestedCameraAngle = _inputManager.CameraRotationInput * CameraRotationSpeed * Time.deltaTime * 100f;
		}

        /// <summary>
        /// 每一帧我们旋转相机
        /// </summary>
        public override void ProcessAbility()
		{
			base.ProcessAbility();
			if (!AbilityAuthorized)
			{
				return;
			}
			RotateCamera();
		}

        /// <summary>
        /// 将相机的旋转更改为与输入匹配
        /// </summary>
        protected virtual void RotateCamera()
		{
			_targetRotationAngle = MMMaths.Lerp(_targetRotationAngle, _requestedCameraAngle, CameraInterpolationSpeed, Time.deltaTime);

			#if MM_CINEMACHINE || MM_CINEMACHINE3
			if (_virtualCamera != null)
			{
				_virtualCamera.transform.Rotate(RotationAxis, _targetRotationAngle, RotationSpace);
				_cameraDirectionAngle = (_character.CharacterDimension == Character.CharacterDimensions.Type3D) ? _virtualCamera.transform.localEulerAngles.y : _virtualCamera.transform.localEulerAngles.z;

			}
			else  if (_mainCamera != null)
			{
				_mainCamera.transform.Rotate(RotationAxis, _targetRotationAngle, RotationSpace);
				_cameraDirectionAngle = (_character.CharacterDimension == Character.CharacterDimensions.Type3D) ? _mainCamera.transform.localEulerAngles.y : _mainCamera.transform.localEulerAngles.z;
			}
			#endif
			_cameraDirection = Quaternion.AngleAxis(_cameraDirectionAngle, RotationAxis) * RotationForward;
			_character.SetCameraDirection(_cameraDirection);
		}

        /// <summary>
        /// 在关卡开始时强制对相机进行新的初始化
        /// </summary>
        /// <param name="engineEvent"></param>
        public virtual void OnMMEvent(TopDownEngineEvent engineEvent)
		{
			if (!AbilityAuthorized)
			{
				return;
			}

			switch (engineEvent.EventType)
			{
				case TopDownEngineEventTypes.LevelStart:
					Initialization();
					break;
			}
		}

        /// <summary>
        /// 在启用时，我们开始监听事件
        /// </summary>
        protected override void OnEnable()
		{
			base.OnEnable();
			this.MMEventStartListening<TopDownEngineEvent>();
		}

        /// <summary>
        /// 在禁用时，我们停止监听事件
        /// </summary>
        protected virtual void OnDestroy()
		{
			this.MMEventStopListening<TopDownEngineEvent>();
		}
	}
}
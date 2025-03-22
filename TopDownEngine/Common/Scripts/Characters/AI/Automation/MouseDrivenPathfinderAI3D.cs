using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.TopDownEngine
{
    /// <summary>
    /// 添加到CharacterPathfinder3D装备角色上的类。
    /// 它将允许你点击屏幕上的任何地方，这将确定一个新的目标和角色将路径找到它的方式
    /// </summary>
    [AddComponentMenu("TopDown Engine/Character/AI/Automation/Mouse Driven Pathfinder AI 3D")]
	public class MouseDrivenPathfinderAI3D : TopDownMonoBehaviour 
	{
		[Header("Testing测试")]
		/// the camera we'll use to determine the destination from
		[Tooltip("我们用来确定目的地的摄像机")]
		public Camera Cam;
		/// a gameobject used to show the destination
		[Tooltip("用于显示目的地的游戏对象")]
		public GameObject Destination;

		protected CharacterPathfinder3D _characterPathfinder3D;
		protected Plane _playerPlane;
		protected bool _destinationSet = false;
		protected Camera _mainCamera;

        /// <summary>
        /// 醒着的时候，我们创建一个平面来捕捉光线
        /// </summary>
        protected virtual void Awake()
		{
			_mainCamera = Camera.main;
			_characterPathfinder3D = this.gameObject.GetComponent<CharacterPathfinder3D>();
			_playerPlane = new Plane(Vector3.up, Vector3.zero);
		}

        /// <summary>
        /// 在更新中，我们寻找鼠标点击
        /// </summary>
        protected virtual void Update()
		{
			DetectMouse();
		}

        /// <summary>
        /// 如果点击鼠标，我们将投射光线，如果光线击中平面，我们将其作为寻路目标
        /// </summary>
        protected virtual void DetectMouse()
		{
			if (Input.GetMouseButtonDown(0))
			{
				Ray ray = _mainCamera.ScreenPointToRay(InputManager.Instance.MousePosition);
				Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
				float distance;
				if (_playerPlane.Raycast(ray, out distance))
				{
					Vector3 target = ray.GetPoint(distance);
					Destination.transform.position = target;
					_destinationSet = true;
					_characterPathfinder3D.SetNewDestination(Destination.transform);
				}
			}
		}
	}
}